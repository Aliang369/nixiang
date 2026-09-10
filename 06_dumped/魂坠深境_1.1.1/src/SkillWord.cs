using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillWord : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IAttachment, IBftOwnable
{
	[SaveField(null, null)]
	public SkillWordDef def;

	[SaveField(null, null)]
	public int stack;

	[SaveField(null, null)]
	public readonly GameSkill gameSkill;

	public BattleFieldTransform OwnerBft => gameSkill?.OwnerBft;

	public Sprite Icon => def.Icon;

	public string WordName => def.wordName;

	public string NameWithQualityColor => def.NameWithQualityColor;

	public object AttachTarget => gameSkill;

	private SkillWord()
	{
	}

	public SkillWord(SkillWordDef def, int stack, GameSkill skill)
	{
		this.def = def;
		gameSkill = skill;
		this.stack = stack;
	}

	public int GetPropModifyValue(SkillDef.SkillPropertyType type)
	{
		SkillPropertyModifier skillPropertyModifier = def.skillPropertyModifiers?.FirstOrDefault((SkillPropertyModifier m) => m.type == type);
		if (skillPropertyModifier != null)
		{
			return skillPropertyModifier.deltaValuePerStack * stack;
		}
		return 0;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return def.GetNodeDisplayInfo(eve);
	}

	public List<IEffectNode> GetChildren()
	{
		return ((IChildren<IEffectNode>)def).GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		if (!(def != null))
		{
			yield break;
		}
		foreach (GameEvent.Type item in def.GetAllSubscribeGameEventType())
		{
			yield return item;
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (InGameGlobalVariableStateInfo.DisableAllSkillWord.GetVariableFlag())
		{
			return false;
		}
		return def.CheckTriggerGameEventAble(eve);
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		def.TriggerGameEvent(eve);
	}

	public string GetFormatDescription(ApplicationStateInfo applicationState, ProfileSkill profileSkill, out TextDataGenerator generator)
	{
		string text = TextDataPreset.FormatSkillWordDescription(this, applicationState, out generator);
		if (profileSkill != null && !def.CheckAttachAble(profileSkill))
		{
			text += ScriptableObjectSingleton<TextDataPreset>.instance.term_enchant_add_invalid;
		}
		return text;
	}

	public static List<SkillWord> CompressSkillWord(List<SkillWord> word, GameSkill skill)
	{
		Dictionary<SkillWordDef, SkillWord> dictionary = new Dictionary<SkillWordDef, SkillWord>();
		foreach (SkillWord item in word.OrEmptyIfNull())
		{
			if (item != null && !(item.def == null))
			{
				if (dictionary.TryGetValue(item.def, out var value))
				{
					value.stack += item.stack;
				}
				else
				{
					dictionary[item.def] = new SkillWord(item.def, item.stack, skill);
				}
			}
		}
		return dictionary.Values.ToList();
	}
}
