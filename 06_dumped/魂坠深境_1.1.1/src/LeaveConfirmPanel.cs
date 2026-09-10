using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaveConfirmPanel : CellUI<LeaveConfirmPanel.Info>
{
	public class Info
	{
		public List<Item> items;

		public string text;

		public Action onConfirm;

		public Action onCancel;
	}

	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private GameObject sellDescriptionPanel;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private Toggle toggle;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	private Info info => base.setupData;

	protected override void Init()
	{
		GetPrefabPool(ref itemDisplayPool, itemDisplayPrefab);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		toggle.onValueChanged.AddListener(delegate(bool v)
		{
			CellUI<Info>.gameState.showConfirm = !v;
		});
	}

	protected override void OnSetup(Info _)
	{
		base.gameObject.SetActive(value: true);
		sellDescriptionPanel.SetActive(value: true);
		confirmButton.interactable = true;
		text.text = info.text;
		itemDisplayPool.Sync(info.items);
		toggle.SetIsOnWithoutNotify(!CellUI<Info>.gameState.showConfirm);
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	private void _OnConfirmButtonClicked()
	{
		info.onConfirm?.Invoke();
		Unset();
	}

	private void _OnCancelButtonClicked()
	{
		info.onCancel?.Invoke();
		Unset();
	}
}
