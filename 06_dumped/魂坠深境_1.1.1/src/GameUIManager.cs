using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : UIManager
{
	[SerializeField]
	public SkeletonAnimation enemyCrystalAnim;

	[SerializeField]
	public VirtualCamera gameVirtualCamera;

	[SerializeField]
	public Canvas ui3DBaseCanvas;

	[SerializeField]
	public Canvas ui3DCameraCanvas;

	[SerializeField]
	public Canvas ui3DWorldCanvas;

	[SerializeField]
	public Canvas mainCanvas;

	[SerializeField]
	public CustomRaycaster[] raycasters;

	[SerializeField]
	public RectTransform basePanels;

	[SerializeField]
	public RectTransform secondaryPanels;

	[SerializeField]
	public RectTransform tertiaryPanels;

	[SerializeField]
	public CanvasGroup basePanelsRoot;

	[SerializeField]
	public CanvasGroup secondaryPanelsRoot;

	[SerializeField]
	public CanvasGroup tertiaryPanelsRoot;

	[SerializeField]
	public BannerHintDisplay bannerHintDisplay;

	[SerializeField]
	public RectTransform uiVFXRoot;

	[SerializeField]
	public RectTransform ui3DBaseCanvasFXRoot;

	[SerializeField]
	public RectTransform ui3DCameraCanvasVFXRoot;

	[SerializeField]
	public RectTransform ui3DWorldCanvasVFXRoot;

	[SerializeField]
	public Transform scene3DRoot;

	[SerializeField]
	public Transform sceneVFXRoot;

	[SerializeField]
	public DungeonMapPanel_Map dungeonMapPanel;

	[SerializeField]
	public DungeonMapPanel_Preview dungeonMapPanel_Preview;

	[SerializeField]
	public BattlePanel battlePanel;

	[SerializeField]
	public UpgradePanel_Select upgradePanel_Select;

	[SerializeField]
	public MintageRunePanel_Select mintageRunePanel_Select;

	[SerializeField]
	public PickBonusSelectionPanel pickSelectPanel;

	[SerializeField]
	public TrigramsDisplay trigramsDisplay;

	[SerializeField]
	public StanceDisplay stanceDisplay;

	[SerializeField]
	public RitualDisplay ritualDisplay;

	[SerializeField]
	public BattleItemSelectPanel battleItemSelectPanel;

	[SerializeField]
	public BattleTableItemSelectPanel battleTableItemSelectPanel;

	[SerializeField]
	public BattleSkillItemSelectPanel battleSkillItemSelectPanel;

	[SerializeField]
	public BattleSkillTableItemSelectPanel battleSkillTableItemSelectPanel;

	[SerializeField]
	public ChoiceSelectPanel choiceSelectPanel;

	[SerializeField]
	public InfluenceSelectPanel influenceSelectPanel;

	[SerializeField]
	public EnchantSelectPanel enchantSelectPanel;

	[SerializeField]
	public RuneSelectPanel runeSelectPanel;

	[SerializeField]
	public SkillSelectPanel skillSelectPanel;

	[SerializeField]
	public MultiInfoDisplayPanel multiInfoDisplayPanel;

	[SerializeField]
	public NormalEventPanel worldEventPanel_Normal;

	[SerializeField]
	public NormalEventPanel worldEventPanel_Welcome;

	[SerializeField]
	public GoldStoreEventPanel worldEventPanel_GoldStore;

	[SerializeField]
	public UpgradeRoomEventPanel worldEventPanel_UpgradeRoom;

	[SerializeField]
	public CrystalUpgradeEventPanel worldEventPanel_CrystalUpgrade;

	[SerializeField]
	public AfterBossUpgradeEventPanel worldEventPanel_AfterBossUpgrade;

	[SerializeField]
	public MintageRuneRoomEventPanel worldEventPanel_MintageRune;

	[SerializeField]
	public BonusLocationPanel bonusLocationPanel;

	[SerializeField]
	public AvatarDialoguePanel avatarDialoguePanel;

	[SerializeField]
	public InputUserNamePanel inputUserNamePanel;

	[SerializeField]
	public StartGameBuffConfigPanel startGameBuffConfigPanel;

	[SerializeField]
	public PVPDefenderPreviewPanel pvpDefenderPreviewPanel;

	[SerializeField]
	public HUDPanel hudPanel;

	[SerializeField]
	public RecommendItemPanel recommendItemPanel;

	[SerializeField]
	public PickBonusSelectionPanel_Common bonusPanel;

	[SerializeField]
	public DialoguePanel dialoguePanel;

	[SerializeField]
	public DialoguePanel cutsceneDialoguePanel;

	[SerializeField]
	public BattleSkillConfirmPanel battleSkillConfirmPanel;

	[SerializeField]
	public DeckPanel deckPanel;

	[SerializeField]
	public DrawPileItemArea drawPileArea;

	[SerializeField]
	public WastePileItemArea wastePileArea;

	[SerializeField]
	public DeckItemSelectPanel deckItemSelectPanel;

	[SerializeField]
	public SkillPickItemPanel skillPickItemPanel;

	[SerializeField]
	public ItemSpellConfirmPanel itemSpellConfirmPanel;

	[SerializeField]
	public BattleItemInfoPanel battleItemInfoPanel;

	[SerializeField]
	public QuickHintPanel quickHintPanel;

	[SerializeField]
	public FinishHintPanel finishHintPanel;

	[SerializeField]
	public EnvironmentInfluenceHintDisplay environmentInfluenceHintDisplay;

	[SerializeField]
	public ClavisHint clavisHint;

	[SerializeField]
	public ForerunnerBasePanel forerunnerPanel;

	[SerializeField]
	public InfluenceInfoPanel influenceInfoPanel;

	[SerializeField]
	public UniversalCommonInfoPanel universalCommonInfoPanel;

	[SerializeField]
	public GameOverPanel gameOverPanel;

	[SerializeField]
	public DeathPendingPanel deathPendingPanel;

	[SerializeField]
	public SettingPanel settingPanel;

	[SerializeField]
	public ShowMapFogPanel showMapFogPanel;

	[SerializeField]
	public SelectSkillActivatePanel selectSkillPanel;

	[SerializeField]
	public SubtitlePanel subtitlePanel;

	[SerializeField]
	public DarkScreenPanel darkScreenPanel;

	[SerializeField]
	public ActivatePotentialTalentPanel activatePotentialTalentPanel;

	[SerializeField]
	public StoryNotePanel storyNotePanel;

	[SerializeField]
	public CutscenePanel cutscenePanel;

	[SerializeField]
	public CompanionActionPanel companionActionPanel;

	[SerializeField]
	public LeaveConfirmPanel leaveConfirmPanel;

	[SerializeField]
	public GlobalRewardPackageDisplay globalRewardInfoDisplay;

	[SerializeField]
	public SecondConfirmPanel commonSecondConfirmPanel;

	[SerializeField]
	public GuideUIPanel guideUIPanel;

	[NonSerialized]
	public ItemInfoDisplayBase pointerOverItemInfoDisplay;

	[NonSerialized]
	public WorldEventPanel currentEventPanel;

	public PlayerBftUIDisplay playerBftUIDisplay => battlePanel.playerBftUIDisplay;

	public PlayerItemArea playerItemArea => playerBftUIDisplay.tableItemArea;

	public EnemyBftUIDisplay enemyBftUIDisplay => battlePanel.enemyBftUIDisplay;

	protected override void OnInit()
	{
		UIManager.applicationState.eventManager.UIHideAllInfoEvent.AddListener(infoPanelManager.HideAllPanel);
		gameVirtualCamera.BindVirtualCamera(UIManager.gameState);
		InitUI(bannerHintDisplay);
		InitUI(battlePanel);
		InitUI(dungeonMapPanel);
		InitUI(dungeonMapPanel_Preview);
		InitUI(upgradePanel_Select);
		InitUI(mintageRunePanel_Select);
		InitUI(bonusPanel);
		InitUI(pickSelectPanel);
		InitUI(hudPanel);
		InitUI(recommendItemPanel);
		InitUI(trigramsDisplay);
		InitUI(stanceDisplay);
		InitUI(ritualDisplay);
		InitUI(battleItemSelectPanel);
		InitUI(battleTableItemSelectPanel);
		InitUI(battleSkillItemSelectPanel);
		InitUI(battleSkillTableItemSelectPanel);
		InitUI(choiceSelectPanel);
		InitUI(influenceSelectPanel);
		InitUI(enchantSelectPanel);
		InitUI(runeSelectPanel);
		InitUI(skillSelectPanel);
		InitUI(multiInfoDisplayPanel);
		InitUI(dialoguePanel);
		InitUI(cutsceneDialoguePanel);
		InitUI(battleSkillConfirmPanel);
		InitUI(drawPileArea);
		InitUI(wastePileArea);
		InitUI(deckPanel);
		InitUI(deckItemSelectPanel);
		InitUI(skillPickItemPanel);
		InitUI(itemSpellConfirmPanel);
		InitUI(battleItemInfoPanel);
		InitUI(quickHintPanel);
		InitUI(finishHintPanel);
		InitUI(clavisHint);
		InitUI(influenceInfoPanel);
		InitUI(universalCommonInfoPanel);
		InitUI(gameOverPanel);
		InitUI(deathPendingPanel);
		InitUI(forerunnerPanel);
		InitUI(settingPanel);
		InitUI(showMapFogPanel);
		InitUI(worldEventPanel_Normal);
		InitUI(worldEventPanel_Welcome);
		InitUI(worldEventPanel_GoldStore);
		InitUI(worldEventPanel_UpgradeRoom);
		InitUI(worldEventPanel_CrystalUpgrade);
		InitUI(worldEventPanel_AfterBossUpgrade);
		InitUI(worldEventPanel_MintageRune);
		InitUI(selectSkillPanel);
		InitUI(subtitlePanel);
		InitUI(darkScreenPanel);
		InitUI(bonusLocationPanel);
		InitUI(activatePotentialTalentPanel);
		InitUI(guideUIPanel);
		InitUI(avatarDialoguePanel);
		InitUI(storyNotePanel);
		InitUI(cutscenePanel);
		InitUI(companionActionPanel);
		InitUI(inputUserNamePanel);
		InitUI(startGameBuffConfigPanel);
		InitUI(pvpDefenderPreviewPanel);
		InitUI(environmentInfluenceHintDisplay);
		InitUI(leaveConfirmPanel);
		InitUI(globalRewardInfoDisplay);
		InitUI(commonSecondConfirmPanel);
		ApplicationEmptyUIArea[] componentsInChildren = GetComponentsInChildren<ApplicationEmptyUIArea>(includeInactive: true);
		foreach (ApplicationEmptyUIArea child in componentsInChildren)
		{
			InitUI(child);
		}
		SetDirty();
	}

	protected override void OnDispose()
	{
		UIManager.applicationState.eventManager.UIHideAllInfoEvent.RemoveListener(infoPanelManager.HideAllPanel);
	}

	public Canvas GetCanvas(CanvasType canvasType)
	{
		return (Canvas)(canvasType switch
		{
			CanvasType.UI3DBaseCanvas => ui3DBaseCanvas, 
			CanvasType.UI3DCameraCanvas => ui3DCameraCanvas, 
			CanvasType.UI3DWorldCanvas => ui3DWorldCanvas, 
			CanvasType.MainCanvas => mainCanvas, 
			_ => throw new ArgumentOutOfRangeException("canvasType", canvasType, null), 
		});
	}
}
