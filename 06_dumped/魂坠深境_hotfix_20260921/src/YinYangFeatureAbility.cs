using System;
using System.Collections.Generic;
using UnityEngine;

public class YinYangFeatureAbility : Ability
{
	public enum Type
	{
		Add = 1,
		Consume,
		Convert
	}

	public class DisplayInfos
	{
		public AddDisplayInfo yangAddInfo;

		public ConsumeDisplayInfo yangConsumeInfo;

		public ConvertDisplayInfo yangConvertInfo;

		public AddDisplayInfo yinAddInfo;

		public ConsumeDisplayInfo yinConsumeInfo;

		public ConvertDisplayInfo yinConvertInfo;
	}

	public interface IYinYangInfoData
	{
		string GetText(GameStateInfo gameState, Item item, bool isBattleDisplay);

		Sprite GetIcon();

		Vector2 GetTextOffset_ItemInfoDisplay();

		Material GetMaterial();

		string GetDescriptionText(GameStateInfo gameState, Item item, bool isBattleDisplay);
	}

	public class ConvertDisplayInfo : IYinYangInfoData
	{
		public YinYangFeatureAbility ability;

		public TrigramsManager.YinYangType type;

		public TrigramsManager.YinYangType sourceType;

		public string GetText(GameStateInfo gameState, Item item, bool isBattleDisplay)
		{
			GameEvent eve = new GameEvent(GameEvent.Type.Display, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay)
			{
				gameState = gameState,
				CasterItem = item
			};
			int? num = sourceType switch
			{
				TrigramsManager.YinYangType.Yin => ability.GetYinCostValue(eve), 
				TrigramsManager.YinYangType.Yang => ability.GetYangCostValue(eve), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			bool? flag = ability.CheckYinYangConvert(eve);
			Color color = ((isBattleDisplay && flag == false) ? Color.red : ((type == TrigramsManager.YinYangType.Yang) ? Color.black : Color.white));
			if (!num.HasValue)
			{
				return "X".ColorText(color);
			}
			return $"{num.Value}".ColorText(color);
		}

		public Sprite GetIcon()
		{
			ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
			return type switch
			{
				TrigramsManager.YinYangType.Yin => itemBackgroundImageConfig.yinYangIcon_Convert_Yin, 
				TrigramsManager.YinYangType.Yang => itemBackgroundImageConfig.yinYangIcon_Convert_Yang, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public Vector2 GetTextOffset_ItemInfoDisplay()
		{
			return type switch
			{
				TrigramsManager.YinYangType.Yin => new Vector2(10f, 0.9f), 
				TrigramsManager.YinYangType.Yang => new Vector2(-8f, 4f), 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public Material GetMaterial()
		{
			ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
			return type switch
			{
				TrigramsManager.YinYangType.Yin => itemBackgroundImageConfig.yinYangMaterial_Convert_Yin, 
				TrigramsManager.YinYangType.Yang => itemBackgroundImageConfig.yinYangMaterial_Convert_Yang, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public string GetDescriptionText(GameStateInfo gameState, Item item, bool isBattleDisplay)
		{
			GameEvent eve = new GameEvent(GameEvent.Type.Display, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay)
			{
				gameState = gameState,
				CasterItem = item
			};
			int? num;
			switch (sourceType)
			{
			case TrigramsManager.YinYangType.Yin:
				num = ability.GetYinCostValue(eve);
				break;
			case TrigramsManager.YinYangType.Yang:
				num = ability.GetYangCostValue(eve);
				break;
			default:
				return "";
			}
			bool? flag = ability.CheckYinYangConvert(eve);
			Color color = ((isBattleDisplay && flag == false) ? Color.red : ((type == TrigramsManager.YinYangType.Yang) ? Color.black : Color.white));
			string text = ((!num.HasValue) ? "X".ColorText(color) : $"{num.Value}".ColorText(color));
			return type switch
			{
				TrigramsManager.YinYangType.Yin => ScriptableObjectSingleton<TextDataPreset>.instance.term_yinyang_convert_count_arg0_text_arg1_to_text_arg2.FormatWith(text, ScriptableObjectSingleton<GameDataManager>.instance.yinTextIcon.ToTMPSpriteText(), ScriptableObjectSingleton<GameDataManager>.instance.yangTextIcon.ToTMPSpriteText()), 
				TrigramsManager.YinYangType.Yang => ScriptableObjectSingleton<TextDataPreset>.instance.term_yinyang_convert_count_arg0_text_arg1_to_text_arg2.FormatWith(text, ScriptableObjectSingleton<GameDataManager>.instance.yangTextIcon.ToTMPSpriteText(), ScriptableObjectSingleton<GameDataManager>.instance.yinTextIcon.ToTMPSpriteText()), 
				_ => "", 
			};
		}
	}

	public class AddDisplayInfo : IYinYangInfoData
	{
		public YinYangFeatureAbility ability;

		public TrigramsManager.YinYangType type;

		public TrigramsManager.YinYangType sourceType;

		public string GetText(GameStateInfo gameState, Item item, bool isBattleDisplay)
		{
			GameEvent eve = new GameEvent(GameEvent.Type.Display, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay)
			{
				gameState = gameState,
				CasterItem = item
			};
			int? num = sourceType switch
			{
				TrigramsManager.YinYangType.Yin => ability.GetYinCostValue(eve), 
				TrigramsManager.YinYangType.Yang => ability.GetYangCostValue(eve), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			Color color = ((type == TrigramsManager.YinYangType.Yang) ? Color.black : Color.white);
			if (!num.HasValue)
			{
				return "X".ColorText(color);
			}
			return $"{num.Value}".ColorText(color);
		}

		public Sprite GetIcon()
		{
			ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
			return type switch
			{
				TrigramsManager.YinYangType.Yin => itemBackgroundImageConfig.yinYangIcon_Add_Yin, 
				TrigramsManager.YinYangType.Yang => itemBackgroundImageConfig.yinYangIcon_Add_Yang, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public Vector2 GetTextOffset_ItemInfoDisplay()
		{
			return type switch
			{
				TrigramsManager.YinYangType.Yin => new Vector2(10f, 0.9f), 
				TrigramsManager.YinYangType.Yang => new Vector2(-8f, 4f), 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public Material GetMaterial()
		{
			ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
			return type switch
			{
				TrigramsManager.YinYangType.Yin => itemBackgroundImageConfig.yinYangMaterial_Convert_Yin, 
				TrigramsManager.YinYangType.Yang => itemBackgroundImageConfig.yinYangMaterial_Convert_Yang, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public string GetDescriptionText(GameStateInfo gameState, Item item, bool isBattleDisplay)
		{
			GameEvent eve = new GameEvent(GameEvent.Type.Display, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay)
			{
				gameState = gameState,
				CasterItem = item
			};
			int? num;
			switch (sourceType)
			{
			case TrigramsManager.YinYangType.Yin:
				num = ability.GetYinCostValue(eve);
				break;
			case TrigramsManager.YinYangType.Yang:
				num = ability.GetYangCostValue(eve);
				break;
			default:
				return "";
			}
			bool? flag = ability.CheckYinYangConvert(eve);
			Color color = ((isBattleDisplay && flag == false) ? Color.red : ((type == TrigramsManager.YinYangType.Yang) ? Color.black : Color.white));
			string text = ((!num.HasValue) ? "X".ColorText(color) : $"{num.Value}".ColorText(color));
			return type switch
			{
				TrigramsManager.YinYangType.Yin => ScriptableObjectSingleton<TextDataPreset>.instance.term_yinyang_add.FormatWith(text, ScriptableObjectSingleton<GameDataManager>.instance.yinTextIcon.ToTMPSpriteText(), ScriptableObjectSingleton<GameDataManager>.instance.yangTextIcon.ToTMPSpriteText()), 
				TrigramsManager.YinYangType.Yang => ScriptableObjectSingleton<TextDataPreset>.instance.term_yinyang_add.FormatWith(text, ScriptableObjectSingleton<GameDataManager>.instance.yangTextIcon.ToTMPSpriteText(), ScriptableObjectSingleton<GameDataManager>.instance.yinTextIcon.ToTMPSpriteText()), 
				_ => "", 
			};
		}
	}

	public class ConsumeDisplayInfo : IYinYangInfoData
	{
		public YinYangFeatureAbility ability;

		public TrigramsManager.YinYangType type;

		public TrigramsManager.YinYangType sourceType;

		public string GetText(GameStateInfo gameState, Item item, bool isBattleDisplay)
		{
			GameEvent gameEvent = new GameEvent(GameEvent.Type.Display, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay)
			{
				gameState = gameState,
				CasterItem = item
			};
			int? num = null;
			switch (sourceType)
			{
			case TrigramsManager.YinYangType.Yin:
				num = ability.GetYinCostValue(gameEvent);
				break;
			case TrigramsManager.YinYangType.Yang:
				num = ability.GetYangCostValue(gameEvent);
				break;
			}
			bool flag = true;
			if (isBattleDisplay && gameEvent.gameState?.trigramsManager != null && num.HasValue)
			{
				switch (type)
				{
				case TrigramsManager.YinYangType.Yin:
					if ((int)gameEvent.gameState.trigramsManager.yinPower < num)
					{
						flag = false;
					}
					break;
				case TrigramsManager.YinYangType.Yang:
					if ((int)gameEvent.gameState.trigramsManager.yangPower < num)
					{
						flag = false;
					}
					break;
				}
			}
			Color color = ((!flag) ? Color.red : ((type == TrigramsManager.YinYangType.Yang) ? Color.black : Color.white));
			if (!num.HasValue)
			{
				return "X".ColorText(color);
			}
			return $"{num.Value}".ColorText(color);
		}

		public Sprite GetIcon()
		{
			ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
			return type switch
			{
				TrigramsManager.YinYangType.Yin => itemBackgroundImageConfig.yinYangIcon_Consume_Yin, 
				TrigramsManager.YinYangType.Yang => itemBackgroundImageConfig.yinYangIcon_Consume_Yang, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public Vector2 GetTextOffset_ItemInfoDisplay()
		{
			return type switch
			{
				TrigramsManager.YinYangType.Yin => new Vector2(10f, 0.9f), 
				TrigramsManager.YinYangType.Yang => new Vector2(-8f, 4f), 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public Material GetMaterial()
		{
			ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
			return type switch
			{
				TrigramsManager.YinYangType.Yin => itemBackgroundImageConfig.yinYangMaterial_Convert_Yin, 
				TrigramsManager.YinYangType.Yang => itemBackgroundImageConfig.yinYangMaterial_Convert_Yang, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public string GetDescriptionText(GameStateInfo gameState, Item item, bool isBattleDisplay)
		{
			GameEvent eve = new GameEvent(GameEvent.Type.Display, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay)
			{
				gameState = gameState,
				CasterItem = item
			};
			int? num;
			switch (sourceType)
			{
			case TrigramsManager.YinYangType.Yin:
				num = ability.GetYinCostValue(eve);
				break;
			case TrigramsManager.YinYangType.Yang:
				num = ability.GetYangCostValue(eve);
				break;
			default:
				return "";
			}
			bool? flag = ability.CheckYinYangConvert(eve);
			Color color = ((isBattleDisplay && flag == false) ? Color.red : ((type == TrigramsManager.YinYangType.Yang) ? Color.black : Color.white));
			string text = ((!num.HasValue) ? "X".ColorText(color) : $"{num.Value}".ColorText(color));
			return type switch
			{
				TrigramsManager.YinYangType.Yin => ScriptableObjectSingleton<TextDataPreset>.instance.term_yinyang_consume.FormatWith(text, ScriptableObjectSingleton<GameDataManager>.instance.yinTextIcon.ToTMPSpriteText(), ScriptableObjectSingleton<GameDataManager>.instance.yangTextIcon.ToTMPSpriteText()), 
				TrigramsManager.YinYangType.Yang => ScriptableObjectSingleton<TextDataPreset>.instance.term_yinyang_consume.FormatWith(text, ScriptableObjectSingleton<GameDataManager>.instance.yangTextIcon.ToTMPSpriteText(), ScriptableObjectSingleton<GameDataManager>.instance.yinTextIcon.ToTMPSpriteText()), 
				_ => "", 
			};
		}
	}

	[SerializeField]
	private Type type;

	[SerializeField]
	public bool upgradeModified;

	[SerializeField]
	private ValueGenerator yang;

	[SerializeField]
	private ValueGenerator yin;

	[SerializeField]
	private TrigramsManager.DurationType durationType;

	[SerializeField]
	private Effect convertEffect;

	private bool durationType_ShouldShow => type == Type.Add;

	private bool convertEffect_ShouldShow => type == Type.Convert;

	public void TriggerAdd(GameEvent eve)
	{
		if (durationType != TrigramsManager.DurationType.Temp && durationType != TrigramsManager.DurationType.Constant && durationType != TrigramsManager.DurationType.Turn)
		{
			TraceLog.Error($"侠客阴阳DurationType错误{durationType} - {eve.CasterItem?.ItemName}");
			return;
		}
		int valueOrDefault = GetYinCostValue(eve).GetValueOrDefault();
		int valueOrDefault2 = GetYangCostValue(eve).GetValueOrDefault();
		int num = (IsGeneratorReversed(yin, eve) ? (-valueOrDefault) : valueOrDefault);
		int num2 = (IsGeneratorReversed(yang, eve) ? (-valueOrDefault2) : valueOrDefault2);
		eve.gameState.actionManager.EnqueueAction(new TrigramsManager.ChangYinYangPowerAction(num, num2, durationType, eve.Caster));
		if (num2 != 0 || num != 0)
		{
			eve.gameState.actionManager.EnqueueAction(new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
		}
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.SpellCastResolve && eve.CasterItem == eve.SourceItem)
		{
			ItemContainer owner = eve.CasterItem.owner;
			if (owner == null || owner.OwnerBft?.isPlayer != true)
			{
				return;
			}
			int? yinCostValue = GetYinCostValue(eve);
			int? yangCostValue = GetYangCostValue(eve);
			bool flag = CheckRequirement(eve, TrigramsManager.YinYangType.Yin, yinCostValue);
			bool flag2 = CheckRequirement(eve, TrigramsManager.YinYangType.Yang, yangCostValue);
			eve.resolveInfo.reject |= !(flag & flag2);
		}
		if (eve.type != GameEvent.Type.BeforeSpellCastHandleCost || eve.CasterItem != eve.SourceItem)
		{
			return;
		}
		ItemContainer owner2 = eve.CasterItem.owner;
		if (owner2 == null || owner2.OwnerBft?.isPlayer != true)
		{
			return;
		}
		int? yinCostValue2 = GetYinCostValue(eve);
		int? yangCostValue2 = GetYangCostValue(eve);
		switch (type)
		{
		case Type.Consume:
		{
			int num = (IsGeneratorReversed(yin, eve) ? yinCostValue2.GetValueOrDefault() : (-yinCostValue2).GetValueOrDefault());
			int num2 = (IsGeneratorReversed(yang, eve) ? yangCostValue2.GetValueOrDefault() : (-yangCostValue2).GetValueOrDefault());
			eve.gameState.actionManager.EnqueueAction(new TrigramsManager.ChangYinYangPowerAction(num, num2, TrigramsManager.DurationType.Temp, eve.Caster));
			if (num2 != 0 || num != 0)
			{
				eve.gameState.actionManager.EnqueueAction(new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
			}
			break;
		}
		case Type.Convert:
			if (convertEffect != null)
			{
				convertEffect.Cast(eve);
			}
			if (yinCostValue2 > 0)
			{
				eve.gameState.actionManager.EnqueueAction(new TrigramsManager.ConvertYinYangPowerAction(yinCostValue2.Value, GetEffectivePowerType(TrigramsManager.YinYangType.Yin, yin, eve), eve.Caster));
			}
			if (yangCostValue2 > 0)
			{
				eve.gameState.actionManager.EnqueueAction(new TrigramsManager.ConvertYinYangPowerAction(yangCostValue2.Value, GetEffectivePowerType(TrigramsManager.YinYangType.Yang, yang, eve), eve.Caster));
			}
			if (yangCostValue2 > 0 || yinCostValue2 > 0)
			{
				eve.gameState.actionManager.EnqueueAction(new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case Type.Add:
			break;
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, yin, yang, convertEffect };
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.SpellCastResolve;
		yield return GameEvent.Type.BeforeSpellCastHandleCost;
	}

	public bool? CheckYinYangConvert(GameEvent eve)
	{
		if (type != Type.Convert)
		{
			return null;
		}
		if (eve.gameState?.trigramsManager == null)
		{
			return null;
		}
		int? yinCostValue = GetYinCostValue(eve);
		int? yangCostValue = GetYangCostValue(eve);
		if (!CheckRequirement(eve, TrigramsManager.YinYangType.Yin, yinCostValue) || !CheckRequirement(eve, TrigramsManager.YinYangType.Yang, yangCostValue))
		{
			return false;
		}
		return true;
	}

	private int? GetYinCostValue(GameEvent eve)
	{
		if (yin == null)
		{
			return null;
		}
		int? num = yin.GenerateValue(eve);
		if (!num.HasValue)
		{
			return num;
		}
		Type effectiveType = GetEffectiveType(yin, eve);
		TrigramsManager.YinYangType effectivePowerType = GetEffectivePowerType(TrigramsManager.YinYangType.Yin, yin, eve);
		if (effectiveType == Type.Convert && (eve.triggerType == GameEvent.TriggerType.InBattleDisplay || eve.triggerType == GameEvent.TriggerType.Real))
		{
			GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.YinYangConvertRequirementResolve(eve.GetEveObj(GameEvent.EveObjType.Caster), num.Value, effectivePowerType, eve.triggerType);
			num += resolveInfo.deltaValue;
			if (resolveInfo.reject)
			{
				num = 0;
			}
		}
		if (effectiveType == Type.Consume && (eve.triggerType == GameEvent.TriggerType.InBattleDisplay || eve.triggerType == GameEvent.TriggerType.Real))
		{
			GameEvent.ResolveInfo resolveInfo2 = eve.gameState.gameEventManager.YinYangConsumeRequirementResolve(eve.GetEveObj(GameEvent.EveObjType.Caster), num.Value, effectivePowerType, eve.triggerType);
			num += resolveInfo2.deltaValue;
			if (resolveInfo2.reject)
			{
				num = 0;
			}
		}
		return num;
	}

	private int? GetYangCostValue(GameEvent eve)
	{
		if (yang == null)
		{
			return null;
		}
		int? num = yang.GenerateValue(eve);
		if (!num.HasValue)
		{
			return num;
		}
		Type effectiveType = GetEffectiveType(yang, eve);
		TrigramsManager.YinYangType effectivePowerType = GetEffectivePowerType(TrigramsManager.YinYangType.Yang, yang, eve);
		if (effectiveType == Type.Convert && (eve.triggerType == GameEvent.TriggerType.InBattleDisplay || eve.triggerType == GameEvent.TriggerType.Real))
		{
			GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.YinYangConvertRequirementResolve(eve.GetEveObj(GameEvent.EveObjType.Caster), num.Value, effectivePowerType, eve.triggerType);
			num += resolveInfo.deltaValue;
			if (resolveInfo.reject)
			{
				num = 0;
			}
		}
		if (effectiveType == Type.Consume && (eve.triggerType == GameEvent.TriggerType.InBattleDisplay || eve.triggerType == GameEvent.TriggerType.Real))
		{
			GameEvent.ResolveInfo resolveInfo2 = eve.gameState.gameEventManager.YinYangConsumeRequirementResolve(eve.GetEveObj(GameEvent.EveObjType.Caster), num.Value, effectivePowerType, eve.triggerType);
			num += resolveInfo2.deltaValue;
			if (resolveInfo2.reject)
			{
				num = 0;
			}
		}
		return num;
	}

	public static bool HasReversibleChange(Item item, GameEvent eve)
	{
		string text = item?.GetDef()?.itemName;
		if (text == null || (!text.StartsWith("升·") && !text.StartsWith("转·") && !text.StartsWith("落·")))
		{
			return false;
		}
		YinYangFeatureAbility yinYangDisplayModule = GetYinYangDisplayModule(item);
		if (yinYangDisplayModule != null)
		{
			if (!yinYangDisplayModule.IsEffectiveConcreteGenerator(yinYangDisplayModule.yin, eve))
			{
				return yinYangDisplayModule.IsEffectiveConcreteGenerator(yinYangDisplayModule.yang, eve);
			}
			return true;
		}
		return false;
	}

	private bool IsEffectiveConcreteGenerator(ValueGenerator generator, GameEvent eve)
	{
		if (generator != null && !(generator is YinYangPowerValueGenerator))
		{
			return generator.GenerateValue(eve).GetValueOrDefault() > 0;
		}
		return false;
	}

	private bool IsGeneratorReversed(ValueGenerator generator, GameEvent eve)
	{
		if (IsEffectiveConcreteGenerator(generator, eve))
		{
			return MeridianReversalAbility.ShouldReverseYinYang(eve);
		}
		return false;
	}

	private Type GetEffectiveType(ValueGenerator generator, GameEvent eve)
	{
		if (!IsGeneratorReversed(generator, eve))
		{
			return type;
		}
		return type switch
		{
			Type.Add => Type.Consume, 
			Type.Consume => Type.Add, 
			_ => type, 
		};
	}

	private TrigramsManager.YinYangType GetEffectivePowerType(TrigramsManager.YinYangType originalType, ValueGenerator generator, GameEvent eve)
	{
		if (type != Type.Convert || !IsGeneratorReversed(generator, eve))
		{
			return originalType;
		}
		if (originalType != TrigramsManager.YinYangType.Yin)
		{
			return TrigramsManager.YinYangType.Yin;
		}
		return TrigramsManager.YinYangType.Yang;
	}

	private bool CheckRequirement(GameEvent eve, TrigramsManager.YinYangType originalType, int? value)
	{
		if (!value.HasValue)
		{
			return true;
		}
		ValueGenerator generator = ((originalType == TrigramsManager.YinYangType.Yin) ? yin : yang);
		Type effectiveType = GetEffectiveType(generator, eve);
		if (effectiveType != Type.Consume && effectiveType != Type.Convert)
		{
			return true;
		}
		return (int)((GetEffectivePowerType(originalType, generator, eve) == TrigramsManager.YinYangType.Yin) ? eve.gameState.trigramsManager.yinPower : eve.gameState.trigramsManager.yangPower) >= value.Value;
	}

	public DisplayInfos GetDisplayInfo(Item item = null, GameStateInfo gameState = null)
	{
		DisplayInfos displayInfos = new DisplayInfos();
		GameEvent eve = new GameEvent(GameEvent.Type.Display, (gameState != null) ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay)
		{
			gameState = gameState,
			CasterItem = item
		};
		AppendDisplayInfo(displayInfos, TrigramsManager.YinYangType.Yin, yin, eve);
		AppendDisplayInfo(displayInfos, TrigramsManager.YinYangType.Yang, yang, eve);
		return displayInfos;
	}

	private void AppendDisplayInfo(DisplayInfos info, TrigramsManager.YinYangType originalType, ValueGenerator generator, GameEvent eve)
	{
		if (generator == null)
		{
			return;
		}
		Type effectiveType = GetEffectiveType(generator, eve);
		TrigramsManager.YinYangType effectivePowerType = GetEffectivePowerType(originalType, generator, eve);
		switch (effectiveType)
		{
		case Type.Add:
		{
			AddDisplayInfo addDisplayInfo = new AddDisplayInfo
			{
				ability = this,
				type = effectivePowerType,
				sourceType = originalType
			};
			if (effectivePowerType == TrigramsManager.YinYangType.Yin)
			{
				info.yinAddInfo = addDisplayInfo;
			}
			else
			{
				info.yangAddInfo = addDisplayInfo;
			}
			break;
		}
		case Type.Consume:
		{
			ConsumeDisplayInfo consumeDisplayInfo = new ConsumeDisplayInfo
			{
				ability = this,
				type = effectivePowerType,
				sourceType = originalType
			};
			if (effectivePowerType == TrigramsManager.YinYangType.Yin)
			{
				info.yinConsumeInfo = consumeDisplayInfo;
			}
			else
			{
				info.yangConsumeInfo = consumeDisplayInfo;
			}
			break;
		}
		case Type.Convert:
		{
			ConvertDisplayInfo convertDisplayInfo = new ConvertDisplayInfo
			{
				ability = this,
				type = effectivePowerType,
				sourceType = originalType
			};
			if (effectivePowerType == TrigramsManager.YinYangType.Yin)
			{
				info.yinConvertInfo = convertDisplayInfo;
			}
			else
			{
				info.yangConvertInfo = convertDisplayInfo;
			}
			break;
		}
		}
	}

	public static YinYangFeatureAbility GetYinYangDisplayModule(Item item)
	{
		YinYangFeatureAbility result = null;
		foreach (Ability item2 in item.GetDef().abilities.OrEmptyIfNull())
		{
			if (item2 is YinYangFeatureAbility yinYangFeatureAbility)
			{
				result = yinYangFeatureAbility;
				break;
			}
		}
		return result;
	}
}
