using System;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class QuickDrop : UIObject
{
	private bool isDragIn;

	private static QuickDrop? instance;

	private Spring spring;

	private float springVal = 1f;

	private float currentStrokeThickness;

	private bool start;

	public QuickDrop()
	{
		base.Layout.StretchHorizontal.SetStaticState(value: true);
		base.Layout.StretchVertical.SetStaticState(value: true);
		base.Layout.MarginHorizontal.SetStaticState(25f);
		base.Layout.MarginVertical.SetStaticState(25f);
		base.Padding.SetStaticState(30);
		instance = this;
		FText fText = new FText(TextModelFactory.CreateBasic("Drop files to Tray!", 14f, bold: true));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.StretchVertical.SetStaticState(value: true);
		fText.Layout.MarginHorizontal.SetStaticState(15f);
		fText.Layout.MarginVertical.SetStaticState(15f);
		fText.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
		fText.SetParent(this);
		DropComponent dropComponent = new DropComponent(this, DropType.FileDrop, DROPEFFECT.Copy | DROPEFFECT.Move);
		dropComponent.OnDragEnter = (Action<FDropData>)Delegate.Combine(dropComponent.OnDragEnter, new Action<FDropData>(DragEnter));
		dropComponent.OnDragLeave = (Action)Delegate.Combine(dropComponent.OnDragLeave, new Action(DragLeave));
		dropComponent.OnDrop = (Action<FDropData>)Delegate.Combine(dropComponent.OnDrop, new Action<FDropData>(Drop));
		spring = new Spring(1.5f, 2.2f);
		base.Transform.LocalPosition.SetStaticState(new Vector2(0f, 350f));
		base.Transform.Scale.SetStaticState(new Vector2(0.1f, 0.1f));
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			start = true;
			base.Transform.LocalPosition.SetResponsiveState(() => new Vector2(0f, (1f - ((RMath.Clamp(spring.GetLastValue().x, 0f, 2f) == 1f) ? Easing.EaseInQuart(RMath.Clamp(spring.GetLastValue().x, 0f, 2f)) : Easing.EaseOutCubic(RMath.Clamp(spring.GetLastValue().x, 0f, 2f)))) * 65f * (float)((springVal == 1f) ? 1 : (-1))));
		}, 5uL);
	}

	private void Drop(FDropData data)
	{
		if ((string[])data.GetData() != null)
		{
			TrayView.AddFilesToTray(data);
			MainView.OpenType(typeof(TrayView));
			Notch.MainNotch.NudgeOpen();
		}
	}

	private void DragLeave()
	{
		isDragIn = false;
		Console.WriteLine("Out");
	}

	private void DragEnter(FDropData data)
	{
		isDragIn = true;
		Console.WriteLine("In");
	}

	protected override void Update()
	{
		base.Update();
		if (start)
		{
			spring.Update(FContext.DeltaTime, new Vector2(springVal, 0f));
			float num = RMath.Clamp(spring.GetLastValue().x, 0f, 2f);
			base.Transform.Scale.SetStaticState(Vector2.One * num);
			_ = currentStrokeThickness;
			currentStrokeThickness = RMath.Lerp(currentStrokeThickness, isDragIn ? 6f : 0f, FContext.DeltaTime * 10f);
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint sKPaint = GetRenderPaint();
		using SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, 45f);
		sKPaint.Color = SKColors.Black;
		using SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(2f, 2f, 7f, 7f, SKColors.Black.WithAlpha(100));
		sKPaint.ImageFilter = imageFilter;
		canvas.DrawPath(path, sKPaint);
		if (currentStrokeThickness > 1f)
		{
			sKPaint.IsStroke = true;
			sKPaint.StrokeWidth = currentStrokeThickness;
			sKPaint.Color = new SKColor(57, 126, 237);
			sKPaint.ImageFilter = null;
			canvas.DrawPath(path, sKPaint);
			return;
		}
		sKPaint.IsStroke = true;
		sKPaint.PathEffect = SKPathEffect.CreateDash(new float[2] { 6f, 12f }, FContext.Time * 10f);
		sKPaint.StrokeWidth = 2.5f;
		sKPaint.IsStroke = true;
		sKPaint.StrokeJoin = SKStrokeJoin.Round;
		sKPaint.StrokeCap = SKStrokeCap.Round;
		sKPaint.Color = new SKColor(57, 126, 237);
		sKPaint.ImageFilter = null;
		canvas.DrawPath(path, sKPaint);
	}

	private void InstanceDestroy()
	{
		springVal = 0f;
	}

	internal static void Destroy()
	{
		instance?.InstanceDestroy();
	}
}
