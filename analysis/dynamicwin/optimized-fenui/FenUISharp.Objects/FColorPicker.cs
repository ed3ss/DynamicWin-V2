using System;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FColorPicker : FPanel
{
	private FHueSlider hueSlider;

	private FAlphaSlider alphaSlider;

	private SKRect pickerInnerBounds;

	private InteractiveSurface pickerSurface;

	private Vector2 pickerKnobPos;

	private Vector2 pickerKnobDisplayPos;

	private Spring knobSpring;

	private Spring knobSizeSpring;

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

	public FColorPicker(SKColor? initialColor = null)
		: base(null, () => new Vector2(150f, 150f))
	{
		FColorPicker fColorPicker = this;
		_drawBasePanel = false;
		knobSpring = new Spring(3.5f, 2f);
		knobSizeSpring = new Spring(2.7f, 2f);
		pickerSurface = new InteractiveSurface(this, FContext.GetCurrentDispatcher(), () => fColorPicker.Transform.DrawLocalToGlobal(fColorPicker.pickerInnerBounds));
		pickerSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = pickerSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, new Action<MouseInputCode>(MouseAction));
		InteractiveSurface interactiveSurface2 = pickerSurface;
		interactiveSurface2.OnDrag = (Action<Vector2>)Delegate.Combine(interactiveSurface2.OnDrag, new Action<Vector2>(DragPicker));
		hueSlider = new FHueSlider();
		hueSlider.Layout.StretchHorizontal.SetStaticState(value: true);
		hueSlider.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		hueSlider.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 1f));
		hueSlider.KnobSize = new Vector2(7.5f, hueSlider.KnobSize.y - 5f);
		hueSlider.Transform.Size.SetStaticState(new Vector2(0f, hueSlider.KnobSize.y - 10f));
		hueSlider.Transform.LocalPosition.SetStaticState(new Vector2(0f, -25f));
		hueSlider.DisplayFill = false;
		FHueSlider fHueSlider = hueSlider;
		fHueSlider.OnValueChanged = (Action<float>)Delegate.Combine(fHueSlider.OnValueChanged, (Action<float>)delegate
		{
			fColorPicker.Invalidate(Invalidation.SurfaceDirty);
			fColorPicker.OnColorUpdated?.Invoke(fColorPicker.PickedColor);
			fColorPicker.OnUserColorUpdated?.Invoke(fColorPicker.PickedColor);
		});
		alphaSlider = new FAlphaSlider();
		alphaSlider.Layout.StretchHorizontal.SetStaticState(value: true);
		alphaSlider.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		alphaSlider.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 1f));
		alphaSlider.KnobSize = new Vector2(7.5f, alphaSlider.KnobSize.y - 5f);
		alphaSlider.Transform.Size.SetStaticState(new Vector2(0f, alphaSlider.KnobSize.y - 10f));
		alphaSlider.Transform.LocalPosition.SetStaticState(new Vector2(0f, -5f));
		alphaSlider.DisplayFill = false;
		FAlphaSlider fAlphaSlider = alphaSlider;
		fAlphaSlider.OnValueChanged = (Action<float>)Delegate.Combine(fAlphaSlider.OnValueChanged, (Action<float>)delegate
		{
			fColorPicker.Invalidate(Invalidation.SurfaceDirty);
			fColorPicker.OnColorUpdated?.Invoke(fColorPicker.PickedColor);
			fColorPicker.OnUserColorUpdated?.Invoke(fColorPicker.PickedColor);
		});
		base.Padding.SetStaticState(15);
		hueSlider.SetParent(this);
		alphaSlider.SetParent(this);
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			fColorPicker.SetColor(initialColor ?? SKColors.White);
		}, 2uL);
	}

	private SKColor GetColor()
	{
		return SKColor.FromHsv(hueSlider.Value * 360f, pickerKnobPos.x / pickerInnerBounds.Width * 100f, (1f - pickerKnobPos.y / pickerInnerBounds.Height) * 100f).WithAlpha((byte)(alphaSlider.Value * 255f));
	}

	private void SetColor(SKColor color)
	{
		color.ToHsv(out var h, out var s, out var v);
		hueSlider.Value = h / 360f;
		alphaSlider.Value = (float)(int)color.Alpha / 255f;
		pickerKnobPos = new Vector2(s / 100f * pickerInnerBounds.Width, (1f - v / 100f) * pickerInnerBounds.Height);
		pickerKnobDisplayPos = pickerKnobPos;
		knobSpring.ResetVector(pickerKnobPos);
		OnColorUpdated?.Invoke(PickedColor);
	}

	private void MouseAction(MouseInputCode code)
	{
		if (code.button == MouseInputButton.Left && code.state == MouseInputState.Down)
		{
			SetToPoint(FContext.GetCurrentWindow().ClientMousePosition);
		}
	}

	private void DragPicker(Vector2 vector)
	{
		SetToPoint(FContext.GetCurrentWindow().ClientMousePosition);
	}

	private void SetToPoint(Vector2 point)
	{
		pickerKnobPos = base.Transform.GlobalToDrawLocal(point);
		pickerKnobPos = Vector2.Clamp(pickerKnobPos, Vector2.Zero, new Vector2(pickerInnerBounds.Width, pickerInnerBounds.Height));
		OnColorUpdated?.Invoke(PickedColor);
		OnUserColorUpdated?.Invoke(PickedColor);
	}

	public override void Dispose()
	{
		base.Dispose();
		pickerSurface.Dispose();
	}

	private void UpdateBounds()
	{
		pickerInnerBounds = base.Shape.LocalBounds;
		pickerInnerBounds.Offset(0f, -20f);
		pickerInnerBounds.Inflate(0f, -20f);
	}

	protected override void Update()
	{
		base.Update();
		UpdateBounds();
		Vector2 vector = pickerKnobPos + knobSizeSpring.GetLastValue().x;
		knobSizeSpring.Update(FContext.DeltaTime, new Vector2(pickerSurface.IsMouseDown ? 12.5f : 5f, 0f));
		pickerKnobDisplayPos = knobSpring.Update(FContext.DeltaTime, pickerKnobPos);
		if (vector != pickerKnobDisplayPos + knobSizeSpring.GetLastValue().x)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		RenderBasePanel(canvas, pickerInnerBounds);
		string errors;
		SKRuntimeEffect sKRuntimeEffect = SKRuntimeEffect.CreateShader("\r\n                uniform float2 iResolution;\r\n                uniform float hue;\r\n\r\n                half4 main(float2 fragCoord) {\r\n                    float2 uv = fragCoord / iResolution;\r\n\r\n                    // Calculate brightness (1.0 at top, 0.0 at bottom)\r\n                    float brightness = 1.0 - uv.y;\r\n\r\n                    // Saturation (0.0 at left, 1.0 at right)\r\n                    float saturation = uv.x;\r\n\r\n                    // Convert hue to radians and then to RGB\r\n                    float c = brightness * saturation;\r\n                    float h = hue / 60.0;\r\n                    float x = c * (1.0 - abs(mod(h, 2.0) - 1.0));\r\n                    \r\n                    float3 rgb;\r\n                    if (0.0 <= h && h < 1.0) rgb = float3(c, x, 0.0);\r\n                    else if (1.0 <= h && h < 2.0) rgb = float3(x, c, 0.0);\r\n                    else if (2.0 <= h && h < 3.0) rgb = float3(0.0, c, x);\r\n                    else if (3.0 <= h && h < 4.0) rgb = float3(0.0, x, c);\r\n                    else if (4.0 <= h && h < 5.0) rgb = float3(x, 0.0, c);\r\n                    else rgb = float3(c, 0.0, x);\r\n\r\n                    // Mix with white (for left side) and black (for bottom)\r\n                    float3 white = float3(1.0);\r\n                    float3 color = mix(white, rgb, saturation); // Horizontal gradient\r\n                    color *= brightness; // Vertical gradient\r\n\r\n                    return half4(color, 1.0);\r\n                }\r\n            ", out errors);
		if (sKRuntimeEffect == null)
		{
			FLogger.Error("Shader compilation failed: " + errors);
		}
		SKRuntimeEffectUniforms sKRuntimeEffectUniforms = new SKRuntimeEffectUniforms(sKRuntimeEffect);
		sKRuntimeEffectUniforms["iResolution"] = new float[2] { pickerInnerBounds.Width, pickerInnerBounds.Height };
		sKRuntimeEffectUniforms["hue"] = hueSlider.Value * 360f;
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Shader = sKRuntimeEffect?.ToShader(sKRuntimeEffectUniforms);
		using SKPath path = GetPanelPath(pickerInnerBounds);
		canvas.DrawPath(path, sKPaint);
		sKPaint.Shader = null;
		sKPaint.IsStroke = true;
		sKPaint.StrokeWidth = 1f;
		sKPaint.BlendMode = SKBlendMode.Plus;
		sKPaint.Color = new SKColor(150, 150, 150);
		canvas.DrawCircle(new SKPoint(pickerKnobDisplayPos.x, pickerKnobDisplayPos.y), knobSizeSpring.GetLastValue().x, sKPaint);
		sKPaint.IsStroke = false;
		sKPaint.Color = new SKColor(65, 65, 65);
		canvas.DrawCircle(new SKPoint(pickerKnobDisplayPos.x, pickerKnobDisplayPos.y), knobSizeSpring.GetLastValue().x, sKPaint);
	}
}
