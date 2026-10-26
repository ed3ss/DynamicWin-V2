using System;
using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;
using FenUISharp.RuntimeEffects;
using SkiaSharp;

namespace DynamicWin;

public class UpdatingView : NotchView
{
	public override Vector2 ExpandedIslandSize => new Vector2(150f, 30f);

	public UpdatingView(Notch island)
		: base(island)
	{
	}

	public override List<UIObject> Create()
	{
		FText fText = new FText(TextModelFactory.CreateBasic("Checking for updates...", 12f, bold: true, italic: false, underlined: false, () => SKColors.DarkGray.Multiply(0.9f).WithAlpha(200)));
		fText.Padding.SetStaticState(20);
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		FGradientSwipeEffect fGradientSwipeEffect = new FGradientSwipeEffect(fText);
		fGradientSwipeEffect.Primary.SetStaticState(new SKColor(byte.MaxValue, 202, 105));
		fGradientSwipeEffect.Secondary.SetStaticState(SKColors.Transparent);
		float timeOff = FContext.Time - 0.5f;
		fGradientSwipeEffect.GradientPosition.SetResponsiveState(() => (FContext.Time - timeOff) / 1.2f - 0.5f);
		fGradientSwipeEffect.GradientWidth = 0.5f;
		fGradientSwipeEffect.GradientRotation = -45f;
		FBloomEffect fBloomEffect = new FBloomEffect();
		fBloomEffect.Downsampling = 1;
		fBloomEffect.BloomIntensity = 0.8f;
		fBloomEffect.BloomThreshold = 0.25f;
		fBloomEffect.BloomSpread = 12.5f;
		fText.PostProcessChain.Attach(fGradientSwipeEffect);
		fText.PostProcessChain.Attach(fBloomEffect);
		Dispatcher dispatcher = FContext.GetCurrentDispatcher();
		Action action = delegate
		{
			DynamicWin.CheckForUpdates(delegate
			{
				dispatcher.InvokeLater(delegate
				{
					ActivityManager.UnregisterView<UpdatingView>();
				}, 0.15f);
			});
		};
		FContext.GetCurrentDispatcher().InvokeLater(action, 4f);
		return new List<UIObject> { fText };
	}
}
