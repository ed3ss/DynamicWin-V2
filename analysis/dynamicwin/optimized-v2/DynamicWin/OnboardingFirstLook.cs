using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class OnboardingFirstLook : View
{
	public override List<UIObject> Create()
	{
		FText fText = new FText(TextModelFactory.CreateBasic("Welcome to V2", 24f, bold: true, italic: false, underlined: false, null, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), null, () => new Vector2(300f, 20f));
		fText.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fText.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		FText fText2 = new FText(TextModelFactory.CreateBasic("An all new look.", 17f, bold: false, italic: false, underlined: false, () => SKColors.Gray, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), null, () => new Vector2(300f, 20f));
		fText2.Transform.LocalPosition.SetStaticState(new Vector2(0f, 32f));
		fText2.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fText2.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		FImage fImage = new FImage(() => Resources.GetImage("notch-preview"), drawBackground: false, dynamicColor: false, () => new Vector2(0f, 30f), () => new Vector2(0f, 140f));
		fImage.Layout.StretchHorizontal.SetStaticState(value: true);
		fImage.ScaleMode.SetStaticState(FImage.ImageScaleMode.Contain);
		return new List<UIObject> { fText, fText2, fImage };
	}
}
