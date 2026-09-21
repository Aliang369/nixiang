using System.Collections;
using System.Reflection;

public class TraverseLocalizeTermBinding
{
	private readonly object target;

	private readonly FieldInfo field;

	private readonly IList list;

	private readonly int listIndex;

	public TraverseLocalizeTermBinding(object target, FieldInfo field)
	{
		this.target = target;
		this.field = field;
	}

	public TraverseLocalizeTermBinding(IList list, int listIndex)
	{
		this.list = list;
		this.listIndex = listIndex;
	}

	public void SetValue(string text)
	{
		if (list != null && listIndex >= 0 && listIndex < list.Count)
		{
			list[listIndex] = text;
		}
		else
		{
			field?.SetValue(target, text);
		}
	}
}
