using TMPro;
using UnityEngine;

public class EnchantNameModule : EnchantDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	public override void Setup()
	{
		if (base.display.DisplayNotDiscovered)
		{
			nameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_undiscovered;
		}
		else
		{
			nameText.text = base.enchant.def.NameWithColor;
		}
	}
}
