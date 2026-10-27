using System;
using System.Runtime.InteropServices;
using System.Threading;
using FenUISharp;
using SharpGen.Runtime;
using SkiaSharp;
using Vortice.DXGI;
using Vortice.Direct3D;
using Vortice.Direct3D11;

public class DXDesktopCapture : IDisposable
{
	private IDXGIOutputDuplication _duplication;

	private ID3D11Device _d3dDevice;

	private ID3D11DeviceContext _d3dContext;

	private ID3D11Texture2D _gpuTexture;

	private ID3D11Texture2D _stagingTexture;

	private readonly Thread _loopThread;

	private volatile bool _running;
	private bool _disposed;

	public event Action<SKImage> FrameReady;

	public DXDesktopCapture()
	{
		InitializeDirectX();
		_loopThread = new Thread(CaptureLoop)
		{
			IsBackground = true,
			Name = "DesktopDuplicationThread"
		};
	}

	private void InitializeDirectX()
	{
		try
		{
			D3D11.D3D11CreateDevice((IDXGIAdapter?)null, DriverType.Hardware, DeviceCreationFlags.None, (FeatureLevel[])null, out _d3dDevice, out _d3dContext);
			using IDXGIDevice iDXGIDevice = _d3dDevice.QueryInterface<IDXGIDevice>();
			using IDXGIAdapter iDXGIAdapter = iDXGIDevice.GetAdapter();
			nint currentMonitorHandle = FContext.GetCurrentWindow().Shape.CurrentMonitorHandle;
			IDXGIOutput iDXGIOutput = null;
			IDXGIOutput outputOut;
			for (uint index = 0; !iDXGIAdapter.EnumOutputs(index, out outputOut).Failure; index++)
			{
				if (outputOut.Description.Monitor == currentMonitorHandle)
				{
					iDXGIOutput = outputOut;
					break;
				}
				outputOut.Dispose();
			}
			if (iDXGIOutput == null)
			{
				throw new InvalidOperationException("Desktop capture is unavailable for this monitor.");
			}
			using IDXGIOutput1 iDXGIOutput2 = iDXGIOutput.QueryInterface<IDXGIOutput1>();
			_duplication = iDXGIOutput2.DuplicateOutput(_d3dDevice);
			OutputDescription description = iDXGIOutput.Description;
			Texture2DDescription description2 = new Texture2DDescription
			{
				Width = (uint)(description.DesktopCoordinates.Right - description.DesktopCoordinates.Left),
				Height = (uint)(description.DesktopCoordinates.Bottom - description.DesktopCoordinates.Top),
				MipLevels = 1u,
				ArraySize = 1u,
				Format = Format.B8G8R8A8_UNorm,
				SampleDescription = new SampleDescription(1u, 0u),
				Usage = ResourceUsage.Staging,
				BindFlags = BindFlags.None,
				CPUAccessFlags = CpuAccessFlags.Read,
				MiscFlags = ResourceOptionFlags.None
			};
			_stagingTexture = _d3dDevice.CreateTexture2D(in description2);
			iDXGIOutput.Dispose();
		}
		catch (Exception)
		{
			_duplication?.Dispose();
			_stagingTexture?.Dispose();
			_d3dContext?.Dispose();
			_d3dDevice?.Dispose();
			throw;
		}
	}

	public void Start()
	{
		if (!_disposed && !_running && _loopThread.ThreadState.HasFlag(ThreadState.Unstarted))
		{
			_running = true;
			_loopThread.Start();
		}
	}

	public void Stop()
	{
		_running = false;
		if (_loopThread != null && _loopThread.IsAlive && Thread.CurrentThread != _loopThread) _loopThread.Join();
	}

	private void EnsureStaging(int width, int height)
	{
		if (_stagingTexture != null)
		{
			Texture2DDescription description = _stagingTexture.Description;
			if (description.Width == width && description.Height == height)
			{
				return;
			}
			_stagingTexture.Dispose();
			_stagingTexture = null;
		}
		Texture2DDescription description2 = new Texture2DDescription
		{
			Width = (uint)width,
			Height = (uint)height,
			MipLevels = 1u,
			ArraySize = 1u,
			Format = Format.B8G8R8A8_UNorm,
			SampleDescription = new SampleDescription(1u, 0u),
			Usage = ResourceUsage.Staging,
			BindFlags = BindFlags.None,
			CPUAccessFlags = CpuAccessFlags.Read,
			MiscFlags = ResourceOptionFlags.None
		};
		_stagingTexture = _d3dDevice.CreateTexture2D(in description2);
	}

	private void CaptureLoop()
	{
		while (_running)
		{
			try
			{
				OutduplFrameInfo frameInfo;
				IDXGIResource desktopResource;
				Result result = _duplication.AcquireNextFrame(500u, out frameInfo, out desktopResource);
				if (result.Failure)
				{
					if (!(result == Vortice.DXGI.ResultCode.WaitTimeout))
					{
						Console.WriteLine("AcquireNextFrame failed: " + result);
						Thread.Sleep(100);
					}
					continue;
				}
				try
				{
					using ID3D11Texture2D iD3D11Texture2D = desktopResource.QueryInterfaceOrNull<ID3D11Texture2D>();
					if (iD3D11Texture2D == null)
					{
						continue;
					}
					Texture2DDescription description = iD3D11Texture2D.Description;
					EnsureStaging((int)description.Width, (int)description.Height);
					_d3dContext.CopyResource(_stagingTexture, iD3D11Texture2D);
					if (frameInfo.LastPresentTime == 0L)
					{
						continue;
					}
					MappedSubresource mappedSubresource = _d3dContext.Map(_stagingTexture, 0u);
					try
					{
						nint dataPointer = mappedSubresource.DataPointer;
						int rowPitch = (int)mappedSubresource.RowPitch;
						int width = (int)description.Width;
						int height = (int)description.Height;
						using SKBitmap sKBitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
						nint pixels = sKBitmap.GetPixels();
						PixelRowCopy.Copy(dataPointer, rowPitch, pixels, sKBitmap.RowBytes, width, height);
						var handler = FrameReady;
						if (handler != null) handler(SKImage.FromBitmap(sKBitmap));
					}
					finally
					{
						_d3dContext.Unmap(_stagingTexture, 0u);
					}
				}
				finally
				{
					_duplication.ReleaseFrame();
					desktopResource.Dispose();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Desktop duplication capture error: " + ex);
				Thread.Sleep(100);
			}
		}
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		Stop();
		_duplication?.Dispose();
		_stagingTexture?.Dispose();
		_gpuTexture?.Dispose();
		_d3dContext.Dispose();
		_d3dDevice.Dispose();
	}
}
