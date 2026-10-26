using System;
using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using SkiaSharp;
using Windows.Devices.Enumeration;

namespace DynamicWin;

public class BTConnectedView : NotchView
{
	private List<UIObject> list = new List<UIObject>();

	private FRadialProgressBar radial;

	private FText smallDeviceName;

	private FImage smallImg;

	private FText deviceName;

	private FText connected;

	private int yPos = -15;

	private FText charge;

	public override Vector2 ExpandedIslandSize => new Vector2(365f, 75f);

	public override float ExpandedCornerRadius => 55f;

	public override float MarginDivisor => 1.5f;

	public BTConnectedView(Notch island)
		: base(island)
	{
	}

	public override List<UIObject> Create()
	{
		list = new List<UIObject>();
		radial = new FRadialProgressBar(() => 0f, () => new Vector2(0f, 10 + yPos), 55f);
		radial.Thickness = 4f;
		radial.BackgroundColor.SetResponsiveState(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Success.WithAlpha(50));
		radial.FillColor.SetResponsiveState(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Success.WithAlpha(215));
		radial.Value.SetStaticState(0.5f);
		radial.MaxValue.SetStaticState(100f);
		radial.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		radial.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		charge = new FText(TextModelFactory.CreateBasic(""), () => Vector2.Zero, () => new Vector2(60f, 60f));
		charge.SetParent(radial);
		connected = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(75f, -8 + yPos), () => new Vector2(100f, 15f));
		connected.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		connected.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		deviceName = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(75f, 24 + yPos), () => new Vector2(130f, 40f));
		deviceName.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		deviceName.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		FImage fImage = new FImage(() => Resources.GetImage("bt-plc"), drawBackground: false, dynamicColor: false, () => new Vector2(0f, 10 + yPos), () => new Vector2(60f, 60f));
		fImage.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		fImage.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		smallImg = new FImage(() => Resources.GetImage("bt"), drawBackground: false, dynamicColor: false, () => Vector2.Zero, () => new Vector2(14f, 14f));
		smallImg.TintColor.SetResponsiveState(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(175));
		smallImg.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		smallImg.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		smallDeviceName = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(16f, 0f), () => new Vector2(130f, 20f));
		smallDeviceName.LayoutModel = new BlurLayoutProcessor(smallDeviceName, new WrapLayout(smallDeviceName))
		{
			FadeLength = 1.5f
		};
		smallDeviceName.Layout.StretchHorizontal.SetStaticState(value: true);
		smallDeviceName.Layout.MarginHorizontal.SetStaticState(25f);
		smallDeviceName.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		smallDeviceName.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		radial.Transform.Scale.Subscribe(delegate
		{
			radial.Invalidate(UIObject.Invalidation.SurfaceDirty | UIObject.Invalidation.LayoutDirty);
		});
		BTListenerActivity.BatteryInfoReceived = (Action<int>)Delegate.Combine(BTListenerActivity.BatteryInfoReceived, new Action<int>(UpdateBattery));
		BTListenerActivity.BatteryInfoReceived = (Action<int>)Delegate.Combine(BTListenerActivity.BatteryInfoReceived, new Action<int>(UpdateAdditional));
		BTListenerActivity.OnDeviceConnect = (Action<DeviceInformation>)Delegate.Combine(BTListenerActivity.OnDeviceConnect, new Action<DeviceInformation>(UpdateInfoConnect));
		BTListenerActivity.OnDeviceDisconnect = (Action)Delegate.Combine(BTListenerActivity.OnDeviceDisconnect, new Action(UpdateInfoDisconnect));
		if (BTListenerActivity.ConnectedDevice != null)
		{
			UpdateInfoConnect(BTListenerActivity.ConnectedDevice);
		}
		if (BTListenerActivity.BatteryInfoReceived != null)
		{
			UpdateBattery(BTListenerActivity.ConnectedDeviceBattery);
		}
		if (BTListenerActivity.BatteryInfoReceived != null)
		{
			UpdateAdditional(BTListenerActivity.ConnectedDeviceBattery);
		}
		list.Add(radial);
		list.Add(connected);
		list.Add(deviceName);
		list.Add(fImage);
		list.Add(smallDeviceName);
		list.Add(smallImg);
		return list;
	}

	public static UIObject GetRadial()
	{
		FRadialProgressBar radial = new FRadialProgressBar(() => 0f, () => new Vector2(0f, 0f), 15f);
		radial.Thickness = 2f;
		radial.BackgroundColor.SetResponsiveState(() => GetBatteryColor(BTListenerActivity.ConnectedDeviceBattery).Item2());
		radial.FillColor.SetResponsiveState(() => GetBatteryColor(BTListenerActivity.ConnectedDeviceBattery).Item1());
		BTListenerActivity.BatteryInfoReceived = (Action<int>)Delegate.Combine(BTListenerActivity.BatteryInfoReceived, (Action<int>)delegate(int x)
		{
			radial.Value.SetStaticState((x == -1) ? 100 : x);
		});
		radial.Value.SetStaticState(100f);
		radial.MaxValue.SetStaticState(100f);
		return radial;
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		BTListenerActivity.BatteryInfoReceived = (Action<int>)Delegate.Remove(BTListenerActivity.BatteryInfoReceived, new Action<int>(UpdateBattery));
		BTListenerActivity.BatteryInfoReceived = (Action<int>)Delegate.Remove(BTListenerActivity.BatteryInfoReceived, new Action<int>(UpdateAdditional));
		BTListenerActivity.OnDeviceConnect = (Action<DeviceInformation>)Delegate.Remove(BTListenerActivity.OnDeviceConnect, new Action<DeviceInformation>(UpdateInfoConnect));
		BTListenerActivity.OnDeviceDisconnect = (Action)Delegate.Remove(BTListenerActivity.OnDeviceDisconnect, new Action(UpdateInfoDisconnect));
	}

	private void UpdateInfoConnect(DeviceInformation information)
	{
		deviceName.Model = TextModelFactory.CreateBasic(information.Name, 14f, bold: true, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(byte.MaxValue), null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.Start
		});
		smallDeviceName.Model = TextModelFactory.CreateBasic(information.Name, 12f, bold: true, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(175), null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.Middle
		});
		connected.Model = TextModelFactory.CreateBasic("Connected", 12f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(75), null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.End
		});
	}

	private void UpdateInfoDisconnect()
	{
		deviceName.Model = TextModelFactory.CreateBasic("");
		smallDeviceName.Model = TextModelFactory.CreateBasic("");
		connected.Model = TextModelFactory.CreateBasic("");
		UpdateBattery(-1);
		UpdateAdditional(-1);
	}

	private static (Func<SKColor>, Func<SKColor>) GetBatteryColor(int battery)
	{
		Func<SKColor> item = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Success.WithAlpha(200);
		Func<SKColor> item2 = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Success.WithAlpha(50);
		if (battery <= 10)
		{
			item = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Error.WithAlpha(200);
			item2 = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Error.WithAlpha(75);
		}
		else if (battery <= 30)
		{
			item = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Warning.WithAlpha(200);
			item2 = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Warning.WithAlpha(75);
		}
		if (battery == -1)
		{
			item = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary;
			item2 = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.Multiply(0.6f);
		}
		return (item, item2);
	}

	private void UpdateAdditional(int battery)
	{
		if (battery == -1)
		{
			charge.Model = TextModelFactory.CreateBasic("", 14f, bold: true, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary);
			radial.Value.SetStaticState(100f);
		}
		else
		{
			charge.Model = TextModelFactory.CreateBasic($"{battery}%", 14f, bold: true, italic: false, underlined: false, GetBatteryColor(battery).Item1);
			radial.Value.SetStaticState(battery);
		}
	}

	private void UpdateBattery(int battery)
	{
		(Func<SKColor>, Func<SKColor>) batteryColor = GetBatteryColor(battery);
		radial.FillColor.SetResponsiveState(batteryColor.Item1);
		radial.BackgroundColor.SetResponsiveState(batteryColor.Item2);
		radial.Invalidate(UIObject.Invalidation.SurfaceDirty);
		charge.Invalidate(UIObject.Invalidation.SurfaceDirty);
		smallImg.TintColor.SetResponsiveState(batteryColor.Item1);
		smallImg.Invalidate(UIObject.Invalidation.SurfaceDirty);
		FText fText = smallDeviceName;
		TextModel model = smallDeviceName.Model;
		Func<SKColor> item = batteryColor.Item2;
		fText.SilentSetModel(TextModelFactory.CopyBasic(model, null, null, null, null, item));
	}

	public override void Update()
	{
		base.Update();
		float opacity = (float)Math.Pow(Math.Clamp(base.RootNotch.IsBigNotch, 0f, 1f), 6.0);
		float value = (float)Math.Pow(1f - Math.Clamp(base.RootNotch.IsBigNotch, 0f, 1f), 6.0);
		charge.Transform.Scale.SetStaticState(Vector2.One * Math.Clamp(base.RootNotch.IsBigNotch, 0f, 1f));
		float num = RMath.Remap(1f - RMath.Clamp(base.RootNotch.IsBigNotch, 0f, 1f), 0f, 1f, 0.6f, 1f);
		smallDeviceName.ImageEffects.Opacity.SetStaticState(value);
		smallDeviceName.Transform.Scale.SetStaticState(Vector2.One * num);
		smallImg.ImageEffects.Opacity.SetStaticState(value);
		smallImg.Transform.Scale.SetStaticState(Vector2.One * num);
		list.ForEach(delegate(UIObject x)
		{
			if (x != radial && x != smallDeviceName && x != smallImg)
			{
				x.ImageEffects.Opacity.SetStaticState(opacity);
			}
		});
		list.ForEach(delegate(UIObject x)
		{
			if (x != radial && x != smallDeviceName && x != smallImg)
			{
				x.Enabled.SetStaticState(base.RootNotch.IsBigNotch > 0.1f);
			}
		});
		State<Vector2> size = radial.Transform.Size;
		radial.Transform.Size.SetStaticState(Vector2.Lerp(new Vector2(14f, 14f), new Vector2(55f, 55f), base.RootNotch.IsBigNotch));
		radial.Thickness = RMath.Clamp(RMath.Lerp(5, 2, 1f - base.RootNotch.IsBigNotch), 2, 5);
		Vector2 value2 = Vector2.Lerp(new Vector2(0f, 10 + yPos), new Vector2(0f, 0f), 1f - base.RootNotch.IsBigNotch);
		radial.Transform.LocalPosition.SetStaticState(value2);
		if (size != radial.Transform.Size)
		{
			smallDeviceName.Invalidate(UIObject.Invalidation.SurfaceDirty);
			smallImg.Invalidate(UIObject.Invalidation.SurfaceDirty);
			radial.Invalidate(UIObject.Invalidation.SurfaceDirty);
		}
	}
}
