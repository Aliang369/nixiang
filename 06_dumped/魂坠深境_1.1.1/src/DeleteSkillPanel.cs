using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;

public class DeleteSkillPanel : UIBase, IShowAsync, IHideAsync
{
	public MatterCell currentSkillWordMatterCell;

	public SkillDisplay storageSkillCellPrefab;

	public RectTransform storageSkillCellParent;

	public SkillDisplay deleteSkillCellPrefab;

	public RectTransform deleteSkillCellParent;

	public MatterCell deleteGetMatterCell;

	public RectTransform deleteGetMatterCellParent;

	public GameButton quickPutInButton;

	public GameButton OpenConfirmPanelButton;

	public GameButton hintButton;

	public RectTransform deleteSkillConfirmPanel;

	public TextMeshProUGUI deleteSkillConfirmText;

	public MatterCell deleteSkillConfirmCostCell;

	public RectTransform deleteMatterConfirmCostCellParent;

	public SkillDisplay deleteConfirmSkillCellPrefab;

	public RectTransform deleteConfirmSkillCellParent;

	public GameButton deleteCancelButton;

	public GameButton deleteConfirmButton;

	public int deleteSkillCountLimit = 10;

	public TMP_Dropdown sortDropDown;

	public IconObjectFilterPanel classFilterPanel;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> deleteSkillCellPool;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> storageSkillCellPool;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> deleteConfirmSkillCellPool;

	[NonSerialized]
	private CellUIPrefabPool<MatterCell, MatterInfo> deleteGainMatterCellPool;

	[NonSerialized]
	private CellUIPrefabPool<MatterCell, MatterInfo> deleteMatterConfirmCostCellPool;

	[NonSerialized]
	private List<ProfileSkill> toDeleteSkills;

	[NonSerialized]
	public Action<List<ProfileSkill>> onDeleteComplete;

	[NonSerialized]
	public UniversalCommonInfoPanel infoPanel;

	[NonSerialized]
	private bool recycleInProgress;

	private bool enableDelete => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillDelete);

	private bool IsDeletable => toDeleteSkills.Any((ProfileSkill s) => s.IsDeleteAble);

	protected override void Init()
	{
		InitUI(currentSkillWordMatterCell);
		InitUI(deleteGetMatterCell);
		InitUI(deleteSkillConfirmCostCell);
		InitUI(classFilterPanel);
		infoPanel = UIHelper.GetDefaultCommonInfoPanel();
		GetPrefabPool(ref storageSkillCellPool, storageSkillCellPrefab, null, infoPanel);
		deleteSkillCellPool = GetPrefabPool<SkillDisplay, GameSkill>(deleteSkillCellPrefab, deleteSkillCellParent);
		deleteSkillCellPool.ConfigInfoPanel(infoPanel);
		deleteGainMatterCellPool = GetPrefabPool<MatterCell, MatterInfo>(deleteGetMatterCell, deleteGetMatterCellParent);
		deleteMatterConfirmCostCellPool = GetPrefabPool<MatterCell, MatterInfo>(deleteSkillConfirmCostCell, deleteMatterConfirmCostCellParent);
		deleteConfirmSkillCellPool = GetPrefabPool<SkillDisplay, GameSkill>(deleteConfirmSkillCellPrefab, deleteConfirmSkillCellParent);
		deleteConfirmSkillCellPool.ConfigInfoPanel(UIBase.fortressState.uiManager.universalCommonInfoPanel);
		toDeleteSkills = new List<ProfileSkill>();
		quickPutInButton.onClick.AddListener(_OnQuickPutInButtonClicked);
		OpenConfirmPanelButton.onClick.AddListener(_OnOpenConfirmPanelButtonClicked);
		deleteCancelButton.onClick.AddListener(_OnCancelButtonClicked);
		deleteConfirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		hintButton.onClick.AddListener(delegate
		{
			UIBase.fortressState.uiManager.fortressGuidePanel.deleteSkillGuideHint.Show();
		});
		sortDropDown.options = (from c in ProfileSkill.GetAllProfileSkillSortMethod()
			select new TMP_Dropdown.OptionData(c.Item1)).ToList();
		sortDropDown.onValueChanged.AddListener(_OnSortDropdownValueChanged);
		OpenConfirmPanelButton.onNotInteractableClick.AddListener(delegate
		{
			if (!enableDelete)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillDelete));
			}
			else if (toDeleteSkills.Count == 0)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill_to_delete);
			}
			else
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_contain_skills_not_deletable);
			}
		});
		classFilterPanel.Sync(UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: true, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills));
	}

	protected override void Dispose()
	{
		OpenConfirmPanelButton.onClick.RemoveListener(_OnOpenConfirmPanelButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		UIBase.fortressState.uiManager.fortressGuidePanel.deleteSkillGuideHint.TryFirstOpenAutoPopup();
		List<ProfileSkill> list = UIBase.applicationState.globalSkillManager.SortedSkills.FindAll((ProfileSkill s) => !toDeleteSkills.Contains(s));
		IconObjectFilter selected = classFilterPanel.selected;
		if ((object)selected != null)
		{
			_ = selected.setupData;
			if (true)
			{
				list = list.Where((ProfileSkill sk) => sk.def.attachCharacterClass == classFilterPanel.selected.setupData.target).ToList();
			}
		}
		list.Sort(delegate(ProfileSkill a, ProfileSkill b)
		{
			int num = b.IsDeleteAble.CompareTo(a.IsDeleteAble);
			return (num != 0) ? num : ProfileSkill.GetAllProfileSkillSortMethod()[sortDropDown.value].Item2.Compare(a, b);
		});
		storageSkillCellPool.DespawnAll();
		storageSkillCellPool.UpdateSequenceDisplay(list.ConvertAll((ProfileSkill ps) => ps.GetGameSkill()), layoutGroup);
		deleteSkillCellPool.DespawnAll();
		deleteSkillCellPool.Spawn(toDeleteSkills.ConvertAll((ProfileSkill ps) => ps.GetGameSkill()));
		deleteConfirmSkillCellPool.DespawnAll();
		deleteConfirmSkillCellPool.Spawn(toDeleteSkills.ConvertAll((ProfileSkill ps) => ps.GetGameSkill()));
		quickPutInButton.interactable = !recycleInProgress;
		OpenConfirmPanelButton.interactable = enableDelete && IsDeletable && !recycleInProgress;
		deleteCancelButton.interactable = !recycleInProgress;
		deleteConfirmButton.interactable = !recycleInProgress;
		currentSkillWordMatterCell.Setup(UIBase.applicationState.matterManager.EssenceMatter);
		List<MatterInfo> setupDatas = MatterInfo.FoldMatterInfo(toDeleteSkills.Where((ProfileSkill s) => s.IsDeleteAble).SelectMany((ProfileSkill s) => s.GetDeleteSkillGainMatter()).ToList());
		deleteGainMatterCellPool.DespawnAll();
		deleteGainMatterCellPool.Spawn(setupDatas);
		deleteSkillConfirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_delete_arg0_count_skill_to_get_below_matter.FormatWith(toDeleteSkills.Count);
		deleteMatterConfirmCostCellPool.DespawnAll();
		deleteMatterConfirmCostCellPool.Spawn(setupDatas);
	}

	public IEnumerator ShowAnim()
	{
		base.gameObject.SetActive(value: true);
		if (!((UnityEngine.Object)(object)animator == null))
		{
			((Behaviour)(object)animator).enabled = true;
			animator.SetTrigger(AnimKeywords.show);
			yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		}
	}

	public IEnumerator HideAnim()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		if ((UnityEngine.Object)(object)animator == null)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
	}

	private void _OnOpenConfirmPanelButtonClicked()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (!recycleInProgress)
		{
			if (!enableDelete)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillDelete));
				return;
			}
			infoPanel.HideAll();
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_amulet_delete);
			deleteSkillConfirmPanel.gameObject.SetActive(value: true);
			SetDirty();
		}
	}

	private void _OnConfirmButtonClicked()
	{
		if (recycleInProgress)
		{
			return;
		}
		if (!enableDelete)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillDelete));
			return;
		}
		infoPanel.HideAll();
		List<ProfileSkill> legalDeleteSkills = (from s in toDeleteSkills
			where s.IsDeleteAble
			group s by s.profileSkillGUID into @group
			select @group.First()).ToList();
		deleteSkillConfirmPanel.gameObject.SetActive(value: false);
		SetDirty();
		List<SkillGem> droppedGems = new List<SkillGem>();
		foreach (ProfileSkill item in legalDeleteSkills)
		{
			foreach (SkillGem item2 in item.skillGemsMounted.OrEmptyIfNull())
			{
				droppedGems.Add(item2);
			}
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			recycleInProgress = true;
			SetDirty();
			GeneralGameAsyncOperation<RecycleSkillRequest, RecycleSkillResult> op = new GeneralGameAsyncOperation<RecycleSkillRequest, RecycleSkillResult>(new RecycleSkillRequest(legalDeleteSkills), UIBase.applicationState.ChillyRoomSDK.Skill.RecycleSkillAsync);
			op.OnComplete(delegate
			{
				UIBase.applicationState.globalSkillManager.DeleteSkills(op.OpResult, legalDeleteSkills, out var outRewardPack);
				AfterRecycle(outRewardPack);
			}).OnOver(delegate
			{
				recycleInProgress = false;
				SetDirty();
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_deleting_skill);
		}
		parent.SetDirty();
		void AfterRecycle(GlobalRewardPackage afterRecycleReward)
		{
			List<ProfileSkill> obj = legalDeleteSkills.ToList();
			foreach (SkillGem item3 in droppedGems)
			{
				UIBase.applicationState.skillGemManager.AddGem(item3);
			}
			UIBase.fortressState.uiManager.globalRewardPackageDisplay.Setup(afterRecycleReward);
			ThinkingAnalyticsExtends.TrackDeleteSkill(UIBase.applicationState, legalDeleteSkills);
			foreach (ProfileSkill item4 in legalDeleteSkills)
			{
				toDeleteSkills.Remove(item4);
			}
			Action<List<ProfileSkill>> action = onDeleteComplete;
			onDeleteComplete = null;
			action?.Invoke(obj);
			SetDirty();
			UIBase.fortressState.uiManager.storehousePanel.skillOverviewPanel.skillStoragePanel.SetDirty();
		}
	}

	private void _OnQuickPutInButtonClicked()
	{
		if (recycleInProgress)
		{
			return;
		}
		infoPanel.HideAll();
		List<ProfileSkill> canDeleteSkills = (from t in storageSkillCellPool.GetActiveElements()
			select t.setupData.profileSkill into p
			where p.IsDeleteAble
			select p).ToList();
		foreach (ProfileSkill item in from s in canDeleteSkills
			where canDeleteSkills.Count((ProfileSkill s) => s.def == s.def) > 1
			where canDeleteSkills.First((ProfileSkill p) => p.def == s.def) != s
			orderby s.QualityLevel
			select s)
		{
			_OnStorageSkillClicked(item);
		}
	}

	private void _OnCancelButtonClicked()
	{
		infoPanel.HideAll();
		deleteSkillConfirmPanel.gameObject.SetActive(value: false);
		SetDirty();
	}

	public void _OnStorageSkillClicked(ProfileSkill skill)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!recycleInProgress && toDeleteSkills.Count < deleteSkillCountLimit && !toDeleteSkills.Any((ProfileSkill s) => s.profileSkillGUID == skill.profileSkillGUID))
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			toDeleteSkills.Add(skill);
			SetDirty();
		}
	}

	public void _OnDeleteSkillClicked(ProfileSkill skill)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!recycleInProgress)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			toDeleteSkills.Remove(skill);
			SetDirty();
		}
	}

	private void _OnEmptyAreaClicked()
	{
		infoPanel.HideAll();
	}

	private void _OnSortDropdownValueChanged(int value)
	{
		SetDirty();
	}
}
