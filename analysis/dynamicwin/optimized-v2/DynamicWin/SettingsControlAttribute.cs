using System;

namespace DynamicWin;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SettingsControlAttribute : Attribute
{
	public string ID { get; }

	public string CategoryID { get; }

	public SettingsControlAttribute(string id, string cid)
	{
		ID = id;
		CategoryID = cid;
	}
}
