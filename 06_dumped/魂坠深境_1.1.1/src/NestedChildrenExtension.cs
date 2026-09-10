using System.Collections.Generic;

public static class NestedChildrenExtension
{
	public static void AddNestedChildToList<T>(this T node, List<T> list) where T : IChildren<T>
	{
		if (node == null)
		{
			return;
		}
		IEnumerable<T> children = node.GetChildren();
		list.Add(node);
		if (children == null)
		{
			return;
		}
		foreach (T item in children)
		{
			if (item != null)
			{
				item.AddNestedChildToList(list);
			}
		}
	}

	public static List<T> GetDescendantChild<T>(this T node, bool includeself = false) where T : IChildren<T>
	{
		List<T> list = new List<T>();
		if (node == null)
		{
			return list;
		}
		if (includeself)
		{
			list.Add(node);
		}
		List<T> children = node.GetChildren();
		if (children != null)
		{
			foreach (T item in children)
			{
				if (item != null)
				{
					item.AddNestedChildToList(list);
				}
			}
		}
		return list;
	}
}
