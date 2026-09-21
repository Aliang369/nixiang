using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MainItemInfoDisplayModule_BattleState : MainItemInfoDisplayBase.DisplayModule
{
	[NonSerialized]
	private Sequence sequence;

	public IEnumerator PlayTween(ItemInfoDisplayBase.AnimSetupInfo setupInfo)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		AnimTween(setupInfo);
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public void ConfigSequence_Insert(Sequence sequence, ItemDisplayBase itemDisplay, Item source, Vector3 endPos)
	{
		this.sequence?.Kill();
		this.sequence = sequence;
		RectTransform rectTransform = base.mainItemInfoDisplay.RectTransform();
		ItemInfoDisplayBase.Config config = ScriptableObjectSingleton<VFXDataPreset>.instance.itemInfoDisplayConfig;
		Vector3 showPosition = UIDisplayModule.gameState.UIManager.battleItemInfoPanel.enemyPivot.GetMainCanvasWorldPosition(UIDisplayModule.gameState) ?? Vector3.zero;
		float num = 0f;
		base.mainItemInfoDisplay.gameObject.SetActive(value: false);
		Vector3 startPos = ((source == null || source.owner == null || source.OwnerBft == null) ? showPosition : (source.GetCanvasWorldPosition(UIDisplayModule.gameState, CanvasType.MainCanvas) ?? showPosition));
		sequence.InsertCallback(num, delegate
		{
			base.mainItemInfoDisplay.gameObject.SetActive(value: true);
			base.mainItemInfoDisplay.SetDirty();
			rectTransform.position = startPos;
			rectTransform.rotation = Quaternion.Euler(config.enemyShowInfoRotation1);
			rectTransform.localScale = config.enemyShowInfoScale1;
		});
		sequence.Insert(num, rectTransform.DOMove(showPosition, config.enemyShowInfoShowTime));
		sequence.Insert(num, rectTransform.DORotate(config.enemyShowInfoRotation2, config.enemyShowInfoShowTime));
		sequence.Insert(num, rectTransform.DOScale(config.enemyShowInfoScale2, config.enemyShowInfoShowTime));
		num += config.enemyShowInfoShowTime;
		num += config.enemyShowInfoStayTime;
		Canvas componentInParent = itemDisplay.GetComponentInParent<Canvas>();
		Canvas componentInParent2 = GetComponentInParent<Canvas>();
		endPos = Vector3.Lerp(showPosition, endPos.WorldPositionFromCanvasToCanvas(componentInParent, componentInParent2), config.showInfoOffsetFactor);
		sequence.InsertCallback(num, delegate
		{
			rectTransform.position = showPosition;
			rectTransform.rotation = Quaternion.Euler(config.enemyShowInfoRotation2);
			rectTransform.localScale = config.enemyShowInfoScale2;
		});
		sequence.Insert(num, rectTransform.DOMove(endPos, config.enemyShowInfoHideTime));
		sequence.Insert(num, rectTransform.DORotate(config.enemyShowInfoRotation1, config.enemyShowInfoHideTime));
		sequence.Insert(num, rectTransform.DOScale(config.enemyShowInfoScale1, config.enemyShowInfoHideTime));
		num += config.enemyShowInfoHideTime;
		sequence.InsertCallback(num, delegate
		{
			base.mainItemInfoDisplay.itemInfoDisplay.Unset();
			base.mainItemInfoDisplay.gameObject.SetActive(value: false);
		});
	}

	public void ConfigSequence_Spell(Sequence sequence, ItemDisplayBase itemDisplay, Item source, PivotManager.PivotType type)
	{
		this.sequence?.Kill();
		this.sequence = sequence;
		RectTransform rectTransform = base.mainItemInfoDisplay.RectTransform();
		ItemInfoDisplayBase.Config config = ScriptableObjectSingleton<VFXDataPreset>.instance.itemInfoDisplayConfig;
		Vector3 showPosition = ItemPosition.GetCanvasWorldPosition(UIDisplayModule.gameState, type, CanvasType.MainCanvas) ?? Vector3.zero;
		float num = 0f;
		Canvas componentInParent = GetComponentInParent<Canvas>();
		Vector3 startPos = ((source == null || source.owner == null || source.OwnerBft == null) ? showPosition : (source.GetPivot(UIDisplayModule.gameState)?.ToCanvasWorldPosition(componentInParent) ?? showPosition));
		startPos.z = 0f;
		base.mainItemInfoDisplay.gameObject.SetActive(value: false);
		sequence.InsertCallback(num, delegate
		{
			base.mainItemInfoDisplay.gameObject.SetActive(value: true);
			base.mainItemInfoDisplay.SetDirty();
			rectTransform.position = startPos;
			rectTransform.rotation = Quaternion.Euler(config.enemyShowInfoRotation1);
			rectTransform.localScale = config.enemyShowInfoScale1;
		});
		sequence.Insert(num, rectTransform.DOMove(showPosition, config.enemyShowInfoShowTime));
		sequence.Insert(num, rectTransform.DORotate(config.enemyShowInfoRotation2, config.enemyShowInfoShowTime));
		sequence.Insert(num, rectTransform.DOScale(config.enemyShowInfoScale2, config.enemyShowInfoShowTime));
		num += config.enemyShowInfoShowTime;
		num += config.enemyShowInfoStayTime;
		Vector3 endValue = Vector3.Lerp(showPosition, itemDisplay.item.GetPivot(UIDisplayModule.gameState)?.ToCanvasWorldPosition(componentInParent) ?? showPosition, config.showInfoOffsetFactor);
		endValue.z = 0f;
		sequence.InsertCallback(num, delegate
		{
			rectTransform.position = showPosition;
			rectTransform.rotation = Quaternion.Euler(config.enemyShowInfoRotation2);
			rectTransform.localScale = config.enemyShowInfoScale2;
		});
		sequence.Insert(num, rectTransform.DOMove(endValue, config.enemyShowInfoHideTime));
		sequence.Insert(num, rectTransform.DORotate(config.enemyShowInfoRotation1, config.enemyShowInfoHideTime));
		sequence.Insert(num, rectTransform.DOScale(config.enemyShowInfoScale1, config.enemyShowInfoHideTime));
		num += config.enemyShowInfoHideTime;
		sequence.InsertCallback(num, delegate
		{
			base.mainItemInfoDisplay.gameObject.SetActive(value: false);
		});
	}

	private void AnimTween(ItemInfoDisplayBase.AnimSetupInfo setupInfo)
	{
		RectTransform rectTransform = base.transform.RectTransform();
		ItemInfoDisplayBase.Config itemInfoDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemInfoDisplayConfig;
		base.gameObject.SetActive(value: true);
		if ((setupInfo.animType == ItemInfoDisplayBase.AnimSetupInfo.AnimType.Show || setupInfo.animType == ItemInfoDisplayBase.AnimSetupInfo.AnimType.Stay) && setupInfo.startShowPosition.HasValue)
		{
			Vector3 value = setupInfo.startShowPosition.Value;
			value.z = 0f;
			rectTransform.position = value;
			if (setupInfo.flip)
			{
				rectTransform.rotation = Quaternion.Euler(itemInfoDisplayConfig.rotateAnimStartShowRotation);
				rectTransform.localScale = itemInfoDisplayConfig.rotateAnimStartShowScale;
			}
			else
			{
				rectTransform.localScale = Vector3.one;
				rectTransform.rotation = Quaternion.Euler(Vector3.zero);
			}
			sequence.Append(rectTransform.DOMove(setupInfo.stayPosition, itemInfoDisplayConfig.enemyShowInfoShowTime));
			sequence.Join(rectTransform.DORotate(Vector3.zero, itemInfoDisplayConfig.enemyShowInfoShowTime));
			sequence.Join(rectTransform.DOScale(Vector3.one, itemInfoDisplayConfig.enemyShowInfoShowTime));
		}
		else
		{
			Vector3 stayPosition = setupInfo.stayPosition;
			stayPosition.z = 0f;
			rectTransform.position = stayPosition;
			rectTransform.localScale = Vector3.one;
			rectTransform.rotation = Quaternion.Euler(Vector3.zero);
		}
		if (setupInfo.animType == ItemInfoDisplayBase.AnimSetupInfo.AnimType.Stay)
		{
			sequence.AppendInterval(itemInfoDisplayConfig.enemyShowInfoStayTime);
		}
		if (setupInfo.animType == ItemInfoDisplayBase.AnimSetupInfo.AnimType.Hide || setupInfo.animType == ItemInfoDisplayBase.AnimSetupInfo.AnimType.Stay)
		{
			Vector3 hidePosition = setupInfo.hidePosition;
			hidePosition.z = 0f;
			sequence.Append(rectTransform.DOMove(hidePosition, itemInfoDisplayConfig.enemyShowInfoShowTime));
			if (setupInfo.flip)
			{
				sequence.Join(rectTransform.DORotate(itemInfoDisplayConfig.rotateAnimStartShowRotation, itemInfoDisplayConfig.enemyShowInfoShowTime));
				sequence.Join(rectTransform.DOScale(itemInfoDisplayConfig.rotateAnimStartShowScale, itemInfoDisplayConfig.enemyShowInfoShowTime));
			}
			else
			{
				sequence.Join(rectTransform.DORotate(Vector3.zero, itemInfoDisplayConfig.enemyShowInfoShowTime));
				sequence.Join(rectTransform.DOScale(Vector3.one, itemInfoDisplayConfig.enemyShowInfoShowTime));
			}
			sequence.AppendCallback(delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}
}
