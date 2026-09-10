using System;
using System.Collections.Generic;
using System.Reflection;

public class SkillWithTypeNode : SkillConstraint
{
	public Type nodeType;

	public override bool? CheckSkillAccept(GameSkill skill)
	{
		bool? flag = base.CheckSkillAccept(skill);
		if (flag != true)
		{
			return flag;
		}
		return (skill?.GetDescendantNodeOfType(nodeType)).Count > 0;
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
