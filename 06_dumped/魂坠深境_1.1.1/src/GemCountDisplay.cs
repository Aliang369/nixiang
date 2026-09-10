using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GemCountDisplay : UIBase, IDisplayModulesController
{
	[SerializeField]
	public Image image;

	[SerializeField]
	private TextMeshProUGUI text;

	public void Setup(GemBonus bonus)
	{
		_setupData = bonus;
		base.gameObject.SetActive(value: true);
		text.text = $"{bonus.totalCount}";
	}

	public void Unset()
	{
		_setupData = null;
		base.gameObject.SetActive(value: false);
	}
}
