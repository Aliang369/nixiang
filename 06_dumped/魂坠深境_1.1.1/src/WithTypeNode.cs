using System;
using System.Collections.Generic;
using System.Reflection;

public class WithTypeNode : ItemConstraint
{
	public Type nodeType;

	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		if (ContainsNode(item?.CurrentSpell) || ContainsNode(item?.BaseSpell) || ContainsNode(item?.GetOriginalSpell()))
		{
			return true;
		}
		List<IEffectNode> obj = item?.GetDescendantNodeOfType(nodeType);
		return obj != null && obj.Count > 0;
	}

	private bool ContainsNode(ItemSpell spell)
	{
		List<IEffectNode> obj = spell?.spellNode?.GetDescendantNodeOfType(nodeType, includeSelf: true);
		if (obj == null)
		{
			return false;
		}
		return obj.Count > 0;
	}

	private List<Type> GetAllIEffectNodeType()
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type[] types = assemblies[i].GetTypes();
			foreach (Type type in types)
			{
				if (typeof(IEffectNode).IsAssignableFrom(type))
				{
					list.Add(type);
				}
			}
		}
		return list;
	}
}
