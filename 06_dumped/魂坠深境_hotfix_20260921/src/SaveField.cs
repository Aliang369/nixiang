using System;

[AttributeUsage(AttributeTargets.Field)]
public class SaveField : Attribute
{
	public string customSaveKey;

	public string defaultValueGetter;

	public SaveField(string customSaveKey = null, string defaultValueGetter = null)
	{
		this.customSaveKey = customSaveKey;
		this.defaultValueGetter = defaultValueGetter;
	}
}
