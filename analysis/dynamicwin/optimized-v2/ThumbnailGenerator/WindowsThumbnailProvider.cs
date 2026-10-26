using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ThumbnailGenerator;

public class WindowsThumbnailProvider
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
	internal interface IShellItem
	{
		void BindToHandler(nint pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out nint ppv);

		void GetParent(out IShellItem ppsi);

		void GetDisplayName(SIGDN sigdnName, out nint ppszName);

		void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

		void Compare(IShellItem psi, uint hint, out int piOrder);
	}

	internal enum SIGDN : uint
	{
		NORMALDISPLAY = 0u,
		PARENTRELATIVEPARSING = 2147581953u,
		PARENTRELATIVEFORADDRESSBAR = 2147598337u,
		DESKTOPABSOLUTEPARSING = 2147647488u,
		PARENTRELATIVEEDITING = 2147684353u,
		DESKTOPABSOLUTEEDITING = 2147794944u,
		FILESYSPATH = 2147844096u,
		URL = 2147909632u
	}

	internal enum HResult
	{
		Ok = 0,
		False = 1,
		InvalidArguments = -2147024809,
		OutOfMemory = -2147024882,
		NoInterface = -2147467262,
		Fail = -2147467259,
		ElementNotFound = -2147023728,
		TypeElementNotFound = -2147319765,
		NoObject = -2147221019,
		Win32ErrorCanceled = 1223,
		Canceled = -2147023673,
		ResourceInUse = -2147024726,
		AccessDenied = -2147287035
	}

	[ComImport]
	[Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellItemImageFactory
	{
		[PreserveSig]
		HResult GetImage([In][MarshalAs(UnmanagedType.Struct)] NativeSize size, [In] ThumbnailOptions flags, out nint phbm);
	}

	internal struct NativeSize
	{
		private int width;

		private int height;

		public int Width
		{
			set
			{
				width = value;
			}
		}

		public int Height
		{
			set
			{
				height = value;
			}
		}
	}

	public struct RGBQUAD
	{
		public byte rgbBlue;

		public byte rgbGreen;

		public byte rgbRed;

		public byte rgbReserved;
	}

	private const string IShellItem2Guid = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string path, nint pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

	[DllImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool DeleteObject(nint hObject);

	public static Bitmap GetThumbnail(string fileName, int width, int height, ThumbnailOptions options)
	{
		nint hBitmap = GetHBitmap(Path.GetFullPath(fileName), width, height, options);
		try
		{
			return GetBitmapFromHBitmap(hBitmap);
		}
		finally
		{
			DeleteObject(hBitmap);
		}
	}

	public static Bitmap GetBitmapFromHBitmap(nint nativeHBitmap)
	{
		Bitmap bitmap = Image.FromHbitmap(nativeHBitmap);
		if (Image.GetPixelFormatSize(bitmap.PixelFormat) < 32)
		{
			return bitmap;
		}
		return CreateAlphaBitmap(bitmap, PixelFormat.Format32bppArgb);
	}

	public static Bitmap CreateAlphaBitmap(Bitmap srcBitmap, PixelFormat targetPixelFormat)
	{
		Bitmap bitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height, targetPixelFormat);
		Rectangle rect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
		BitmapData bitmapData = srcBitmap.LockBits(rect, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);
		bool flag = false;
		try
		{
			for (int i = 0; i <= bitmapData.Height - 1; i++)
			{
				for (int j = 0; j <= bitmapData.Width - 1; j++)
				{
					Color color = Color.FromArgb(Marshal.ReadInt32(bitmapData.Scan0, bitmapData.Stride * i + 4 * j));
					if ((color.A > 0) & (color.A < byte.MaxValue))
					{
						flag = true;
					}
					bitmap.SetPixel(j, i, color);
				}
			}
		}
		finally
		{
			srcBitmap.UnlockBits(bitmapData);
		}
		if (flag)
		{
			return bitmap;
		}
		return srcBitmap;
	}

	private static nint GetHBitmap(string fileName, int width, int height, ThumbnailOptions options)
	{
		Guid riid = new Guid("7E9FB0D3-919F-4307-AB2E-9B1860310C93");
		IShellItem shellItem;
		int num = SHCreateItemFromParsingName(fileName, IntPtr.Zero, ref riid, out shellItem);
		if (num != 0)
		{
			throw Marshal.GetExceptionForHR(num);
		}
		NativeSize size = new NativeSize
		{
			Width = width,
			Height = height
		};
		nint phbm;
		HResult image = ((IShellItemImageFactory)shellItem).GetImage(size, options, out phbm);
		Marshal.ReleaseComObject(shellItem);
		if (image == HResult.Ok)
		{
			return phbm;
		}
		throw Marshal.GetExceptionForHR((int)image);
	}
}
