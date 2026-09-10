using TMPro;
using UnityEngine;

public class SeasonPassBundlePurchaseCell : CellUI<PurchaseInfo>
{
	public GameButton buyButton;

	public TextMeshProUGUI buyButtonText;

	public GlobalItemDisplay getNowGlobalItemDisplayPrefab;

	public RectTransform getNowGlobalItemDisplayParent;

	public GlobalItemDisplay getLaterGlobalItemDisplayPrefab;

	public RectTransform getLaterGlobalItemDisplayParent;

	public TextMeshProUGUI highSeasonPassHintText;
}
