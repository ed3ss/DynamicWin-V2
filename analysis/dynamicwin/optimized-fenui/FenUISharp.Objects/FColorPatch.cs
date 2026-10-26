using System;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FColorPatch : Button, IStateListener
{
	private SKColor pickedColor;

	private FPopupPanel? activePickerPanel;

	private FColorPicker? activePicker;

	public SKColor PickedColor
	{
		get
		{
			return GetColor();
		}
		set
		{
			SetColor(value);
		}
	}

	public Action<SKColor>? OnColorUpdated { get; set; }

	public Action<SKColor>? OnUserColorUpdated { get; set; }

	public FColorPatch(SKColor? initialColor = null, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(null, position, size ?? ((Func<Vector2>)(() => new Vector2(50f, 25f))))
	{
		PickedColor = initialColor ?? new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
		base.Padding.SetStaticState(10);
	}

	private void SetColor(SKColor color, bool notifyPicker = true)
	{
		pickedColor = color;
		if (activePicker != null && notifyPicker)
		{
			activePicker.PickedColor = pickedColor;
		}
		OnColorUpdated?.Invoke(color);
		Invalidate(Invalidation.SurfaceDirty);
	}

	private SKColor GetColor()
	{
		return pickedColor;
	}

	protected override void OnInteract()
	{
		base.OnInteract();
		OpenPicker();
	}

	public void OpenPicker()
	{
		if (activePickerPanel == null)
		{
			CreatePicker();
		}
		activePickerPanel?.ToggleShow(() => base.Transform.LocalToGlobal(base.Transform.LocalPosition.CachedValue));
	}

	private void CreatePicker()
	{
		activePickerPanel = new FPopupPanel(() => new Vector2(225f, 150f), addLayout: false);
		activePickerPanel.CornerRadius.SetStaticState(30f);
		activePicker = new FColorPicker(pickedColor);
		activePicker.Layout.StretchHorizontal.SetStaticState(value: true);
		activePicker.Layout.StretchVertical.SetStaticState(value: true);
		activePicker.Transform.LocalPosition.SetStaticState(new Vector2(7.5f, 0f));
		activePicker.Layout.MarginHorizontal.SetStaticState(50f);
		activePicker.Layout.MarginVertical.SetStaticState(7.5f);
		activePicker.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		activePicker.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		FColorPicker? fColorPicker = activePicker;
		fColorPicker.OnColorUpdated = (Action<SKColor>)Delegate.Combine(fColorPicker.OnColorUpdated, (Action<SKColor>)delegate(SKColor x)
		{
			SetColor(x, notifyPicker: false);
		});
		FColorPicker? fColorPicker2 = activePicker;
		fColorPicker2.OnUserColorUpdated = (Action<SKColor>)Delegate.Combine(fColorPicker2.OnUserColorUpdated, (Action<SKColor>)delegate(SKColor x)
		{
			OnUserColorUpdated?.Invoke(x);
		});
		activePicker.SetParent(activePickerPanel);
		FDisplayButton fDisplayButton = new FDisplayButton(new FImage(() => Resources.GetImage("fenui-builtin-copy")), null, null, () => new Vector2(15f, 15f));
		fDisplayButton.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fDisplayButton.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		fDisplayButton.Transform.LocalPosition.SetStaticState(new Vector2(activePicker.Layout.ClampSize(activePicker.Transform.Size.CachedValue).x, 7.5f));
		fDisplayButton.RenderMaterial.Value = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.TransparentInteractableMaterial;
		fDisplayButton.SetParent(activePickerPanel);
		FDisplayButton fDisplayButton2 = new FDisplayButton(new FImage(() => Resources.GetImage("fenui-builtin-paste")), null, null, () => new Vector2(15f, 15f));
		fDisplayButton2.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fDisplayButton2.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		fDisplayButton2.RenderMaterial.Value = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.TransparentInteractableMaterial;
		fDisplayButton2.Transform.LocalPosition.SetStaticState(new Vector2(activePicker.Layout.ClampSize(activePicker.Transform.Size.CachedValue).x + 15f + 5f, 7.5f));
		fDisplayButton2.SetParent(activePickerPanel);
		FText text = new FText(TextModelFactory.CreateBasic(""), null, () => new Vector2(65f, 0f));
		text.Layout.StretchVertical.SetStaticState(value: true);
		text.Transform.LocalPosition.SetResponsiveState(() => new Vector2(activePicker.Layout.ClampSize(activePicker.Transform.Size.CachedValue).x, 25f));
		text.Layout.MarginVertical.SetStaticState(7.5f);
		text.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		text.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		text.Quality.SetStaticState(0.85f);
		text.SetParent(activePickerPanel);
		FColorPicker? fColorPicker3 = activePicker;
		fColorPicker3.OnColorUpdated = (Action<SKColor>)Delegate.Combine(fColorPicker3.OnColorUpdated, (Action<SKColor>)delegate(SKColor x)
		{
			text.Model = TextModelFactory.CreateBasic("R: " + x.Red + "\nG: " + x.Green + "\nB: " + x.Blue + "\n\n" + x, 12f, bold: false, italic: false, underlined: false, null, null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.Start
			});
		});
	}

	public SKShader GetColorPatchShader(SKColor color, SKRect rect)
	{
		string errors;
		SKRuntimeEffect sKRuntimeEffect = SKRuntimeEffect.CreateShader("\r\n                uniform float2 iResolution;\r\n                uniform float2 iOff;\r\n                uniform float2 iGlobOff;\r\n                uniform float4 iColor;\r\n\r\n                half4 lerp(half4 a, half4 b, half t) {\r\n                    return a + (b - a) * t;\r\n                }\r\n\r\n                half4 main(float2 fragCoord) {\r\n                    float2 uv = (fragCoord - iOff) / iResolution;\r\n\r\n                    half4 col = half4(iColor.rgb, 1);\r\n\r\n                    float2 coord = fragCoord - (iGlobOff);\r\n                    \r\n                    float checker = mod(floor(coord.x / 5) + floor(coord.y / 5), 2.0);\r\n                    checker = clamp(checker, 0.6, 0.9);\r\n\r\n                    half4 colAlpha = lerp(half4(checker, checker, checker, 1), col, iColor.a);\r\n                    if((uv.x > 0.5)) col = colAlpha;\r\n\r\n                    return col;\r\n                }\r\n            ", out errors);
		if (sKRuntimeEffect == null)
		{
			FLogger.Error("Shader compilation failed: " + errors);
		}
		SKRuntimeEffectUniforms sKRuntimeEffectUniforms = new SKRuntimeEffectUniforms(sKRuntimeEffect);
		sKRuntimeEffectUniforms["iResolution"] = new float[2] { rect.Width, rect.Height };
		sKRuntimeEffectUniforms["iOff"] = new float[2] { rect.Left, rect.Top };
		sKRuntimeEffectUniforms["iGlobOff"] = new float[2]
		{
			base.Transform.DrawLocalToGlobal(new Vector2(0f, 0f)).x,
			base.Transform.DrawLocalToGlobal(new Vector2(0f, 0f)).y
		};
		sKRuntimeEffectUniforms["iColor"] = new float[4]
		{
			(float)(int)color.Red / 255f,
			(float)(int)color.Green / 255f,
			(float)(int)color.Blue / 255f,
			(float)(int)color.Alpha / 255f
		};
		return sKRuntimeEffect?.ToShader(sKRuntimeEffectUniforms) ?? SKShader.CreateEmpty();
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint sKPaint = GetRenderPaint();
		SKRect localBounds = base.Shape.LocalBounds;
		localBounds.Inflate(-5f, -5f);
		using SKPath path = SKSquircle.CreateSquircle(localBounds, base.CornerRadius.CachedValue / 2f);
		using SKShader shader = GetColorPatchShader(pickedColor, localBounds);
		sKPaint.Shader = shader;
		sKPaint.Color = SKColors.White;
		canvas.DrawPath(path, sKPaint);
	}
}
