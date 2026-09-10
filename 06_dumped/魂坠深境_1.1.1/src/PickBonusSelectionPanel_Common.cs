using System;
using System.Collections;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickBonusSelectionPanel_Common : PickBonusSelectionPanelBase
{
	private enum BonusCollectionResult
	{
		Claimed,
		Cancelled,
		Unavailable
	}

	private sealed class BonusCollectionSession
	{
		public readonly BattleBonus info;

		public int nextBonusIndex;

		public bool hasGrantedBonus;

		public bool running;

		public bool finishing;

		public WrapperEnumerator coroutine;

		public BonusCollectionSession(BattleBonus info)
		{
			this.info = info;
		}
	}

	[SerializeField]
	public ConfirmItemSelectionPanel itemSelectionPanel;

	[SerializeField]
	private ConfirmInfluenceSelectionPanel influenceSelectionPanel;

	[SerializeField]
	private PickBonusSelector selectorPrefab;

	[SerializeField]
	private RectTransform selectorParent;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	public GameButton leaveButton;

	[SerializeField]
	public GameButton getAllButton;

	[SerializeField]
	private Image background_Normal;

	[SerializeField]
	private Image background_Elite;

	[SerializeField]
	private Image background_Boss;

	[SerializeField]
	private RectTransform coinArea;

	[SerializeField]
	private RectTransform crystalArea;

	[NonSerialized]
	private BattleBonus info;

	[NonSerialized]
	private CellUIPrefabPool<PickBonusSelector, BattleBonusObject> selectorPool;

	[NonSerialized]
	private BonusCollectionSession collectionSession;

	protected override void Init()
	{
		selectorPrefab.gameObject.SetActive(value: false);
		selectorPool = GetPrefabPool<PickBonusSelector, BattleBonusObject>(selectorPrefab, selectorParent);
		CellUIPrefabPool<PickBonusSelector, BattleBonusObject> cellUIPrefabPool = selectorPool;
		cellUIPrefabPool.onSetup = (Action<PickBonusSelector>)Delegate.Combine(cellUIPrefabPool.onSetup, (Action<PickBonusSelector>)delegate(PickBonusSelector display)
		{
			display.singleItemBonusDisplayBase.GetModule<InfoModule>().infoPanel = itemInfoPanel;
		});
		leaveButton.onClick.AddListener(_OnLeaveButtonClicked);
		getAllButton.onClick.AddListener(_OnSelectAllButtonClicked);
		InitUI(itemSelectionPanel);
		InitUI(influenceSelectionPanel);
		InitUI(itemInfoPanel);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnHideAllInfo);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnHideAllInfo);
	}

	public void Setup(BattleBonus info)
	{
		CancelCollectionSession();
		this.info = info;
		PickBonusSelectionPanelBase.RemoveInvalidRuneBonuses(info);
		collectionSession = new BonusCollectionSession(info);
		SetCollectionButtons(canCollect: true, canLeave: true);
		base.gameObject.SetActive(value: true);
		coinArea.gameObject.SetActive(value: false);
		crystalArea.gameObject.SetActive(value: false);
		foreach (BattleBonusObject bonuse in info.bonuses)
		{
			selectorPool.Spawn(bonuse);
		}
		itemSelectionPanel.gameObject.SetActive(value: false);
		influenceSelectionPanel.gameObject.SetActive(value: false);
		PathLocation.LocationType? locationType = UIBase.gameState.dungeonMapManager.currentLocation?.type;
		background_Normal.gameObject.SetActive(value: false);
		background_Elite.gameObject.SetActive(value: false);
		background_Boss.gameObject.SetActive(value: false);
		if (locationType == PathLocation.LocationType.EliteFight)
		{
			background_Elite.gameObject.SetActive(value: true);
		}
		else if (locationType == PathLocation.LocationType.Boss)
		{
			background_Boss.gameObject.SetActive(value: true);
		}
		else
		{
			background_Normal.gameObject.SetActive(value: true);
		}
		itemInfoPanel.HideAllCommonInfo();
		SetDirty();
	}

	public void Unset()
	{
		BattleBonus battleBonus = info;
		CancelCollectionSession();
		if (battleBonus != null)
		{
			battleBonus.onEnd?.Invoke();
			selectorPool.DespawnAll();
			base.gameObject.SetActive(value: false);
			UIBase.gameState.UIManager.bonusLocationPanel.isPlayingGame = false;
			info = null;
		}
	}

	protected override void UpdateDisplay()
	{
		if (selectorPool.activeCount == 0 && (collectionSession == null || collectionSession.finishing))
		{
			Unset();
		}
	}

	public override void OnSubPanelFinish(BattleBonusObject info)
	{
		MarkBonusResolved(info);
	}

	public override void Select(PickBonusSelector selector)
	{
	}

	private void _OnLeaveButtonClicked()
	{
		BonusCollectionSession bonusCollectionSession = collectionSession;
		if (bonusCollectionSession != null && !bonusCollectionSession.running && !bonusCollectionSession.finishing && !bonusCollectionSession.hasGrantedBonus)
		{
			bonusCollectionSession.finishing = true;
			SetCollectionButtons(canCollect: false, canLeave: false);
			UIBase.gameState.gameEventManager.TriggerFinishBonus(bonusCollectionSession.info);
			Unset();
			UIBase.gameState.UIManager.bonusLocationPanel.RefuseAndFight();
		}
	}

	private void _OnSelectAllButtonClicked()
	{
		BonusCollectionSession session = collectionSession;
		if (session != null && !session.running && !session.finishing)
		{
			session.running = true;
			SetCollectionButtons(canCollect: false, canLeave: false);
			WrapperEnumerator coroutine = UIBase.gameState.StartCoroutine(() => RunCollection(session));
			if (IsCurrentSession(session))
			{
				session.coroutine = coroutine;
			}
		}
	}

	private IEnumerator RunCollection(BonusCollectionSession session)
	{
		try
		{
			while (session.nextBonusIndex < session.info.bonuses.Count)
			{
				if (!IsCurrentSession(session))
				{
					yield break;
				}
				BattleBonusObject bonus = session.info.bonuses[session.nextBonusIndex];
				if (bonus is SelectGetItemBonus)
				{
					session.nextBonusIndex++;
					continue;
				}
				BonusCollectionResult? result = null;
				if (bonus is AddInfluenceBonus addInfluenceBonus)
				{
					addInfluenceBonus.selectedInfluence = addInfluenceBonus.selectableInfluence?.FirstOrDefault();
					addInfluenceBonus.TriggerBonus(UIBase.gameState, delegate
					{
						Resolve(BonusCollectionResult.Claimed);
					}, delegate
					{
						Resolve(BonusCollectionResult.Cancelled);
					}, delegate
					{
						Resolve(BonusCollectionResult.Unavailable);
					});
				}
				else
				{
					bonus.TriggerBonus(UIBase.gameState, delegate
					{
						Resolve(BonusCollectionResult.Claimed);
					}, delegate
					{
						Resolve(BonusCollectionResult.Cancelled);
					}, delegate
					{
						Resolve(BonusCollectionResult.Unavailable);
					});
				}
				if (!result.HasValue)
				{
					yield return new WaitUntil(() => result.HasValue || !IsCurrentSession(session));
				}
				if (!IsCurrentSession(session))
				{
					yield break;
				}
				if (result == BonusCollectionResult.Cancelled)
				{
					if (!UIBase.gameState.actionManager.isEmpty)
					{
						yield return new WaitUntil(() => UIBase.gameState.actionManager.isEmpty || !IsCurrentSession(session));
					}
					if (IsCurrentSession(session))
					{
						PauseCollection(session);
					}
					yield break;
				}
				if (result == BonusCollectionResult.Unavailable)
				{
					MarkBonusResolved(bonus);
					session.nextBonusIndex++;
					continue;
				}
				session.hasGrantedBonus = true;
				if (!UIBase.gameState.actionManager.isEmpty)
				{
					yield return new WaitUntil(() => UIBase.gameState.actionManager.isEmpty || !IsCurrentSession(session));
				}
				if (!IsCurrentSession(session))
				{
					yield break;
				}
				MarkBonusResolved(bonus);
				session.nextBonusIndex++;
				void Resolve(BonusCollectionResult value)
				{
					if (!result.HasValue)
					{
						result = value;
					}
				}
			}
			if (IsCurrentSession(session))
			{
				session.running = false;
				session.finishing = true;
				session.coroutine = null;
				SetCollectionButtons(canCollect: false, canLeave: false);
				UIBase.gameState.gameEventManager.TriggerFinishBonus(session.info);
				UIBase.gameState.UIManager.bonusLocationPanel.SelectEnd();
				Unset();
			}
		}
		finally
		{
			PickBonusSelectionPanel_Common pickBonusSelectionPanel_Common = this;
			if (pickBonusSelectionPanel_Common.collectionSession == session && session.running && !session.finishing)
			{
				pickBonusSelectionPanel_Common.PauseCollection(session);
			}
		}
	}

	private bool IsCurrentSession(BonusCollectionSession session)
	{
		if (collectionSession == session && info == session.info && session.running)
		{
			return !session.finishing;
		}
		return false;
	}

	private void PauseCollection(BonusCollectionSession session)
	{
		if (collectionSession == session)
		{
			session.running = false;
			session.coroutine = null;
			SetCollectionButtons(canCollect: true, !session.hasGrantedBonus);
		}
	}

	private void CancelCollectionSession()
	{
		BonusCollectionSession bonusCollectionSession = collectionSession;
		collectionSession = null;
		if (bonusCollectionSession != null)
		{
			bonusCollectionSession.running = false;
			bonusCollectionSession.finishing = true;
			WrapperEnumerator coroutine = bonusCollectionSession.coroutine;
			bonusCollectionSession.coroutine = null;
			coroutine?.Stop();
		}
	}

	private void MarkBonusResolved(BattleBonusObject bonus)
	{
		PickBonusSelector pickBonusSelector = selectorPool.GetActiveElements().Find((PickBonusSelector s) => s.bonus == bonus);
		if (pickBonusSelector != null)
		{
			selectorPool.Despawn(pickBonusSelector);
		}
		itemInfoPanel.HideAllCommonInfo();
	}

	private void SetCollectionButtons(bool canCollect, bool canLeave)
	{
		getAllButton.interactable = canCollect;
		leaveButton.interactable = canLeave;
	}

	private void _OnHideAllInfo()
	{
		itemInfoPanel.HideAllCommonInfo();
	}
}
