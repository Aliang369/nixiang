using TMPro;
using UnityEngine;

namespace Script.GameScenes.Fortress.MatterCells;

public class CounterModule_Info_Required : MatterCell.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI numText;

	public override void UpdateDisplay()
	{
		if (base.matterInfo.matterDef.matterType == MatterDef.MatterType.ObsessionDebris)
		{
			int required = base.matterInfo.GetAttachedPlayerCharacterState(Bootstrap.applicationState)?.currentObsessionDebrisRequiredCount ?? ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUpgradeCount;
			numText.text = GetText(base.matterInfo.count, required);
		}
		else
		{
			numText.text = base.matterInfo.count.ToString();
		}
	}

	private string GetText(int info, int required)
	{
		return $"{info.ToString().ColorText((info < required) ? ScriptableObjectSingleton<VFXDataPreset>.instance.matterColor_Lack : ScriptableObjectSingleton<VFXDataPreset>.instance.matterColor_Normal)}/{required}";
	}
}
