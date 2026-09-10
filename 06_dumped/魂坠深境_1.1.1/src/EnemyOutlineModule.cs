using UnityEngine;
using UnityEngine.UI;

public class EnemyOutlineModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image outlineImage;

	public override void Setup()
	{
		outlineImage.gameObject.SetActive(base.slotDisplay.item.HasSpell);
	}
}
