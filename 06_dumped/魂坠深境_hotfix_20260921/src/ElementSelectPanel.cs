using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class ElementSelectPanel<T> : UIBase, ISelectablePanel where T : class
{
	public class SelectInfo
	{
		public List<T> pool;

		public List<T> output;

		public Func<bool> canCancel;

		public Func<bool> canConfirm;

		public Func<T, bool> canSelect;

		public Func<bool> shouldCancel;

		public Func<string> panelHintGetter;

		public Action onCancel;

		public Action onConfirm;

		public TextDataGenerator generator;

		public BattleFieldTransform selectorBft;

		public bool singleSelect;

		public bool CanSelect(T element)
		{
			if (canSelect == null)
			{
				return true;
			}
			return canSelect(element);
		}

		public bool CanConfirm()
		{
			bool flag = true;
			if (canConfirm != null)
			{
				Delegate[] invocationList = canConfirm.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					Func<bool> func = (Func<bool>)invocationList[i];
					flag &= func();
				}
			}
			return flag;
		}

		public bool CanCancel()
		{
			bool flag = true;
			if (canCancel != null)
			{
				Delegate[] invocationList = canCancel.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					Func<bool> func = (Func<bool>)invocationList[i];
					flag &= func();
				}
			}
			return flag;
		}

		public static SelectInfo GenerateNodeSelectInfo(GameEvent eve, List<T> pool, CheckNode shouldCancel, CheckNode selectable, CheckNode confirmable, Action onConfirm, Action onCancel, Func<bool> canCancel, bool isSingleSelect)
		{
			return new SelectInfo
			{
				pool = pool,
				singleSelect = isSingleSelect,
				shouldCancel = ShouldCancel,
				canConfirm = CanConfirm,
				canSelect = CanSelect,
				onConfirm = onConfirm,
				onCancel = onCancel,
				canCancel = canCancel,
				output = new List<T>()
			};
			bool CanConfirm()
			{
				if (confirmable == null)
				{
					return true;
				}
				return confirmable.Check(eve) == true;
			}
			bool CanSelect(T item)
			{
				if (selectable != null)
				{
					T eveObj = eve.GetEveObj<T>(GameEvent.EveObjType.Target);
					eve.SetEveObj(GameEvent.EveObjType.Target, item);
					bool result = selectable.Check(eve) == true;
					eve.SetEveObj(GameEvent.EveObjType.Target, eveObj);
					return result;
				}
				return true;
			}
			bool ShouldCancel()
			{
				if (pool == null || pool.Count == 0)
				{
					return true;
				}
				if (shouldCancel != null && shouldCancel.Check(eve) == true)
				{
					return true;
				}
				return false;
			}
		}
	}

	[SerializeField]
	protected TextMeshProUGUI text;

	[SerializeField]
	protected GameButton confirmButton;

	[SerializeField]
	protected GameButton cancelButton;

	[NonSerialized]
	public SelectInfo info;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		confirmButton.interactable = false;
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinish;
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, text, base.rectTransform);
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinish;
	}

	protected override void UpdateDisplay()
	{
		text.text = info.panelHintGetter?.Invoke() ?? "";
		textLinkObjectDisplayer.SetGenerator(info.generator);
		cancelButton.gameObject.SetActive(CanCancel());
		confirmButton.interactable = CanConfirm();
	}

	public void Setup(SelectInfo info)
	{
		base.gameObject.SetActive(value: true);
		this.info = info;
		DespawnAll();
		foreach (T item in info.pool)
		{
			Spawn(item);
		}
		SetDirty();
	}

	public virtual void Unset()
	{
		base.gameObject.SetActive(value: false);
	}

	private bool CanSelect(T item)
	{
		return info.CanSelect(item);
	}

	private bool Selected(T item)
	{
		return info.output.Contains(item);
	}

	private void Select(T item)
	{
		if (info.singleSelect && info.output.Count > 0)
		{
			info.output.Clear();
		}
		info.output.Add(item);
		OnSelect(item);
		SetDirty();
	}

	private void UnselectItem(T item)
	{
		info.output.Remove(item);
		OnUnselect(item);
		SetDirty();
	}

	private bool CanCancel()
	{
		return info.CanCancel();
	}

	private bool CanConfirm()
	{
		return info.CanConfirm();
	}

	protected abstract T GetElement(SelectableModuleBase selectableModule);

	protected abstract void Spawn(T element);

	protected abstract void DespawnAll();

	protected virtual void OnSelect(T item)
	{
	}

	protected virtual void OnUnselect(T item)
	{
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Select(GetElement(module));
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		UnselectItem(GetElement(module));
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		return Selected(GetElement(module));
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		return CanSelect(GetElement(module));
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
