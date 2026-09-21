using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.MatterCells;

public class CounterModule_UpgradeConfigPanel : MatterCell.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI numText;

	public override void UpdateDisplay()
	{
		int num = ((base.matterInfo.matterDef.matterType != MatterDef.MatterType.ObsessionDebris) ? 1 : ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUpgradeCount);
		int num2 = num;
		numText.text = $"{base.matterInfo.count.ToString().ColorText((base.matterInfo.count < num2) ? ScriptableObjectSingleton<VFXDataPreset>.instance.matterColor_Lack : ScriptableObjectSingleton<VFXDataPreset>.instance.matterColor_Normal)}<size=70%><color=#C2C2C2>/{num2}</color></size>";
	}
}
