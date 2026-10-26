using System;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class IconButton : SelectableButton
{
	private AnimatorComponent anim;

	public FImage Image { get; private set; }

	public FText Text { get; private set; }

	public IconButton(string title, Func<SKImage>? icon = null, Action? onClick = null, float width = 125f, bool selectable = false)
		: base(onClick, null, () => Vector2.Zero, () => new Vector2(width, 30f))
	{
		IconButton iconButton = this;
		base.Transform.Size.SetStaticState(new Vector2(width, 30f));
		base.RenderMaterial.Value = () => new EmptyDefaultMaterial
		{
			BaseColor = () => SKColors.Transparent
		};
		base.EnabledFillColor.Value = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.WithAlpha((byte)(selectable ? 50u : 0u));
		anim = new AnimatorComponent(this, Easing.EaseOutCubic, Easing.EaseOutCubic);
		AnimatorComponent obj = anim;
		obj.OnValueUpdate = (Action<float>)Delegate.Combine(obj.OnValueUpdate, (Action<float>)delegate(float x)
		{
			iconButton.Transform.Scale.SetStaticState(RMath.Remap(x, 1f, 0f, 0.975f, 1f) * Vector2.One);
		});
		anim.Duration = 0.2f;
		StackContentComponent stackContentComponent = new StackContentComponent(this, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.Overflow);
		stackContentComponent.AlignInside = new Vector2(0f, 0.5f);
		stackContentComponent.Pad.SetStaticState(5f);
		stackContentComponent.Gap.SetStaticState(10f);
		Image = new FImage(icon ?? ((Func<SKImage>)(() => Resources.GetImage("default-settings-category-icon"))), drawBackground: false, dynamicColor: true, () => Vector2.Zero, () => new Vector2(20f, 20f));
		Image.CornerRadius.SetStaticState(7.5f);
		Image.UseSquircle.SetStaticState(value: false);
		Image.SetParent(this);
		Text = new FText(TextModelFactory.CreateBasic(title, 12f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.Middle
		}), () => Vector2.Zero, () => new Vector2(75f, 20f));
		Text.Layout.StretchHorizontal.SetStaticState(value: true);
		Text.SetParent(this);
		Text.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
		Image.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
		base.InteractiveSurface.ExtendInteractionRadius.SetStaticState(0);
		base.Padding.SetStaticState(2);
		base.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(base.OnSelectionChanged, (Action<bool, SelectableButton>)delegate
		{
			if (iconButton.ButtonGroup != null)
			{
				iconButton.Invalidate(Invalidation.SurfaceDirty);
			}
		});
	}
}
