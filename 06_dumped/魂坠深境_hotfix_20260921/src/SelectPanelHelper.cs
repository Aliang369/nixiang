using System;
using System.Collections.Generic;

public static class SelectPanelHelper
{
	public static string FormatSelectPanelHint(GameEvent selectEve, string rawHint, out TextDataGenerator textGenerator)
	{
		if (!rawHint.IsNullOrEmpty() && selectEve.GetEveObj(GameEvent.EveObjType.Caster) is IEffectObjectDisplay obj)
		{
			return TextDataPreset.FormatEffectObjectDescription(obj, rawHint, selectEve, out textGenerator);
		}
		textGenerator = null;
		return rawHint;
	}

	public static ElementSelectPanel<GameSkill>.SelectInfo SelectSkillCount(GameEvent eve, SkillSelector pool, ValueGenerator countGenerator, ValueCompareCheckNode.CompareType compareType, Action onConfirm, Action onCancel, BattleFieldTransform selector, Func<bool> canCancel, string panelHint)
	{
		List<GameSkill> poolItems = pool?.GetTargetSkills(eve);
		eve.SelectedSkills = new List<GameSkill>();
		int count = countGenerator?.GenerateValue(eve) ?? (-1);
		ElementSelectPanel<GameSkill>.SelectInfo info = new ElementSelectPanel<GameSkill>.SelectInfo
		{
			pool = poolItems,
			canConfirm = CanConfirm,
			canSelect = CanSelect,
			canCancel = canCancel,
			onCancel = onCancel,
			onConfirm = onConfirm,
			shouldCancel = ShouldCancel,
			singleSelect = (count == 1),
			output = eve.SelectedSkills,
			selectorBft = selector
		};
		info.panelHintGetter = () => FormatSelectPanelHint(eve, panelHint, out info.generator);
		return info;
		bool CanConfirm()
		{
			if (countGenerator == null)
			{
				return true;
			}
			return compareType switch
			{
				ValueCompareCheckNode.CompareType.Equal => eve.SelectedSkills.Count == count, 
				ValueCompareCheckNode.CompareType.Greater => eve.SelectedSkills.Count > count, 
				ValueCompareCheckNode.CompareType.Less => eve.SelectedSkills.Count < count, 
				ValueCompareCheckNode.CompareType.GreaterEqual => eve.SelectedSkills.Count >= count, 
				ValueCompareCheckNode.CompareType.LessEqual => eve.SelectedSkills.Count <= count, 
				_ => false, 
			};
		}
		bool CanSelect(GameSkill item)
		{
			if (countGenerator == null)
			{
				return true;
			}
			return compareType switch
			{
				ValueCompareCheckNode.CompareType.Equal => eve.SelectedSkills.Count != countGenerator.GenerateValue(eve), 
				ValueCompareCheckNode.CompareType.Greater => true, 
				ValueCompareCheckNode.CompareType.Less => eve.SelectedSkills.Count < countGenerator.GenerateValue(eve) - 1, 
				ValueCompareCheckNode.CompareType.GreaterEqual => true, 
				ValueCompareCheckNode.CompareType.LessEqual => eve.SelectedSkills.Count < countGenerator.GenerateValue(eve), 
				_ => false, 
			};
		}
		bool ShouldCancel()
		{
			if (poolItems == null || poolItems.Count == 0)
			{
				return true;
			}
			int count2 = poolItems.Count;
			if (countGenerator != null)
			{
				switch (compareType)
				{
				case ValueCompareCheckNode.CompareType.Equal:
					if (poolItems.Count < count || count2 < count)
					{
						return true;
					}
					break;
				case ValueCompareCheckNode.CompareType.Greater:
					if (poolItems.Count <= count || count2 <= count)
					{
						return true;
					}
					break;
				case ValueCompareCheckNode.CompareType.GreaterEqual:
					if (poolItems.Count < count || count2 < count)
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}
