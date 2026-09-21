using System;
using System.Collections.Generic;

public class TempValueGenerator : ValueGenerator
{
	public TempValue.Type type;

	public bool outBattleDisplayUnknown;

	public string key;

	public ValueGenerator defaultValueGenerator;

	public override int? GenerateValue(GameEvent eve)
	{
		if (outBattleDisplayUnknown && eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		if (type == TempValue.Type.GameGlobal)
		{
			return GeneratorGlobalValue(eve);
		}
		return GeneratorNormalValue(eve);
	}

	private int? GeneratorNormalValue(GameEvent eve)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		int? defaultValue = defaultValueGenerator?.GenerateValue(eve);
		return GetTempValueInDic(eve, key, dictionary, defaultValue);
	}

	public static int? GetTempValueInDic(GameEvent eve, string key, Dictionary<string, object> dic, int? defaultValue)
	{
		if (dic == null)
		{
			return null;
		}
		object obj = dic.TryGetValue(key);
		if (obj != null)
		{
			return Convert.ToInt32(obj);
		}
		if (defaultValue.HasValue)
		{
			dic[key] = defaultValue;
			return defaultValue;
		}
		return null;
	}

	private int? GeneratorGlobalValue(GameEvent eve)
	{
		return (eve.gameState?.globalVariableStateInfo)?.GetVariableValue(key);
	}

	private List<string> GetDropDownPresetKeys()
	{
		if (type == TempValue.Type.Caster || type == TempValue.Type.Node)
		{
			return TempValue.defaultKeys;
		}
		if (type == TempValue.Type.GameGlobal)
		{
			return InGameGlobalVariableStateInfo.GetAllGlobalVariableSetups().ConvertAll((GlobalVariableSetup s) => s.id);
		}
		if (type == TempValue.Type.ApplicationGlobal)
		{
			return ApplicationGlobalVariableManager.GetAllGlobalVariableSetups().ConvertAll((GlobalVariableSetup s) => s.id);
		}
		return new List<string>();
	}
}
