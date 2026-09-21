using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrystalDisplay : UIBase, IDisplayModulesController
{
	[SerializeField]
	public Image image;

	[SerializeField]
	private TextMeshProUGUI text;

	public void Setup(CrystalBonus bonus)
	{
		_setupData = bonus;
		base.gameObject.SetActive(value: true);
		text.text = $"{bonus.count}";
	}

	public void Unset()
	{
		_setupData = null;
		base.gameObject.SetActive(value: false);
	}
}
