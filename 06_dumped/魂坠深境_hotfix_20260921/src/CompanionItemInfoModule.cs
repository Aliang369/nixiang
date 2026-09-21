using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CompanionItemInfoModule : ItemDisplayBase.DisplayModule
{
	private class DisplayInfo
	{
		public int count;

		public int index;

		public static bool Equal(DisplayInfo first, DisplayInfo second)
		{
			if (first == null || second == null)
			{
				return false;
			}
			if (first.index == second.index)
			{
				return first.count == second.count;
			}
			return false;
		}
	}

	[SerializeField]
	private CompanionItemArea itemArea;

	[NonSerialized]
	private Canvas baseCanvas;

	[NonSerialized]
	private DisplayInfo displayInfo;

	[NonSerialized]
	private InputModule inputHandle;

	[NonSerialized]
	public bool selected;

	[NonSerialized]
	public bool over;

	[NonSerialized]
	private Sequence sequence;

	public override void Init()
	{
		baseCanvas = GetComponentInParent<Canvas>();
	}

	public override void Setup()
	{
		selected = false;
		inputHandle = base.slotDisplay.itemSlotInputModule;
		if (inputHandle != null)
		{
			InputModule inputModule = inputHandle;
			inputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(inputModule.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
		}
	}

	public override void Unset()
	{
		if (inputHandle != null)
		{
			InputModule itemSlotInputModule = base.slotDisplay.itemSlotInputModule;
			itemSlotInputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Remove(itemSlotInputModule.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
		}
	}

	public void OnSelect()
	{
		if (!selected)
		{
			itemArea.UnselectDisplay();
			selected = true;
			UIDisplayModule.gameState.UIManager.battleItemInfoPanel.Show(base.slotDisplay.item, PivotManager.PivotType.PlayerTablePivot);
		}
	}

	public void OnUnselect()
	{
		if (selected)
		{
			selected = false;
		}
	}

	public IEnumerator PlayTween_Add(Vector3? fromPosition)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
		Vector3 idlePosition = GetIdlePosition();
		if (!fromPosition.HasValue)
		{
			fromPosition = idlePosition;
		}
		base.gameObject.SetActive(value: true);
		rectTransform.transform.position = fromPosition.Value;
		sequence.Insert(0f, rectTransform.DOMove(idlePosition, playerItemAnimConfig.drawTime));
		sequence.Insert(0f, rectTransform.DOScale(playerItemAnimConfig.idleScale, playerItemAnimConfig.drawTime));
		sequence.Insert(0f, rectTransform.DORotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.drawTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_Spell()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		RectTransform rectTransform = base.transform as RectTransform;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.enemyItemAnimConfig;
		ItemInfoDisplayBase.Config itemInfoDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemInfoDisplayConfig;
		Vector3 startPosition = rectTransform.position;
		Vector3 endValue = Vector3.Lerp(startPosition, UIDisplayModule.gameState.UIManager.battleItemInfoPanel.enemyPivot.ToCanvasWorldPosition(baseCanvas), 1f - itemInfoDisplayConfig.showInfoOffsetFactor);
		base.slotDisplay.gameObject.SetActive(value: true);
		sequence.Append(rectTransform.DOMove(startPosition, 0f));
		sequence.Join(rectTransform.DOLocalRotate(Vector3.zero, 0f));
		sequence.Join(rectTransform.DOScale(Vector3.one, 0f));
		sequence.Append(rectTransform.DOMove(endValue, enemyItemAnimConfig.showInfoTime));
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.showInfoRotate, enemyItemAnimConfig.showInfoTime));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.showInfoScale, enemyItemAnimConfig.showInfoTime));
		sequence.AppendCallback(delegate
		{
			base.slotDisplay.gameObject.SetActive(value: false);
			rectTransform.position = startPosition;
			rectTransform.rotation = Quaternion.Euler(Vector3.zero);
			rectTransform.localScale = Vector3.one;
		});
		UIDisplayModule.gameState.UIManager.battleItemInfoPanel.ConfigTween_Spell(sequence, base.slotDisplay, base.slotDisplay.item, PivotManager.PivotType.CompanionPivot);
		sequence.AppendCallback(delegate
		{
			base.slotDisplay.gameObject.SetActive(value: true);
		});
		sequence.Append(rectTransform.DOMove(endValue, 0f));
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.showInfoRotate, 0f));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.showInfoScale, 0f));
		sequence.Append(rectTransform.DOMove(startPosition, enemyItemAnimConfig.showInfoTime));
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.idleRotation, enemyItemAnimConfig.showInfoTime));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.idleScale, enemyItemAnimConfig.showInfoTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			base.slotDisplay.SetDirty();
		});
		sequence.Play();
		yield return sequence.WaitForKill();
		base.slotDisplay.SetDirty();
	}

	public IEnumerator OnConvert()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RectTransform target = base.slotDisplay.transform as RectTransform;
		Vector3 idlePosition = GetIdlePosition();
		sequence.Append(target.DOMove(idlePosition, playerItemAnimConfig.drawTime));
		sequence.Join(target.DOScale(playerItemAnimConfig.idleScale, playerItemAnimConfig.drawTime));
		sequence.Join(target.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.drawTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public void OnShift()
	{
		if (RefreshAndCheckDisplayInfo() && sequence == null)
		{
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
			ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
			RectTransform target = base.slotDisplay.transform as RectTransform;
			Vector3 endValue = target.AnchoredToWorldPosition(itemArea.customLayoutGroup.GetPosition(target, base.slotDisplay.item.ownerSlotIndex ?? (-1), base.slotDisplay.item.owner.ItemsCount));
			base.gameObject.SetActive(value: true);
			sequence.Insert(0f, target.DOMove(endValue, playerItemAnimConfig.drawTime));
			sequence.Insert(0f, target.DOScale(playerItemAnimConfig.idleScale, playerItemAnimConfig.drawTime));
			sequence.Insert(0f, target.DORotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.drawTime));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}

	private Vector3 GetIdlePosition()
	{
		RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
		return rectTransform.AnchoredToWorldPosition(itemArea.customLayoutGroup.GetPosition(rectTransform, base.slotDisplay.item.ownerSlotIndex ?? (-1), base.slotDisplay.item.owner.ItemsCount));
	}

	private bool RefreshAndCheckDisplayInfo()
	{
		DisplayInfo second = new DisplayInfo
		{
			index = (base.slotDisplay.item.ownerSlotIndex ?? (-1)),
			count = base.slotDisplay.item.owner.ItemsCount
		};
		if (DisplayInfo.Equal(displayInfo, second))
		{
			return false;
		}
		displayInfo = second;
		return true;
	}

	private void _OnHandleInput(InputModuleBase inputModule)
	{
		if (inputModule.pointerClicked)
		{
			OnSelect();
		}
		if (inputModule.pointerEnter)
		{
			over = true;
		}
		if (inputModule.pointerExit)
		{
			over = false;
		}
	}
}
