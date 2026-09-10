using Common.UI;
using TMPro;

public class ForerunnerLegacyRecordCell : CellUI<ForerunnerLegacy>
{
	public ItemDisplayBase itemDisplayBase;

	public SkillDisplay skillDisplay;

	public TextMeshProUGUI leaveLocationText;

	public TextMeshProUGUI leaveNoteText;

	public TextMeshProUGUI leaveSignatureText;

	public TextMeshProUGUI voteCountText;

	public GameButton openLegacyDetailButton;

	protected override void Init()
	{
		InitUI(itemDisplayBase);
		InitUI(skillDisplay);
		openLegacyDetailButton.onClick.AddListener(_OnOpenLegacyDetailButtonClicked);
		if (CellUI<ForerunnerLegacy>.applicationState.fortressState?.uiManager != null)
		{
			itemDisplayBase.GetModule<InfoModule>().infoPanel = CellUI<ForerunnerLegacy>.applicationState.fortressState.uiManager.universalCommonInfoPanel;
			skillDisplay.GetModule<InfoModule>().infoPanel = CellUI<ForerunnerLegacy>.applicationState.fortressState.uiManager.universalCommonInfoPanel;
		}
	}

	protected override void OnSetup(ForerunnerLegacy setupData)
	{
		if (setupData.legacyItem != null)
		{
			itemDisplayBase.Setup(setupData.legacyItem.DisplayItem);
			itemDisplayBase.SetActiveAndDirty(active: true);
			skillDisplay.SetActiveAndDirty(active: false);
		}
		else if (setupData.legacySkill != null)
		{
			skillDisplay.Setup(setupData.legacySkill.DisplaySkill);
			itemDisplayBase.SetActiveAndDirty(active: false);
			skillDisplay.SetActiveAndDirty(active: true);
		}
		leaveLocationText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_leave_location_at_arg0.FormatWith(setupData.sourceDraftMap?.GetAssetPreData(CellUI<ForerunnerLegacy>.applicationState.assetCatalog)?.mapDisplayName ?? "");
		leaveNoteText.text = ForerunnerPresetMessageResolver.ResolveText(setupData);
		leaveSignatureText.text = setupData.signature;
		voteCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_legacy_vote_count_arg0.FormatWith(setupData.SumVoteCount);
	}

	private void _OnOpenLegacyDetailButtonClicked()
	{
		if (CellUI<ForerunnerLegacy>.applicationState.fortressState != null)
		{
			CellUI<ForerunnerLegacy>.applicationState.fortressState.uiManager.storyNotePanel.Setup(StoryNoteInfo.DisplayForerunnerLegacy(base.setupData));
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<ForerunnerLegacy>.applicationState, "档案馆-先行者遗物-留下");
		}
	}
}
