using System;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using SkiaSharp;

namespace DynamicWin;

public class TrayDropArea : UIObject
{
	private DropComponent dropComponent;

	private AnimatorComponent anim;

	public State<SKColor> Primary { get; set; }

	public TrayDropArea(Func<Vector2>? position = null)
		: base(position, () => new Vector2(170f, 75f))
	{
		anim = new AnimatorComponent(this, Easing.EaseOutCubic, Easing.EaseOutCubic);
		AnimatorComponent animatorComponent = anim;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
		{
			base.Transform.Scale.SetStaticState(Vector2.One + x * 0.01f);
			base.ImageEffects.Brightness.SetStaticState(1f + x * 0.5f);
		});
		anim.Duration = 0.35f;
		dropComponent = new DropComponent(this, DropType.FileDrop, Settings.GetSetting("ft.always-copy-to-tray-folder", defaultValue: false) ? DROPEFFECT.Copy : DROPEFFECT.Move);
		DropComponent obj = dropComponent;
		obj.OnDragEnter = (Action<FDropData>)Delegate.Combine(obj.OnDragEnter, new Action<FDropData>(DragEnter));
		DropComponent obj2 = dropComponent;
		obj2.OnDragLeave = (Action)Delegate.Combine(obj2.OnDragLeave, new Action(DragLeave));
		DropComponent obj3 = dropComponent;
		obj3.OnDrop = (Action<FDropData>)Delegate.Combine(obj3.OnDrop, new Action<FDropData>(Drop));
		Primary = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary, this, this);
		base.Padding.SetStaticState(5);
		FText fText = new FText(TextModelFactory.CreateBasic("Tray", 15f, bold: true, italic: false, underlined: false, () => Primary.CachedValue, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), () => new Vector2(0f, 16f), () => new Vector2(126f, 25f));
		fText.ImageEffects.Brightness.SetStaticState(1.75f);
		fText.SetParent(this);
		FImage fImage = new FImage(() => Resources.GetImage("tray"), drawBackground: false, dynamicColor: false, () => new Vector2(-56.25f, -10.5f), () => new Vector2(18f, 18f));
		fImage.TintColor.SetResponsiveState(() => Primary.CachedValue);
		fImage.ImageEffects.Brightness.SetStaticState(1.75f);
		fImage.TintBlendMode.SetStaticState(SKBlendMode.Modulate);
		fImage.Transform.Anchor.SetStaticState(new Vector2(0f, 0.5f));
		fImage.SetParent(this);
	}

	private void Drop(FDropData data)
	{
		TrayView.AddFilesToTray(data);
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			Notch.MainNotch.DissolveTempView(delegate
			{
				MainView.OpenType(typeof(TrayView));
			});
		}, 0.05f);
	}

	private void DragLeave()
	{
		anim.Inverse = true;
		anim.Restart();
	}

	private void DragEnter(FDropData data)
	{
		anim.Inverse = false;
		anim.Restart();
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Color = Primary.CachedValue.WithAlpha(100);
		sKPaint.PathEffect = SKPathEffect.CreateDash(new float[2] { 7f, 7f }, 0f);
		sKPaint.StrokeWidth = 2f;
		sKPaint.IsStroke = true;
		sKPaint.StrokeJoin = SKStrokeJoin.Round;
		sKPaint.StrokeCap = SKStrokeCap.Round;
		using SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, 15f);
		canvas.DrawPath(path, sKPaint);
	}
}
