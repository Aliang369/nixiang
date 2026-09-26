using System.Reflection;

public class zbTableBaseRecord
{
	public int[][] GetFieldIntArray(string fieldName)
	{
		PropertyInfo property = GetType().GetProperty(fieldName);
		if (property == null)
		{
			return null;
		}
		return (int[][])property.GetValue(this);
	}

	public int GetFieldInt(string fieldName)
	{
		PropertyInfo property = GetType().GetProperty(fieldName);
		if (property == null)
		{
			return 0;
		}
		return (int)property.GetValue(this);
	}

	public string GetFieldString(string fieldName)
	{
		PropertyInfo property = GetType().GetProperty(fieldName);
		if (property == null)
		{
			return "";
		}
		return (string)property.GetValue(this);
	}
}
