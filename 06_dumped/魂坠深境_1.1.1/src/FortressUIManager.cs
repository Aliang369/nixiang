using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using ContainerOfSoul.Display.Fortress;
using ContainerOfSoul.Display.Fortress.AchievementDisplay;
using ContainerOfSoul.Display.Fortress.ContractSelectDisplay;
using ContainerOfSoul.Display.Fortress.IndexDisplay;
using ContainerOfSoul.Display.Fortress.Purchase;
using DG.Tweening;
using MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class FortressUIManager : UIManager
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class SubPanel : UIBase
	{
		protected Sequence tweenSequence;

		protected CanvasGroup panelCanvasGroup => GetComponent<CanvasGroup>();

		public virtual IEnumerator ShowCor(float tweenTime = 0.25f)
		{
			SetDirty();
			base.gameObject.SetActive(value: true);
			if (tweenSequence != null && tweenSequence.Playing())
			{
				tweenSequence.Complete();
			}
			tweenSequence = DOTween.Sequence();
			panelCanvasGroup.alpha = 0f;
			tweenSequence.Append(DOTweenModuleUI.DOFade(panelCanvasGroup, 1f, tweenTime));
			tweenSequence.Play();
			yield return tweenSequence.WaitForCompletion();
		}

		public virtual IEnumerator HideCor(float tweenTime = 0.25f)
		{
			SetDirty();
			if (tweenSequence != null && tweenSequence.Playing())
			{
				tweenSequence.Complete();
			}
			tweenSequence = DOTween.Sequence();
			tweenSequence.Append(DOTweenModuleUI.DOFade(panelCanvasGroup, 1f, 0f));
			tweenSequence.Append(DOTweenModuleUI.DOFade(panelCanvasGroup, 0f, tweenTime));
			Sequence sequence = tweenSequence;
			sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, (TweenCallback)delegate
			{
				base.gameObject.SetActive(value: false);
			});
			tweenSequence.Play();
			yield return tweenSequence.WaitForCompletion();
		}
	}

	[SerializeField]
	private CharacterInfoBasePanel characterInfoBasePanelPrefab;

	[SerializeField]
	private IdolBasePanel idolBasePanelPrefab;

	[SerializeField]
	private ForgetShrineBasePanel forgetShrineBasePanelPrefab;

	[SerializeField]
	private SkillConfigPanel skillConfigPanelPrefab;

	[SerializeField]
	private Skill_GemPanel skillGemPanelPrefab;

	[SerializeField]
	private FestivalCheckInPanel festivalCheckInPanelPrefab;

	[SerializeField]
	private CarnivalCheckInPanel carnivalCheckInPanelPrefab;

	[SerializeField]
	private MonthCheckPanel monthCheckPanelPrefab;

	[SerializeField]
	private ForerunnerLegacyOverviewPanel forerunnerLegacyOverviewPanelPrefab;

	[SerializeField]
	private FortressPurchaseUIPanel purchaseUIPanelPrefab;

	[SerializeField]
	private PurchaseRecordPanel purchaseRecordPanelPrefab;

	[SerializeField]
	private FortressMailPanel fortressMailPanelPrefab;

	[SerializeField]
	private FortressGiftCodePanel fortressGiftCodePanelPrefab;

	[SerializeField]
	private GachaPanel gachaPanelPrefab;

	[SerializeField]
	private PurchasePromotionPanel purchasePromotionPanelPrefab;

	[SerializeField]
	private LimitedTimeBundlePanel limitedTimeBundlePanelPrefab;

	[SerializeField]
	private AnnouncementPanel fortressAnnouncementPanelPrefab;

	[SerializeField]
	private DifficultyUnlockStage difficultyUnlockStagePrefab;

	[SerializeField]
	private NewbieFestivalPanel newbieFestivalPanelPrefab;

	[SerializeField]
	private YuanQiActivityPanel yuanQiActivityPanelPrefab;

	[SerializeField]
	private IntermissionPlayPanel intermissionPlayPanelPrefab;

	[SerializeField]
	private PleaseReviewPopup pleaseReviewPopupPrefab;

	[SerializeField]
	private SecondConfirmPanel commonSecondConfirmPanelPrefab;

	[SerializeField]
	private PurchaseConfirmPanel purchaseConfirmPanelPrefab;

	[SerializeField]
	private ItemSkinSetSelectPanel itemSkinSetSelectPanelPrefab;

	[SerializeField]
	private CardSkinOfferPanel cardSkinOfferPanelPrefab;

	[SerializeField]
	private GlobalRewardBundleExchangePanel globalRewardBundleExchangePanelPrefab;

	[SerializeField]
	private RandomGlobalRewardBundleConfirmPanel randomGlobalRewardBundleConfirmPanelPrefab;

	[SerializeField]
	private RecycleObsessionPanel recycleObsessionPanelPrefab;

	[SerializeField]
	private RecommendItemPanel recommendItemPanelPrefab;

	[SerializeField]
	private GachaResultDisplay gachaResultDisplayPrefab;

	[SerializeField]
	private PurchaseBundleCheckPanel purchaseBundleCheckPanelPrefab;

	[SerializeField]
	private CommonGlobalItemsPanel commonGlobalItemsPanelPrefab;

	[SerializeField]
	private GlobalRewardPackageDisplay globalRewardPackageDisplayPrefab;

	[SerializeField]
	private CharacterGetDisplay characterGetDisplayPrefab;

	[SerializeField]
	private CharacterTalentUnlockPanel characterTalentUnlockPanelPrefab;

	[SerializeField]
	private CharacterObsessionPanel characterObsessionPanelPrefab;

	[SerializeField]
	private ItemSkinPreviewPanel itemSkinPreviewPanelPrefab;

	[SerializeField]
	private StoryNotePanel storyNotePanelPrefab;

	[SerializeField]
	private ClavisHint clavisHintPrefab;

	[SerializeField]
	private UniversalCommonInfoPanel universalCommonInfoPanelPrefab;

	[SerializeField]
	private QuickHintPanel quickHintPanelPrefab;

	[SerializeField]
	private BindAccountLoginHint bindAccountLoginHintPrefab;

	[SerializeField]
	public RectTransform panels1;

	[SerializeField]
	public RectTransform stackedPanelPivot;

	[SerializeField]
	public RectTransform panels2;

	[SerializeField]
	public RectTransform panels3;

	[SerializeField]
	public RectTransform panels4;

	[SerializeField]
	public Canvas uiCanvas;

	[SerializeField]
	public VirtualCamera fortressCamera;

	[SerializeField]
	public FortressMainUIPanel mainUIPanel;

	[SerializeField]
	public FortressBaseGameUIPanel baseGameUIPanel;

	[SerializeField]
	public StarMapPanel starMapPanel;

	[SerializeField]
	public FortressIndexUIPanel indexUIPanel;

	[SerializeField]
	public FortressRecordUIPanel recordPanel;

	[SerializeField]
	public FortressAchievementUIPanel achievementUIPanel;

	[SerializeField]
	public DailyQuestPanel dailyQuestPanel;

	[SerializeField]
	public SettingPanel settingPanel;

	[SerializeField]
	public FortressGuidePanel fortressGuidePanel;

	[SerializeField]
	public FortressMainBaseUI fortressMainBaseUI;

	[SerializeField]
	public StorehousePanel storehousePanel;

	[SerializeField]
	public AvatarDialoguePanel avatarDialoguePanel;

	[SerializeField]
	public SeasonDetailPanel seasonDetailPanel;

	[SerializeField]
	public DialoguePanel DialoguePanel;

	[SerializeField]
	public DialoguePanel cutsceneDialoguePanel;

	[SerializeField]
	public CutscenePanel cutscenePanel;

	[SerializeField]
	public RectTransform itemSkinSetPanelRoot;

	[SerializeField]
	public PVPModeMainPanel pvpModeMainPanel;

	[SerializeField]
	public RectTransform basePanels;

	[SerializeField]
	public RectTransform secondaryPanels;

	[SerializeField]
	public Image gachaAnimPanel;

	[SerializeField]
	public RectTransform vfxParent;

	[NonSerialized]
	public ItemSkinSetPanel itemSkinSetPanel;

	[NonSerialized]
	public ItemInfoDisplayBase pointerOverItemInfoDisplay;

	[NonSerialized]
	public SubPanel currentPanel;

	[NonSerialized]
	private Coroutine transformingCoroutine;

	[NonSerialized]
	public CharacterInfoBasePanel characterInfoBasePanel;

	[NonSerialized]
	public CharacterTalentUnlockPanel characterTalentUnlockPanel;

	[NonSerialized]
	public CharacterObsessionPanel characterObsessionPanel;

	[NonSerialized]
	public IdolBasePanel idolBasePanel;

	[NonSerialized]
	public ForgetShrineBasePanel forgetShrineBasePanel;

	[NonSerialized]
	public SkillConfigPanel skillConfigPanel;

	[NonSerialized]
	public Skill_GemPanel skill_GemPanel;

	[NonSerialized]
	public FestivalCheckInPanel festivalCheckInPanel;

	[NonSerialized]
	public CarnivalCheckInPanel carnivalCheckInPanel;

	[NonSerialized]
	public MonthCheckPanel monthCheckPanel;

	[NonSerialized]
	public ForerunnerLegacyOverviewPanel forerunnerLegacyOverviewPanel;

	[NonSerialized]
	public FortressPurchaseUIPanel purchaseUIPanel;

	[NonSerialized]
	public PurchaseRecordPanel purchaseRecordPanel;

	[NonSerialized]
	public FortressMailPanel fortressMailPanel;

	[NonSerialized]
	public FortressGiftCodePanel fortressGiftCodePanel;

	[NonSerialized]
	public GachaPanel gachaPanel;

	[NonSerialized]
	public PurchasePromotionPanel purchasePromotionPanel;

	[NonSerialized]
	public LimitedTimeBundlePanel limitedTimeBundlePanel;

	[NonSerialized]
	public AnnouncementPanel fortressAnnouncementPanel;

	[NonSerialized]
	public DifficultyUnlockStage difficultyUnlockStage;

	[NonSerialized]
	public YuanQiActivityPanel yuanQiActivityPanel;

	[NonSerialized]
	public NewbieFestivalPanel newbieFestivalPanel;

	[NonSerialized]
	public IntermissionPlayPanel intermissionPlayPanel;

	[NonSerialized]
	public GachaResultDisplay gachaResultDisplay;

	[NonSerialized]
	public PleaseReviewPopup pleaseReviewPopup;

	[NonSerialized]
	public SecondConfirmPanel commonSecondConfirmPanel;

	[NonSerialized]
	public PurchaseConfirmPanel purchaseConfirmPanel;

	[NonSerialized]
	public ItemSkinSetSelectPanel itemSkinSetSelectPanel;

	[NonSerialized]
	public CardSkinOfferPanel cardSkinOfferPanel;

	[NonSerialized]
	public GlobalRewardBundleExchangePanel globalRewardBundleExchangePanel;

	[NonSerialized]
	public RandomGlobalRewardBundleConfirmPanel randomGlobalRewardBundleConfirmPanel;

	[NonSerialized]
	public RecycleObsessionPanel recycleObsessionPanel;

	[NonSerialized]
	public RecommendItemPanel recommendItemPanel;

	[NonSerialized]
	public PurchaseBundleCheckPanel purchaseBundleCheckPanel;

	[NonSerialized]
	public CommonGlobalItemsPanel commonGlobalItemsPanel;

	[NonSerialized]
	public GlobalRewardPackageDisplay globalRewardPackageDisplay;

	[NonSerialized]
	public CharacterGetDisplay characterGetDisplay;

	[NonSerialized]
	public ItemSkinPreviewPanel itemSkinPreviewPanel;

	[NonSerialized]
	public StoryNotePanel storyNotePanel;

	[NonSerialized]
	public ClavisHint clavisHint;

	[NonSerialized]
	public UniversalCommonInfoPanel universalCommonInfoPanel;

	[NonSerialized]
	public QuickHintPanel quickHintPanel;

	[NonSerialized]
	public BindAccountLoginHint bindAccountLoginHint;

	public List<Common.UI.UIBase> panel1;

	public List<Common.UI.UIBase> panel2;

	public List<Common.UI.UIBase> panel3;

	public List<Common.UI.UIBase> panel4;

	public bool transforming => transformingCoroutine != null;

	protected override void OnInit()
	{
		currentPanel = mainUIPanel;
		base.gameObject.SetActive(value: false);
		UIManager.applicationState.eventManager.UIHideAllInfoEvent.AddListener(infoPanelManager.HideAllPanel);
		fortressCamera.BindVirtualCamera(UIManager.fortressState);
		Spawn();
		Catalog();
		Initialize();
		ApplicationEmptyUIArea[] componentsInChildren = GetComponentsInChildren<ApplicationEmptyUIArea>(includeInactive: true);
		foreach (ApplicationEmptyUIArea child in componentsInChildren)
		{
			InitUI(child);
		}
		baseGameUIPanel.gameObject.SetActive(value: false);
		indexUIPanel.gameObject.SetActive(value: false);
		achievementUIPanel.gameObject.SetActive(value: false);
		purchaseUIPanel.gameObject.SetActive(value: false);
		void Catalog()
		{
			panel1 = new List<Common.UI.UIBase>();
			panel2 = new List<Common.UI.UIBase>();
			panel3 = new List<Common.UI.UIBase>();
			panel4 = new List<Common.UI.UIBase>();
			panel1.Add(mainUIPanel);
			panel1.Add(baseGameUIPanel);
			panel1.Add(starMapPanel);
			panel1.Add(indexUIPanel);
			panel1.Add(recordPanel);
			panel1.Add(achievementUIPanel);
			panel1.Add(dailyQuestPanel);
			panel1.Add(settingPanel);
			panel1.Add(fortressGuidePanel);
			panel1.Add(fortressMainBaseUI);
			panel1.Add(storehousePanel);
			panel1.Add(avatarDialoguePanel);
			panel1.Add(seasonDetailPanel);
			panel1.Add(DialoguePanel);
			panel1.Add(cutscenePanel);
			panel1.Add(pvpModeMainPanel);
			panel2.Add(characterInfoBasePanel);
			panel2.Add(characterTalentUnlockPanel);
			panel2.Add(characterObsessionPanel);
			panel2.Add(idolBasePanel);
			panel2.Add(forgetShrineBasePanel);
			panel2.Add(skillConfigPanel);
			panel2.Add(skill_GemPanel);
			panel2.Add(festivalCheckInPanel);
			panel2.Add(carnivalCheckInPanel);
			panel2.Add(monthCheckPanel);
			panel2.Add(forerunnerLegacyOverviewPanel);
			panel2.Add(purchaseUIPanel);
			panel2.Add(purchaseRecordPanel);
			panel2.Add(fortressMailPanel);
			panel2.Add(fortressGiftCodePanel);
			panel2.Add(gachaPanel);
			panel2.Add(purchasePromotionPanel);
			panel2.Add(limitedTimeBundlePanel);
			panel2.Add(fortressAnnouncementPanel);
			panel2.Add(difficultyUnlockStage);
			panel2.Add(yuanQiActivityPanel);
			panel2.Add(newbieFestivalPanel);
			panel2.Add(intermissionPlayPanel);
			panel2.Add(pleaseReviewPopup);
			panel4.Add(commonSecondConfirmPanel);
			panel4.Add(purchaseConfirmPanel);
			panel4.Add(globalRewardBundleExchangePanel);
			panel4.Add(itemSkinSetSelectPanel);
			panel4.Add(cardSkinOfferPanel);
			panel4.Add(randomGlobalRewardBundleConfirmPanel);
			panel4.Add(recycleObsessionPanel);
			panel4.Add(recommendItemPanel);
			panel4.Add(gachaResultDisplay);
			panel4.Add(purchaseBundleCheckPanel);
			panel4.Add(commonGlobalItemsPanel);
			panel4.Add(globalRewardPackageDisplay);
			panel4.Add(characterGetDisplay);
			panel4.Add(itemSkinPreviewPanel);
			panel4.Add(storyNotePanel);
			panel4.Add(clavisHint);
			panel4.Add(universalCommonInfoPanel);
			panel4.Add(quickHintPanel);
			panel4.Add(bindAccountLoginHint);
		}
		void Initialize()
		{
			foreach (Common.UI.UIBase item in panel1)
			{
				InitUI(item);
			}
			foreach (Common.UI.UIBase item2 in panel2)
			{
				InitUI(item2);
			}
			foreach (Common.UI.UIBase item3 in panel3)
			{
				InitUI(item3);
			}
			foreach (Common.UI.UIBase item4 in panel4)
			{
				InitUI(item4);
			}
		}
		void Spawn()
		{
			characterInfoBasePanel = SpawnPanel(characterInfoBasePanelPrefab, panels2);
			characterTalentUnlockPanel = SpawnPanel(characterTalentUnlockPanelPrefab, panels2);
			characterObsessionPanel = SpawnPanel(characterObsessionPanelPrefab, panels2);
			idolBasePanel = SpawnPanel(idolBasePanelPrefab, panels2);
			forgetShrineBasePanel = SpawnPanel(forgetShrineBasePanelPrefab, panels2);
			skillConfigPanel = SpawnPanel(skillConfigPanelPrefab, panels2);
			skill_GemPanel = SpawnPanel(skillGemPanelPrefab, panels2);
			festivalCheckInPanel = SpawnPanel(festivalCheckInPanelPrefab, panels2);
			carnivalCheckInPanel = SpawnPanel(carnivalCheckInPanelPrefab, panels2);
			monthCheckPanel = SpawnPanel(monthCheckPanelPrefab, panels2);
			forerunnerLegacyOverviewPanel = SpawnPanel(forerunnerLegacyOverviewPanelPrefab, panels2);
			purchaseUIPanel = SpawnPanel(purchaseUIPanelPrefab, panels2);
			purchaseRecordPanel = SpawnPanel(purchaseRecordPanelPrefab, panels2);
			fortressMailPanel = SpawnPanel(fortressMailPanelPrefab, panels2);
			fortressGiftCodePanel = SpawnPanel(fortressGiftCodePanelPrefab, panels2);
			gachaPanel = SpawnPanel(gachaPanelPrefab, panels2);
			purchasePromotionPanel = SpawnPanel(purchasePromotionPanelPrefab, panels2);
			limitedTimeBundlePanel = SpawnPanel(limitedTimeBundlePanelPrefab, panels2);
			fortressAnnouncementPanel = SpawnPanel(fortressAnnouncementPanelPrefab, panels2);
			difficultyUnlockStage = SpawnPanel(difficultyUnlockStagePrefab, panels2);
			yuanQiActivityPanel = SpawnPanel(yuanQiActivityPanelPrefab, panels2);
			newbieFestivalPanel = SpawnPanel(newbieFestivalPanelPrefab, panels2);
			intermissionPlayPanel = SpawnPanel(intermissionPlayPanelPrefab, panels2);
			gachaResultDisplay = SpawnPanel(gachaResultDisplayPrefab, panels2);
			pleaseReviewPopup = SpawnPanel(pleaseReviewPopupPrefab, panels2);
			commonSecondConfirmPanel = SpawnPanel(commonSecondConfirmPanelPrefab, panels4);
			purchaseConfirmPanel = SpawnPanel(purchaseConfirmPanelPrefab, panels4);
			globalRewardBundleExchangePanel = SpawnPanel(globalRewardBundleExchangePanelPrefab, panels4);
			itemSkinSetSelectPanel = SpawnPanel(itemSkinSetSelectPanelPrefab, panels4);
			cardSkinOfferPanel = SpawnPanel(cardSkinOfferPanelPrefab, panels4);
			randomGlobalRewardBundleConfirmPanel = SpawnPanel(randomGlobalRewardBundleConfirmPanelPrefab, panels4);
			recycleObsessionPanel = SpawnPanel(recycleObsessionPanelPrefab, panels4);
			recommendItemPanel = SpawnPanel(recommendItemPanelPrefab, panels4);
			purchaseBundleCheckPanel = SpawnPanel(purchaseBundleCheckPanelPrefab, panels4);
			commonGlobalItemsPanel = SpawnPanel(commonGlobalItemsPanelPrefab, panels4);
			globalRewardPackageDisplay = SpawnPanel(globalRewardPackageDisplayPrefab, panels4);
			characterGetDisplay = SpawnPanel(characterGetDisplayPrefab, panels4);
			itemSkinPreviewPanel = SpawnPanel(itemSkinPreviewPanelPrefab, panels4);
			storyNotePanel = SpawnPanel(storyNotePanelPrefab, panels4);
			clavisHint = SpawnPanel(clavisHintPrefab, panels4);
			universalCommonInfoPanel = SpawnPanel(universalCommonInfoPanelPrefab, panels4);
			mainUIPanel.ConfigureMatterDisplayInfoPanel(universalCommonInfoPanel);
			quickHintPanel = SpawnPanel(quickHintPanelPrefab, panels4);
			bindAccountLoginHint = SpawnPanel(bindAccountLoginHintPrefab, panels4);
		}
	}

	protected override void OnDispose()
	{
		UIManager.applicationState.eventManager.UIHideAllInfoEvent.RemoveListener(infoPanelManager.HideAllPanel);
		uiCanvas.worldCamera = null;
	}

	public override void HideUI(Common.UI.UIBase ui)
	{
		UIManager.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		base.HideUI(ui);
	}

	protected override void M_StartCoroutine(IEnumerator coroutine)
	{
		StartCoroutine(coroutine);
	}

	public void ToState(SubPanel newPanel)
	{
		if (transformingCoroutine == null)
		{
			transformingCoroutine = StartCoroutine(ToStateCor(newPanel));
		}
	}

	private IEnumerator ToStateCor(SubPanel newPanel)
	{
		SubPanel subPanel = UIManager.fortressState.uiManager.currentPanel;
		UIManager.fortressState.uiManager.currentPanel = newPanel;
		subPanel.SetDirty();
		yield return subPanel.HideCor();
		newPanel.SetDirty();
		yield return newPanel.ShowCor();
		transformingCoroutine = null;
	}

	private T SpawnPanel<T>(T prefab, RectTransform pivot) where T : Common.UI.UIBase
	{
		if (prefab == null)
		{
			Debug.LogError("prefab is null, panel type: " + typeof(T).Name);
			return null;
		}
		T val = UnityEngine.Object.Instantiate(prefab, pivot);
		val.gameObject.SetActive(value: false);
		return val;
	}

	public void ShowStackPanel(UIBase panel)
	{
		panel.transform.parent = stackedPanelPivot;
		panel.transform.SetAsLastSibling();
		ShowUI(panel);
	}
}
