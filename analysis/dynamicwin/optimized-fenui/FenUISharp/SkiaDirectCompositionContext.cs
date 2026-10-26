using System;
using System.Threading;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using SharpGen.Runtime;
using SkiaSharp;
using Vortice.DXGI;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DirectComposition;
using Vortice.Mathematics;

namespace FenUISharp;

public class SkiaDirectCompositionContext : IDisposable
{
	internal enum ResizingMethod
	{
		Stretch,
		PerFrame,
		SmartSmoothing
	}

	private ID3D12Resource? backBuffer;

	private GRD3DTextureResourceInfo? resourceInfo;

	private GRBackendTexture? backendTexture;

	private ID3D12Resource? persistentRenderTarget;

	private IDXGIAdapter1? adapter;

	private GRD3DBackendContext? backendContext;

	private uint currentBackBufferIndex = uint.MaxValue;

	protected int width;

	protected int height;

	private bool resourcesNeedRecreation = true;

	private readonly object resourceLock = new object();

	private bool deviceLost;

	private int consecutiveDrawErrors;

	private const int MAX_CONSECUTIVE_ERRORS = 3;

	private int resizeOverflowPadding = 350;

	private bool _wasResizing;

	private Vector2 currentBufferSizeIncludingPad;

	private bool _isDisposed;

	private WeakReference<FWindow> window { get; set; }

	public FWindow Window
	{
		get
		{
			if (!window.TryGetTarget(out FWindow target))
			{
				throw new Exception("Window not set.");
			}
			return target;
		}
	}

	public Action<SKCanvas>? DrawAction { get; private set; }

	public DirectCompositionContext? DirectCompositionContext { get; protected set; }

	public SKSurface? Surface { get; protected set; }

	public GRContext? grContext { get; protected set; }

	public static SKSamplingOptions SamplingOptions => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

	public Action? OnRebuildAdditionals { get; set; }

	public Action? OnDisposeAdditionals { get; set; }

	public Action<SKSurface>? OnFrameDone { get; set; }

	internal ResizingMethod Method { get; set; } = ResizingMethod.SmartSmoothing;

	public SkiaDirectCompositionContext(FWindow window, Action<SKCanvas> drawAction)
	{
		this.window = new WeakReference<FWindow>(window);
		DrawAction = drawAction;
		if (FenUI.debugEnabled && D3D12.D3D12GetDebugInterface<ID3D12Debug>(out ID3D12Debug debugInterface).Success)
		{
			debugInterface?.EnableDebugLayer();
		}
		FLogger.Log<SkiaDirectCompositionContext>("Creating SkiaDirectCompositionContext...");
		width = (int)window.Shape.ClientSize.x;
		height = (int)window.Shape.ClientSize.y;
		InitializeDeviceAndContext();
		FWindowCallbacks callbacks = Window.Callbacks;
		callbacks.OnWindowResize = (Action<Vector2>)Delegate.Combine(callbacks.OnWindowResize, new Action<Vector2>(OnResize));
		FWindowCallbacks callbacks2 = Window.Callbacks;
		callbacks2.OnWindowEndResize = (Action<Vector2>)Delegate.Combine(callbacks2.OnWindowEndResize, new Action<Vector2>(OnResizeEnd));
		FWindowCallbacks callbacks3 = Window.Callbacks;
		callbacks3.DPIChanged = (Action)Delegate.Combine(callbacks3.DPIChanged, new Action(WindowDPIChanged));
		FLogger.Log<SkiaDirectCompositionContext>("Done creating SkiaDirectCompositionContext!");
	}

	private void WindowDPIChanged()
	{
		FLogger.Log<SkiaDirectCompositionContext>($"New window DPI: {Window.Shape.WindowDPIScale}");
		UpdateResizeDPIScale();
		PerformBufferResize(Window.Shape.Size);
		Window.Redraw();
	}

	private void InitializeDeviceAndContext()
	{
		try
		{
			FLogger.Log<SkiaDirectCompositionContext>("Creating DirectCompositionContext for SDXCC...");
			DirectCompositionContext = new DirectCompositionContext(Window.hWnd, width, height);
			FLogger.Log<SkiaDirectCompositionContext>("Getting adapter...");
			adapter = DirectCompositionContext.GetHardwareAdapter();
			FLogger.Log<SkiaDirectCompositionContext>("Creating D3D Backend Context...");
			backendContext = new GRD3DBackendContext
			{
				Device = DirectCompositionContext.Device.NativePointer,
				Adapter = adapter.NativePointer,
				ProtectedContext = false,
				Queue = (DirectCompositionContext.CommandQueue ?? throw new NullReferenceException("Native Pointer of Command Queue is null.")).NativePointer
			};
			FLogger.Log<SkiaDirectCompositionContext>("Creating Skia D3D GRContext...");
			grContext = GRContext.CreateDirect3D(backendContext);
			if (grContext == null)
			{
				throw new Exception("Failed to create Skia D3D GRContext");
			}
			EnsureResourcesCreated();
			deviceLost = false;
			consecutiveDrawErrors = 0;
		}
		catch (Exception ex)
		{
			deviceLost = true;
			FLogger.Error("Failed to initialize device and context: " + ex.Message);
			throw new InvalidOperationException(DirectCompositionContext?.Device.DeviceRemovedReason.Description);
		}
	}

	private bool IsDeviceValid()
	{
		if (DirectCompositionContext?.Device == null)
		{
			return false;
		}
		return DirectCompositionContext.Device.DeviceRemovedReason == 0;
	}

	private void RecoverFromDeviceRemoval()
	{
		if (deviceLost)
		{
			return;
		}
		deviceLost = true;
		FLogger.Warn("Device removed, attempting recovery...");
		lock (resourceLock)
		{
			try
			{
				DisposeSkiaResources();
				grContext?.Dispose();
				grContext = null;
				adapter?.Dispose();
				adapter = null;
				DirectCompositionContext?.Dispose();
				DirectCompositionContext = null;
				consecutiveDrawErrors = 0;
				resourcesNeedRecreation = true;
				currentBackBufferIndex = uint.MaxValue;
				Thread.Sleep(200);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				InitializeDeviceAndContext();
				deviceLost = false;
				FLogger.Log<SkiaDirectCompositionContext>("Device recovery successful");
			}
			catch (Exception ex)
			{
				FLogger.Error("Device recovery failed: " + ex.Message);
				deviceLost = true;
				try
				{
					grContext?.Dispose();
					grContext = null;
					DirectCompositionContext?.Dispose();
					DirectCompositionContext = null;
					adapter?.Dispose();
					adapter = null;
				}
				catch
				{
				}
				throw new InvalidOperationException("Device recovery failed: " + ex.Message, ex);
			}
		}
	}

	private void EnsureResourcesCreated()
	{
		lock (resourceLock)
		{
			if (resourcesNeedRecreation || Surface == null || !IsDeviceValid())
			{
				resourcesNeedRecreation = false;
				CreateTexturesAndSurface();
			}
		}
	}

	private void CreateTexturesAndSurface()
	{
		FLogger.Log<SkiaDirectCompositionContext>("Recreating skia surface...");
		try
		{
			if (IsDeviceValid())
			{
				DirectCompositionContext?.WaitForGpu();
				DisposeSkiaResources();
				ResourceDescription description = ResourceDescription.Texture2D(Format.B8G8R8A8_UNorm, (uint)width, (uint)height, 1, 1, 1u, 0u, ResourceFlags.AllowRenderTarget, TextureLayout.Unknown, 0uL);
				ClearValue value = new ClearValue
				{
					Format = Format.B8G8R8A8_UNorm,
					Color = new Color4(0f, 0f, 0f, 0f)
				};
				persistentRenderTarget = DirectCompositionContext?.Device.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, description, ResourceStates.RenderTarget, value);
				if (persistentRenderTarget == null)
				{
					throw new Exception("Failed to create persistent render target");
				}
				resourceInfo = new GRD3DTextureResourceInfo
				{
					Resource = persistentRenderTarget.NativePointer,
					ResourceState = 4u,
					Format = 87u,
					SampleCount = 1u,
					LevelCount = 1u,
					SampleQualityPattern = 0u,
					Protected = false
				};
				backendTexture = new GRBackendTexture(width, height, resourceInfo);
				Surface = SKSurface.Create(grContext, backendTexture, GRSurfaceOrigin.TopLeft, SKColorType.Bgra8888);
				if (Surface == null)
				{
					throw new Exception("Failed to create Skia surface");
				}
				Surface.Canvas.Clear(SKColors.Transparent);
				Surface.Canvas.Flush();
				Window.Redraw();
			}
		}
		catch (Exception ex)
		{
			FLogger.Error("Failed to create textures and surface: " + ex.Message);
			DisposeSkiaResources();
			resourcesNeedRecreation = true;
			throw;
		}
	}

	private void SwapFromPersistent((ID3D12GraphicsCommandList commandList, ID3D12Resource backBuffer) context)
	{
		try
		{
			if (persistentRenderTarget == null)
			{
				throw new InvalidOperationException("Persistent render target is null");
			}
			Surface?.Canvas?.Flush();
			Surface?.Flush();
			grContext?.Submit(synchronous: true);
			ResourceBarrier[] barriers = new ResourceBarrier[2]
			{
				ResourceBarrier.BarrierTransition(persistentRenderTarget, ResourceStates.RenderTarget, ResourceStates.CopySource),
				ResourceBarrier.BarrierTransition(context.backBuffer, ResourceStates.RenderTarget, ResourceStates.CopyDest)
			};
			context.commandList.ResourceBarrier(barriers);
			context.commandList.CopyResource(context.backBuffer, persistentRenderTarget);
			ResourceBarrier[] barriers2 = new ResourceBarrier[1] { ResourceBarrier.BarrierTransition(persistentRenderTarget, ResourceStates.CopySource, ResourceStates.RenderTarget) };
			context.commandList.ResourceBarrier(barriers2);
		}
		catch (Exception ex)
		{
			FLogger.Error("Error in SwapFromPersistent: " + ex.Message);
			throw;
		}
	}

	internal void Draw()
	{
		if (deviceLost || _isDisposed)
		{
			return;
		}
		try
		{
			if (Window.Procedure._isSizeMoving && Method == ResizingMethod.Stretch)
			{
				return;
			}
			lock (resourceLock)
			{
				if (!IsDeviceValid())
				{
					RecoverFromDeviceRemoval();
					return;
				}
				EnsureResourcesCreated();
				if (Surface?.Canvas == null)
				{
					FLogger.Warn("Surface or Canvas is null, skipping draw");
					return;
				}
				DirectCompositionContext?.WaitForGpu();
				consecutiveDrawErrors = 0;
				Surface.Canvas.Save();
				Surface.Canvas.Scale(Window.Shape.WindowDPIScale, Window.Shape.WindowDPIScale);
				try
				{
					DrawAction?.Invoke(Surface.Canvas);
					OnFrameDone?.Invoke(Surface);
				}
				finally
				{
					Surface.Canvas.Restore();
				}
				Surface.Canvas.Flush();
				Surface.Flush();
				grContext?.Submit(synchronous: true);
				DirectCompositionContext?.Present(SwapFromPersistent);
			}
		}
		catch (SharpGenException ex) when (ex.ResultCode == Vortice.DXGI.ResultCode.DeviceRemoved || ex.ResultCode == Vortice.DXGI.ResultCode.DeviceReset || ex.ResultCode == Vortice.DXGI.ResultCode.DeviceHung)
		{
			FLogger.Warn("Device lost during draw: " + ex.Message);
			RecoverFromDeviceRemoval();
		}
		catch (Exception ex2)
		{
			consecutiveDrawErrors++;
			FLogger.Error($"Error during draw (attempt {consecutiveDrawErrors}): {ex2.Message}");
			if (consecutiveDrawErrors >= 3)
			{
				FLogger.Warn($"Too many consecutive draw errors ({consecutiveDrawErrors}), attempting device recovery");
				RecoverFromDeviceRemoval();
			}
			else
			{
				resourcesNeedRecreation = true;
			}
		}
	}

	internal void OnResize(Vector2 size)
	{
		if (!_wasResizing)
		{
			OnResizeStart(size);
		}
		_wasResizing = true;
		if (Method == ResizingMethod.PerFrame)
		{
			PerformBufferResize(size);
		}
		else if (Method == ResizingMethod.Stretch)
		{
			ResizeWithStretch(size);
		}
		else if (Method == ResizingMethod.SmartSmoothing && (size.x >= currentBufferSizeIncludingPad.x - 25f || size.y >= currentBufferSizeIncludingPad.y - 25f))
		{
			currentBufferSizeIncludingPad = size + new Vector2(resizeOverflowPadding, resizeOverflowPadding);
			PerformBufferResize(currentBufferSizeIncludingPad);
			FLogger.Log<SkiaDirectCompositionContext>("Resize including padding");
		}
	}

	internal void OnResizeStart(Vector2 size)
	{
		if (Method == ResizingMethod.SmartSmoothing)
		{
			currentBufferSizeIncludingPad = size;
		}
	}

	internal void OnResizeEnd(Vector2 size)
	{
		_wasResizing = false;
		FLogger.Log<SkiaDirectCompositionContext>("Resize using correct dimensions");
		PerformBufferResize(size);
	}

	private void ResizeWithStretch(Vector2 size)
	{
		if (DirectCompositionContext != null && !(DirectCompositionContext.DCompDevice == null) && Window.Procedure._isSizeMoving)
		{
			int num = (int)size.x;
			int num2 = (int)size.y;
			IDCompositionVisual rootVisual = DirectCompositionContext.RootVisual;
			float scaleX = (float)num / (float)DirectCompositionContext.SizeX;
			float scaleY = (float)num2 / (float)DirectCompositionContext.SizeY;
			IDCompositionScaleTransform iDCompositionScaleTransform = DirectCompositionContext?.DCompDevice?.CreateScaleTransform();
			iDCompositionScaleTransform?.SetScaleX(scaleX);
			iDCompositionScaleTransform?.SetScaleY(scaleY);
			rootVisual?.SetTransform(iDCompositionScaleTransform);
			DirectCompositionContext?.RootVisual?.SetContent(rootVisual);
			DirectCompositionContext?.DCompDevice?.Commit();
		}
	}

	private void UpdateResizeDPIScale()
	{
	}

	internal void PerformBufferResize(Vector2 size)
	{
		lock (resourceLock)
		{
			if ((float)width == size.x && (float)height == size.y)
			{
				return;
			}
			FLogger.Log<SkiaDirectCompositionContext>($"Resizing SDXCC: {size}");
			width = (int)size.x;
			height = (int)size.y;
			DirectCompositionContext?.WaitForGpu();
			FLogger.Log<SkiaDirectCompositionContext>($"Resizing SDXCC: {width}, {height}");
			DirectCompositionContext?.Resize(width, height, DisposeRenderTargets);
			resourcesNeedRecreation = true;
			EnsureResourcesCreated();
			OnDisposeAdditionals?.Invoke();
			OnRebuildAdditionals?.Invoke();
			DirectCompositionContext?.RootVisual?.SetContent(DirectCompositionContext.SwapChain);
			DirectCompositionContext?.DCompDevice?.Commit();
			FLogger.Log<SkiaDirectCompositionContext>("Resize completed");
		}
		UpdateResizeDPIScale();
	}

	public void WaitForGPU()
	{
		DirectCompositionContext?.WaitForGpu();
	}

	public SKImage? CaptureWindowRegion(SKRect region, float quality)
	{
		lock (resourceLock)
		{
			if (Surface == null || !IsDeviceValid())
			{
				return null;
			}
			try
			{
				DirectCompositionContext?.WaitForGpu();
				grContext?.Submit(synchronous: true);
				Surface.Canvas.Flush();
				Surface.Flush();
				Compositor.Dump(Surface.Snapshot(), "rcontext_buffer_surf_whole");
				using SKImage sourceImage = Surface.Snapshot(new SKRectI((int)region.Left, (int)region.Top, (int)region.Right, (int)region.Bottom));
				SKImage? sKImage = RMath.CreateLowResImage(sourceImage, RMath.Clamp(quality, 0.01f, 1f), SamplingOptions);
				Compositor.Dump(sKImage, "rcontext_buffer_cropped_scaled");
				return sKImage;
			}
			catch (Exception ex)
			{
				FLogger.Error("Error capturing window region: " + ex.Message);
				return null;
			}
		}
	}

	public FAdditionalSurface CreateAdditional(SKImageInfo info)
	{
		if (_isDisposed)
		{
			FLogger.Error("CreateAdditional was called but devices were already disposed.");
			return new FAdditionalSurface(SKSurface.Create(info), null, null, null, info, null);
		}
		if (grContext == null || !IsDeviceValid())
		{
			throw new InvalidOperationException("GRContext is not initialized or device is invalid.");
		}
		if (DirectCompositionContext?.Device == null)
		{
			throw new InvalidOperationException("D3D12 Device is null");
		}
		ResourceDescription description = new ResourceDescription
		{
			Dimension = ResourceDimension.Texture2D,
			Alignment = 0uL,
			Width = (ulong)info.Width,
			Height = (uint)info.Height,
			DepthOrArraySize = 1,
			MipLevels = 1,
			Format = Format.B8G8R8A8_UNorm,
			SampleDescription = new SampleDescription(1u, 0u),
			Layout = TextureLayout.Unknown,
			Flags = ResourceFlags.AllowRenderTarget
		};
		ClearValue value = new ClearValue
		{
			Format = Format.B8G8R8A8_UNorm,
			Color = new Color4(0f, 0f, 0f, 0f)
		};
		ID3D12Resource iD3D12Resource = DirectCompositionContext?.Device.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, description, ResourceStates.RenderTarget, value);
		GRD3DTextureResourceInfo gRD3DTextureResourceInfo = new GRD3DTextureResourceInfo();
		if ((object)iD3D12Resource == null)
		{
			throw new NullReferenceException("Texture is null");
		}
		gRD3DTextureResourceInfo.Resource = iD3D12Resource.NativePointer;
		gRD3DTextureResourceInfo.ResourceState = 4u;
		gRD3DTextureResourceInfo.Format = 87u;
		gRD3DTextureResourceInfo.SampleCount = 1u;
		gRD3DTextureResourceInfo.LevelCount = 1u;
		gRD3DTextureResourceInfo.SampleQualityPattern = 0u;
		gRD3DTextureResourceInfo.Protected = false;
		GRD3DTextureResourceInfo gRD3DTextureResourceInfo2 = gRD3DTextureResourceInfo;
		GRBackendTexture gRBackendTexture = new GRBackendTexture(info.Width, info.Height, gRD3DTextureResourceInfo2);
		return new FAdditionalSurface(SKSurface.Create(grContext, gRBackendTexture, GRSurfaceOrigin.TopLeft, info.ColorType), iD3D12Resource, gRD3DTextureResourceInfo2, gRBackendTexture, info, this);
	}

	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}
		FLogger.Log<SkiaDirectCompositionContext>("Disposing SkiaDirectCompositionContext...");
		_isDisposed = true;
		lock (resourceLock)
		{
			DirectCompositionContext?.WaitForGpu();
			DisposeSkiaResources();
			grContext?.Dispose();
			grContext = null;
			DrawAction = null;
			DirectCompositionContext?.Dispose();
			DirectCompositionContext = null;
			adapter?.Dispose();
		}
	}

	private void DisposeRenderTargets()
	{
		lock (resourceLock)
		{
			Surface?.Canvas?.Flush();
			Surface?.Flush();
			grContext?.Flush();
			grContext?.Submit(synchronous: true);
			DisposeSkiaResources();
			resourcesNeedRecreation = true;
			DirectCompositionContext?.WaitForGpu();
		}
	}

	private void DisposeSkiaResources()
	{
		Surface?.Dispose();
		Surface = null;
		backendTexture?.Dispose();
		backendTexture = null;
		resourceInfo?.Dispose();
		resourceInfo = null;
		backBuffer?.Dispose();
		backBuffer = null;
		persistentRenderTarget?.Dispose();
		persistentRenderTarget = null;
		backendContext?.Dispose();
	}
}
