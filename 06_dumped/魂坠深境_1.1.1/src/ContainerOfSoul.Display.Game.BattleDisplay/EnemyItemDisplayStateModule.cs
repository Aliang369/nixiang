using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

[Serializable]
public class EnemyItemDisplayStateModule : ItemDisplayBase.DisplayModule
{
	public enum State
	{
		OutOfControl = -2,
		PlayingMutualExcludeAnim,
		Idle,
		UnDraw,
		Selected,
		Over
	}

	private const int MaxFullSpacingItemCount = 4;

	[SerializeField]
	private float offset;

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private RectTransform hiddenTransform;

	[NonSerialized]
	private Canvas baseCanvas;

	[NonSerialized]
	private int currentCount;

	[NonSerialized]
	public int currentIndex;

	[NonSerialized]
	public State currentState;

	[NonSerialized]
	private bool isIntentHidden;

	[NonSerialized]
	private bool playingStateAnim;

	[NonSerialized]
	private Sequence sequence;

	public override void Init()
	{
		baseCanvas = GetComponentInParent<Canvas>();
	}

	public override void Setup()
	{
		currentState = State.UnDraw;
		RectTransform obj = base.slotDisplay.transform as RectTransform;
		EnemyItemArea tableItemArea = UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		obj.position = tableItemArea.itemSpawnPivot.ToCanvasWorldPosition(baseCanvas);
		obj.rotation = Quaternion.Euler(enemyItemAnimConfig.drawRotation1);
		obj.localScale = enemyItemAnimConfig.drawScale1;
		isIntentHidden = base.slotDisplay.item.IsHidden(UIDisplayModule.gameState);
		contentTransform.localScale = Vector3.one;
		hiddenTransform.localScale = Vector3.one;
		contentTransform.gameObject.SetActive(!isIntentHidden);
		hiddenTransform.gameObject.SetActive(isIntentHidden);
	}

	public override void Unset()
	{
		sequence?.Kill();
	}

	public override void UpdateDisplay()
	{
		if (currentState != State.PlayingMutualExcludeAnim && currentState != State.UnDraw && base.slotDisplay.enableUpdate && Valid())
		{
			UpdateHideIntend();
		}
	}

	public void RefreshDisplay()
	{
		if (currentState != State.PlayingMutualExcludeAnim && currentState != State.UnDraw && base.slotDisplay.enableUpdate && Valid())
		{
			UpdateStateAnim();
		}
	}

	private void UpdateStateAnim()
	{
		State state = GetState();
		if ((state != currentState || RefreshIndex()) && Valid())
		{
			currentState = state;
			switch (state)
			{
			case State.Idle:
				OnToIdle();
				break;
			case State.Selected:
				OnSelected();
				break;
			case State.Over:
				OnOver();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case State.OutOfControl:
			case State.PlayingMutualExcludeAnim:
			case State.UnDraw:
				break;
			}
			UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.SetDirty();
		}
	}

	private State GetState()
	{
		PlayerItemArea playerItemArea = UIDisplayModule.gameState.UIManager.playerItemArea;
		bool flag = playerItemArea.controlInfo.selected == base.slotDisplay.item;
		bool flag2 = playerItemArea.controlInfo.targetItem == base.slotDisplay.item;
		bool flag3 = playerItemArea.controlInfo.rawOver == base.slotDisplay;
		switch (playerItemArea.controlInfo.state)
		{
		case PlayerControlInfo.State.Prepare:
		case PlayerControlInfo.State.ClickSelected:
		case PlayerControlInfo.State.DragSelecting:
			if (flag)
			{
				return State.Selected;
			}
			if (flag3)
			{
				return State.Over;
			}
			return State.Idle;
		case PlayerControlInfo.State.GettingTarget:
		case PlayerControlInfo.State.TargetSelected:
			if (flag2)
			{
				return State.Selected;
			}
			return State.Idle;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private bool Valid()
	{
		if (currentCount < 0 || currentIndex < 0)
		{
			return false;
		}
		return true;
	}

	private void OnToIdle()
	{
		sequence?.Kill();
		playingStateAnim = true;
		RefreshIndex();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		AppendToIdleSequence(sequence, playerItemAnimConfig.unselectTime);
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			playingStateAnim = false;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
	}

	private void OnSelected()
	{
		sequence?.Kill();
		playingStateAnim = true;
		RefreshIndex();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		Vector2 idlePosition = GetIdlePosition();
		RectTransform rectTransform = base.slotDisplay.RectTransform();
		sequence.Append(DOTweenModuleUI.DOAnchorPos(rectTransform, idlePosition + Vector2.up * playerItemAnimConfig.selectPointerMinOffset, playerItemAnimConfig.selectTime1, false));
		sequence.Join(rectTransform.DOLocalRotate(playerItemAnimConfig.selectRotation, playerItemAnimConfig.selectTime1));
		sequence.Append(rectTransform.DOScale(playerItemAnimConfig.selectAnimScale2, playerItemAnimConfig.selectTime2));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			playingStateAnim = false;
		});
		sequence.Play();
	}

	private void OnOver()
	{
		sequence?.Kill();
		playingStateAnim = true;
		RefreshIndex();
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		Vector2 idlePosition = GetIdlePosition();
		RectTransform rectTransform = base.slotDisplay.RectTransform();
		sequence.Append(DOTweenModuleUI.DOAnchorPos(rectTransform, idlePosition + Vector2.up * enemyItemAnimConfig.overOffset, enemyItemAnimConfig.overTime1, false));
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.overRotation, enemyItemAnimConfig.overTime1));
		sequence.Append(rectTransform.DOScale(enemyItemAnimConfig.overScale, enemyItemAnimConfig.overTime2));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			playingStateAnim = false;
		});
		sequence.Play();
	}

	private void AppendToIdleSequence(Sequence sequence, float time)
	{
		if (currentIndex != -1)
		{
			ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
			RectTransform rectTransform = base.slotDisplay.RectTransform();
			Vector2 idlePosition = GetIdlePosition();
			sequence.Append(DOTweenModuleUI.DOAnchorPos(rectTransform, idlePosition, time, false));
			sequence.Join(rectTransform.DOLocalMoveZ(0f, time));
			sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.idleRotation, time));
			sequence.Join(rectTransform.DOScale(Vector3.one, time));
		}
	}

	public bool HasAnim()
	{
		if (!playingStateAnim)
		{
			return currentState == State.PlayingMutualExcludeAnim;
		}
		return true;
	}

	public void SkipMutualExcludeAnim()
	{
		sequence?.Kill();
		sequence = null;
		playingStateAnim = false;
		currentState = State.OutOfControl;
		RefreshIndex();
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		RectTransform obj = base.slotDisplay.RectTransform();
		obj.anchoredPosition3D = GetIdlePosition();
		obj.localRotation = Quaternion.Euler(enemyItemAnimConfig.idleRotation);
		obj.localScale = enemyItemAnimConfig.idleScale;
		base.gameObject.SetActive(value: true);
		base.slotDisplay.gameObject.SetActive(value: true);
		base.slotDisplay.SetDirty();
	}

	public void ConfigTween_TransferFromPlayer1(Sequence sequence)
	{
		this.sequence = sequence;
		currentState = State.PlayingMutualExcludeAnim;
		RectTransform obj = base.slotDisplay.RectTransform();
		Vector2 idlePosition = GetIdlePosition();
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		obj.anchoredPosition = idlePosition;
		obj.localScale = enemyItemAnimConfig.idleScale;
		obj.rotation = Quaternion.Euler(enemyItemAnimConfig.idleRotation);
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			currentState = State.OutOfControl;
		});
		base.gameObject.SetActive(value: false);
	}

	public void ConfigTween_TransferFromPlayer2(Sequence sequence)
	{
		this.sequence = sequence;
		currentState = State.PlayingMutualExcludeAnim;
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: true);
		});
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			currentState = State.OutOfControl;
		});
	}

	public IEnumerator OnItemAbilityTrigger(Ability.AbilityAnimPriority priority)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		currentState = State.PlayingMutualExcludeAnim;
		float battleScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		sequence.timeScale = battleScale;
		bool finished = false;
		RefreshIndex();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.y = playerItemAnimConfig.spellPositionY;
		Vector2 anchoredPosition2 = rectTransform.anchoredPosition;
		if (priority == Ability.AbilityAnimPriority.Strong)
		{
			sequence.Append(rectTransform.DOLocalRotate(Vector3.zero, playerItemAnimConfig.passiveSpellTime1));
			sequence.Join(DOTweenModuleUI.DOAnchorPos3D(rectTransform, (Vector3)(anchoredPosition2 + playerItemAnimConfig.passiveOffset1), playerItemAnimConfig.passiveSpellTime1, false));
		}
		if (priority == Ability.AbilityAnimPriority.Weak)
		{
			sequence.AppendCallback(delegate
			{
				UIDisplayModule.gameState.StartCoroutine(() => PlayIconEffect(1));
			});
		}
		else
		{
			sequence.AppendCallback(delegate
			{
				UIDisplayModule.gameState.StartCoroutine(() => PlayIconEffect(4));
			});
		}
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			finished = true;
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		if (battleScale < 2000f)
		{
			yield return new WaitUntil(() => finished);
		}
	}

	public IEnumerator OnEnchantAbilityTrigger(Enchant enchant, int times = 1)
	{
		Image enchantEffectImage = UIDisplayModule.gameState.UIManager.playerItemArea.enchantEffectImage;
		List<Image> images = new List<Image>();
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < times; i++)
		{
			Image image = UnityEngine.Object.Instantiate(enchantEffectImage, base.transform);
			image.gameObject.SetActive(value: true);
			image.sprite = enchant.def.Icon;
			image.rectTransform.position = base.slotDisplay.GetModule<EnchantModule>().GetEnchantDisplay(enchant).transform.position;
			image.rectTransform.localScale = Vector3.one;
			images.Add(image);
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image.rectTransform.DOScale(new Vector3(3f, 3f, 1f), 1f));
		}
		sequence.Play();
		yield return sequence.WaitForKill();
		for (int j = 0; j < times; j++)
		{
			if (images[j]?.gameObject != null)
			{
				UnityEngine.Object.Destroy(images[j].gameObject);
			}
		}
	}

	public IEnumerator OnItemAbilityTriggerEnd()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		float battleScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		sequence.timeScale = battleScale;
		currentState = State.PlayingMutualExcludeAnim;
		bool finished = false;
		RefreshIndex();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
		Vector2 idlePosition = GetIdlePosition();
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		sequence.Append(DOTweenModuleUI.DOAnchorPos3D(rectTransform, (Vector3)idlePosition, playerItemAnimConfig.addTime, false));
		sequence.Join(rectTransform.DOScale(Vector3.one, playerItemAnimConfig.addTime));
		sequence.Join(rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.addTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			finished = true;
		});
		sequence.Play();
		if (battleScale < 2000f)
		{
			yield return new WaitUntil(() => finished);
		}
	}

	public IEnumerator OnAddNewItem(Item source)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		RefreshIndex();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.enemyItemAnimConfig;
		ItemInfoDisplayBase.Config itemInfoDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemInfoDisplayConfig;
		Vector2 idlePosition = GetIdlePosition();
		RectTransform rectTransform = base.slotDisplay.RectTransform();
		Vector3 vector = rectTransform.AnchoredToWorldPosition(idlePosition);
		Vector3 endPos = (rectTransform.position = Vector3.Lerp(vector, UIDisplayModule.gameState.UIManager.battleItemInfoPanel.enemyPivot.ToCanvasWorldPosition(baseCanvas), 1f - itemInfoDisplayConfig.showInfoOffsetFactor));
		rectTransform.localScale = enemyItemAnimConfig.showInfoScale;
		rectTransform.rotation = Quaternion.Euler(enemyItemAnimConfig.showInfoRotate);
		base.gameObject.SetActive(value: false);
		UIDisplayModule.gameState.UIManager.battleItemInfoPanel.ConfigTween_Insert(sequence, base.slotDisplay, source, endPos);
		sequence.AppendCallback(delegate
		{
			base.slotDisplay.gameObject.SetActive(value: true);
		});
		sequence.Append(rectTransform.DOMove(vector, enemyItemAnimConfig.showInfoTime));
		sequence.Join(rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, enemyItemAnimConfig.showInfoTime));
		sequence.Join(rectTransform.DOScale(playerItemAnimConfig.idleScale, enemyItemAnimConfig.showInfoTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnAddItem(Vector3? fromPosition)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		RefreshIndex();
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.RectTransform();
		EnemyItemArea tableItemArea = UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea;
		Vector2 idlePosition = GetIdlePosition();
		Vector3 position = fromPosition ?? tableItemArea.itemSpawnPivot.ToCanvasWorldPosition(baseCanvas);
		rectTransform.position = position;
		Vector3 anchoredPosition3D = rectTransform.anchoredPosition3D;
		float? z = 0f;
		rectTransform.anchoredPosition3D = anchoredPosition3D.GetWith(null, null, z);
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.drawRotation1, 0f));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.drawScale1, 0f));
		sequence.Append(rectTransform.DOLocalRotate(enemyItemAnimConfig.idleRotation, enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.drawScale2, enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2));
		sequence.Join(DOTweenModuleUI.DOAnchorPosX(rectTransform, idlePosition.x, enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2, false));
		sequence.Join(DOTweenModuleUI.DOAnchorPosY(rectTransform, (idlePosition + enemyItemAnimConfig.drawPosition1).y, enemyItemAnimConfig.drawTime1_1, false));
		sequence.Append(DOTweenModuleUI.DOAnchorPosY(rectTransform, (idlePosition + enemyItemAnimConfig.drawPosition2).y, enemyItemAnimConfig.drawTime1_2, false));
		sequence.Append(DOTweenModuleUI.DOAnchorPos3D(rectTransform, (Vector3)idlePosition, enemyItemAnimConfig.drawTime2, false));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.idleScale, enemyItemAnimConfig.drawTime2));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnDraw()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		RefreshIndex();
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.enemyItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.RectTransform();
		EnemyItemArea tableItemArea = UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea;
		Vector2 idlePosition = GetIdlePosition();
		sequence.Append(rectTransform.DOMove(tableItemArea.itemSpawnPivot.ToCanvasWorldPosition(baseCanvas), 0f));
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.drawRotation1, 0f));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.drawScale1, 0f));
		sequence.Append(rectTransform.DOLocalRotate(enemyItemAnimConfig.idleRotation, enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.drawScale2, enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2));
		sequence.Join(DOTweenModuleUI.DOAnchorPosX(rectTransform, idlePosition.x, enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2, false));
		sequence.Join(DOTweenModuleUI.DOAnchorPos3DZ(rectTransform, 0f, enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2, false));
		sequence.Join(DOTweenModuleUI.DOAnchorPosY(rectTransform, (idlePosition + enemyItemAnimConfig.drawPosition1).y, enemyItemAnimConfig.drawTime1_1, false));
		sequence.Append(DOTweenModuleUI.DOAnchorPosY(rectTransform, (idlePosition + enemyItemAnimConfig.drawPosition2).y, enemyItemAnimConfig.drawTime1_2, false));
		sequence.Append(DOTweenModuleUI.DOAnchorPos3D(rectTransform, (Vector3)idlePosition, enemyItemAnimConfig.drawTime2, false));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.idleScale, enemyItemAnimConfig.drawTime2));
		sequence.onKill = delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			base.slotDisplay.SetDirty();
		};
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnRemove()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		EnemyItemArea tableItemArea = UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
		rectTransform.SetSiblingIndex(0);
		Vector3 position = base.transform.position;
		Vector3 endValue = base.slotDisplay.item.GetPivot(UIDisplayModule.gameState)?.ToCanvasWorldPosition(baseCanvas) ?? tableItemArea.itemSpawnPivot.ToCanvasWorldPosition(baseCanvas);
		sequence.Append(rectTransform.DOMove(position, 0f));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.idleScale, 0f));
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.idleRotation, 0f));
		float duration = enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2 + enemyItemAnimConfig.drawTime2;
		sequence.Append(rectTransform.DOMove(endValue, duration));
		sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.drawScale1, duration));
		sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.drawRotation1, duration));
		sequence.onKill = delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			base.slotDisplay.SetDirty();
		};
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnExile()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		EffectModule module = base.slotDisplay.GetModule<EffectModule>();
		float time = enemyItemAnimConfig.drawTime1_1 + enemyItemAnimConfig.drawTime1_2 + enemyItemAnimConfig.drawTime2;
		base.slotDisplay.GetModule<ItemNameModule>().OnExile();
		base.slotDisplay.GetModule<CounterModule>().OnExile();
		base.slotDisplay.GetModule<EnemyItemIntentModule>()?.OnExile();
		module.AppendEffectTween(sequence, 0f, time);
		sequence.onKill = delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			base.slotDisplay.SetDirty();
		};
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnSpell()
	{
		if (UIDisplayModule.applicationState.settingManager.uiTimeScaleTypeSetting != TimeScaleManager.TimeScaleType.X1)
		{
			UIDisplayModule.gameState.StartCoroutine(() => PlayIconEffect(1));
			yield break;
		}
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
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
		UIDisplayModule.gameState.UIManager.battleItemInfoPanel.ConfigTween_Spell(sequence, base.slotDisplay, base.slotDisplay.item, PivotManager.PivotType.EnemyPivot);
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
			currentState = State.OutOfControl;
			base.slotDisplay.SetDirty();
		});
		sequence.Play();
		yield return sequence.WaitForKill();
		base.slotDisplay.SetDirty();
	}

	public IEnumerator OnShuffle()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		RefreshIndex();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
		Vector2 idlePosition = GetIdlePosition();
		sequence.Append(DOTweenModuleUI.DOAnchorPos3D(rectTransform, new Vector3(0f, 0f, 0f), playerItemAnimConfig.shuffleTime1, false));
		sequence.Join(rectTransform.DOScale(playerItemAnimConfig.idleScale, playerItemAnimConfig.shuffleTime1));
		sequence.Join(rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.shuffleTime1));
		sequence.AppendInterval(playerItemAnimConfig.shuffleTime2);
		sequence.Append(DOTweenModuleUI.DOAnchorPos3D(rectTransform, (Vector3)idlePosition, playerItemAnimConfig.shuffleTime3, false));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnHideIntend()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		sequence.Append(contentTransform.DOScaleX(0f, enemyItemAnimConfig.hideIntentTime / 2f));
		sequence.AppendCallback(delegate
		{
			contentTransform.gameObject.SetActive(value: false);
			hiddenTransform.gameObject.SetActive(value: true);
		});
		sequence.Append(hiddenTransform.DOScaleX(0f, 0f));
		sequence.Append(hiddenTransform.DOScaleX(1f, enemyItemAnimConfig.hideIntentTime / 2f));
		sequence.onKill = delegate
		{
			contentTransform.localScale = Vector3.one;
			hiddenTransform.localScale = Vector3.one;
			contentTransform.gameObject.SetActive(value: false);
			hiddenTransform.gameObject.SetActive(value: true);
			currentState = State.OutOfControl;
			sequence = null;
			base.slotDisplay.SetDirty();
		};
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnShowIntend()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
		sequence.Append(hiddenTransform.DOScaleX(0f, enemyItemAnimConfig.hideIntentTime / 2f));
		sequence.AppendCallback(delegate
		{
			contentTransform.gameObject.SetActive(value: true);
			hiddenTransform.gameObject.SetActive(value: false);
		});
		sequence.Append(contentTransform.DOScaleX(0f, 0f));
		sequence.Append(contentTransform.DOScaleX(1f, enemyItemAnimConfig.hideIntentTime / 2f));
		sequence.onKill = delegate
		{
			contentTransform.localScale = Vector3.one;
			hiddenTransform.localScale = Vector3.one;
			contentTransform.gameObject.SetActive(value: true);
			hiddenTransform.gameObject.SetActive(value: false);
			sequence = null;
			currentState = State.OutOfControl;
			base.slotDisplay.SetDirty();
		};
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnConvert()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
		Vector2 idlePosition = GetIdlePosition();
		sequence.Append(DOTweenModuleUI.DOAnchorPos(rectTransform, idlePosition, playerItemAnimConfig.drawTime, false));
		sequence.Join(rectTransform.DOScale(playerItemAnimConfig.idleScale, playerItemAnimConfig.drawTime));
		sequence.Join(rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.drawTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	private void ShowInfo()
	{
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
	}

	private void HideInfo()
	{
	}

	private void UpdateHideIntend()
	{
		bool flag = base.slotDisplay.item.IsHidden(UIDisplayModule.gameState);
		if (isIntentHidden != flag)
		{
			isIntentHidden = flag;
			if (isIntentHidden)
			{
				UIDisplayModule.gameState.StartCoroutine(OnHideIntend);
			}
			else
			{
				UIDisplayModule.gameState.StartCoroutine(OnShowIntend);
			}
		}
	}

	private bool RefreshIndex()
	{
		int num = base.slotDisplay.item.owner?.GetItemIndex(base.slotDisplay.item) ?? (-1);
		int num2 = base.slotDisplay.item.owner?.GetAllItems().Count ?? (-1);
		if (currentIndex == num && currentCount == num2)
		{
			return false;
		}
		currentIndex = num;
		currentCount = num2;
		return true;
	}

	private void UpdateIdlePosition()
	{
		if (currentState == State.Idle)
		{
			ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.enemyItemAnimConfig;
			RectTransform rectTransform = base.slotDisplay.transform as RectTransform;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
			sequence.Append(DOTweenModuleUI.DOAnchorPos3D(rectTransform, (Vector3)GetIdlePosition(), enemyItemAnimConfig.moveTime, false));
			sequence.Join(rectTransform.DOScale(enemyItemAnimConfig.idleScale, enemyItemAnimConfig.moveTime));
			sequence.Join(rectTransform.DOLocalRotate(enemyItemAnimConfig.idleRotation, enemyItemAnimConfig.moveTime));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		}
	}

	private Vector2 GetIdlePosition()
	{
		if (!Valid())
		{
			return Vector2.zero;
		}
		float num = ((currentCount > 4) ? (offset * 3f / (float)(currentCount - 1)) : offset);
		return new Vector2((float)currentIndex * num - 0.3f * offset, 0f);
	}

	private IEnumerator PlayIconEffect(int times)
	{
		Image itemEffectImage = UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.itemEffectImage;
		List<Image> images = new List<Image>();
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < times; i++)
		{
			Image image = UnityEngine.Object.Instantiate(itemEffectImage, base.transform);
			image.gameObject.SetActive(value: true);
			image.sprite = base.slotDisplay.item.GetSkinIcon(UIDisplayModule.applicationState);
			image.rectTransform.position = base.transform.position;
			image.rectTransform.localScale = Vector3.one;
			images.Add(image);
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image.rectTransform.DOScale(new Vector3(2f, 2f, 1f), 1f));
		}
		sequence.Play();
		yield return sequence.WaitForKill();
		for (int j = 0; j < times; j++)
		{
			if (images[j]?.gameObject != null)
			{
				UnityEngine.Object.Destroy(images[j].gameObject);
			}
		}
	}
}
