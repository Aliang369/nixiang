using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.MatterCells;

public class CounterModule_Info_All : MatterCell.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI numText;

	public override void UpdateDisplay()
	{
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_material.FormatWith(UIDisplayModule.applicationState.matterManager.GetMatterCount(base.matterInfo.matterDef).ToString().ColorText((UIDisplayModule.applicationState.matterManager.GetMatterCount(base.matterInfo.matterDef) < base.matterInfo.count) ? ScriptableObjectSingleton<VFXDataPreset>.instance.matterColor_Lack : ScriptableObjectSingleton<VFXDataPreset>.instance.matterColor_Normal), base.matterInfo.count.ToString());
	}
}
