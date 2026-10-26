using System;
using System.Threading;
using FenUISharp.Logging;
using SharpGen.Runtime;
using Vortice.DXGI;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DirectComposition;

namespace FenUISharp;

public class DirectCompositionContext : IDisposable
{
	public readonly ID3D12Device Device;

	public readonly FeatureLevel FeatureLevel;

	private ID3D11Device? d3d11Device;

	private ID3D11DeviceContext? d3d11Context;

	private ID3D12Fence? Fence;

	private AutoResetEvent? FenceEvent;

	private ulong _fenceValue;

	public ColorSpaceType ColorSpace;

	private IDXGIAdapter1? _cachedAdapter;

	private bool _disposed;

	public int SizeX { get; protected set; }

	public int SizeY { get; protected set; }

	public nint hWnd { get; }

	public Format ColorFormat { get; }

	public Format DepthStencilFormat { get; }

	public IDXGIFactory2 Factory { get; private set; }

	public IDCompositionDevice? DCompDevice { get; private set; }

	public IDCompositionDevice3? DCompDevice3 { get; private set; }

	public IDCompositionTarget? DCompTarget { get; private set; }

	public IDCompositionVisual? RootVisual { get; private set; }

	public IDXGISwapChain3? SwapChain { get; private set; }

	public ID3D12CommandQueue CommandQueue { get; private set; }

	public ID3D12GraphicsCommandList? CommandList { get; private set; }

	public ID3D12CommandAllocator? CommandAllocator { get; private set; }

	private ID3D12Resource? LastBackBuffer { get; set; }

	public uint BufferCount { get; } = 2u;

	public DirectCompositionContext(nint windowHandle, int sx, int sy, Format colorFormat = Format.B8G8R8A8_UNorm, Format depthStencilFormat = Format.D32_Float)
	{
		try
		{
			FLogger.Log<DirectCompositionContext>("Starting DirectCompositionContext initialization...");
			SizeX = sx;
			SizeY = sy;
			hWnd = windowHandle;
			ColorFormat = colorFormat;
			DepthStencilFormat = depthStencilFormat;
			if (FenUI.debugEnabled)
			{
				D3D12.D3D12GetDebugInterface<ID3D12Debug3>(out ID3D12Debug3 debugInterface);
				if (debugInterface != null)
				{
					debugInterface.EnableDebugLayer();
					debugInterface.SetEnableGPUBasedValidation(false);
					debugInterface.Dispose();
				}
			}
			FLogger.Log<DirectCompositionContext>("Creating DXGI Factory...");
			Factory = DXGI.CreateDXGIFactory1<IDXGIFactory2>();
			FLogger.Log<DirectCompositionContext>("Getting hardware adapter...");
			_cachedAdapter = GetHardwareAdapter();
			FLogger.Log<DirectCompositionContext>("Hardware adapter: " + _cachedAdapter.Description1.Description);
			FLogger.Log<DirectCompositionContext>("Creating D3D12 Device...");
			FeatureLevel = FeatureLevel.Level_11_0;
			ID3D12Device iD3D12Device = null;
			int num = 0;
			while (iD3D12Device == null && num < 3)
			{
				try
				{
					iD3D12Device = D3D12.D3D12CreateDevice<ID3D12Device>(_cachedAdapter, FeatureLevel);
				}
				catch (SharpGenException ex) when (ex.ResultCode == Vortice.DXGI.ResultCode.InvalidCall && num < 2)
				{
					FLogger.Log<DirectCompositionContext>($"Device creation attempt {num + 1} failed, retrying...");
					Thread.Sleep(100);
					num++;
				}
			}
			if (iD3D12Device == null)
			{
				throw new InvalidOperationException("Failed to create D3D12 device after multiple attempts");
			}
			Device = iD3D12Device;
			FLogger.Log<DirectCompositionContext>($"D3D12 Device created: {Device.NativePointer}");
			CommandQueueDescription description = new CommandQueueDescription(CommandListType.Direct);
			CommandQueue = Device.CreateCommandQueue(in description);
			CreateDirectCompositionDevice();
			CreateSwapchain();
			CreateCommandObjects();
			InitFence();
			FLogger.Log<DirectCompositionContext>("DirectCompositionContext initialization completed!");
		}
		catch (Exception value)
		{
			FLogger.Log<DirectCompositionContext>($"Error in DirectCompositionContext constructor: {value}");
			try
			{
				Dispose();
			}
			catch
			{
			}
			throw;
		}
	}

	private void CreateDirectCompositionDevice()
	{
		FLogger.Log<DirectCompositionContext>("Creating D3D11 device for DirectComposition interop...");
		if (_cachedAdapter == null)
		{
			throw new InvalidOperationException("Cached adapter is null");
		}
		DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.BgraSupport;
		if (FenUI.debugEnabled)
		{
			deviceCreationFlags |= DeviceCreationFlags.Debug;
		}
		FeatureLevel[] featureLevels = new FeatureLevel[3]
		{
			FeatureLevel.Level_11_0,
			FeatureLevel.Level_10_1,
			FeatureLevel.Level_10_0
		};
		int num = 0;
		while (d3d11Device == null && num < 3)
		{
			try
			{
				if (D3D11.D3D11CreateDevice(_cachedAdapter, DriverType.Unknown, deviceCreationFlags, featureLevels, out d3d11Device, out FeatureLevel featureLevel, out d3d11Context).Success && d3d11Device != null)
				{
					FLogger.Log<DirectCompositionContext>($"D3D11 device created with feature level: {featureLevel}");
					break;
				}
			}
			catch (Exception ex) when (num < 2)
			{
				FLogger.Log<DirectCompositionContext>($"D3D11 device creation attempt {num + 1} failed: {ex.Message}");
				Thread.Sleep(50);
			}
			num++;
		}
		if (d3d11Device == null)
		{
			throw new InvalidOperationException("Failed to create D3D11 device after multiple attempts");
		}
		using IDXGIDevice iDXGIDevice = d3d11Device.QueryInterface<IDXGIDevice>();
		if (iDXGIDevice == null)
		{
			throw new InvalidOperationException("Failed to get IDXGIDevice from D3D11 device");
		}
		FLogger.Log<DirectCompositionContext>("Creating DirectComposition device from D3D11 DXGI device...");
		Result value = DComp.DCompositionCreateDevice3<IDCompositionDevice>(iDXGIDevice, out IDCompositionDevice compositionDevice);
		if (value.Failure || compositionDevice == null)
		{
			throw new InvalidOperationException($"Failed to create DirectComposition device. HRESULT: {value}");
		}
		DCompDevice = compositionDevice;
		DCompDevice3 = DCompDevice.QueryInterface<IDCompositionDevice3>();
		FLogger.Log<DirectCompositionContext>("Creating composition target for window...");
		value = DCompDevice.CreateTargetForHwnd(hWnd, true, out var target);
		if (value.Failure || target == null)
		{
			throw new InvalidOperationException($"Failed to create DirectComposition target. HRESULT: {value}");
		}
		DCompTarget = target;
		FLogger.Log<DirectCompositionContext>("Creating root visual...");
		value = DCompDevice.CreateVisual(out var visual);
		if (value.Failure || visual == null)
		{
			throw new InvalidOperationException($"Failed to create DirectComposition visual. HRESULT: {value}");
		}
		RootVisual = visual;
		FLogger.Log<DirectCompositionContext>("Setting root visual on target...");
		DCompTarget.SetRoot(RootVisual);
		FLogger.Log<DirectCompositionContext>("Done creating DXCC");
	}

	private void CreateSwapchain()
	{
		try
		{
			SwapChain?.Dispose();
			uint sizeX = (uint)SizeX;
			uint sizeY = (uint)SizeY;
			FLogger.Log<DirectCompositionContext>($"Creating swap chain with size {sizeX}x{sizeY}");
			SwapChainDescription1 desc = new SwapChainDescription1
			{
				Width = sizeX,
				Height = sizeY,
				Format = ColorFormat,
				BufferCount = BufferCount,
				BufferUsage = Usage.RenderTargetOutput,
				SampleDescription = new SampleDescription(1u, 0u),
				Scaling = Scaling.Stretch,
				SwapEffect = SwapEffect.FlipDiscard,
				AlphaMode = AlphaMode.Premultiplied
			};
			FLogger.Log<DirectCompositionContext>("Creating swap chain for composition...");
			IDXGISwapChain1 iDXGISwapChain = Factory.CreateSwapChainForComposition(CommandQueue, desc);
			SwapChain = iDXGISwapChain.QueryInterface<IDXGISwapChain3>();
			FLogger.Log<DirectCompositionContext>("Setting swap chain content on root visual...");
			RootVisual?.SetContent(SwapChain);
			DCompDevice?.Commit();
			FLogger.Log<DirectCompositionContext>("Upading color space...");
			UpdateColorSpace();
			FLogger.Log<DirectCompositionContext>("Done creating SwapChain");
			FLogger.Log<DirectCompositionContext>("");
		}
		catch (Exception value)
		{
			FLogger.Log<DirectCompositionContext>($"Exception in CreateSwapchain: {value}");
			throw;
		}
	}

	private void CreateCommandObjects()
	{
		try
		{
			FLogger.Log<DirectCompositionContext>("Creating command allocator...");
			CommandAllocator = Device.CreateCommandAllocator(CommandListType.Direct);
			FLogger.Log<DirectCompositionContext>("Creating command list...");
			CommandList = Device.CreateCommandList<ID3D12GraphicsCommandList>(0u, CommandListType.Direct, CommandAllocator);
			CommandList.Close();
			FLogger.Log<DirectCompositionContext>("Command objects created successfully");
			FLogger.Log<DirectCompositionContext>("");
		}
		catch (Exception value)
		{
			FLogger.Log<DirectCompositionContext>($"Exception in CreateCommandObjects: {value}");
			throw;
		}
	}

	private void UpdateColorSpace()
	{
		try
		{
			if (!Factory.IsCurrent)
			{
				Factory.Dispose();
				Factory = DXGI.CreateDXGIFactory1<IDXGIFactory2>();
			}
			ColorSpace = ColorSpaceType.RgbFullG22NoneP709;
			using IDXGISwapChain3 iDXGISwapChain = SwapChain?.QueryInterfaceOrNull<IDXGISwapChain3>();
			if (iDXGISwapChain != null && (iDXGISwapChain.CheckColorSpaceSupport(ColorSpace) & SwapChainColorSpaceSupportFlags.Present) != SwapChainColorSpaceSupportFlags.None)
			{
				iDXGISwapChain.SetColorSpace1(ColorSpace);
			}
		}
		catch (Exception ex)
		{
			FLogger.Log<DirectCompositionContext>("Warning: UpdateColorSpace failed: " + ex.Message);
		}
	}

	public IDXGIAdapter1 GetHardwareAdapter()
	{
		try
		{
			FLogger.Log<DirectCompositionContext>("Enumerating adapters...");
			IDXGIFactory6 iDXGIFactory = Factory.QueryInterfaceOrNull<IDXGIFactory6>();
			if ((object)iDXGIFactory != null)
			{
				FLogger.Log<DirectCompositionContext>("Using IDXGIFactory6 for adapter enumeration");
				IDXGIAdapter1 adapter;
				for (uint num = 0u; iDXGIFactory.EnumAdapterByGpuPreference<IDXGIAdapter1>(num, GpuPreference.HighPerformance, out adapter).Success; num++)
				{
					if (adapter != null && (adapter.Description1.Flags & AdapterFlags.Software) == 0)
					{
						FLogger.Log<DirectCompositionContext>("Found hardware adapter: " + adapter.Description1.Description);
						FLogger.Log<DirectCompositionContext>("");
						return adapter;
					}
					adapter?.Dispose();
				}
				iDXGIFactory.Dispose();
			}
			FLogger.Log<DirectCompositionContext>("Using IDXGIFactory2 for adapter enumeration");
			IDXGIAdapter1 adapterOut;
			for (uint num2 = 0u; Factory.EnumAdapters1(num2, out adapterOut).Success; num2++)
			{
				if (adapterOut != null && (adapterOut.Description1.Flags & AdapterFlags.Software) == 0)
				{
					FLogger.Log<DirectCompositionContext>("Found hardware adapter: " + adapterOut.Description1.Description);
					FLogger.Log<DirectCompositionContext>("");
					return adapterOut;
				}
				adapterOut?.Dispose();
			}
			throw new InvalidOperationException("No suitable D3D12 adapter found.");
		}
		catch (Exception value)
		{
			FLogger.Log<DirectCompositionContext>($"Exception in GetHardwareAdapter: {value}");
			throw;
		}
	}

	public void Resize(int newWidth, int newHeight, Action disposeTargets)
	{
		if (newWidth != SizeX || newHeight != SizeY)
		{
			SizeX = newWidth;
			SizeY = newHeight;
			d3d11Context?.ClearState();
			d3d11Context?.Flush();
			FLogger.Log<DirectCompositionContext>("Disposing previous render targets...");
			LastBackBuffer?.Dispose();
			disposeTargets?.Invoke();
			WaitForGpu();
			FLogger.Log<DirectCompositionContext>("Resize SwapChain...");
			Result? result = SwapChain?.ResizeBuffers(BufferCount, (uint)SizeX, (uint)SizeY, ColorFormat, SwapChainFlags.None);
			FLogger.Log($"ResizeBuffers HRESULT: 0x{result?.Code:X8} ({result?.Description})");
			SwapChainDescription1? swapChainDescription = SwapChain?.Description1;
			FLogger.Log($"SwapChain desc after ResizeBuffers: {swapChainDescription?.Width}x{swapChainDescription?.Height}");
		}
	}

	private void InitFence()
	{
		Fence = Device.CreateFence(0uL);
		_fenceValue = 1uL;
		FenceEvent = new AutoResetEvent(initialState: false);
	}

	internal void WaitForGpu()
	{
		DateTime now = DateTime.Now;
		CommandQueue?.Signal(Fence, ++_fenceValue);
		if (Fence?.CompletedValue < _fenceValue)
		{
			Fence?.SetEventOnCompletion(_fenceValue, FenceEvent);
			if (!(FenceEvent ?? throw new InvalidOperationException("FenceEvent is null")).WaitOne(TimeSpan.FromSeconds(5L)))
			{
				FLogger.Log<DirectCompositionContext>("GPU TIMEOUT! Fence never completed");
				return;
			}
		}
		_ = DateTime.Now - now;
	}

	public void Present(Action<(ID3D12GraphicsCommandList commandList, ID3D12Resource backBuffer)>? drawAction = null, PresentFlags flags = PresentFlags.None)
	{
		try
		{
			CommandAllocator?.Reset();
			CommandList?.Reset(CommandAllocator, null);
			LastBackBuffer?.Dispose();
			ID3D12Resource iD3D12Resource = (LastBackBuffer = SwapChain?.GetBuffer<ID3D12Resource>(SwapChain.CurrentBackBufferIndex) ?? throw new NullReferenceException("SwapChain null"));
			CommandList?.ResourceBarrierTransition(iD3D12Resource, ResourceStates.Common, ResourceStates.RenderTarget);
			drawAction?.Invoke((CommandList ?? throw new NullReferenceException("CommandList null"), iD3D12Resource));
			CommandList?.ResourceBarrierTransition(iD3D12Resource, ResourceStates.RenderTarget, ResourceStates.Common);
			CommandList?.Close();
			CommandQueue?.ExecuteCommandLists(new ID3D12CommandList[1] { CommandList });
			SwapChain.Present(1u, flags);
			DCompDevice?.Commit();
		}
		catch (Exception value)
		{
			FLogger.Log<DirectCompositionContext>($"Exception in Present: {value}");
			throw;
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			_disposed = true;
			try
			{
				WaitForGpu();
			}
			catch (Exception ex)
			{
				FLogger.Log<DirectCompositionContext>("Warning: WaitForGpu failed during dispose: " + ex.Message);
			}
			FLogger.Log<DirectCompositionContext>("Disposing DirectCompositionContext...");
			try
			{
				RootVisual?.SetContent(null);
				DCompDevice?.Commit();
			}
			catch
			{
			}
			RootVisual?.Dispose();
			RootVisual = null;
			DCompTarget?.Dispose();
			DCompTarget = null;
			DCompDevice3?.Dispose();
			DCompDevice3 = null;
			DCompDevice?.Dispose();
			DCompDevice = null;
			d3d11Context?.ClearState();
			d3d11Context?.Flush();
			d3d11Context?.Dispose();
			d3d11Context = null;
			d3d11Device?.Dispose();
			d3d11Device = null;
			LastBackBuffer?.Dispose();
			LastBackBuffer = null;
			CommandList?.Dispose();
			CommandList = null;
			CommandAllocator?.Dispose();
			CommandAllocator = null;
			SwapChain?.Dispose();
			SwapChain = null;
			CommandQueue?.Dispose();
			CommandQueue = null;
			try
			{
				FenceEvent?.Set();
			}
			catch
			{
			}
			FenceEvent?.Dispose();
			FenceEvent = null;
			Fence?.Dispose();
			Fence = null;
			Device?.Dispose();
			_cachedAdapter?.Dispose();
			_cachedAdapter = null;
			Factory?.Dispose();
			Factory = null;
			FLogger.Log<DirectCompositionContext>("DirectCompositionContext disposed successfully");
		}
		catch (Exception value)
		{
			FLogger.Log<DirectCompositionContext>($"Exception during disposal: {value}");
		}
	}
}
