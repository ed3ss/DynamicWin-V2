using System;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class Transform : IDisposable, IStateListener
{
	private WeakReference<UIObject>? WeakOwner;

	internal Vector2 CalculatedLayoutOff;

	internal Vector2 CalculatedAnchorCorrection;

	private Vector2? thisFrameVisibleSize;

	private Vector2? lastVisibleSize;

	public UIObject? Owner
	{
		get
		{
			WeakReference<UIObject>? weakOwner = WeakOwner;
			if (weakOwner != null && weakOwner.TryGetTarget(out UIObject target))
			{
				return target;
			}
			return null;
		}
	}

	public State<Vector2> LocalPosition { get; init; }

	public Vector2 Position { get; private set; }

	public Vector2 Pivot { get; private set; }

	public State<bool> SnapPositionToPixelGrid { get; init; }

	public Vector2 VisibleSize => Owner.Layout.ClampSize(Size.CachedValue);

	public State<Vector2> Size { get; init; }

	public State<Vector2> Scale { get; init; }

	public State<float> Rotation { get; init; }

	public State<Vector2> Anchor { get; init; }

	public TransformMatrixProcessor? MatrixProcessor { get; set; }

	public SKMatrix DrawMatrix { get; internal set; }

	public SKMatrix RecursiveDrawMatrix { get; internal set; }

	public Transform(UIObject owner)
	{
		WeakOwner = new WeakReference<UIObject>(owner);
		SnapPositionToPixelGrid = new State<bool>(() => false, owner, this);
		LocalPosition = new State<Vector2>(() => new Vector2(0f, 0f), owner, this);
		Size = new State<Vector2>(() => new Vector2(0f, 0f), owner, this);
		Scale = new State<Vector2>(() => new Vector2(1f, 1f), owner, this);
		Rotation = new State<float>(() => 0f, owner, this);
		Anchor = new State<Vector2>(() => new Vector2(0.5f, 0.5f), owner, this);
	}

	public void UpdateTransform()
	{
		SKMatrix sKMatrix = SKMatrix.CreateIdentity();
		if (MatrixProcessor != null)
		{
			sKMatrix = SKMatrix.Concat(sKMatrix, MatrixProcessor.ProcessMatrix(sKMatrix));
		}
		lastVisibleSize = thisFrameVisibleSize;
		thisFrameVisibleSize = Owner?.Layout.ApplyLayoutToSize(Size.CachedValue);
		Owner?.Layout.ApplyLayoutToPositioning(thisFrameVisibleSize ?? new Vector2(0f, 0f), out CalculatedLayoutOff, out CalculatedAnchorCorrection);
		if ((thisFrameVisibleSize?.x != lastVisibleSize?.x || thisFrameVisibleSize?.y != lastVisibleSize?.y) && Owner != null)
		{
			Owner.LayoutChangedThisFrame = true;
			Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		}
		Position = LocalPosition.CachedValue + (CalculatedLayoutOff - CalculatedAnchorCorrection);
		Vector2? vector = Owner?.Layout.ApplyLayoutToSize(Owner.Transform.Size.CachedValue);
		Pivot = new Vector2(vector.GetValueOrDefault().x * Anchor.CachedValue.x, vector.GetValueOrDefault().y * Anchor.CachedValue.y);
		if (SnapPositionToPixelGrid.CachedValue)
		{
			Position = new Vector2(MathF.Round(Position.x), MathF.Round(Position.y));
		}
		if (SnapPositionToPixelGrid.CachedValue)
		{
			Pivot = new Vector2(MathF.Round(Pivot.x), MathF.Round(Pivot.y));
		}
		sKMatrix = SKMatrix.Concat(sKMatrix, SKMatrix.CreateTranslation(Position.x, Position.y));
		sKMatrix = SKMatrix.Concat(sKMatrix, SKMatrix.CreateRotationDegrees(Rotation.CachedValue, Pivot.x, Pivot.y));
		sKMatrix = SKMatrix.Concat(sKMatrix, SKMatrix.CreateScale(Scale.CachedValue.x, Scale.CachedValue.y, Pivot.x, Pivot.y));
		DrawMatrix = sKMatrix;
		RecursiveDrawMatrix = GetRecursiveDrawMatrix();
	}

	public SKMatrix GetRecursiveDrawMatrix()
	{
		SKMatrix sKMatrix = DrawMatrix;
		for (UIObject uIObject = Owner?.Parent; uIObject != null; uIObject = uIObject.Parent)
		{
			sKMatrix = SKMatrix.Concat(uIObject.Transform.DrawMatrix, sKMatrix);
		}
		return sKMatrix;
	}

	public Vector2 DrawLocalToGlobal(Vector2 local, SKMatrix? matrix = null)
	{
		SKPoint sKPoint = (matrix ?? RecursiveDrawMatrix).MapPoint(local.x, local.y);
		return new Vector2(sKPoint.X, sKPoint.Y);
	}

	public SKRect DrawLocalToGlobal(SKRect local, SKMatrix? matrix = null)
	{
		return (matrix ?? RecursiveDrawMatrix).MapRect(local);
	}

	public Vector2 GlobalToDrawLocal(Vector2 local, SKMatrix? matrix = null)
	{
		SKPoint sKPoint = (matrix ?? RecursiveDrawMatrix).Invert().MapPoint(local.x, local.y);
		return new Vector2(sKPoint.X, sKPoint.Y);
	}

	public SKRect GlobalToDrawLocal(SKRect local, SKMatrix? matrix = null)
	{
		return (matrix ?? RecursiveDrawMatrix).Invert().MapRect(local);
	}

	public SKMatrix LocalToGlobalMatrix()
	{
		return SKMatrix.Concat(Owner?.Parent?.Transform.RecursiveDrawMatrix ?? SKMatrix.CreateIdentity(), SKMatrix.CreateTranslation(CalculatedLayoutOff.x, CalculatedLayoutOff.y));
	}

	public SKMatrix GlobalToLocalMatrix()
	{
		LocalToGlobalMatrix().TryInvert(out var inverse);
		return inverse;
	}

	public Vector2 LocalToGlobal(Vector2 local)
	{
		SKPoint sKPoint = LocalToGlobalMatrix().MapPoint(local.x, local.y);
		return new Vector2(sKPoint.X, sKPoint.Y);
	}

	public SKRect LocalToGlobal(SKRect local)
	{
		return LocalToGlobalMatrix().MapRect(local);
	}

	public Vector2 GlobalToLocal(Vector2 global)
	{
		SKPoint sKPoint = GlobalToLocalMatrix().MapPoint(global.x, global.y);
		return new Vector2(sKPoint.X, sKPoint.Y);
	}

	public SKRect GlobalToLocal(SKRect global)
	{
		return GlobalToLocalMatrix().MapRect(global);
	}

	public virtual void OnInternalStateChanged<T>(T value)
	{
		Owner?.Invalidate(UIObject.Invalidation.TransformDirty);
	}

	public void Dispose()
	{
		MatrixProcessor = null;
		WeakOwner = null;
	}
}
