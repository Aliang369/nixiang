using System;

public class LocalizeTextAttribute : Attribute
{
	public string description;

	public LocalizeTextAttribute()
	{
		description = "";
	}

	public LocalizeTextAttribute(string description)
	{
		this.description = description;
	}
}
