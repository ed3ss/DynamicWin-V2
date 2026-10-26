using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Logging;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class TrayView : NotchView
{
	private FPanel fileListPanel;

	private static Dispatcher? currentDispatcher;

	private static TrayView? currentInstance;

	private FPopupPanel? panel;

	private FButtonGroup lastBtnGroup;

	public override Vector2 ExpandedIslandSize => new Vector2(385f, 154f);

	public override float ExpandedCornerRadius => 50f;

	public TrayView(Notch island)
		: base(island)
	{
		currentDispatcher = FContext.GetCurrentDispatcher();
	}

	public override List<UIObject> Create()
	{
		List<UIObject> list = new List<UIObject>();
		currentInstance = this;
		FText fText = new FText(TextModelFactory.CreateBasic($"{GetItemCount()} Files, {GetAllFilesSize()} Total", 12f, bold: false, italic: false, underlined: false, null, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Middle,
			VerticalAlign = TextAlign.AlignType.Middle
		}), () => new Vector2(0f, RMath.Remap(base.RootNotch.IsBigNotch, 0f, 1f, -2f, 2f)), () => new Vector2(150f, 10f));
		fText.ImageEffects.Opacity.SetResponsiveState(() => RMath.Clamp(base.RootNotch.IsBigNotch * 2f - 1f, 0.4f, 1f));
		fText.Layout.Alignment.SetResponsiveState(() => new Vector2(0.5f, RMath.Remap(base.RootNotch.IsBigNotch, 0f, 1f, 0.5f, 0f)));
		fText.Layout.AlignmentAnchor.SetResponsiveState(() => new Vector2(0.5f, RMath.Remap(base.RootNotch.IsBigNotch, 0f, 1f, 0.5f, 1f)));
		list.Add(fText);
		fileListPanel = new FPanel(() => new Vector2(0f, 10f), () => new Vector2(350f, 100f));
		fileListPanel.RenderMaterial.SetStaticState(new EmptyDefaultMaterial
		{
			BaseColor = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface.Multiply(0.75f).AddMix(new SKColor(0, 0, 20)).WithAlpha(75)
		});
		fileListPanel.Transform.SnapPositionToPixelGrid.SetStaticState(value: false);
		fileListPanel.ImageEffects.Opacity.SetResponsiveState(() => RMath.Clamp(RMath.Remap(base.RootNotch.IsBigNotch, 0f, 1f, -2.5f, 1f), 0f, 2f));
		fileListPanel.Transform.Scale.SetResponsiveState(delegate
		{
			float num = RMath.Clamp(base.RootNotch.IsBigNotch * 2f - 1f, 0f, 2f);
			return new Vector2(num / 2f + 0.5f, num);
		});
		fileListPanel.Enabled.SetResponsiveState(() => base.RootNotch.IsBigNotch > 0.05f);
		fileListPanel.CornerRadius.SetStaticState(35f);
		fileListPanel.Padding.SetStaticState(0);
		fileListPanel.InteractiveSurface.ExtendInteractionRadius.SetStaticState(-30);
		list.Add(fileListPanel);
		FText fText2 = new FText(TextModelFactory.CreateBasic("Drop Files Here", 24f, bold: true, italic: false, underlined: false, null, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Middle,
			VerticalAlign = TextAlign.AlignType.Middle
		}), null, () => new Vector2(200f, 100f));
		new LayoutObject(fText2).IgnoreParentLayout.SetStaticState(value: true);
		fText2.SetParent(fileListPanel);
		fText2.Enabled.SetResponsiveState(() => GetItemCount() == 0);
		fText2.ImageEffects.Opacity.SetResponsiveState(() => RMath.Clamp(base.RootNotch.IsBigNotch, 0f, 0.4f));
		list.Add(fText2);
		StackContentComponent stackContentComponent = new StackContentComponent(fileListPanel, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.Scroll);
		stackContentComponent.ContentClipBehaviorProvider = new ScaleContentClipBehavior(stackContentComponent)
		{
			ClipStart = 100f,
			ClipLength = 25f
		};
		stackContentComponent.ContentFade = true;
		stackContentComponent.FadeLength = 15f;
		stackContentComponent.SnappingProvider = (float x) => MathF.Round(x / 110f) * 110f;
		UpdateFileList();
		ControlButton delFiles = new ControlButton(() => Resources.GetImage("trash"), 0f, delegate
		{
			Notch.MainNotch.DissolveTempView();
		}, () => new Vector2(1f, 0.5f), () => new Vector2(15f, 15f));
		delFiles.Enabled.SetResponsiveState(() => base.RootNotch.IsBigNotch > 0.5f);
		ControlButton controlButton = delFiles;
		controlButton.OnClick = (Action)Delegate.Combine(controlButton.OnClick, (Action)delegate
		{
			AskToDeleteTray(delFiles);
		});
		delFiles.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		delFiles.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		delFiles.TintColor.SetStaticState(new SKColor(100, 100, 100));
		delFiles.SetParent(fText);
		delFiles.Composition.LocalZIndex.SetStaticState(10);
		delFiles.InteractiveSurface.ExtendInteractionRadius.SetStaticState(20, 1u);
		list.Add(delFiles);
		return list;
	}

	private void AskToDeleteTray(UIObject? button)
	{
		if (panel != null)
		{
			panel.Dispose();
			panel = null;
		}
		if (button == null || button.IsDisposed)
		{
			return;
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
		FText fText = new FText(TextModelFactory.CreateBasic("Remove all files from Tray?"));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.MarginHorizontal.SetStaticState(25f);
		fText.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		fText.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		fText.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(10f, 0f));
		fText.Layout.StretchVertical.SetStaticState(value: true);
		fText.SetParent(panel);
		FSimpleButton fSimpleButton = new FSimpleButton(new FText(TextModelFactory.CreateBasic("YES")), delegate
		{
			RemoveAllTrayFiles();
			panel.ToggleShow();
			panel = null;
		});
		fSimpleButton.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		fSimpleButton.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(0f, 10f));
		fSimpleButton.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		fSimpleButton.SetParent(panel);
		UIObject? uIObject = button;
		uIObject.OnObjectDisposed = (Action)Delegate.Combine(uIObject.OnObjectDisposed, (Action)delegate
		{
			panel?.Close();
		});
		Vector2 lastPos = default(Vector2);
		panel.ToggleShow(delegate
		{
			if (button == null || button.IsDisposed)
			{
				return lastPos;
			}
			lastPos = button.Transform.LocalToGlobal(button.Transform.LocalPosition.CachedValue) - new Vector2(6f, 0f);
			return lastPos;
		});
	}

	private void RemoveAllTrayFiles()
	{
		List<string> list = Directory.GetFiles(ValidateFolder()).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			File.Delete(list[i]);
		}
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			UpdateFileList();
		}, 0.05f);
	}

	private void OnDragStart(DFile draggedFile)
	{
		if (lastBtnGroup == null)
		{
			return;
		}
		List<string> selectedFiles = new List<string>();
		lastBtnGroup.Buttons.ForEach(delegate(SelectableButton x)
		{
			if (x.IsSelected)
			{
				selectedFiles.Add(((DFile)x).FilePath);
			}
		});
		selectedFiles.ForEach(delegate(string x)
		{
			FLogger.Log("Dragging file with path: " + x);
		});
		if (draggedFile != null && !selectedFiles.Contains(draggedFile.FilePath))
		{
			selectedFiles.Add(draggedFile.FilePath);
		}
		if (selectedFiles.Count != 0)
		{
			FContext.GetCurrentWindow().WindowDispatcher.Invoke(delegate
			{
				DragSourceHelper.DragFiles(selectedFiles.ToArray());
			});
		}
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		fileListPanel?.Dispose();
		fileListPanel = null;
		lastBtnGroup = null;
		currentInstance = null;
	}

	private void UpdateFileList()
	{
		string path = ValidateFolder();
		foreach (UIObject item in fileListPanel.Children.ToList())
		{
			if (item is DFile)
			{
				item.Dispose();
			}
		}
		List<string> list = Directory.GetFiles(path).ToList();
		lastBtnGroup = new FButtonGroup();
		lastBtnGroup.AlwaysMustSelectOne = false;
		lastBtnGroup.AllowMultiSelect = false;
		int num = 0;
		foreach (string item2 in list)
		{
			ThumbnailProviderBridge.FetchThumbnail(item2);
			DFile fileO = new DFile(item2, num);
			fileO.SetParent(fileListPanel);
			fileO.Enabled.SetResponsiveState(() => base.RootNotch.IsBigNotch > 0.1f);
			num++;
			DFile dFile = fileO;
			dFile.OnClick = (Action)Delegate.Combine(dFile.OnClick, (Action)delegate
			{
				if (FContext.GetCurrentWindow().WindowKeyboardInput != null && FContext.IsValidContext())
				{
					lastBtnGroup.AllowMultiSelect = FContext.GetCurrentWindow().WindowKeyboardInput.IsControlPressed;
				}
			});
			InteractiveSurface interactiveSurface = fileO.InteractiveSurface;
			interactiveSurface.OnDragStart = (Action)Delegate.Combine(interactiveSurface.OnDragStart, (Action)delegate
			{
				OnDragStart(fileO);
			});
			lastBtnGroup.Add(fileO);
		}
		fileListPanel.Layout.RecursivelyUpdateLayout();
	}

	private int GetItemCount()
	{
		return Directory.GetFiles(ValidateFolder()).Length;
	}

	private string GetAllFilesSize()
	{
		string path = ValidateFolder();
		long num = 0L;
		string[] files = Directory.GetFiles(path);
		foreach (string fileName in files)
		{
			num += new FileInfo(fileName).Length;
		}
		return GetFileSize(num);
	}

	private string GetFileSize(long bytes)
	{
		string[] array = new string[9] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
		int num = 0;
		while (bytes >= 1024 && num < array.Length - 1)
		{
			num++;
			bytes /= 1024;
		}
		return $"{bytes:0.#} {array[num]}";
	}

	public static void SUpdateFileList()
	{
		currentDispatcher?.InvokeLater(delegate
		{
			currentInstance?.UpdateFileList();
		}, 0.1f);
	}

	public static void AddFilesToTray(FDropData data)
	{
		string path = ValidateFolder();
		string[] array = (string[])data.GetData();
		if (array != null)
		{
			try
			{
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (Settings.GetSetting("ft.always-copy-to-tray-folder", defaultValue: false))
					{
						File.Copy(text, Path.Combine(path, Path.GetFileName(text)));
					}
					else
					{
						File.Move(text, Path.Combine(path, Path.GetFileName(text)));
					}
				}
			}
			catch (Exception)
			{
			}
		}
		SUpdateFileList();
	}

	public static void RemoveFileFromTray(string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		SUpdateFileList();
	}

	private static string ValidateFolder()
	{
		string text = Path.Combine(DynamicWin.AppDataPath, "Tray");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}
}
