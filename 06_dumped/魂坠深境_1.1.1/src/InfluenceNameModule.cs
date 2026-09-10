using TMPro;
using UnityEngine;

public class InfluenceNameModule : InfluenceDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI influenceNameText;

	public override void Setup()
	{
		if (base.display.DisplayNotDiscovered)
		{
			influenceNameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_undiscovered;
		}
		else
		{
			influenceNameText.text = base.influence.def.GetNameWithQualityColor();
		}
	}
}
