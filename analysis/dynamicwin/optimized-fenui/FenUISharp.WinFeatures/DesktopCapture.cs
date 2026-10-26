using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.WinFeatures;

public class DesktopCapture
{
	private struct BITMAPINFOHEADER
	{
		public uint biSize;

		public int biWidth;

		public int biHeight;

		public ushort biPlanes;

		public ushort biBitCount;

		public uint biCompression;

		public uint biSizeImage;

		public int biXPelsPerMeter;

		public int biYPelsPerMeter;

		public uint biClrUsed;

		public uint biClrImportant;
	}

	private struct BITMAPINFO
	{
		public BITMAPINFOHEADER bmiHeader;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public RGBQUAD[] bmiColors;
	}

	private struct RGBQUAD
	{
		public byte rgbBlue;

		public byte rgbGreen;

		public byte rgbRed;

		public byte rgbReserved;
	}

	public static DesktopCapture? instance;

	public SKImage? previousCapture;

	public SKImage? lastCapture;

	public Action<SKImage>? onCapture;

	public int timeSinceLastCapture;

	private const int SM_CXSCREEN = 0;

	private const int SM_CYSCREEN = 1;

	private const uint DIB_RGB_COLORS = 0u;

	private const int SRCCOPY = 13369376;

	private const uint WDA_NONE = 0u;

	private const uint WDA_EXCLUDEFROMCAPTURE = 17u;

	private const int ULW_ALPHA = 2;

	public float CaptureQuality { get; set; } = 0.01f;

	public int CaptureInterval { get; set; } = 2500;

	public DesktopCapture()
	{
		instance = this;
	}

	private static nint CaptureDesktop(out int width, out int height, out nint ppBits)
	{
		nint dC = GetDC(IntPtr.Zero);
		nint num = CreateCompatibleDC(dC);
		width = GetSystemMetrics(0);
		height = GetSystemMetrics(1);
		BITMAPINFO pbmi = new BITMAPINFO
		{
			bmiHeader = 
			{
				biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
				biWidth = width,
				biHeight = -height,
				biPlanes = 1,
				biBitCount = 32,
				biCompression = 0u,
				biSizeImage = (uint)(width * height * 4)
			}
		};
		nint ppvBits;
		nint num2 = CreateDIBSection(dC, ref pbmi, 0u, out ppvBits, IntPtr.Zero, 0u);
		if (num2 == IntPtr.Zero)
		{
			throw new Exception("Failed to create DIB section.");
		}
		nint hgdiobj = SelectObject(num, num2);
		if (!BitBlt(num, 0, 0, width, height, dC, 0, 0, 13369376))
		{
			throw new Exception("BitBlt failed.");
		}
		SelectObject(num, hgdiobj);
		DeleteDC(num);
		ReleaseDC(IntPtr.Zero, dC);
		ppBits = ppvBits;
		return num2;
	}

	private static SKImage GetSKImageFromCapture(nint pBits, int width, int height)
	{
		SKImageInfo info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
		SKBitmap sKBitmap = new SKBitmap();
		if (!sKBitmap.InstallPixels(info, pBits, info.RowBytes, null, IntPtr.Zero))
		{
			throw new Exception("Failed to install pixels into SKBitmap.");
		}
		SKImage result = SKImage.FromBitmap(sKBitmap);
		sKBitmap.Dispose();
		return result;
	}

	public void RequestCaptureDesktop()
	{
		Task.Run(delegate
		{
			while (lastCapture == null)
			{
				int width;
				int height;
				nint ppBits;
				nint num = CaptureDesktop(out width, out height, out ppBits);
				try
				{
					SKImage sKImageFromCapture = GetSKImageFromCapture(ppBits, width, height);
					lastCapture = RMath.CreateLowResImage(sKImageFromCapture, CaptureQuality, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
					sKImageFromCapture.Dispose();
					onCapture?.Invoke(lastCapture);
				}
				finally
				{
					if (num != IntPtr.Zero)
					{
						DeleteObject(num);
					}
				}
				Thread.Sleep(25);
			}
		});
	}

	public SKImage? GetLastCapture()
	{
		return lastCapture;
	}

	[DllImport("gdi32.dll", SetLastError = true)]
	private static extern bool DeleteObject(nint hObject);

	[DllImport("user32.dll")]
	private static extern bool SetWindowDisplayAffinity(nint hwnd, uint dwAffinity);

	[DllImport("user32.dll")]
	private static extern nint GetDC(nint hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int ReleaseDC(nint hWnd, nint hDC);

	[DllImport("gdi32.dll", SetLastError = true)]
	private static extern nint SelectObject(nint hdc, nint hgdiobj);

	[DllImport("gdi32.dll", SetLastError = true)]
	private static extern bool DeleteDC(nint hdc);

	[DllImport("gdi32.dll")]
	private static extern bool BitBlt(nint hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, nint hdcSrc, int nXSrc, int nYSrc, int dwRop);

	[DllImport("gdi32.dll", SetLastError = true)]
	private static extern nint CreateDIBSection(nint hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out nint ppvBits, nint hSection, uint dwOffset);

	[DllImport("user32.dll")]
	private static extern int GetSystemMetrics(int nIndex);

	[DllImport("gdi32.dll", SetLastError = true)]
	public static extern nint CreateCompatibleDC(nint hdc);
}
