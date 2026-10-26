using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
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

public class DFile : SelectableButton
{
	private FPopupPanel? panel;

	public string FilePath { get; private set; }

	public DFile(string filePath, int index)
		: base(null, null, null, () => new Vector2(100f, 85f))
	{
		DFile dFile = this;
		FilePath = filePath;
		base.Padding.SetStaticState(25);
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: false);
		FImage image = new FImage(() => ThumbnailProviderBridge.GetFileThumbnail(filePath), drawBackground: false, dynamicColor: false, () => new Vector2(0f, -12f), () => new Vector2(55f, 55f));
		image.CornerRadius.SetStaticState(0f);
		image.SetParent(this);
		base.ImageEffects.Opacity.SetStaticState(0f);
		base.Transform.Scale.SetStaticState(Vector2.Zero);
		if (image.Image.CachedValue == null)
		{
			image.Image.Subscribe(delegate
			{
				dFile.AnimateFile(index, image);
			});
		}
		else
		{
			AnimateFile(index, image);
		}
		FRadialProgressBar fRadialProgressBar = new FRadialProgressBar(null, 50f);
		fRadialProgressBar.ImageEffects.InheritFromParent.SetStaticState(value: false);
		fRadialProgressBar.SetParent(this);
		fRadialProgressBar.Enabled.SetResponsiveState(() => image.Image.CachedValue == null);
		FText title = new FText(TextModelFactory.CreateBasic(Path.GetFileName(filePath), 9f, bold: true, italic: false, underlined: false, null, null, new TextAlign
		{
			VerticalAlign = TextAlign.AlignType.Start,
			HorizontalAlign = TextAlign.AlignType.Middle
		}), () => new Vector2(0f, -9f), () => new Vector2(70f, 35f));
		title.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		title.DisableWhenOutOfParentBounds = false;
		title.Transform.SnapPositionToPixelGrid.SetStaticState(value: false);
		title.SetParent(this);
		base.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(base.OnSelectionChanged, (Action<bool, SelectableButton>)delegate
		{
			title.Model = TextModelFactory.CopyBasic(title.Model, null, null, null, null, null, () => (!dFile.IsSelected) ? SKColors.Transparent : FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary);
		});
		Spring spring = new Spring(0.8f, 1.4f);
		base.Transform.LocalPosition.SetProcessor((Vector2 x) => spring.Update(FContext.DeltaTime, x));
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnDoubleMouseAction = (Action<MouseInputButton>)Delegate.Combine(interactiveSurface.OnDoubleMouseAction, (Action<MouseInputButton>)delegate(MouseInputButton x)
		{
			if (x == MouseInputButton.Left)
			{
				dFile.OnDoubleClick();
			}
		});
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface2.OnMouseAction, (Action<MouseInputCode>)delegate(MouseInputCode x)
		{
			if (x.button == MouseInputButton.Right && x.state == MouseInputState.Up)
			{
				dFile.OnRightClick();
			}
		});
		base.OnObjectDisposed = (Action)Delegate.Combine(base.OnObjectDisposed, (Action)delegate
		{
			dFile.panel?.Close();
		});
	}

	private void OnRightClick()
	{
		if (panel != null)
		{
			panel.Dispose();
			panel = null;
		}
		panel = new FPopupPanel(() => new Vector2(165f, 55f), addLayout: false);
		panel.DisposeOnClose = true;
		panel.DistanceToTarget = 10;
		panel.CornerRadius.SetStaticState(20f);
		FPopupPanel? fPopupPanel = panel;
		fPopupPanel.OnObjectDisposed = (Action)Delegate.Combine(fPopupPanel.OnObjectDisposed, (Action)delegate
		{
			panel = null;
		});
		panel.RenderMaterial.SetStaticState(new DefaultPanelMaterial());
		StackContentComponent stackContentComponent = new StackContentComponent(panel, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent.Pad.SetStaticState(10f);
		stackContentComponent.Gap.SetStaticState(7.5f);
		FImage fImage = new FImage(() => Resources.GetImage("external"));
		FDisplayButton fDisplayButton = new FDisplayButton(fImage, delegate
		{
			OpenFile();
			panel.ToggleShow();
			panel = null;
		});
		fImage.Transform.Size.SetStaticState(new Vector2(15f, 15f));
		fImage.Layout.StretchHorizontal.SetStaticState(value: false);
		fImage.Layout.StretchVertical.SetStaticState(value: false);
		fDisplayButton.Transform.Size.SetStaticState(new Vector2(30f, 30f));
		fDisplayButton.SetParent(panel);
		FImage fImage2 = new FImage(() => Resources.GetImage("clipboard-copy"));
		FDisplayButton fDisplayButton2 = new FDisplayButton(fImage2, delegate
		{
			Clipboard.SetFileDropList(new StringCollection { FilePath });
			panel.ToggleShow();
			panel = null;
		});
		fImage2.Transform.Size.SetStaticState(new Vector2(15f, 15f));
		fImage2.Layout.StretchHorizontal.SetStaticState(value: false);
		fImage2.Layout.StretchVertical.SetStaticState(value: false);
		fDisplayButton2.Transform.Size.SetStaticState(new Vector2(30f, 30f));
		fDisplayButton2.SetParent(panel);
		FImage fImage3 = new FImage(() => Resources.GetImage("delete"));
		FDisplayButton fDisplayButton3 = new FDisplayButton(fImage3, delegate
		{
			TrayView.RemoveFileFromTray(FilePath);
			panel.ToggleShow();
			panel = null;
		});
		fImage3.TintColor.SetStaticState(SKColors.Red);
		fImage3.Transform.Size.SetStaticState(new Vector2(15f, 15f));
		fImage3.Layout.StretchHorizontal.SetStaticState(value: false);
		fImage3.Layout.StretchVertical.SetStaticState(value: false);
		fDisplayButton3.Transform.Size.SetStaticState(new Vector2(30f, 30f));
		fDisplayButton3.SetParent(panel);
		stackContentComponent.FullUpdateLayout();
		Vector2 mousepos = FContext.GetCurrentWindow().ClientMousePosition;
		panel.ToggleShow(() => mousepos);
	}

	private void OnDoubleClick()
	{
		OpenFile();
	}

	private void OpenFile()
	{
		Process.Start("explorer", FilePath);
	}

	public override void Render(SKCanvas canvas)
	{
	}

	private void AnimateFile(int index, FImage image)
	{
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			if (!base.IsDisposed)
			{
				float opacity = 0f;
				base.ImageEffects.Opacity.SetResponsiveState(delegate
				{
					opacity = RMath.Lerp(opacity, (image.Image.CachedValue == null) ? 0f : 1f, FContext.DeltaTime * 3f);
					return opacity;
				});
				base.ImageEffects.BlurRadius.SetResponsiveState(() => (1f - opacity) * 15f);
				base.Transform.Scale.SetResponsiveState(() => Vector2.One * (opacity / 2f + 0.5f));
			}
		}, 0.1f * (float)index);
	}
}
