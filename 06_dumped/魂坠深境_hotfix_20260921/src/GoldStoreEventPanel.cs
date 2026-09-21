using System;
using System.Collections.Generic;
using Common.UI;
using Spine.Unity;
using UnityEngine;

public class GoldStoreEventPanel : WorldEventPanel
{
	public enum AnimState
	{
		Null,
		Idle,
		Buy,
		Sell
	}

	public const string npcIdleAniName = "#idle#";

	public const string npcEnterBuyPanelAniName = "#进入购买卡牌#";

	public const string npcEnterSellPanelAniName = "#进入移除卡牌#";

	public const string npcBuyItemAniName = "#购买#";

	public const string npcRefreshItemAniName = "#换一批#";

	public const string npcSellItemAniName = "#删卡#";

	private static readonly int animatorParameter_State = Animator.StringToHash("State");

	private static readonly int animatorParameter_Reset = Animator.StringToHash("Reset");

	[SerializeField]
	private WorldEventChoiceButtonDisplay choicePrefab;

	[SerializeField]
	public DialoguePlayer buyPanelDialoguePlayer;

	[SerializeField]
	public DialoguePlayer menuPanelDialoguePlayer;

	[SerializeField]
	public SecondConfirmPanel refreshConfirmPanel;

	[SerializeField]
	public BuyPanel buyPanel;

	[SerializeField]
	public SellPanel sellPanel;

	[SerializeField]
	public Animator animator;

	[SerializeField]
	private PauseButton pauseButton;

	[SerializeField]
	private PlayerCoinDisplay coinDisplay;

	[SerializeField]
	private PlayerCrystalDisplay crystalDisplay;

	[SerializeField]
	private ShowMapButton showMapButton;

	[SerializeField]
	private BftHealthDisplay bftHealthDisplay;

	[SerializeField]
	private DeckButton deckButton;

	[SerializeField]
	private InfluenceDisplayArea influencePanel;

	[SerializeField]
	private SkeletonGraphic npcSkeletonGraphic;

	[SerializeField]
	public LeaveButton exitButton;

	[SerializeField]
	private GameButton selectPanel1_BuyButton;

	[SerializeField]
	private GameButton selectPanel1_SellButton;

	[SerializeField]
	private GameButton selectPanel2_BuyButton_Select;

	[SerializeField]
	private GameButton selectPanel2_SellButton_Select;

	[SerializeField]
	private GameButton selectPanel2_BuyButton_Unselect;

	[SerializeField]
	private GameButton selectPanel2_SellButton_Unselect;

	[SerializeField]
	private GameButton selectPanel2_SwitchButton;

	[SerializeField]
	private GameButton backButton;

	[NonSerialized]
	private CellUIPrefabPool<WorldEventChoiceButtonDisplay, WorldEventChoice> choicePool;

	[NonSerialized]
	private AnimState animState;

	[NonSerialized]
	public StoreEvent storeEvent;

	protected override void Init()
	{
		base.Init();
		InitUI(refreshConfirmPanel);
		InitUI(buyPanel);
		InitUI(sellPanel);
		InitUI(pauseButton);
		InitUI(coinDisplay);
		InitUI(crystalDisplay);
		InitUI(showMapButton);
		InitUI(bftHealthDisplay);
		InitUI(deckButton);
		InitUI(influencePanel);
		InitUI(exitButton);
		choicePool = GetPrefabPool<WorldEventChoiceButtonDisplay, WorldEventChoice>(choicePrefab, choicePrefab.transform.parent);
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinishEvent;
		selectPanel1_BuyButton.onClick.AddListener(_OnBuyButtonClicked);
		selectPanel1_SellButton.onClick.AddListener(_OnSellButtonClicked);
		selectPanel2_BuyButton_Select.onClick.AddListener(_OnSellButtonClicked);
		selectPanel2_SellButton_Select.onClick.AddListener(_OnBuyButtonClicked);
		selectPanel2_BuyButton_Unselect.onClick.AddListener(_OnBuyButtonClicked);
		selectPanel2_SellButton_Unselect.onClick.AddListener(_OnSellButtonClicked);
		selectPanel2_SwitchButton.onClick.AddListener(_OnSwitchButtonClicked);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		LeaveButton leaveButton = exitButton;
		leaveButton.onLeave = (Action)Delegate.Combine(leaveButton.onLeave, new Action(_OnExitButtonClicked));
		npcSkeletonGraphic.PlayAnimation("#idle#", loop: true);
	}

	public override void Setup(WorldEvent worldEvent)
	{
		base.Setup(worldEvent);
		storeEvent = (StoreEvent)worldEvent;
		sellPanel.Setup();
		animator.SetTrigger(animatorParameter_Reset);
		animState = AnimState.Idle;
		animator.SetInteger(animatorParameter_State, (int)animState);
		exitButton.Show();
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		worldEvent.UpdateWorldEventDisplayInfo();
		animator.SetInteger(animatorParameter_State, (int)animState);
		UpdateChoiceButton();
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinishEvent;
	}

	public void RefreshItems()
	{
		PlayNPCSpineShotAnimation("#换一批#");
		storeEvent.RefreshBuyItems();
		SetDirty();
	}

	public StoreEvent.PricedItem TryGetPricedItem(Item item)
	{
		return storeEvent.itemsForSell.Find((StoreEvent.PricedItem p) => p.item == item);
	}

	public void Finish()
	{
		ThinkingAnalyticsExtends.TrackStore(UIBase.gameState, storeEvent);
		sellPanel.Unset();
		storeEvent.FinishedWorldEvent();
	}

	public void SetAnim(AnimState state)
	{
		animState = state;
		SetDirty();
	}

	public override bool HasAnim()
	{
		return exitButton.HasAnim();
	}

	private void HideInfos()
	{
		buyPanel.UnselectItem();
		sellPanel.Unselect();
	}

	private void PlayerSellPanelIntroDialogue()
	{
		int variableValue = UIBase.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.StoreFreeRecycleItemCount);
		List<StoreEvent.PricedItem> list = storeEvent.soldItems.FindAll((StoreEvent.PricedItem p) => p.free);
		if (variableValue > 0 && variableValue > list.Count)
		{
			UIBase.gameState.actionManager.EnqueueActionToEnd(new TriggerDialogueAction(storeEvent.Config.collectorDialogue, buyPanelDialoguePlayer));
		}
	}

	private void UpdateChoiceButton()
	{
		choicePool.DespawnAll();
		foreach (WorldEventChoice item in worldEvent.currentDisplayInfo.choices.OrEmptyIfNull())
		{
			choicePool.Spawn(item).GetComponent<CanvasGroup>().alpha = 0f;
		}
		ClavisDef clavis = storeEvent.Config.clavis;
		if (!UIBase.applicationState.clavisManager.IsActivated(clavis))
		{
			choicePool.Spawn(new WorldEventChoice
			{
				buttonText = clavis.GetMajorText(UIBase.applicationState),
				descriptionText = clavis.GetMinorText(UIBase.applicationState),
				executeAbleChecker = () => UIBase.applicationState.clavisManager.IsUnlocked(clavis),
				executeAction = _OnClavisButtonClicked
			});
		}
	}

	private void _OnActionFrameFinishEvent()
	{
		SetDirty();
	}

	private void _OnExitButtonClicked()
	{
		StoreEvent.StoreConfig storeConfig = ScriptableObjectSingleton<GameDataManager>.instance.storeConfig;
		List<Item> list = UIBase.gameState.playerState.DeckContainer.GetAllItems().FindAll((Item i) => i.treasureValue > 0);
		if (UIBase.gameState.showConfirm && list.Count > 0)
		{
			UIBase.gameState.UIManager.leaveConfirmPanel.Setup(new LeaveConfirmPanel.Info
			{
				text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unsell_treasure,
				onConfirm = delegate
				{
					Finish();
					SetDirty();
				},
				onCancel = delegate
				{
					SetAnim(AnimState.Sell);
					exitButton.Show();
				}
			});
		}
		else
		{
			UIHelper.DisableAllButton();
			storeEvent.PlayRoomNpcShootAnimation("#退场#", resetToIdle: false);
			if (UIBase.applicationState.settingManager.uiTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X1)
			{
				UIBase.gameState.actionManager.EnqueueAction(new TriggerDialogueAction(storeConfig.exitDialogue, menuPanelDialoguePlayer, halt: true));
			}
			HideInfos();
			Finish();
			UIBase.gameState.actionManager.EnqueueAction(new ActionAction(UIHelper.EnableAllButton));
		}
	}

	private void _OnSellButtonClicked()
	{
		animState = AnimState.Sell;
		HideInfos();
		PlayerSellPanelIntroDialogue();
		PlayNPCSpineShotAnimation("#进入移除卡牌#");
		sellPanel.SetActiveAndDirty(active: true);
		SetDirty();
	}

	private void _OnBuyButtonClicked()
	{
		animState = AnimState.Buy;
		HideInfos();
		PlayNPCSpineShotAnimation("#进入购买卡牌#");
		buyPanel.SetActiveAndDirty(active: true);
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		animState = AnimState.Idle;
		HideInfos();
		SetDirty();
	}

	private void _OnClavisButtonClicked()
	{
		UIBase.applicationState.clavisManager.ActivatedClavis(storeEvent.Config.clavis, silent: false, SetDirty);
	}

	private void _OnSwitchButtonClicked()
	{
		switch (animState)
		{
		case AnimState.Buy:
			animState = AnimState.Sell;
			break;
		case AnimState.Sell:
			animState = AnimState.Buy;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		HideInfos();
		SetDirty();
	}

	public void PlayNPCSpineShotAnimation(string aniName)
	{
		npcSkeletonGraphic.PlayAnimation(aniName, loop: false).Complete += delegate
		{
			npcSkeletonGraphic.PlayAnimation("#idle#", loop: true);
		};
	}
}
