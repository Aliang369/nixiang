using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class EnemyRingModule : ItemDisplayBase.DisplayModule, IFrameUpdate
{
	[NonSerialized]
	private bool enable;

	[NonSerialized]
	private Image ringImage;

	void IFrameUpdate.FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy && !(ringImage == null) && enable && ringImage.transform.position != base.transform.position)
		{
			ringImage.transform.position = base.transform.position;
		}
	}

	public override void Setup()
	{
		enable = base.slotDisplay.item.GetDef().ringSprite != null;
		if (enable)
		{
			ringImage = UnityEngine.Object.Instantiate(UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.ringImage, UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.ringImagePivot);
			ringImage.gameObject.SetActive(value: true);
			ringImage.sprite = base.slotDisplay.item.GetDef().ringSprite;
			UIDisplayModule.gameState.battleState.enemy.bft.attachDisplay.TriggerEnemyRing(base.slotDisplay.item, show: true);
		}
	}

	public override void Unset()
	{
		if (enable)
		{
			UnityEngine.Object.DestroyImmediate(ringImage);
			if (UIDisplayModule.gameState.battleState.enemy != null)
			{
				UIDisplayModule.gameState.battleState.enemy.bft.attachDisplay.TriggerEnemyRing(base.slotDisplay.item, show: false);
			}
		}
		enable = false;
	}
}
