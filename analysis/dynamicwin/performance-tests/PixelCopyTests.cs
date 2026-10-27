internal static class PixelCopyTests
{
    internal static unsafe void Run()
    {
        foreach (int width in new[] { 1, 3, 1366, 1920, 2560 })
        foreach (int padding in new[] { 0, 16, 256 })
        {
            const int height = 7, guard = 32;
            int row = width * 4, stride = row + padding;
            byte[] source = new byte[stride * height];
            new Random(42).NextBytes(source);
            byte[] destination = Enumerable.Repeat((byte)0xCD, row * height + 2 * guard).ToArray();
            fixed (byte* src = source)
            fixed (byte* dst = destination)
                PixelRowCopy.Copy((IntPtr)src, stride, (IntPtr)(dst + guard), row, width, height);
            for (int y = 0; y < height; y++)
                if (!source.AsSpan(y * stride, row).SequenceEqual(destination.AsSpan(guard + y * row, row)))
                    throw new Exception("Pixel row mismatch");
            if (destination.Take(guard).Any(b => b != 0xCD) || destination.Skip(guard + row * height).Any(b => b != 0xCD))
                throw new Exception("Pixel buffer overrun");
        }
        Console.WriteLine("PASS pixel copies: 15 padded-stride cases, exact rows and intact boundary guards.");
    }
}
