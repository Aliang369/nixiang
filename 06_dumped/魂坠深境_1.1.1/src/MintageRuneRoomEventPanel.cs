using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

public class MintageRuneRoomEventPanel : WorldEventPanel
{
	public enum AnimState
	{
		Null,
		Idle,
		Mintage
	}

	private static readonly int animatorParameter_State = Animator.StringToHash("State");

	private static readonly int animatorParameter_Reset = Animator.StringToHash("Reset");

	[SerializeField]
	private WorldEventChoiceButtonDisplay choicePrefab;

	[SerializeField]
	public MintageRunePanel_Event mintageRunePanel;

	[SerializeField]
	private LeaveButton exitButton;

	[SerializeField]
	private GameObject titlePivot;

	[SerializeField]
	private TextEmergeEffect dialogBoxTextEffect;

	[SerializeField]
	private Animator animator;

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

	[NonSerialized]
	private CellUIPrefabPool<WorldEventChoiceButtonDisplay, WorldEventChoice> choicePool;

	[NonSerialized]
	public AnimState animState;

	[NonSerialized]
	public bool viewed;

	public MintageRuneStoneEvent.MintageRuneRoomConfig config => ScriptableObjectSingleton<GameDataManager>.instance.mintageRuneRoomConfig;

	private bool RuneMintaged => mintageRunePanel.mintaged;

	public bool EndlessRuneRegenerate
	{
		get
		{
			PathLocation currentLocation = UIBase.gameState.dungeonMapManager.currentLocation;
			if (currentLocation != null)
			{
				return currentLocation.variantType == PathLocation.LocationVariantType.AttachRuneEndlessRegenerate;
			}
			return false;
		}
	}

	public MintageRuneStoneEvent mintageEvent => worldEvent as MintageRuneStoneEvent;

	public bool IsEnhanceRune => mintageEvent.IsEnhanceRune;

	public bool IsAiMintagePanelActive
	{
		get
		{
			if (base.gameObject.activeInHierarchy)
			{
				return animState == AnimState.Mintage;
			}
			return false;
		}
	}

	protected override void Init()
	{
		base.Init();
		GetPrefabPool(ref choicePool, choicePrefab);
		LeaveButton leaveButton = exitButton;
		leaveButton.onLeave = (Action)Delegate.Combine(leaveButton.onLeave, new Action(_OnExitButtonClicked));
		InitUI(mintageRunePanel);
		InitUI(pauseButton);
		InitUI(coinDisplay);
		InitUI(crystalDisplay);
		InitUI(showMapButton);
		InitUI(bftHealthDisplay);
		InitUI(deckButton);
		InitUI(exitButton);
		InitUI(influencePanel);
	}

	public override void Setup(WorldEvent worldEvent)
	{
		base.Setup(worldEvent);
		titlePivot.SetActive(value: true);
		dialogBoxTextEffect.text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_attach_rune_init_text;
		mintageRunePanel.Reset();
		animator.speed = UIBase.applicationState.settingManager.GetUIScale();
		animator.SetTrigger(animatorParameter_Reset);
		animState = AnimState.Idle;
		exitButton.Show();
		SetDirty();
		viewed = false;
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		animator.speed = UIBase.applicationState.settingManager.GetUIScale();
		animator.SetInteger(animatorParameter_State, (int)animState);
		if (RuneMintaged)
		{
			titlePivot.SetActive(value: false);
			dialogBoxTextEffect.text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_attach_rune_after_text;
		}
		UpdateChoiceButton();
	}

	private void UpdateChoiceButton()
	{
		choicePool.DespawnAll();
		foreach (WorldEventChoice item in worldEvent.currentDisplayInfo.choices.OrEmptyIfNull())
		{
			choicePool.Spawn(item).GetComponent<CanvasGroup>().alpha = 0f;
		}
		if (!UIBase.applicationState.clavisManager.IsActivated(config.clavis1))
		{
			choicePool.Spawn(new WorldEventChoice
			{
				buttonText = config.clavis1.GetMajorText(UIBase.applicationState),
				descriptionText = config.clavis1.GetMinorText(UIBase.applicationState),
				executeAbleChecker = () => UIBase.applicationState.clavisManager.IsUnlocked(config.clavis1),
				executeAction = _OnClavis1ButtonClicked
			});
		}
		if (!UIBase.applicationState.clavisManager.IsActivated(config.clavis2))
		{
			choicePool.Spawn(new WorldEventChoice
			{
				buttonText = config.clavis2.GetMajorText(UIBase.applicationState),
				descriptionText = config.clavis2.GetMinorText(UIBase.applicationState),
				executeAbleChecker = () => UIBase.applicationState.clavisManager.IsUnlocked(config.clavis2),
				executeAction = _OnClavis2ButtonClicked
			});
		}
		if (!RuneMintaged)
		{
			if (IsEnhanceRune)
			{
				choicePool.Spawn(new WorldEventChoice
				{
					buttonText = config.enhanceRuneChoiceText.choiceName,
					descriptionText = config.enhanceRuneChoiceText.choiceDescription,
					executeAbleChecker = () => true,
					executeAction = _OnEnhanceRuneButtonClicked
				});
			}
			else
			{
				choicePool.Spawn(new WorldEventChoice
				{
					buttonText = config.mintageChoiceText.choiceName,
					descriptionText = config.mintageChoiceText.choiceDescription,
					executeAbleChecker = () => !UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.MintageRuneCantMintage),
					executeAction = _OnOpenMintagePanelButtonClicked
				}).transform.SetSiblingIndex(1);
			}
		}
		else if (EndlessRuneRegenerate)
		{
			choicePool.Spawn(new WorldEventChoice
			{
				buttonText = config.regenerateChoiceText.choiceName,
				descriptionText = config.regenerateChoiceText.choiceDescription.FormatWith(config.regenerateRunePrice),
				executeAbleChecker = () => UIBase.gameState.playerState.HasEnoughCoin(config.regenerateRunePrice),
				executeAction = _OnRegenerateRuneButtonClicked
			}).transform.SetSiblingIndex(1);
		}
	}

	private void Exit()
	{
		ThinkingAnalyticsExtends.TrackMintageRuneEvent(UIBase.gameState, mintageRunePanel);
		UIHelper.DisableAllButton();
		mintageRunePanel.Unset();
		worldEvent.FinishedWorldEvent();
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(UIHelper.EnableAllButton));
	}

	public override bool HasAnim()
	{
		return exitButton.HasAnim();
	}

	private void _OnClavis1ButtonClicked()
	{
		UIBase.applicationState.clavisManager.ActivatedClavis(config.clavis1, silent: false, SetDirty);
	}

	private void _OnClavis2ButtonClicked()
	{
		UIBase.applicationState.clavisManager.ActivatedClavis(config.clavis2, silent: false, SetDirty);
	}

	private void _OnOpenMintagePanelButtonClicked()
	{
		viewed = true;
		animState = AnimState.Mintage;
		SetDirty();
	}

	public bool CanOpenMintageForAiSession()
	{
		if (base.gameObject.activeInHierarchy && animState == AnimState.Idle && !RuneMintaged && !IsEnhanceRune)
		{
			return !UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.MintageRuneCantMintage);
		}
		return false;
	}

	public void OpenMintagePanelForAiSession()
	{
		if (CanOpenMintageForAiSession())
		{
			_OnOpenMintagePanelButtonClicked();
		}
	}

	private void _OnEnhanceRuneButtonClicked()
	{
		List<Item> toEnhanceItems = new List<Item>();
		SelectItemInfo info = SelectItemInfo.SimpleSelectSingleItem(UIBase.gameState.playerState.bft.deckContainer.GetAllItems(), OnConfirm, OnCancel, toEnhanceItems, SelectableChecker, () => config.enhanceRuneChoiceText.choiceDescription);
		UIBase.gameState.actionManager.EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.PileSelect));
		viewed = true;
		static void OnCancel()
		{
		}
		void OnConfirm()
		{
			foreach (Item item in toEnhanceItems)
			{
				foreach (Rune item2 in item.Runes.Where((Rune r) => r.def.stacking))
				{
					ChangeRuneAction action = new ChangeRuneAction(item, item2.def, item2.value, null);
					UIBase.gameState.actionManager.EnqueueAction(action);
				}
			}
			mintageRunePanel.mintaged = true;
			SetDirty();
		}
		static bool SelectableChecker(Item item)
		{
			if (item.Runes.Any((Rune r) => r.def.stacking))
			{
				return true;
			}
			return false;
		}
	}

	private void _OnRegenerateRuneButtonClicked()
	{
		viewed = false;
		mintageRunePanel.Reset();
		UIBase.gameState.actionManager.EnqueueAction(new ChangeCoinAction(-config.regenerateRunePrice));
		mintageEvent.ShowMintageEffectObject();
		SetDirty();
	}

	private void _OnExitButtonClicked()
	{
		if (UIBase.gameState.showConfirm && !viewed)
		{
			UIBase.gameState.UIManager.leaveConfirmPanel.Setup(new LeaveConfirmPanel.Info
			{
				text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unview_rune,
				onConfirm = Exit,
				onCancel = delegate
				{
					exitButton.Show();
				}
			});
		}
		else
		{
			Exit();
		}
	}
}
