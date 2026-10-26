namespace FenUISharp;

public class FDropData
{
	public DropType dropType;

	private string? textData;

	private string[]? fileData;

	private nint? bitmapPointer;

	public FDropData(DropType dropType, string[] stringData)
	{
		this.dropType = dropType;
		fileData = stringData;
	}

	public FDropData(DropType dropType, string textData)
	{
		this.dropType = dropType;
		this.textData = textData;
	}

	public FDropData(DropType dropType, nint bitmapPointer)
	{
		this.dropType = dropType;
		this.bitmapPointer = bitmapPointer;
	}

	public object? GetData()
	{
		switch (dropType)
		{
		case DropType.FileDrop:
			return fileData;
		case DropType.AnsiText:
		case DropType.UnicodeText:
			return textData;
		case DropType.Bitmap:
		case DropType.DeviceIndependentBitmap:
			return bitmapPointer;
		default:
			return null;
		}
	}
}
