using TMPro;
using UnityEngine;

public class RuneNameModule : RuneDisplay.DisplayModule
{
	[SerializeField]
	protected TextMeshProUGUI nameText;

	public override void Setup()
	{
		if (base.display.DisplayNotDiscovered)
		{
			nameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_undiscovered;
			return;
		}
		TextMeshProUGUI textMeshProUGUI = nameText;
		string text = (nameText.text = base.rune.GetNameWithValue());
		textMeshProUGUI.text = text;
	}
}
