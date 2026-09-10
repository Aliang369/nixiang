using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleSelectItemModules : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image darkImage;

	[SerializeField]
	protected ItemSelectPanel panel;

	[NonSerialized]
	private bool canSelect;

	[NonSerialized]
	private bool selected;

	[NonSerialized]
	private Sequence sequence;

	public override void Init()
	{
		InputModule itemSlotInputModule = base.slotDisplay.itemSlotInputModule;
		itemSlotInputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(itemSlotInputModule.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
	}

	public override void Setup()
	{
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RectTransform obj = base.slotDisplay.transform as RectTransform;
		obj.rotation = Quaternion.Euler(playerItemAnimConfig.idleRotation);
		obj.localScale = Vector3.one;
		selected = false;
	}

	public override void UpdateDisplay()
	{
		base.slotDisplay.RectTransform().anchoredPosition = GetIdlePosition();
		bool flag = panel.info.output.Contains(base.slotDisplay.item);
		canSelect = panel.info.canSelect(base.slotDisplay.item);
		darkImage.gameObject.SetActive(!canSelect && !flag);
		if (flag)
		{
			ToSelected();
		}
		else
		{
			ToUnselected();
		}
	}

	private void ToUnselected()
	{
		if (selected)
		{
			selected = false;
			ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
			RectTransform target = base.slotDisplay.transform as RectTransform;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
			sequence.Append(target.DORotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.unselectTime));
			sequence.Join(target.DOScale(Vector3.one, playerItemAnimConfig.unselectTime));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}

	private void ToSelected()
	{
		if (!selected)
		{
			selected = true;
			ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
			RectTransform target = base.slotDisplay.transform as RectTransform;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
			sequence.Append(target.DORotate(Vector3.zero, playerItemAnimConfig.selectTime1));
			sequence.Join(target.DOScale(playerItemAnimConfig.selectAnimScale1, playerItemAnimConfig.selectTime1));
			sequence.Append(target.DOScale(playerItemAnimConfig.selectAnimScale2, playerItemAnimConfig.selectTime2));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}

	private void _OnHandleInput(InputModuleBase inputModule)
	{
		BattleTableItemSelectPanel battleTableItemSelectPanel = panel as BattleTableItemSelectPanel;
		int pointerId = inputModule.eventData?.pointerId ?? int.MinValue;
		if (battleTableItemSelectPanel != null)
		{
			if (inputModule.pointerDown)
			{
				battleTableItemSelectPanel.BeginSlideSelection(base.slotDisplay.item, pointerId);
			}
			if (inputModule.over)
			{
				battleTableItemSelectPanel.ContinueSlideSelection(base.slotDisplay.item, pointerId, inputModule.eventData?.dragging ?? false);
			}
		}
		if (inputModule.pointerClicked && ((object)battleTableItemSelectPanel == null || !battleTableItemSelectPanel.IsSlideSelecting(pointerId)))
		{
			panel.ShowItemInfo(base.slotDisplay.item);
			panel.TriggerItemSelect(base.slotDisplay.item);
		}
		if (battleTableItemSelectPanel != null && (inputModule.endDrag || inputModule.pointerUp))
		{
			battleTableItemSelectPanel.EndSlideSelection(pointerId);
		}
	}

	protected virtual Vector2 GetIdlePosition()
	{
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
		float num = playerItemAnimConfig.itemOffset + playerItemAnimConfig.itemIdleSpace + 0.001f;
		float width = (base.slotDisplay.transform.parent as RectTransform).GetWidth();
		int num2 = (int)(width / num);
		int num3 = panel.itemDisplayPool.IndexOf(base.slotDisplay);
		int activeCount = panel.itemDisplayPool.activeCount;
		if (activeCount <= num2)
		{
			return new Vector2(((float)num3 - (float)activeCount / 2f + 0.5f) * num, 0f);
		}
		float num4 = width / (float)activeCount;
		return new Vector2(((float)num3 - (float)activeCount / 2f + 0.5f) * num4, 0f);
	}
}
