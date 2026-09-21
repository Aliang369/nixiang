using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusLocationPanel : WorldEventPanel
{
	public class GamePanel : UIBase
	{
		[NonSerialized]
		protected BonusLocationEvent bonusEvent;

		public virtual void Setup(BonusLocationEvent bonusEvent)
		{
			this.bonusEvent = bonusEvent;
		}

		public virtual void Unset()
		{
			if (this != null && base.gameObject != null)
			{
				StopAllCoroutines();
				base.gameObject.SetActive(value: false);
			}
		}
	}

	[SerializeField]
	private WorldEventChoiceButtonDisplay choicePrefab;

	[SerializeField]
	private GameObject title;

	[SerializeField]
	private ChooseSequenceGamePanel chooseSequenceGamePanel;

	[SerializeField]
	private MinerGamePanel minerGamePanel;

	[SerializeField]
	private RouletteGamePanel rouletteGamePanel;

	[SerializeField]
	private ShellGamePanel shellGamePanel;

	[SerializeField]
	private RectTransform gamePanelPivot;

	[SerializeField]
	private GameObject introArea;

	[SerializeField]
	private TextEmergeEffect openButtonDescriptionTextEffect;

	[SerializeField]
	private TextMeshProUGUI introDescription;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private LeaveButton leaveButton;

	[SerializeField]
	private ItemInfoPanel itemInfoPanel;

	[NonSerialized]
	private CellUIPrefabPool<WorldEventChoiceButtonDisplay, WorldEventChoice> choicePool;

	[NonSerialized]
	private GamePanel currentPanel;

	[NonSerialized]
	private UncollectedStatusCommonInfoPanel uncollectedStatusInfoPanel;

	[NonSerialized]
	public bool isPlayingGame;

	protected BonusLocationEvent bonusEvent => worldEvent as BonusLocationEvent;

	protected override void Init()
	{
		chooseSequenceGamePanel.gameObject.SetActive(value: false);
		minerGamePanel.gameObject.SetActive(value: false);
		rouletteGamePanel.gameObject.SetActive(value: false);
		shellGamePanel.gameObject.SetActive(value: false);
		introArea.SetActive(value: true);
		GetPrefabPool(ref choicePool, choicePrefab);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyUIClicked);
		LeaveButton obj = leaveButton;
		obj.onLeave = (Action)Delegate.Combine(obj.onLeave, (Action)delegate
		{
			bonusEvent.FinishedWorldEvent();
		});
		InitUI(itemInfoPanel);
		InitUI(leaveButton);
		uncollectedStatusInfoPanel = new UncollectedStatusCommonInfoPanel(() => itemInfoPanel);
	}

	public override void Setup(WorldEvent worldEvent)
	{
		base.Setup(worldEvent);
		title.gameObject.SetActive(value: true);
		introDescription.text = bonusEvent.introDescription;
		backgroundImage.sprite = worldEvent.backgroundImage;
		backgroundImage.gameObject.SetActive(backgroundImage.sprite != null);
		base.gameObject.SetActive(value: true);
		introArea.SetActive(value: true);
		leaveButton.Hide();
		isPlayingGame = false;
		SetDirty();
	}

	public override void Unset()
	{
		choicePool.DespawnAll();
		HideCurrentGamePanel();
		base.Unset();
		backgroundImage.sprite = null;
		backgroundImage.gameObject.SetActive(value: false);
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyUIClicked);
	}

	protected override void UpdateDisplay()
	{
		if (isPlayingGame)
		{
			return;
		}
		choicePool.DespawnAll();
		foreach (WorldEventChoice item in worldEvent.currentDisplayInfo.choices.OrEmptyIfNull())
		{
			choicePool.Spawn(item);
		}
	}

	public override bool HasAnim()
	{
		return leaveButton.HasAnim();
	}

	public void FinishBonusLocation(List<BonusLocationBonus> finalBonus)
	{
		if (finalBonus.Count > 0)
		{
			BattleBonus info = new BattleBonus(finalBonus.ConvertAll((BonusLocationBonus b) => b.bonus))
			{
				title = ScriptableObjectSingleton<TextDataPreset>.instance.term_select_bonus
			};
			UIBase.gameState.UIManager.bonusPanel.Setup(info);
		}
		else
		{
			HideCurrentGamePanel();
			introArea.SetActive(value: true);
			openButtonDescriptionTextEffect.StartEmergeText(bonusEvent.leaveWithoutBonusDescription, delegate
			{
				leaveButton.Show();
			});
		}
	}

	public void SelectEnd()
	{
		worldEvent.currentDisplayInfo.ClearChoice();
		SetDirty();
		HideCurrentGamePanel();
		introArea.SetActive(value: true);
		openButtonDescriptionTextEffect.StartEmergeText(bonusEvent.leaveDescription, delegate
		{
			leaveButton.Show();
		});
	}

	public void RefuseAndFight()
	{
		HideCurrentGamePanel();
		introArea.SetActive(value: true);
		worldEvent.currentDisplayInfo.ClearChoice();
		openButtonDescriptionTextEffect.StartEmergeText(bonusEvent.refuseAndFightDescription, UpdateFightButton);
		SetDirty();
		void Fight()
		{
			EncounterSetupInfo setupInfo = UIBase.gameState.currentEncounter.setupInfo;
			setupInfo.fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
			setupInfo.roomBonus = new BattleBonus();
			FinishBattleEncounterAction.AddNormalFightBonus(UIBase.gameState, setupInfo.roomBonus);
			setupInfo.enemySpawnType = EnemyEncounterType.Normal;
			worldEvent.FinishedWorldEvent(exitRoom: false);
			UIBase.gameState.actionManager.EnqueueAction(new SaveRunningGameAction());
			UIBase.gameState.actionManager.EnqueueAction(new TransitReloadRoomAction(setupInfo));
		}
		void UpdateFightButton()
		{
			worldEvent.currentDisplayInfo.AddChoice(new WorldEventChoice
			{
				buttonText = bonusEvent.fightConfirmText,
				descriptionText = bonusEvent.fightConfirmDescriptionText,
				executeAbleChecker = () => true,
				executeAction = Fight
			});
			SetDirty();
		}
	}

	public void HideCurrentGamePanel()
	{
		if (!(currentPanel == null))
		{
			currentPanel.Unset();
			DisposeUI(currentPanel);
			UnityEngine.Object.Destroy(currentPanel.gameObject);
			currentPanel = null;
		}
	}

	public void ShowCurrentGamePanel()
	{
		GamePanel gamePanel = null;
		currentPanel = UnityEngine.Object.Instantiate(bonusEvent.currentPanelType switch
		{
			BonusLocationEvent.BonusPanelType.Shell => shellGamePanel, 
			BonusLocationEvent.BonusPanelType.Miner => minerGamePanel, 
			BonusLocationEvent.BonusPanelType.Roulette => rouletteGamePanel, 
			BonusLocationEvent.BonusPanelType.ChooseSequence => chooseSequenceGamePanel, 
			_ => throw new ArgumentOutOfRangeException(), 
		}, gamePanelPivot);
		InitUI(currentPanel);
		currentPanel.Setup(bonusEvent);
	}

	public void OpenSelect()
	{
		choicePool.DespawnAll();
		ShowCurrentGamePanel();
		introArea.SetActive(value: false);
		title.gameObject.SetActive(value: false);
		isPlayingGame = true;
		SetDirty();
	}

	public void ConfigInfoPanel(CellUIPrefabPool<BonusLocationBonusCell, BonusLocationBonus> pool)
	{
		pool.onSetup = (Action<BonusLocationBonusCell>)Delegate.Combine(pool.onSetup, (Action<BonusLocationBonusCell>)delegate(BonusLocationBonusCell cell)
		{
			cell.slotDisplay.GetModule<InfoModule>().infoPanel = uncollectedStatusInfoPanel;
		});
	}

	private void _OnEmptyUIClicked()
	{
		itemInfoPanel.HideAllCommonInfo();
	}
}
