using UnityEngine;

public class TreasureModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private GameObject treasureDisplay;

	public override void Setup()
	{
		treasureDisplay.SetActive(base.slotDisplay.item.treasureValue != 0);
	}
}
