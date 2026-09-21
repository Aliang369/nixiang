using System;
using Common.UI;
using TMPro;
using UnityEngine;

public abstract class ItemSelectPanel : UIBase
{
	protected enum State
	{
		Hide,
		Hint,
		Confirm
	}

	[SerializeField]
	protected ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	protected RectTransform itemDisplayParent;

	[SerializeField]
	protected TextMeshProUGUI confirmText;

	[SerializeField]
	protected GameButton confirmButton;

	[SerializeField]
	protected GameButton cancelButton;

	[NonSerialized]
	protected TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	public SelectItemInfo info;

	[NonSerialized]
	public CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	[NonSerialized]
	protected State state;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		confirmButton.interactable = false;
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, confirmText, confirmText.rectTransform);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinish;
		InitPools();
		state = State.Hide;
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinish;
	}

	protected override void UpdateDisplay()
	{
		confirmText.text = info.panelHintGetter?.Invoke() ?? "";
		textLinkObjectDisplayer.SetGenerator(info.generator);
		cancelButton.gameObject.SetActive(CanCancel());
		confirmButton.interactable = CanConfirm();
	}

	public virtual void Setup(SelectItemInfo info)
	{
		this.info = info;
		base.gameObject.SetActive(value: true);
		ToHint();
		itemDisplayPool.DespawnAll();
		int num = 0;
		foreach (Item item in info.pool.OrEmptyIfNull())
		{
			ItemDisplayBase itemDisplayBase = SpawnDisplay(item);
			SetupUncollectedTag(itemDisplayBase, item);
			itemDisplayBase.transform.SetSiblingIndex(num++);
		}
		if (CanConfirm())
		{
			ToConfirm();
		}
		SetDirty();
	}

	public virtual void Unset()
	{
		info = null;
		itemDisplayPool.DespawnAll();
		ToHide();
	}

	public void TriggerItemSelect(Item item)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Selected(item))
		{
			UnselectItem(item);
		}
		else if (CanSelect(item))
		{
			SelectItem(item);
		}
		else
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_nagative);
		}
		SetDirty();
	}

	protected void ToConfirm()
	{
		if (state == State.Hint)
		{
			state = State.Confirm;
			SetDirty();
		}
	}

	protected void ToHint()
	{
		if (state == State.Hide || state == State.Confirm)
		{
			state = State.Hint;
			base.gameObject.SetActive(value: true);
			SetDirty();
		}
	}

	protected void ToHide()
	{
		if (state != State.Hide)
		{
			state = State.Hide;
			base.gameObject.SetActive(value: false);
			SetDirty();
		}
	}

	protected bool CanSelect(Item item)
	{
		return info.canSelect(item);
	}

	protected bool Selected(Item item)
	{
		return info.output.Contains(item);
	}

	protected virtual void SelectItem(Item item)
	{
		if (info.singleSelect && info.output.Count > 0)
		{
			info.output.Clear();
		}
		info.output.Add(item);
		if (CanConfirm())
		{
			ToConfirm();
		}
		else
		{
			ToHint();
		}
	}

	public virtual void ShowItemInfo(Item item)
	{
		UIBase.gameState.UIManager.battleItemInfoPanel.Show(item, PivotManager.PivotType.PlayerTablePivot);
	}

	protected void UnselectItem(Item item)
	{
		info.output.Remove(item);
		HideItemInfo(item);
		if (CanConfirm())
		{
			ToConfirm();
		}
		else
		{
			ToHint();
		}
	}

	protected virtual void HideItemInfo(Item item)
	{
		UIBase.gameState.UIManager.battleItemInfoPanel.Hide();
	}

	protected virtual void InitPools()
	{
		GetPrefabPool(ref itemDisplayPool, itemDisplayPrefab);
	}

	protected bool CanCancel()
	{
		return info.CanCancel();
	}

	protected virtual bool CanConfirm()
	{
		return info.CanConfirm();
	}

	private ItemDisplayBase SpawnDisplay(Item item)
	{
		return itemDisplayPool.Spawn(item);
	}

	private void SetupUncollectedTag(ItemDisplayBase display, Item item)
	{
		RewardUncollectedTag rewardUncollectedTag = RewardUncollectedTag.AttachTo(display, RewardUncollectedTagLayout.SelectionCard);
		SelectItemInfo selectItemInfo = info;
		if (selectItemInfo != null && selectItemInfo.showUncollectedStatus)
		{
			rewardUncollectedTag?.Setup(item?.GetDef(), UIBase.applicationState);
		}
		else
		{
			rewardUncollectedTag?.Hide();
		}
	}

	private void _OnConfirmButtonClicked()
	{
		info.onConfirm?.Invoke();
		Unset();
	}

	private void _OnCancelButtonClicked()
	{
		if (CanCancel())
		{
			info.onCancel?.Invoke();
			Unset();
		}
	}

	private void _OnActionFrameFinish()
	{
		SetDirty();
	}

	private void _OnEmptyClicked()
	{
		if (info != null)
		{
			SetDirty();
		}
	}
}
