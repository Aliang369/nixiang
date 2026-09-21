using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameSkill : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IBftOwnable
{
	[SaveField(null, null)]
	public int cooldown;

	[SaveField(null, null)]
	public int durability;

	[SaveField(null, null)]
	public BattleFieldTransform owner;

	[SaveField(null, null)]
	public SkillDef skillDef;

	[SaveField(null, null)]
	public int archeTypeIndex;

	[SaveField(null, null)]
	public string sourceProfileSkillGUID;

	public ProfileSkill _profileSkill;

	[SaveField(null, null)]
	public HexShape skillHexShape;

	[SaveField(null, null)]
	public List<SkillGem> skillGems;

	[SaveField(null, null)]
	public List<SkillWord> skillWords;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues = new Dictionary<string, object>();

	public SkillDef.SkillArchetype SkillArchetype => skillDef.skillArchetypes.IndexOrNull(archeTypeIndex) ?? skillDef.skillArchetypes.First();

	public int CastCooldown
	{
		get
		{
			if (!SkillArchetype.GetPropFlag(SkillDef.SkillPropertyType.CastCoolDown))
			{
				return 0;
			}
			return Math.Max(GetSkillPropertyValue(SkillDef.SkillPropertyType.CastCoolDown), 1);
		}
	}

	public int InitDurability
	{
		get
		{
			if (DurabilityType == SkillDef.SkillPropertyType.None)
			{
				return 0;
			}
			return GetSkillPropertyValue(DurabilityType);
		}
	}

	public int ActivateCost
	{
		get
		{
			if (ActivateCostType == SkillDef.SkillPropertyType.None)
			{
				return 0;
			}
			return GetSkillPropertyValue(ActivateCostType);
		}
	}

	public bool ReplaceRecycle => GetSkillPropertyFlag(SkillDef.SkillPropertyType.ReplaceRecycle);

	public bool ReplaceToActivate => GetSkillPropertyFlag(SkillDef.SkillPropertyType.ReplaceToActivate);

	public bool LightActivate => GetSkillPropertyFlag(SkillDef.SkillPropertyType.LightActivate);

	public bool OneCastPerBattle => GetSkillPropertyFlag(SkillDef.SkillPropertyType.CastOncePerBattle);

	public SkillDef.SkillPropertyType DurabilityType => SkillArchetype.ArchDurabilityType;

	public SkillDef.SkillPropertyType ActivateCostType => SkillArchetype.ArchActivateCostType;

	public bool IsBaseArchType => archeTypeIndex == 0;

	public QualityLevel QualityLevel => SkillArchetype.qualityLevel;

	public ProfileSkill profileSkill => _profileSkill ?? Bootstrap.applicationState.globalSkillManager.allSkills.FirstOrDefault((ProfileSkill p) => p.profileSkillGUID == sourceProfileSkillGUID);

	public string NameWithQualityColor => skillDef.skillName.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(QualityLevel));

	public string Name => skillDef.skillName;

	public string NameWithQualityAndUpgrade => Name.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(QualityLevel));

	public bool CoolDowned => cooldown == 0;

	public bool isPassiveSkill => skillDef.skillSpell == null;

	public Sprite Icon => skillDef.Icon;

	public SkillSpell SkillSpell => skillDef.skillSpell;

	public bool WithDurability => DurabilityType != SkillDef.SkillPropertyType.None;

	public bool WithSpell => !isPassiveSkill;

	public BattleFieldTransform OwnerBft => owner;

	public int GetSkillPropertyValue(SkillDef.SkillPropertyType type)
	{
		int? num = SkillArchetype.GetPropValue(type);
		if (!num.HasValue)
		{
			if (SkillDef.IsModifyValuePropertyType(type))
			{
				return 0;
			}
			num = 0;
		}
		return (num + skillWords.Sum((SkillWord w) => w.GetPropModifyValue(type))).Value;
	}

	public bool GetSkillPropertyFlag(SkillDef.SkillPropertyType type)
	{
		return GetSkillPropertyValue(type) > 0;
	}

	public void AddRuntimeSkillGem(SkillGemDef skillGemDef)
	{
		if (!(skillGemDef == null))
		{
			if (skillGems == null)
			{
				skillGems = new List<SkillGem>();
			}
			if (skillWords == null)
			{
				skillWords = new List<SkillWord>();
			}
			skillGems.Add(new SkillGem(skillGemDef));
			skillWords.AddRange(from word in skillGemDef.GetGemSkillWords()
				select new SkillWord(word.def, word.stack, this));
			skillWords = SkillWord.CompressSkillWord(skillWords, this);
		}
	}

	private GameSkill()
	{
	}

	public GameSkill(PresetSkillSlot presetSkillSlot)
	{
		ProfileSkill profileSkill = (_profileSkill = presetSkillSlot.skill);
		sourceProfileSkillGUID = profileSkill.profileSkillGUID;
		archeTypeIndex = profileSkill.archetypeIndex;
		skillDef = profileSkill.def;
		skillGems = profileSkill.skillGemsMounted.OrEmptyIfNull().ToList();
		skillHexShape = _profileSkill.hexShape;
		skillWords = SkillGem.GemToWords(skillGems, this);
		durability = InitDurability;
	}

	public GameSkill(ProfileSkill profileSkill)
	{
		_profileSkill = profileSkill;
		sourceProfileSkillGUID = profileSkill.profileSkillGUID;
		archeTypeIndex = profileSkill.archetypeIndex;
		skillDef = profileSkill.def;
		skillHexShape = profileSkill.hexShape;
		skillGems = profileSkill.skillGemsMounted.OrEmptyIfNull().ToList();
		skillWords = SkillGem.GemToWords(skillGems, this);
		durability = InitDurability;
	}

	public bool IsCastAble(GameStateInfo gs)
	{
		if (skillDef.skillSpell == null)
		{
			return false;
		}
		if (!gs.playerState.ActivatedSkills.Contains(this))
		{
			return false;
		}
		if (OneCastPerBattle && gs.recordManager.castedSkillsThisBattle.Contains(this))
		{
			return false;
		}
		if (skillDef.skillSpell.castableCheck != null)
		{
			GameEvent gameEvent = new GameEvent(GameEvent.Type.SpellCastNodeCheck, GameEvent.TriggerType.Real);
			gameEvent.gameState = gs;
			gameEvent.Caster = this;
			if (skillDef.skillSpell.castableCheck.Check(gameEvent) != true)
			{
				return false;
			}
		}
		return CoolDowned;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public List<IEffectNode> GetChildren()
	{
		return skillDef.GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (Ability item in skillDef.abilities.OrEmptyIfNull())
		{
			if (item == null)
			{
				continue;
			}
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		bool result = false;
		foreach (Ability item in skillDef.abilities.OrEmptyIfNull())
		{
			if (item.CheckTriggerGameEventAble(eve))
			{
				result = true;
			}
		}
		return result;
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability item in skillDef.abilities.OrEmptyIfNull())
		{
			item.TriggerGameEvent(eve);
		}
	}

	public bool IsInSlotSkill(GameStateInfo gameState)
	{
		return gameState.playerState.ActivatedSkills_inSlot.Contains(this);
	}

	public void Cast(GameEvent eve)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_amulet_trigger);
		skillDef.skillSpell.castEffect.Cast(eve);
		if (!eve.gameState.gameEventManager.TriggerSkillCastCoolDownResolve(this).reject)
		{
			cooldown = CastCooldown;
		}
		if (DurabilityType == SkillDef.SkillPropertyType.CastDurability)
		{
			eve.gameState.actionManager.EnqueueAction(new ChangeSkillDurabilityAction(this, -1));
		}
		eve.gameState.actionManager.EnqueueAction(delegate
		{
			eve.gameState.gameEventManager.TriggerAfterSkillCast(this, eve);
		});
	}

	public string GetSkillWordsDescription(GameStateInfo gameState)
	{
		return new StringBuilder().ToString();
	}

	public string GetDurabilityDescription()
	{
		if (DurabilityType == SkillDef.SkillPropertyType.None)
		{
			return string.Empty;
		}
		if (DurabilityType == SkillDef.SkillPropertyType.CastDurability)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_with_cast_durability_arg0.FormatWith(durability);
		}
		if (DurabilityType == SkillDef.SkillPropertyType.BattleDurability)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_with_battle_durability_arg0.FormatWith(durability);
		}
		if (DurabilityType == SkillDef.SkillPropertyType.EffectDurability)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_with_effect_durability_arg0.FormatWith(durability);
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_with_durability_arg0.FormatWith(durability);
	}

	public string GetCoolDownDescription()
	{
		if (WithSpell && !OneCastPerBattle)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_cooldown_arg0.FormatWith(CastCooldown);
		}
		return string.Empty;
	}

	public string GetActivateCostDescription()
	{
		if (ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCoinCost)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_with_activate_cost_count_arg0_sprite_arg1.FormatWith(ActivateCost, ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText());
		}
		if (ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCrystalCost)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_with_activate_cost_count_arg0_sprite_arg1.FormatWith(ActivateCost, ScriptableObjectSingleton<GameDataManager>.instance.crystalInTextIcon.ToTMPSpriteText());
		}
		return string.Empty;
	}

	public string GetNeedReplaceActivateDescription()
	{
		if (ReplaceToActivate)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_need_another_replace_activate;
		}
		return string.Empty;
	}

	public string GetOnCastDescription()
	{
		if (OneCastPerBattle)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_one_cast_only_per_battle;
		}
		return string.Empty;
	}

	public string GetBasicInfoDescription()
	{
		return "".AppendNewLineIfNotEmpty(GetDurabilityDescription()).AppendNewLineIfNotEmpty(GetCoolDownDescription()).AppendNewLineIfNotEmpty(GetActivateCostDescription())
			.AppendNewLineIfNotEmpty(GetNeedReplaceActivateDescription())
			.AppendNewLineIfNotEmpty(GetOnCastDescription());
	}

	public string GetCastDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = TextDataGenerator.Create(applicationState, this);
		string text = string.Empty;
		if (SkillArchetype.GetPropFlag(SkillDef.SkillPropertyType.InitialActivate))
		{
			text = ScriptableObjectSingleton<TextDataPreset>.instance.term_initialActivate + "\n";
		}
		if (!WithSpell)
		{
			return text;
		}
		return text + generator.Parse(skillDef.description);
	}

	public string GetPassiveDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = TextDataGenerator.Create(applicationState, this);
		string result = "";
		if (!WithSpell)
		{
			result = generator.Parse(skillDef.description);
		}
		return result;
	}

	public List<(string, TextDataGenerator)> GetAllWordDescriptions(ApplicationStateInfo applicationState)
	{
		List<(string, TextDataGenerator)> list = new List<(string, TextDataGenerator)>();
		foreach (SkillWord skillWord in skillWords)
		{
			string formatDescription = skillWord.GetFormatDescription(applicationState, profileSkill, out var generator);
			list.Add((formatDescription, generator));
		}
		return list;
	}

	public GameSkill Copy()
	{
		GameSkill gameSkill = MemberwiseClone() as GameSkill;
		gameSkill.skillGems = (from gem in skillGems.OrEmptyIfNull()
			where gem != null
			select new SkillGem(gem)).ToList();
		gameSkill.skillWords = SkillWord.CompressSkillWord(skillWords, gameSkill);
		gameSkill.tempValues = ((tempValues == null) ? new Dictionary<string, object>() : new Dictionary<string, object>(tempValues));
		return gameSkill;
	}
}
