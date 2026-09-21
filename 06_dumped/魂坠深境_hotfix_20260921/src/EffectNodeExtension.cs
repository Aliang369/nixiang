using System;
using System.Collections.Generic;

public static class EffectNodeExtension
{
	public static bool CheckDescendantContains(this IEffectNode parent, IEffectNode node)
	{
		return parent.GetDescendantNodes().Contains(node);
	}

	public static List<T> GetDescendantNodeOfType<T>(this IEffectNode node, bool includeSelf = false) where T : class, IEffectNode
	{
		List<T> list = new List<T>();
		if (node == null)
		{
			return list;
		}
		foreach (IEffectNode descendantNode in node.GetDescendantNodes(includeSelf))
		{
			if (descendantNode is T)
			{
				list.Add(descendantNode as T);
			}
		}
		return list;
	}

	public static List<IEffectNode> GetDescendantNodeOfType(this IEffectNode node, Type type, bool includeSelf = false)
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (node == null || type == null)
		{
			return list;
		}
		foreach (IEffectNode descendantNode in node.GetDescendantNodes(includeSelf))
		{
			if (type.IsInstanceOfType(descendantNode))
			{
				list.Add(descendantNode);
			}
		}
		return list;
	}

	public static List<IEffectNode> GetDescendantNodes(this IEffectNode node, bool includeself = false)
	{
		return node.GetDescendantChild(includeself);
	}
}
