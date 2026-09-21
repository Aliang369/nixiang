using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class SkinSetModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image unlockImage;

	[SerializeField]
	private GameObject equipMark;

	public override void UpdateDisplay()
	{
		unlockImage.gameObject.SetActive(!UIDisplayModule.applicationState.itemSkinManager.IsItemSkinUnlocked(base.item.skin));
	}
}
