using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public static class TempValue
{
	public enum Type
	{
		Node = 1,
		Caster = 2,
		GameGlobal = 10,
		ApplicationGlobal = 11
	}

	public class TempValueDropdownAttribute : ValueDropdownAttribute
	{
		public TempValueDropdownAttribute(string memberName)
			: base(memberName)
		{
			base.AppendNextDrawer = true;
		}
	}

	public const string AimItem = "AimItem";

	public const string AttachItems = "AttachItems";

	public const string AttachItemDefs = "AttachItemDefs";

	public const string Flag = "Flag";

	public const string MusicSpiritUpgradeProgress = "音灵升级进度";

	public const string MusicSpiritUpgradeMaxProgress = "音灵升级最大进度";

	public static readonly List<string> defaultKeys = new List<string> { "AttachItemDefs", "AttachItems", "AimItem", "Flag", "NodeTempItemKey", "音灵升级进度", "音灵升级最大进度" };

	public static bool TryGetDictionary(GameEvent eve, Type type, out Dictionary<string, object> dictionary)
	{
		dictionary = null;
		if (eve == null)
		{
			return false;
		}
		switch (type)
		{
		case Type.Node:
			dictionary = eve.nodeTempValues;
			return dictionary != null;
		case Type.Caster:
			if (eve.CasterItem != null)
			{
				dictionary = eve.CasterItem.tempValues;
			}
			else if (eve.CasterEnchant != null)
			{
				dictionary = eve.CasterEnchant.tempValues;
			}
			else if (eve.CasterRune != null)
			{
				dictionary = eve.CasterRune.tempValues;
			}
			else if (eve.CasterInfluence != null)
			{
				dictionary = eve.CasterInfluence.tempValues;
			}
			else if (eve.CasterStanceDef != null)
			{
				dictionary = eve.CasterStanceDef.tempValues;
			}
			else if (eve.CasterItemTag != null)
			{
				dictionary = eve.CasterItemTag.tempValues;
			}
			else if (eve.CasterSkill != null)
			{
				GameSkill casterSkill = eve.CasterSkill;
				if (casterSkill.tempValues == null)
				{
					casterSkill.tempValues = new Dictionary<string, object>();
				}
				dictionary = eve.CasterSkill.tempValues;
			}
			else if (eve.GetEveObj<EnvironmentInfluence>(GameEvent.EveObjType.Caster) != null)
			{
				dictionary = eve.GetEveObj<EnvironmentInfluence>(GameEvent.EveObjType.Caster).tempValues;
			}
			else if (eve.GetEveObj<WorldEvent>(GameEvent.EveObjType.Caster) != null)
			{
				dictionary = eve.GetEveObj<WorldEvent>(GameEvent.EveObjType.Caster).tempValues;
			}
			return dictionary != null;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static Dictionary<string, object> GetDictionary(GameEvent eve, Type type)
	{
		if (TryGetDictionary(eve, type, out var dictionary))
		{
			return dictionary;
		}
		TraceLog.Error($"{eve?.Caster}不支持tempValue");
		return new Dictionary<string, object>();
	}
}
