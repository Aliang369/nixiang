using System.Collections.Generic;

public abstract class SetTempKey : Effect, IOutGameEffectNode, IEffectCastNode, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public TempValue.Type type;

	public string key;

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

	public bool IsCustomKey()
	{
		return !GetDropDownPresetKeys().Contains(key);
	}
}
