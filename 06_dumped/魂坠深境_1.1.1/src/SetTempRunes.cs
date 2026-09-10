using System.Collections.Generic;
using UnityEngine;

public class SetTempRunes : SetTempKey
{
	[SerializeField]
	private RuneGenerator runeGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Rune> list = runeGenerator?.GenerateRunes(eve);
		if (list.IsNullOrEmpty())
		{
			return NodeCastResult.Failure;
		}
		if (!SetNormalTempRunes(eve, list))
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	private bool SetNormalTempRunes(GameEvent eve, List<Rune> runes)
	{
		TempValue.GetDictionary(eve, type)[key] = runes;
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { runeGenerator };
	}
}
