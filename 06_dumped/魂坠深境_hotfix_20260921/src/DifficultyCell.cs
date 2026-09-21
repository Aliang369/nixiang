using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyCell : CellUI<DifficultyInfo>
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private TextMeshProUGUI text;

	protected override void OnSetup(DifficultyInfo setupData)
	{
		SetDifficulty(setupData);
	}

	public void SetDifficulty(DifficultyInfo difficultyInfo)
	{
		if (image != null)
		{
			image.sprite = ((difficultyInfo != null) ? difficultyInfo.icon : null);
			image.gameObject.SetActive(difficultyInfo?.icon != null);
		}
		if (text != null)
		{
			text.text = ((difficultyInfo != null) ? difficultyInfo.difficultyName : "");
		}
	}
}
