using UnityEngine;
using UnityEngine.UI;

public class CombineGemSlotDisplay : UIBase
{
	[SerializeField]
	public SkillGemDisplay skillGemDisplay;

	[SerializeField]
	public Image outline;

	[SerializeField]
	public GameButton button;

	protected override void Init()
	{
		InitUI(skillGemDisplay);
		button.onClick.AddListener(delegate
		{
			(parent as GemCombinePanel)?.OnCombineSlotClicked(this);
		});
	}

	protected override void UpdateDisplay()
	{
		outline.gameObject.SetActive((parent as GemCombinePanel).current == this);
	}
}
