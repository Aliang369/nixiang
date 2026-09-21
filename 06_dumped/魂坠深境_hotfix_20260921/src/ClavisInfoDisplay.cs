using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClavisInfoDisplay : CellUI<ClavisDef>
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Image icon;

	protected override void OnSetup(ClavisDef dailyContract)
	{
		base.gameObject.SetActive(value: true);
		nameText.text = dailyContract.NameWithColor;
		descriptionText.text = dailyContract.storyDescription;
		icon.sprite = dailyContract.icon;
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}
}
