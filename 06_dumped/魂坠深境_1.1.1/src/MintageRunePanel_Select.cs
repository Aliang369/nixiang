using System;
using System.Collections.Generic;
using UnityEngine;

public class MintageRunePanel_Select : MintageRunePanel
{
	public class SetupInfo
	{
		public Func<bool> canBack;

		public Func<Rune, Item, bool> canSelect;

		public Action<(List<Rune>, Item)> onConfirm;

		public Action onCancel;

		public Action onUnavailable;

		public Action onFinish;

		public List<Rune> runes;

		public bool runeSlotLimit;

		public SetupInfo(List<Rune> runes)
		{
			this.runes = runes;
			onConfirm = (Action<(List<Rune>, Item)>)Delegate.Combine(onConfirm, (Action<(List<Rune>, Item)>)delegate
			{
				this.runes.Clear();
			});
		}

		public bool CanBack()
		{
			if (canBack == null)
			{
				return true;
			}
			bool flag = true;
			Delegate[] invocationList = canBack.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Func<bool> func = (Func<bool>)invocationList[i];
				flag &= func();
			}
			return flag;
		}
	}

	[SerializeField]
	private GameButton backButton;

	[NonSerialized]
	private SetupInfo info;

	[NonSerialized]
	private bool selectionResolved;

	protected override void Init()
	{
		base.Init();
		switchRuneButton.gameObject.SetActive(value: false);
		backButton.onClick.AddListener(_OnBackButtonClicked);
	}

	public void Setup(SetupInfo info)
	{
		this.info = info;
		selectionResolved = false;
		selected = null;
		itemArea.Setup(UIBase.gameState.playerState.DeckContainer);
		base.gameObject.SetActive(value: true);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		backButton.gameObject.SetActive(CanBack());
	}

	public bool HasSelectableItem()
	{
		foreach (Item allItem in UIBase.gameState.playerState.DeckContainer.GetAllItems())
		{
			if (CanMintage(allItem, out var _))
			{
				return true;
			}
		}
		return false;
	}

	protected override bool CanMintage(Item item, out MintageStateReason reason)
	{
		if (info.canSelect != null && !info.canSelect(null, item))
		{
			reason = MintageStateReason.ConstrainNotAccept;
			return false;
		}
		if (info.runeSlotLimit && !WithEnoughRuneSlot(item))
		{
			reason = MintageStateReason.WithoutEnoughRuneSlot;
			return false;
		}
		return base.CanMintage(item, out reason);
	}

	protected override void MintageRune(Item item, List<Rune> runes)
	{
		if (TryResolveSelection())
		{
			SetupInfo resolvedInfo = info;
			base.MintageRune(item, runes);
			resolvedInfo.onConfirm?.Invoke((runes, item));
			UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				resolvedInfo.onFinish?.Invoke();
			}));
		}
	}

	public override List<Rune> GetRunes()
	{
		return info.runes;
	}

	protected bool CanBack()
	{
		if (!info.CanBack())
		{
			return !HasSelectableItem();
		}
		return true;
	}

	protected override void ClosePanel()
	{
		SetupInfo closingInfo = info;
		base.gameObject.SetActive(value: false);
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			closingInfo.onFinish?.Invoke();
		}));
	}

	private void _OnBackButtonClicked()
	{
		if (TryResolveSelection())
		{
			info.onCancel?.Invoke();
			ClosePanel();
		}
	}

	private bool TryResolveSelection()
	{
		if (selectionResolved)
		{
			return false;
		}
		selectionResolved = true;
		return true;
	}

	private void ResolveUnavailable()
	{
		if (TryResolveSelection())
		{
			info.onUnavailable?.Invoke();
		}
	}
}
