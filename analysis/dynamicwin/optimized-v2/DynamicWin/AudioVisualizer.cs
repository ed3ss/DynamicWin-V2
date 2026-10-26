using System;
using System.Runtime.InteropServices;
using FenUISharp.Logging;
using NAudio.Dsp;
using NAudio.Wave;

namespace DynamicWin;

public class AudioVisualizer : IDisposable
{
	private WasapiLoopbackCapture _capture;

	private SpectrumProcessor processor;

	private bool _isCapturing;

	public int BarCount { get; }

	public event EventHandler<float[]> DataAvailable;

	public AudioVisualizer(int barCount = 5)
	{
		BarCount = Math.Max(4, Math.Min(6, barCount));
		InitializeCapture();
	}

	private void InitializeCapture()
	{
		try
		{
			_capture = new WasapiLoopbackCapture();
			processor = new SpectrumProcessor(BarCount, _capture.WaveFormat.SampleRate);
			_capture.DataAvailable += OnDataAvailable;
			_capture.RecordingStopped += OnRecordingStopped;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException("Failed to initialize audio capture: " + ex.Message, ex);
		}
	}

	public void StartCapture()
	{
		if (_capture != null && !_isCapturing)
		{
			try
			{
				_capture.StartRecording();
				_isCapturing = true;
			}
			catch (COMException ex) when (ex.HResult == -2004287478)
			{
				_isCapturing = false;
			}
		}
	}

	public void StopCapture()
	{
		if (_capture != null && _isCapturing)
		{
			_capture.StopRecording();
			_isCapturing = false;
		}
	}

	private void OnDataAvailable(object sender, WaveInEventArgs e)
	{
		if (e.BytesRecorded < _capture.WaveFormat.BlockAlign) return;
		this.DataAvailable?.Invoke(this, processor.Process(e.Buffer, e.BytesRecorded, _capture.WaveFormat.Channels));
	}
	private void OnRecordingStopped(object sender, StoppedEventArgs e)
	{
		_isCapturing = false;
		if (e.Exception != null)
		{
			FLogger.Error("Visualizer recording stopped due to error: " + e.Exception.Message);
		}
	}

	public void Dispose()
	{
		StopCapture();
		_capture?.Dispose();
		this.DataAvailable = null;
	}
}

