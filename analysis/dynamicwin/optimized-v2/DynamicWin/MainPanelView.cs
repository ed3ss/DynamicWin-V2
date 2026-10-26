using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;

namespace DynamicWin;

public class MainPanelView : View
{
	private string id;

	public MainPanelView(string id)
	{
		this.id = id;
	}

	public override List<UIObject> Create()
	{
		List<UIObject> list = new List<UIObject>();
		SettingsCategory settingsCategory = Settings.Categories.Where((SettingsCategory x) => x.CategoryID == id).Last();
		if (settingsCategory == null)
		{
			return new List<UIObject>();
		}
		FText fText = new FText(TextModelFactory.CreateBasic(settingsCategory.Name ?? "Settings", 30f, bold: true, italic: false, underlined: false, null, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), () => Vector2.Zero, () => new Vector2(300f, 40f));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.MarginHorizontal.SetStaticState(20f);
		list.Add(fText);
		foreach (ISettingsControlGroup item in Settings.Controls.Where((ISettingsControlGroup x) => x.GetType().GetCustomAttribute<SettingsControlAttribute>()?.CategoryID == id).ToList())
		{
			item.Create().ForEach(delegate(UIObject x)
			{
				list.Add(x);
			});
		}
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			base.PaneRoot?.Layout.RecursivelyUpdateLayout();
		}, 2uL);
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			base.PaneRoot?.Layout.RecursivelyUpdateLayout();
		}, 3uL);
		return list;
	}
}
