using TMPro;
using UnityEngine;

public class PurchaseRecordCell : CellUI<PurchaseManager.PurchaseHistoryRecord>
{
	[SerializeField]
	private TextMeshProUGUI orderIdText;

	[SerializeField]
	private TextMeshProUGUI purchaseDateText;

	[SerializeField]
	private TextMeshProUGUI purchaseItemText;

	[SerializeField]
	private TextMeshProUGUI statusText;

	protected override void OnSetup(PurchaseManager.PurchaseHistoryRecord purchaseRecord)
	{
		orderIdText.text = purchaseRecord.orderId;
		purchaseDateText.text = purchaseRecord.purchaseDateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
		purchaseItemText.text = purchaseRecord.purchaseItem;
		statusText.text = purchaseRecord.orderStatus;
	}
}
