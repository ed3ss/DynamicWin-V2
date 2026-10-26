using System.Collections.Generic;
using System.Threading.Tasks;
using FenUISharp;
using FenUISharp.Objects;

namespace DynamicWin;

[SettingsControl("base.calendar", "base.calendarcg")]
public class CalendarSettings : ISettingsControlGroup
{
	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		if (CalendarFetcher.IsUserLoggedInGoogle())
		{
			Settings.CreateSettingControlBase(list, new IconButton("Disconnect", () => Resources.GetImage("unlink"), delegate
			{
				CalendarFetcher.LogoutUserGoogle();
				Settings.RefreshView();
			}, 100f), "Google Calendar Connected");
		}
		else
		{
			Settings.CreateSettingControlBase(list, new IconButton("Connect", () => Resources.GetImage("link"), delegate
			{
				Task.Run(() => CalendarFetcher.TryLoginGoogle(Settings.RefreshView));
			}, 86f), "Connect Google Calendar");
		}
		return list;
	}
}
