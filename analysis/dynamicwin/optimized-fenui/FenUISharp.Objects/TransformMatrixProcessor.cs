using SkiaSharp;

namespace FenUISharp.Objects;

public abstract class TransformMatrixProcessor
{
	private TransformMatrixProcessor? _inner;

	public TransformMatrixProcessor(TransformMatrixProcessor? inner = null)
	{
		_inner = inner;
	}

	public SKMatrix ProcessMatrix(SKMatrix matrix)
	{
		return Process((_inner != null) ? _inner.ProcessMatrix(matrix) : matrix);
	}

	protected abstract SKMatrix Process(SKMatrix matrix);
}
