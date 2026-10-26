using System;
using NAudio.Dsp;

namespace DynamicWin;

// Buffers belong to one capture callback thread. Consumers receive a snapshot.
internal sealed class SpectrumProcessor
{
    private const int Length = 1024;
    private readonly Complex[] fft = new Complex[Length];
    private readonly float[] window = new float[Length];
    private readonly float[] smoothed;
    private readonly (int Start, int End)[] ranges;
    private int windowLength = -1;

    public SpectrumProcessor(int barCount, int sampleRate)
    {
        smoothed = new float[barCount];
        ranges = new (int, int)[barCount];
        ReadOnlySpan<float> edges = [60f, 170f, 310f, 600f, 1000f, 2000f];
        float resolution = (float)sampleRate / Length;
        for (int i = 0; i < barCount; i++)
            ranges[i] = (Math.Max(1, (int)(edges[i] / resolution)),
                Math.Min(Length / 2 - 1, i + 1 < barCount ? (int)(edges[i + 1] / resolution) : Length / 2 - 1));
    }

    public float[] Process(byte[] buffer, int byteCount, int channels)
    {
        int samples = Math.Min(byteCount / (sizeof(float) * channels), Length);
        if (windowLength != samples)
        {
            windowLength = samples;
            for (int i = 0; i < samples; i++)
                window[i] = samples > 1 ? (float)(0.5 * (1 - Math.Cos(2 * Math.PI * i / (samples - 1)))) : 1;
        }
        Array.Clear(fft);
        for (int i = 0; i < samples; i++)
        {
            float mono = 0;
            for (int channel = 0; channel < channels; channel++)
                mono += BitConverter.ToSingle(buffer, (i * channels + channel) * sizeof(float));
            fft[i].X = mono / channels * window[i];
        }
        FastFourierTransform.FFT(true, 10, fft);
        for (int i = 0; i < ranges.Length; i++)
        {
            float sum = 0;
            int count = 0;
            for (int bin = ranges[i].Start; bin <= ranges[i].End; bin++)
            {
                sum += (float)Math.Sqrt(fft[bin].X * fft[bin].X + fft[bin].Y * fft[bin].Y);
                count++;
            }
            float value = count > 0 ? Math.Min(sum / count * 100, 1) : 0;
            smoothed[i] = smoothed[i] * 0.7f + value * 0.3f;
        }
        // Preserve the public event's ownership semantics for extensions.
        return (float[])smoothed.Clone();
    }
}
