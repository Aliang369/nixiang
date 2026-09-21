using Common.UI;
using ContainerOfSoul.Display.Fortress.Purchase;
using TMPro;
using UnityEngine;

public class ItemSkinPurchaseCellModule : MonoDisplayModule
{
	[SerializeField]
	private TextMeshProUGUI skinGotHintText;

	private PurchaseCell purchaseCell => parent as PurchaseCell;

	private PurchaseInfo purchase => purchaseCell?.setupData;

	public override void UpdateDisplay()
	{
		skinGotHintText.gameObject.SetActive(value: false);
		purchaseCell.ResetGlobalItemCountAreaAnchoredPosition();
		if (purchase is ShuffleSkinBundlePurchase shuffleSkinBundlePurchase)
		{
			skinGotHintText.gameObject.SetActive(value: true);
			skinGotHintText.text = shuffleSkinBundlePurchase.CurrentSkinSetBatch.GetSkinUnlockProgressText();
			purchaseCell.SetGlobalItemCountAreaAnchoredPosition(new Vector2(-45f, 20f));
		}
	}
}
