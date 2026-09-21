using System;
using System.Collections;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.DotManagers;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using TMPro;
using UnityEngine;

public class StorehousePanel : UIBase, IShowAsync
{
	public SwitchPanel switchPanel;

	public SwitchTag skillSwitchTag;

	public SwitchTag skillGemSwitchTag;

	public SwitchTag clavisSwitchTag;

	public SwitchTag matterSwitchTag;

	public SwitchTag skinSetSwitchTag;

	public GameButton closeButton;

	public GameObject topBarImage;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI cardSkinTitleText;

	public SkillGemPanel skillGemPanel;

	public ClavisPanel clavisPanel;

	public MatterPanel matterPanel;

	public SkinSetBookPanel skinSetPanel;

	public SkillOverviewPanel skillOverviewPanel;

	public HintRedDot lootRedDot;

	private bool enteredLoot;

	private bool skinSetOnlyMode;

	protected override void Init()
	{
		InitUI(skillGemPanel);
		InitUI(clavisPanel);
		InitUI(skillOverviewPanel);
		InitUI(switchPanel);
		InitUI(skinSetPanel);
		InitUI(lootRedDot);
		closeButton.onClick.AddListener(_OnCloseButtonClicked);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, new Action(_OnTagSwitched));
		SetNormalStorehouseTagsActive(normalMode: true);
		switchPanel.Select(matterSwitchTag);
		lootRedDot.Setup(new CustomModule
		{
			shouldShow = () => UIBase.applicationState.playerInfoManager.newLoots.Any()
		});
		RefreshTitle();
	}

	protected override void Dispose()
	{
		closeButton.onClick.RemoveListener(_OnCloseButtonClicked);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Remove(obj.onTagSwitched, new Action(_OnTagSwitched));
	}

	IEnumerator IShowAsync.ShowAnim()
	{
		enteredLoot = false;
		skinSetOnlyMode = false;
		SetNormalStorehouseTagsActive(normalMode: true);
		switchPanel.gameObject.SetActive(value: true);
		SetActiveAndDirtyWithFadeAnim(active: true);
		switchPanel.targetShowTag = null;
		switchPanel.Select(matterSwitchTag);
		RefreshTitle();
		yield return UIBase.applicationState.matterManager.SyncMatter();
		base.gameObject.SetActive(value: true);
	}

	public void Show()
	{
		this.ShowUI();
	}

	public void ShowSkinSetOnly()
	{
		enteredLoot = false;
		skinSetOnlyMode = true;
		SetNormalStorehouseTagsActive(normalMode: false);
		SetActiveAndDirtyWithFadeAnim(active: true);
		skinSetPanel.JumpToFirstUnviewedPageOnNextDisplay();
		switchPanel.Select(skinSetSwitchTag);
		switchPanel.gameObject.SetActive(value: false);
		RefreshTitle();
	}

	private void _OnCloseButtonClicked()
	{
		if (enteredLoot)
		{
			UIBase.applicationState.playerInfoManager.newLoots.Clear();
			UIBase.applicationState.savingManager.SetProfileDirtyToSave();
		}
		enteredLoot = false;
		if (skinSetOnlyMode)
		{
			switchPanel.gameObject.SetActive(value: true);
			SetNormalStorehouseTagsActive(normalMode: true);
			switchPanel.Select(matterSwitchTag);
		}
		skinSetOnlyMode = false;
		RefreshTitle();
		SetActiveAndDirtyWithFadeAnim(active: false);
		manager.SetDirty();
	}

	private void _OnTagSwitched()
	{
		skillGemPanel.gameObject.SetActive(value: false);
		clavisPanel.gameObject.SetActive(value: false);
		skillOverviewPanel.gameObject.SetActive(value: false);
		matterPanel.gameObject.SetActive(value: false);
		skinSetPanel.gameObject.SetActive(value: false);
		if (topBarImage != null)
		{
			topBarImage.SetActive(switchPanel.targetShowTag != skinSetSwitchTag);
		}
		if (switchPanel.targetShowTag == skinSetSwitchTag)
		{
			skinSetPanel.SetActiveAndDirtyWithFadeAnim(active: true);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-仓库-素材");
			}
			SetDirty();
			return;
		}
		if (switchPanel.targetShowTag == skillSwitchTag)
		{
			matterPanel.SetExternalMatterFilter(default(MatterDef.MatterDisplayGroup));
			matterPanel.SetActiveAndDirtyWithFadeAnim(active: true);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-仓库-素材");
			}
		}
		if (switchPanel.targetShowTag == skillGemSwitchTag)
		{
			matterPanel.SetExternalMatterFilter(MatterDef.MatterDisplayGroup.CharacterMatter, MatterDef.MatterDisplayGroup.Obsession);
			matterPanel.SetActiveAndDirtyWithFadeAnim(active: true);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-仓库-素材");
			}
		}
		if (switchPanel.targetShowTag == clavisSwitchTag)
		{
			matterPanel.ClearExternalMatterFilter();
			clavisPanel.SetActiveAndDirtyWithFadeAnim(active: true);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-仓库-钥匙");
			}
		}
		if (switchPanel.targetShowTag == matterSwitchTag)
		{
			enteredLoot = true;
			matterPanel.SetExternalMatterFilter(MatterDef.MatterDisplayGroup.Chest);
			matterPanel.SetActiveAndDirtyWithFadeAnim(active: true);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-仓库-素材");
			}
		}
		if (switchPanel.targetShowTag != matterSwitchTag)
		{
			UIBase.applicationState.playerInfoManager.newLoots.Clear();
		}
		SetDirty();
	}

	private void SetNormalStorehouseTagsActive(bool normalMode)
	{
		skillSwitchTag.gameObject.SetActive(normalMode);
		skillGemSwitchTag.gameObject.SetActive(normalMode);
		matterSwitchTag.gameObject.SetActive(normalMode);
		clavisSwitchTag.gameObject.SetActive(normalMode);
		skinSetSwitchTag.gameObject.SetActive(!normalMode);
	}

	private void RefreshTitle()
	{
		if (titleText != null)
		{
			titleText.gameObject.SetActive(!skinSetOnlyMode);
		}
		if (cardSkinTitleText != null)
		{
			cardSkinTitleText.gameObject.SetActive(skinSetOnlyMode);
		}
	}
}
