using System;

namespace DynamicWin;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RegisterableMetadataAttribute : Attribute
{
	public string Name { get; }

	public string Author { get; }

	public string Description { get; }

	public string RegisterableID { get; }

	public Version Version { get; }

	public bool IsBaseExtension
	{
		get
		{
			if (Author == "fen4flo")
			{
				return RegisterableID == "dynamicwin-builtin";
			}
			return false;
		}
	}

	public RegisterableMetadataAttribute(string name, string author, string description, string registerableID, string version)
	{
		Name = name;
		Author = author;
		Description = description;
		RegisterableID = registerableID;
		if (System.Version.TryParse(version, out Version result))
		{
			Version = result;
		}
		else
		{
			Version = new Version(1, 0, 0);
		}
	}
}
