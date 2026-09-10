using System;
using TMPro;
using UnityEngine;

public class ItemSpellConfirmPanel : UIBase
{
	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[NonSerialized]
	private Action onConfirm;

	[NonSerialized]
	private Action onCancel;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
	}

	public void Setup(Action onConfirm, Action onCancel, string description)
	{
		this.onCancel = onCancel;
		this.onConfirm = onConfirm;
		descriptionText.text = description;
		base.gameObject.SetActive(value: true);
	}

	private void _OnConfirmButtonClicked()
	{
		base.gameObject.SetActive(value: false);
		onConfirm?.Invoke();
		onCancel = null;
		onConfirm = null;
	}

	private void _OnCancelButtonClicked()
	{
		base.gameObject.SetActive(value: false);
		onCancel?.Invoke();
		onCancel = null;
		onConfirm = null;
	}
}
