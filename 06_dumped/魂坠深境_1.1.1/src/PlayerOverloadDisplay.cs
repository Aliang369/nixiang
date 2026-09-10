using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOverloadDisplay : UIBase, IFrameUpdate
{
	[Serializable]
	public class Config
	{
		[SerializeField]
		public Color stayColor;

		[SerializeField]
		public Color overFlowColor;

		[SerializeField]
		public float shiftTime;
	}

	public class DisplayInfo
	{
		public int costCount;

		public int max;

		public int overload;

		public Item selected;
	}

	[SerializeField]
	private Image areaImage;

	[SerializeField]
	private Image markImage;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private IndexDisplay indexTextPrefab;

	[SerializeField]
	private RectTransform indexTextParent;

	[SerializeField]
	private Image markEffect;

	[SerializeField]
	private Image indexEffectPrefab;

	[SerializeField]
	private RectTransform indexEffectParent;

	[NonSerialized]
	public DisplayInfo displayInfo;

	[NonSerialized]
	private PrefabPool<Image> indexEffectPool;

	[NonSerialized]
	private CellUIPrefabPool<IndexDisplay, ItemDisplayBase> indexTextPool;

	[NonSerialized]
	private Sequence sequence_Fade;

	[NonSerialized]
	private Sequence sequence_MarkEffect;

	[NonSerialized]
	private Sequence sequence_Shift;

	[NonSerialized]
	private bool showing;

	[NonSerialized]
	private bool showingMark;

	private bool ShouldShow
	{
		get
		{
			if (UIBase.gameState.battleState.isInBattle)
			{
				if (UIBase.gameState.battleState.turnPhase != TurnPhase.PlayerControlPhase)
				{
					if (UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.OverflowConsumeOnTurnStart))
					{
						return UIBase.gameState.battleState.turnPhase == TurnPhase.PlayerTurnStartPhase;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	void IFrameUpdate.FrameUpdate()
	{
		for (int i = 0; i < indexTextPool.activeCount; i++)
		{
			indexTextPool.GetActiveElement(i).RefreshIndexDisplay();
		}
	}

	protected override void Init()
	{
		showing = false;
		showingMark = false;
		displayInfo = new DisplayInfo();
		base.gameObject.SetActive(value: false);
		indexTextPool = GetPrefabPool<IndexDisplay, ItemDisplayBase>(indexTextPrefab, indexTextParent);
		indexEffectPool = new PrefabPool<Image>(indexEffectPrefab, indexEffectParent);
		markEffect.gameObject.SetActive(value: false);
		indexEffectPrefab.gameObject.SetActive(value: false);
		AddListener(UIEvent.OnPanelShow, _OnPanelShow);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.TriggerTurnPhaseChange, _OnTurnPhaseChange);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterPlayerTurnEndWaste, _OnAfterPlayerTurnEndWaste);
	}

	protected override void UpdateDisplay()
	{
		if (base.gameObject.activeInHierarchy && !UIBase.gameState.battleState.isConsumingOverflowItems)
		{
			int finalFixedSlotCount = UIBase.gameState.playerState.TableContainer.GetFinalFixedSlotCount();
			int num = ((UIBase.gameState.playerState.playerSpellInfo.castState == SpellCastInfo.State.Preparing) ? UIBase.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo.addCostCount : 0);
			int itemsCount = UIBase.gameState.playerState.TableContainer.ItemsCount;
			if (displayInfo.overload != finalFixedSlotCount || displayInfo.max != itemsCount || displayInfo.costCount != num || displayInfo.selected != UIBase.gameState.UIManager.playerItemArea.controlInfo.spellItem)
			{
				displayInfo.overload = finalFixedSlotCount;
				displayInfo.max = itemsCount;
				displayInfo.costCount = num;
				displayInfo.selected = UIBase.gameState.UIManager.playerItemArea.controlInfo.spellItem;
				UIBase.gameState.StartCoroutine(OnShift);
			}
		}
	}

	public IndexDisplay AddIndexDisplay(ItemDisplayBase itemDisplay)
	{
		return indexTextPool.Spawn(itemDisplay);
	}

	public void RemoveIndexDisplays(IndexDisplay indexDisplay)
	{
		indexTextPool.Despawn(indexDisplay);
	}

	public void PlayMarkEffect()
	{
		if (showingMark)
		{
			sequence_MarkEffect?.Kill();
			sequence_MarkEffect = DOTween.Sequence();
			markEffect.gameObject.SetActive(value: true);
			markEffect.SetA(0f);
			markEffect.transform.position = markImage.transform.position;
			markEffect.transform.localScale = new Vector3(1f, 0.23f, 1f);
			sequence_MarkEffect.Append(DOTweenModuleUI.DOFade(markEffect, 1f, 0.08f));
			sequence_MarkEffect.Join(markEffect.transform.DOScale(new Vector3(9.6f, 0.75f, 1f), 0.08f));
			sequence_MarkEffect.Append(markEffect.transform.DOScale(new Vector3(0.85f, 2.25f, 1f), 0.16f));
			sequence_MarkEffect.Append(DOTweenModuleUI.DOFade(markEffect, 0f, 0.44f));
			sequence_MarkEffect.Join(markEffect.transform.DOScale(new Vector3(0.65f, 1.75f, 1f), 0.44f));
			Sequence sequence = sequence_MarkEffect;
			sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
			{
				markEffect.gameObject.SetActive(value: false);
				sequence_MarkEffect = null;
			});
			sequence_MarkEffect.Play();
		}
	}

	public IEnumerator PlayIndexEffect(Item item)
	{
		yield return PlayIndexEffect(GetIndexDisplay(item));
	}

	public IEnumerator PlayIndexEffect(IndexDisplay indexDisplay)
	{
		if (indexDisplay == null)
		{
			yield break;
		}
		Sequence sequence = DOTween.Sequence();
		try
		{
			Image indexEffect = indexEffectPool.Spawn();
			if (indexEffect == null)
			{
				Debug.LogError("PlayIndexEffect - indexEffect == null");
			}
			indexEffect.gameObject.SetActive(value: true);
			indexEffect.SetA(0f);
			indexEffect.transform.position = indexDisplay.transform.position;
			indexEffect.transform.localScale = new Vector3(1f, 0.23f, 1f);
			sequence.Append(DOTweenModuleUI.DOFade(indexEffect, 1f, 0.08f));
			sequence.Join(indexEffect.transform.DOScale(new Vector3(9.6f, 0.75f, 1f), 0.08f));
			sequence.Append(indexEffect.transform.DOScale(new Vector3(0.85f, 0.95f, 1f), 0.08f));
			sequence.Append(DOTweenModuleUI.DOFade(indexEffect, 0f, 0.16f));
			sequence.Join(indexEffect.transform.DOScale(new Vector3(0.85f, 1.4f, 1f), 0.16f));
			Sequence sequence2 = sequence;
			sequence2.onKill = (TweenCallback)Delegate.Combine(sequence2.onKill, (TweenCallback)delegate
			{
				indexEffectPool.Despawn(indexEffect);
				sequence = null;
			});
		}
		catch (Exception arg)
		{
			Debug.LogError($"PlayIndexEffect - \nError: \n{arg}");
			yield break;
		}
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	private IEnumerator OnShow()
	{
		sequence_Fade?.Kill();
		sequence_Fade = DOTween.Sequence();
		sequence_Fade.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		showing = true;
		base.gameObject.SetActive(value: true);
		sequence_Fade.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.5f));
		Sequence sequence = sequence_Fade;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			sequence_Fade = null;
		});
		sequence_Fade.Play();
		yield return sequence_Fade.WaitForKill();
	}

	private IEnumerator OnHide()
	{
		sequence_Fade?.Kill();
		sequence_Fade = DOTween.Sequence();
		sequence_Fade.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		showing = false;
		sequence_Fade.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.5f));
		Sequence sequence = sequence_Fade;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			sequence_Fade = null;
			base.gameObject.SetActive(value: false);
		});
		sequence_Fade.Play();
		yield return sequence_Fade.WaitForKill();
	}

	private IEnumerator OnShift()
	{
		sequence_Shift?.Kill();
		sequence_Shift = DOTween.Sequence();
		sequence_Shift.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		Sequence sequence = sequence_Shift;
		bool flag;
		try
		{
			Config playerOverloadDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerOverloadDisplayConfig;
			List<Item> allItems = UIBase.gameState.playerState.TableContainer.GetAllItems();
			int num = displayInfo.overload + displayInfo.costCount;
			if (allItems.Count > num && num > 0)
			{
				showingMark = true;
				int num2;
				for (num2 = num - 1; num2 > 0; num2--)
				{
					int itemPreviewIndex = allItems[num2].owner.GetItemPreviewIndex(allItems[num2]);
					if (itemPreviewIndex != -1 && itemPreviewIndex < displayInfo.overload)
					{
						break;
					}
				}
				if (num2 < 0 || num2 + 1 >= allItems.Count)
				{
					throw new IndexOutOfRangeException($"OnShift - overloadIndex={num2}, items.Count={allItems.Count}, boundaryIndex={num}");
				}
				PlayerItemDisplay slotDisplay = UIBase.gameState.UIManager.playerItemArea.GetSlotDisplay(allItems[num2]);
				PlayerItemDisplay slotDisplay2 = UIBase.gameState.UIManager.playerItemArea.GetSlotDisplay(allItems[num2 + 1]);
				if (slotDisplay == null || slotDisplay2 == null)
				{
					sequence_Shift?.Kill();
					sequence_Shift = null;
					showingMark = false;
					areaImage.materialForRendering.SetFloat(ShaderFields._Factor, 1f);
					markImage.SetA(0f);
					yield break;
				}
				Vector3 worldPosition = (slotDisplay.GetModule<PlayerTableDisplayStateModule>().GetIdleWorldPosition(num2) + slotDisplay2.GetModule<PlayerTableDisplayStateModule>().GetIdleWorldPosition(num2 + 1)) / 2f;
				float x = markImage.RectTransform().WorldPositionToAnchored(worldPosition).x;
				sequence_Shift.Insert(0f, DOTweenModuleUI.DOFade(markImage, 1f, playerOverloadDisplayConfig.shiftTime));
				sequence_Shift.Insert(0f, DOTweenModuleUI.DOAnchorPosX(markImage.RectTransform(), x, playerOverloadDisplayConfig.shiftTime, false));
				sequence_Shift.Insert(0f, areaImage.materialForRendering.DOShaderFactor(ShaderFields._Factor, 0.5f + x / areaImage.RectTransform().GetWidth(), playerOverloadDisplayConfig.shiftTime));
			}
			else
			{
				showingMark = false;
				areaImage.materialForRendering.SetFloat(ShaderFields._Factor, 1f);
				sequence_Shift.Insert(0f, DOTweenModuleUI.DOFade(markImage, 0f, playerOverloadDisplayConfig.shiftTime));
				sequence_Shift.Insert(0f, DOTweenModuleUI.DOAnchorPosX(markImage.rectTransform, areaImage.rectTransform.GetWidth() / 2f, playerOverloadDisplayConfig.shiftTime, false));
				sequence_Shift.Insert(0f, areaImage.materialForRendering.DOShaderFactor(ShaderFields._Factor, 1f, playerOverloadDisplayConfig.shiftTime));
			}
			sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
			{
				sequence_Shift = null;
			});
			sequence.Play();
			flag = true;
		}
		catch (Exception arg)
		{
			Debug.LogError($"OnShift - stop animation due to error\n{arg}");
			sequence_Shift?.Kill();
			sequence_Shift = null;
			showingMark = false;
			areaImage.materialForRendering.SetFloat(ShaderFields._Factor, 1f);
			markImage.SetA(0f);
			yield break;
		}
		if (flag)
		{
			yield return sequence.WaitForKill();
		}
	}

	private IndexDisplay GetIndexDisplay(Item item)
	{
		foreach (IndexDisplay activeElement in indexTextPool.GetActiveElements())
		{
			if (activeElement.itemDisplay.item == item)
			{
				return activeElement;
			}
		}
		return null;
	}

	private void _OnPanelShow()
	{
		if (ShouldShow)
		{
			showing = true;
			base.gameObject.SetActive(value: true);
		}
		else
		{
			showing = false;
			base.gameObject.SetActive(value: false);
		}
	}

	private void _OnTurnPhaseChange(GameEvent.Type t, GameEvent eve)
	{
		if (ShouldShow && !showing)
		{
			UIBase.gameState.StartCoroutine(OnShow);
		}
	}

	private void _OnAfterPlayerTurnEndWaste(GameEvent.Type t, GameEvent eve)
	{
		if (showing)
		{
			UIBase.gameState.StartCoroutine(OnHide);
		}
	}
}
