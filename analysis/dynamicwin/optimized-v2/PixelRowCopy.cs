using System;

internal static class PixelRowCopy
{
    internal static unsafe void Copy(IntPtr source, int sourceStride, IntPtr destination, int destinationStride, int width, int height)
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        int rowBytes = checked(width * 4);
        if (source == IntPtr.Zero || destination == IntPtr.Zero || sourceStride < rowBytes || destinationStride < rowBytes)
            throw new ArgumentException("Invalid pixel buffer or stride.");
        _ = checked(sourceStride * height);
        _ = checked(destinationStride * height);
        for (int y = 0; y < height; y++)
            Buffer.MemoryCopy((byte*)source + y * sourceStride, (byte*)destination + y * destinationStride, destinationStride, rowBytes);
    }
}
