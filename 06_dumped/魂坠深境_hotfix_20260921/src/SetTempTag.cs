using System;
using System.Collections.Generic;
using UnityEngine;

public class SetTempTag : SetTempKey
{
	[SerializeField]
	private ItemTagGenerator tagGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<ItemTag> tags = tagGenerator.GenerateItemTags(eve);
		if (type == TempValue.Type.GameGlobal)
		{
			throw new Exception("Global tag is not supported.");
		}
		if (!SetNormalTempTags(eve, tags))
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	private bool SetNormalTempTags(GameEvent eve, List<ItemTag> tags)
	{
		TempValue.GetDictionary(eve, type)[key] = tags;
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { tagGenerator };
	}
}
