using System;
using DG.Tweening;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class StoreBuyStateModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private GoldStoreEventPanel panel;

	[Header("Tween")]
	[SerializeField]
	private RectTransform tweenPivot;

	[SerializeField]
	private GameObject outline;

	[NonSerialized]
	public StoreEvent.PricedItem priceItem;

	[NonSerialized]
	private bool selected;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private float tweenTime;

	public RectTransform UncollectedTagPivot => tweenPivot;

	public override void Init()
	{
		InputModule itemSlotInputModule = base.slotDisplay.itemSlotInputModule;
		itemSlotInputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(itemSlotInputModule.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
	}

	public override void Setup()
	{
		tweenTime = 0.4f;
		selected = false;
		tweenPivot.localPosition = Vector3.zero;
		tweenPivot.localScale = Vector3.one;
	}

	public override void UpdateDisplay()
	{
		if (panel.buyPanel.selected == base.slotDisplay)
		{
			Select();
		}
		else
		{
			UnSelect();
		}
	}

	private void Select()
	{
		if (!selected)
		{
			sequence?.Kill();
			outline.SetActive(value: true);
			sequence = DOTween.Sequence();
			sequence.Append(tweenPivot.DOLocalMoveY(20f, tweenTime));
			sequence.Join(tweenPivot.DOScale(1.1f, tweenTime));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			selected = true;
		}
	}

	private void UnSelect()
	{
		if (selected)
		{
			sequence?.Kill();
			outline.SetActive(value: false);
			sequence = DOTween.Sequence();
			sequence.Append(tweenPivot.DOLocalMoveY(0f, tweenTime));
			sequence.Join(tweenPivot.DOScale(1f, tweenTime));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			selected = false;
		}
	}

	private void _OnHandleInput(InputModuleBase module)
	{
		if (module.pointerDown)
		{
			panel.buyPanel.SelectItem(base.slotDisplay);
		}
	}
}
