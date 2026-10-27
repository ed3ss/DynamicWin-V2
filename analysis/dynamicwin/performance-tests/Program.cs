using System.Diagnostics;
using System.Text.Json;
using DynamicWin;
using FenUISharp;
using NAudio.Dsp;

foreach (string invalid in new[] { "", "123", "token.secret", "1234567890123456x", "18446744073709551616" })
    if (DiscordActivity.ValidApplicationId(invalid)) throw new Exception("Invalid Discord ID accepted");
if (!DiscordActivity.ValidApplicationId("123456789012345678")) throw new Exception("Valid Discord ID rejected");
if (DiscordActivity.Create(false, "Song", "Artist") != null) throw new Exception("Pause must clear Discord activity");
var activity = DiscordActivity.Create(true, "Song", "Artist")!;
if (activity.Type != DiscordRPC.ActivityType.Listening || activity.Details != "Song" || activity.State != "Artist")
    throw new Exception("Incorrect Discord payload");
var payload = Newtonsoft.Json.Linq.JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(activity));
if ((int)payload["type"]! != 2 || (string?)payload["details"] != "Song") throw new Exception("Discord serialization failed");
var unicode = DiscordActivity.Text(string.Concat(Enumerable.Repeat("🎵é", 100)), "fallback");
if (System.Text.Encoding.UTF8.GetByteCount(unicode) > 120 || unicode.Contains('\uFFFD') || char.IsHighSurrogate(unicode[^1]))
    throw new Exception("Discord UTF-8 truncation failed");
if (DiscordActivity.Create(true, "", "")!.State != "Avec DynamicWin") throw new Exception("Discord fallback failed");
Console.WriteLine("PASS Discord: ID validation, listening payload, serialization, pause, Unicode and fallbacks.");
await ArtworkTests.RunAsync(); PixelCopyTests.Run();

// Compare against the original stereo algorithm, including its allocation pattern.
static float[] Legacy(byte[] input, int samples, int rate, float[] last, Complex[]? scratch = null)
{
    var fft = scratch ?? new Complex[1024];
    Array.Clear(fft);
    for (int i = 0; i < samples; i++)
    {
        fft[i].X = (BitConverter.ToSingle(input, i * 8) + BitConverter.ToSingle(input, i * 8 + 4)) * .5f;
        fft[i].X *= (float)(.5 * (1 - Math.Cos(2 * Math.PI * i / (samples - 1))));
    }
    FastFourierTransform.FFT(true, 10, fft);
    var values = new float[6];
    var ranges = new (int, int)[6];
    var edges = new float[] { 60, 170, 310, 600, 1000, 2000 };
    for (int i = 0; i < 6; i++)
    {
        float resolution = rate / 1024f;
        ranges[i] = (Math.Max(1, (int)(edges[i] / resolution)), Math.Min(511, i < 5 ? (int)(edges[i + 1] / resolution) : 511));
        float total = 0; int count = 0;
        for (int k = ranges[i].Item1; k <= ranges[i].Item2; k++)
        {
            total += (float)Math.Sqrt(fft[k].X * fft[k].X + fft[k].Y * fft[k].Y);
            count++;
        }
        values[i] = count > 0 ? Math.Min(total / count * 100, 1) : 0;
        last[i] = last[i] * .7f + values[i] * .3f;
    }
    return (float[])last.Clone();
}

int cases = 0;
foreach (int rate in new[] { 44100, 48000, 96000 })
foreach (int samples in new[] { 64, 480, 960, 1024 })
foreach (double frequency in new[] { 0d, 100d, 440d, 1500d, 8000d })
{
    var data = new byte[samples * 8];
    for (int i = 0; i < samples; i++)
    {
        float sample = (float)(.02 * Math.Sin(2 * Math.PI * frequency * i / rate));
        BitConverter.TryWriteBytes(data.AsSpan(i * 8), sample);
        BitConverter.TryWriteBytes(data.AsSpan(i * 8 + 4), sample);
    }
    var processor = new SpectrumProcessor(6, rate);
    var last = new float[6];
    for (int pass = 0; pass < 3; pass++)
    {
        var expected = Legacy(data, samples, rate, last);
        var actual = processor.Process(data, data.Length, 2);
        for (int i = 0; i < 6; i++)
            if (!float.IsFinite(actual[i]) || Math.Abs(actual[i] - expected[i]) > 1e-5)
                throw new Exception($"Spectrum mismatch: {rate}/{samples}/{frequency}/{i}");
        cases++;
    }
}
// Variable packet length, silence, one sample and snapshot ownership.
var spectrum = new SpectrumProcessor(6, 48000);
var buffer = new byte[8192];
var retained = spectrum.Process(buffer, buffer.Length, 2);
if (ReferenceEquals(retained, spectrum.Process(buffer, buffer.Length, 2))) throw new Exception("Shared snapshot");
foreach (int size in new[] { 0, 8, 64, 8192 })
    if (spectrum.Process(buffer, size, 2).Any(x => !float.IsFinite(x))) throw new Exception("Nonfinite output");
if (retained.Any(x => x != 0)) throw new Exception("Snapshot was mutated");
for (int i = 0; i < 50; i++) spectrum.Process(buffer, buffer.Length, 2);
long before = GC.GetAllocatedBytesForCurrentThread();
for (int i = 0; i < 1000; i++) spectrum.Process(buffer, buffer.Length, 2);
long optimizedBytes = (GC.GetAllocatedBytesForCurrentThread() - before) / 1000;
if (optimizedBytes > 64) throw new Exception($"Unexpected callback allocations: {optimizedBytes}");
var legacyScratch = new Complex[1024];
var legacyLast = new float[6];
for (int i = 0; i < 50; i++) Legacy(buffer, 1024, 48000, legacyLast, legacyScratch);
before = GC.GetAllocatedBytesForCurrentThread();
for (int i = 0; i < 1000; i++) Legacy(buffer, 1024, 48000, legacyLast, legacyScratch);
long legacyBytes = (GC.GetAllocatedBytesForCurrentThread() - before) / 1000;

static object MeasureWait(bool optimized)
{
    using var waiter = new FrameWaiter();
    using var process = Process.GetCurrentProcess();
    var cpu = process.TotalProcessorTime;
    var time = Stopwatch.StartNew();
    for (int frame = 0; frame < 500; frame++)
    {
        var deadline = time.Elapsed.TotalMilliseconds + 2;
        while (time.Elapsed.TotalMilliseconds < deadline)
        {
            if (optimized) waiter.Wait(deadline - time.Elapsed.TotalMilliseconds);
            else Thread.SpinWait(20);
        }
    }
    process.Refresh();
    return new { elapsedMs = time.Elapsed.TotalMilliseconds, cpuMs = (process.TotalProcessorTime - cpu).TotalMilliseconds };
}
Console.WriteLine(JsonSerializer.Serialize(new { spectrumComparisons = cases, legacyAllocatedBytesPerPacket = legacyBytes, allocatedBytesPerPacket = optimizedBytes, oldFinalFrameWait = MeasureWait(false), newFinalFrameWait = MeasureWait(true) }, new JsonSerializerOptions { WriteIndented = true }));
