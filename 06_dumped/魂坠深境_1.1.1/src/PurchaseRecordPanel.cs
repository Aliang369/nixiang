using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class PurchaseRecordPanel : UIBase, IStackPanel
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private PurchaseRecordCell purchaseRecordCellPrefab;

	[SerializeField]
	private TextMeshProUGUI customerServiceHint;

	private CellUIPrefabPool<PurchaseRecordCell, PurchaseManager.PurchaseHistoryRecord> pool;

	private long lastOrderUpdateTime;

	protected override void Init()
	{
		backButton.onClick.AddListener(_OnBackButtonClicked);
		pool = GetPrefabPool<PurchaseRecordCell, PurchaseManager.PurchaseHistoryRecord>(purchaseRecordCellPrefab, purchaseRecordCellPrefab.transform.parent);
		UIBase.applicationState.purchaseManager.PreparePurchaseName();
	}

	protected override void UpdateDisplay()
	{
		FetchOrders();
		customerServiceHint.text = ScriptableObjectSingleton<TextDataPreset>.instance.GetOrderListMenuCustomerSupportHint();
	}

	private void FetchOrders()
	{
		if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastOrderUpdateTime >= 60000)
		{
			lastOrderUpdateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			UIBase.applicationState.purchaseManager.ListOrders(delegate
			{
				pool.DespawnAll();
				pool.Spawn(UIBase.applicationState.purchaseManager.orders);
			});
		}
	}

	private void _OnBackButtonClicked()
	{
		UIBase.fortressState.uiManager.HideUI(this);
	}
}
