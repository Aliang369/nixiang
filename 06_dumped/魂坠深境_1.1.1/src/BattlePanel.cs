using System;
using System.Collections;
using Common.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlePanel : UIBase, IFrameUpdate
{
	private const float EmptyClickMaxSqrDistance = 625f;

	[SerializeField]
	public PlayerBftUIDisplay playerBftUIDisplay;

	[SerializeField]
	public EnemyBftUIDisplay enemyBftUIDisplay;

	[SerializeField]
	public PlayerCoinDisplay coinDisplay;

	[SerializeField]
	public PlayerCrystalDisplay crystalDisplay;

	[SerializeField]
	public BftHealthDisplay bftHealthDisplay;

	[SerializeField]
	public EnemyBftHealthDisplay enemyBftHealthDisplay;

	[SerializeField]
	public CurrentTurnCountDisplay currentTurnCountDisplay;

	[SerializeField]
	public DeckButton deckButton;

	[SerializeField]
	public SpeedButton speedButton;

	[SerializeField]
	public DrawPileButton drawPileButton;

	[SerializeField]
	public WastePileButton wastePileButton;

	[SerializeField]
	public EndTurnButton endTurnButton;

	[SerializeField]
	public AutoPlayButton autoPlayButton;

	[SerializeField]
	public InfluenceDisplayArea influencePanel;

	[SerializeField]
	public PauseButton pauseButton;

	[SerializeField]
	public ContractInfoDisplay contractInfoDisplay;

	[SerializeField]
	public EnvironmentInfluenceDisplay environmentInfluenceDisplay;

	[SerializeField]
	public ShowMapButton showMapButton;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private CanvasGroup[] inBattleOnlyCanvasGroups;

	[SerializeField]
	private RectTransform[] battlePanels;

	[NonSerialized]
	public ConvertablePivot gemDisplayPivot;

	[NonSerialized]
	public ConvertablePivot coinDisplayPivot;

	[NonSerialized]
	public ConvertablePivot crystalDisplayPivot;

	[NonSerialized]
	public ConvertablePivot deckButtonPivot;

	[NonSerialized]
	public ConvertablePivot drawPileButtonPivot;

	[NonSerialized]
	private bool pointerDownStartedOverUI;

	[NonSerialized]
	private int trackingPointerId = int.MinValue;

	[NonSerialized]
	private Vector2 pointerDownPosition;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	public ConvertablePivot wastePileButtonPivot;

	protected override void Init()
	{
		drawPileButtonPivot = new ConvertablePivot(drawPileButton.transform);
		wastePileButtonPivot = new ConvertablePivot(wastePileButton.transform);
		deckButtonPivot = new ConvertablePivot(deckButton.transform);
		coinDisplayPivot = new ConvertablePivot(coinDisplay.image.transform);
		crystalDisplayPivot = new ConvertablePivot(crystalDisplay.image.transform);
		InitUI(playerBftUIDisplay);
		InitUI(enemyBftUIDisplay);
		InitUI(coinDisplay);
		InitUI(crystalDisplay);
		InitUI(bftHealthDisplay);
		InitUI(enemyBftHealthDisplay);
		InitUI(currentTurnCountDisplay);
		InitUI(pauseButton);
		InitUI(contractInfoDisplay);
		InitUI(environmentInfluenceDisplay);
		InitUI(showMapButton);
		InitUI(drawPileButton);
		InitUI(wastePileButton);
		InitUI(deckButton);
		InitUI(speedButton);
		InitUI(endTurnButton);
		InitUI(autoPlayButton);
		InitUI(influencePanel);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, OnBattleStart);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, OnBattleEnd);
		UIBase.gameState.applicationState.runningGameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinished;
		manager.HideUI(this);
		base.gameObject.SetActive(value: false);
		for (int i = 0; i < inBattleOnlyCanvasGroups.Length; i++)
		{
			((Component)(object)inBattleOnlyCanvasGroups[i]).gameObject.SetActive(value: false);
		}
		for (int j = 0; j < battlePanels.Length; j++)
		{
			battlePanels[j].gameObject.SetActive(value: false);
		}
	}

	protected override void UpdateDisplay()
	{
		endTurnButton.gameObject.SetActive(UIBase.applicationState.guideManager.showEndTurnButton);
	}

	public IEnumerator ShowPanel(bool anim)
	{
		ActiveBattlePanel();
		SetDirty();
		TriggerEvent(UIEvent.OnPanelShow);
		for (int i = 0; i < inBattleOnlyCanvasGroups.Length; i++)
		{
			((Component)(object)inBattleOnlyCanvasGroups[i]).gameObject.SetActive(UIBase.gameState.battleState.isInBattle);
		}
		if (anim)
		{
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = base.uiTimeScale;
			DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
			float num = dungeonMapDisplayConfig.darkenTime + dungeonMapDisplayConfig.darkenWaitTime / 2f;
			sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0f));
			sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, num));
			for (int j = 0; j < inBattleOnlyCanvasGroups.Length; j++)
			{
				((Component)(object)inBattleOnlyCanvasGroups[j]).gameObject.SetActive(value: true);
				sequence.Insert(0f, DOTweenModuleUI.DOFade(inBattleOnlyCanvasGroups[j], 1f, num));
			}
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			yield return sequence.WaitForKill();
			SetDirty();
		}
		else
		{
			canvasGroup.alpha = 1f;
		}
	}

	public IEnumerator HidePanel(bool anim = true)
	{
		ActiveBattlePanel();
		SetDirty();
		if (anim)
		{
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = base.uiTimeScale;
			DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
			float num = dungeonMapDisplayConfig.darkenTime + dungeonMapDisplayConfig.darkenWaitTime / 2f;
			sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0f));
			sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, num));
			for (int i = 0; i < inBattleOnlyCanvasGroups.Length; i++)
			{
				((Component)(object)inBattleOnlyCanvasGroups[i]).gameObject.SetActive(value: true);
				sequence.Insert(0f, DOTweenModuleUI.DOFade(inBattleOnlyCanvasGroups[i], 0f, num));
			}
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			yield return sequence.WaitForKill();
			SetDirty();
		}
		DeactiveBattlePanel();
	}

	private void ActiveBattlePanel()
	{
		base.gameObject.SetActive(value: true);
		for (int i = 0; i < battlePanels.Length; i++)
		{
			battlePanels[i].gameObject.SetActive(value: true);
		}
		for (int j = 0; j < inBattleOnlyCanvasGroups.Length; j++)
		{
			((Component)(object)inBattleOnlyCanvasGroups[j]).gameObject.SetActive(value: true);
		}
	}

	private void DeactiveBattlePanel()
	{
		base.gameObject.SetActive(value: false);
		for (int i = 0; i < battlePanels.Length; i++)
		{
			battlePanels[i].gameObject.SetActive(value: false);
		}
		for (int j = 0; j < inBattleOnlyCanvasGroups.Length; j++)
		{
			((Component)(object)inBattleOnlyCanvasGroups[j]).gameObject.SetActive(value: false);
		}
	}

	private void OnBattleStart(GameEvent.Type _, GameEvent _2)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		float num = dungeonMapDisplayConfig.darkenTime + dungeonMapDisplayConfig.darkenWaitTime / 2f;
		for (int i = 0; i < inBattleOnlyCanvasGroups.Length; i++)
		{
			((Component)(object)inBattleOnlyCanvasGroups[i]).gameObject.SetActive(value: true);
			sequence.Insert(0f, DOTweenModuleUI.DOFade(inBattleOnlyCanvasGroups[i], 1f, num));
		}
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
	}

	private void OnBattleEnd(GameEvent.Type _, GameEvent _2)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		float num = dungeonMapDisplayConfig.darkenTime + dungeonMapDisplayConfig.darkenWaitTime / 2f;
		for (int i = 0; i < inBattleOnlyCanvasGroups.Length; i++)
		{
			((Component)(object)inBattleOnlyCanvasGroups[i]).gameObject.SetActive(value: true);
			sequence.Insert(0f, DOTweenModuleUI.DOFade(inBattleOnlyCanvasGroups[i], 0f, num));
		}
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			for (int j = 0; j < inBattleOnlyCanvasGroups.Length; j++)
			{
				((Component)(object)inBattleOnlyCanvasGroups[j]).gameObject.SetActive(value: false);
			}
		});
		sequence.Play();
	}

	private void _OnActionFrameFinished()
	{
		SetDirty();
	}

	public void FrameUpdate()
	{
		if (!HasBattleSkillInfo())
		{
			trackingPointerId = int.MinValue;
			return;
		}
		if (Input.touchCount == 0 && trackingPointerId >= 0)
		{
			trackingPointerId = int.MinValue;
		}
		if (Input.touchCount > 0)
		{
			HandleTouchEmptyClick();
		}
		else
		{
			HandleMouseEmptyClick();
		}
	}

	private void HandleMouseEmptyClick()
	{
		if (Input.GetMouseButtonDown(0))
		{
			trackingPointerId = -1;
			pointerDownPosition = Input.mousePosition;
			pointerDownStartedOverUI = IsPointerOverUI();
		}
		else if (trackingPointerId == -1 && Input.GetMouseButtonUp(0))
		{
			float sqrMagnitude = ((Vector2)Input.mousePosition - pointerDownPosition).sqrMagnitude;
			if (!pointerDownStartedOverUI && !IsPointerOverUI() && sqrMagnitude <= 625f)
			{
				TriggerBattleEmptyClick();
			}
			trackingPointerId = int.MinValue;
		}
	}

	private void HandleTouchEmptyClick()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Invalid comparison between Unknown and I4
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if ((int)((Touch)(ref touch)).phase == 0 && trackingPointerId == int.MinValue)
			{
				trackingPointerId = ((Touch)(ref touch)).fingerId;
				pointerDownPosition = ((Touch)(ref touch)).position;
				pointerDownStartedOverUI = IsPointerOverUI(((Touch)(ref touch)).fingerId);
				break;
			}
			if (((Touch)(ref touch)).fingerId != trackingPointerId)
			{
				continue;
			}
			if ((int)((Touch)(ref touch)).phase == 4)
			{
				trackingPointerId = int.MinValue;
			}
			else if ((int)((Touch)(ref touch)).phase == 3)
			{
				float sqrMagnitude = (((Touch)(ref touch)).position - pointerDownPosition).sqrMagnitude;
				if (!pointerDownStartedOverUI && !IsPointerOverUI(((Touch)(ref touch)).fingerId) && sqrMagnitude <= 625f)
				{
					TriggerBattleEmptyClick();
				}
				trackingPointerId = int.MinValue;
			}
			break;
		}
	}

	private bool HasBattleSkillInfo()
	{
		if (playerBftUIDisplay?.skillPanel?.infoPanel != null)
		{
			return playerBftUIDisplay.skillPanel.infoPanel.HasCommonInfo;
		}
		return false;
	}

	private static bool IsPointerOverUI(int pointerId = -1)
	{
		if (EventSystem.current == null)
		{
			return false;
		}
		if (pointerId >= 0)
		{
			return EventSystem.current.IsPointerOverGameObject(pointerId);
		}
		return EventSystem.current.IsPointerOverGameObject();
	}

	private void TriggerBattleEmptyClick()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.TriggerEvent();
	}
}
