using TMPro;
using UnityEngine;

public class NameModule : GlobalItemDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI text;

	public override void Setup()
	{
		text.text = base.globalItem.GlobalItemCellName;
	}
}
