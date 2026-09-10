using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class SkillIndexPanel : IndexPagePanel
{
	private class FilterData
	{
		public IconObjectFilterPanel.IconFilterData displayData;

		public IndexBookPage page;
	}

	[SerializeField]
	private SkillDisplay skillDisplayPrefab;

	[SerializeField]
	private Transform skillCellParent;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[SerializeField]
	private TextMeshProUGUI filterNameText;

	[SerializeField]
	private SkillInfoPanel skillInfoPanel;

	[NonSerialized]
	private List<FilterData> filterDatas;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> skillDisplayPool;

	private IndexBookPage currentPage = IndexBookPage.ClassSkill_Warrior;

	public override string TASubPanelTagName => "档案馆-图鉴-护符";

	public override IndexBookPage GetIndexPageType()
	{
		return currentPage;
	}

	public override bool WithIndexBookRewardToGet()
	{
		if (filterDatas == null)
		{
			return GetConfiguredSkillPages().Any((IndexBookPage page) => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(page));
		}
		return filterDatas.Any((FilterData data) => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(data.page));
	}

	public override void ClearTransientSelection()
	{
		skillInfoPanel?.HideAllCommonInfo();
	}

	public override bool TrySelectFirstRewardPage()
	{
		FilterData filterData = filterDatas?.FirstOrDefault((FilterData data) => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(data.page));
		if (filterData == null)
		{
			return false;
		}
		IconObjectFilter filter = characterClassFilterPanel.GetFilter(filterData.displayData.target);
		if (filter == null)
		{
			return false;
		}
		if (characterClassFilterPanel.selected != filter)
		{
			characterClassFilterPanel.selected = filter;
			characterClassFilterPanel.OnChangeSelected();
		}
		return true;
	}

	public override bool TryFocusFirstClaimableSingleReward()
	{
		return TryFocusFirstClaimableSingleReward(layoutGroup, (object data) => (data is GameSkill gameSkill) ? UIBase.applicationState.assetCatalog.GetSkillQualityIndexEntry(gameSkill.skillDef, gameSkill.QualityLevel) : null);
	}

	public override bool EmphasizeNextRewardTarget()
	{
		FilterData filterData = filterDatas?.FirstOrDefault((FilterData data) => data.page != currentPage && UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(data.page));
		IconObjectFilter iconObjectFilter = ((filterData == null) ? null : characterClassFilterPanel.GetFilter(filterData.displayData.target));
		if (iconObjectFilter == null)
		{
			return false;
		}
		iconObjectFilter.transform.DOKill();
		iconObjectFilter.transform.DOPunchScale(Vector3.one * 0.1f, 0.45f, 2, 0.35f);
		return true;
	}

	protected override void Init()
	{
		InitUI(characterClassFilterPanel);
		InitUI(skillInfoPanel);
		GetPrefabPool(ref skillDisplayPool, skillDisplayPrefab, null, skillInfoPanel);
		InitFilterData();
		characterClassFilterPanel.Sync(filterDatas.ConvertAll((FilterData f) => f.displayData));
		characterClassFilterPanel.canUnselect = false;
		if (filterDatas.Count > 0)
		{
			characterClassFilterPanel.selected = characterClassFilterPanel.GetFilter(filterDatas[0].displayData.target);
		}
		characterClassFilterPanel.onChangeSelected = _OnFilterChanged;
		_OnFilterChanged();
	}

	protected override void UpdateDisplay()
	{
		filterNameText.text = ((base.CurrentPageConfig != null) ? base.CurrentPageConfig.pageTitle : "");
		layoutGroup.RefreshLayout();
	}

	private void _OnFilterChanged()
	{
		ClearTransientSelection();
		object target = characterClassFilterPanel.selected?.setupData.target;
		FilterData filterData = filterDatas?.FirstOrDefault((FilterData d) => object.Equals(d.displayData.target, target));
		if (filterData == null && filterDatas != null && filterDatas.Count > 0)
		{
			filterData = filterDatas[0];
		}
		if (filterData != null)
		{
			currentPage = filterData.page;
			ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay((from entry in GetIndexPageAllContent<SkillQualityIndexEntry>()
				orderby entry.SkillDef.skillName, SkillQualityIndexEntry.GetQualityOrdinal(entry.QualityLevel), entry.ArchetypeIndex
				select entry.ToProfileSkill()).ToList().ConvertAll((ProfileSkill ps) => new GameSkill(ps)), skillDisplayPool, layoutGroup);
			parent.SetDirty();
			SetDirty();
		}
	}

	private void InitFilterData()
	{
		HashSet<IndexBookPage> configuredPages = GetConfiguredSkillPages().ToHashSet();
		filterDatas = (from data in UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: false, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills)
			select ToFilterData(data, DiscoverManager.GetSkillIndexPageByCharacterClass((data.target as CharacterClassDef).mClass)) into data
			where configuredPages.Contains(data.page)
			select data).ToList();
		if (configuredPages.Contains(IndexBookPage.Skill_Common))
		{
			IndexBookConfig indexBookConfig = ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == IndexBookPage.Skill_Common);
			filterDatas.Add(ToFilterData(new IconObjectFilterPanel.IconFilterData(IndexBookPage.Skill_Common, ScriptableObjectSingleton<GameDataManager>.instance.generalClassIcon, indexBookConfig?.pageTitle ?? ScriptableObjectSingleton<TextDataPreset>.instance.term_all_character_class_general_skills), IndexBookPage.Skill_Common));
		}
	}

	private IEnumerable<IndexBookPage> GetConfiguredSkillPages()
	{
		IEnumerable<IndexBookPage> classSkillPages = from pair in UIBase.applicationState.discoverManager.GetAllClassSkillIndexPagePairs()
			select pair.Item2;
		return from config in ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs
			select config.page into page
			where classSkillPages.Contains(page) || page == IndexBookPage.Skill_Common
			select page;
	}

	private FilterData ToFilterData(IconObjectFilterPanel.IconFilterData data, IndexBookPage page)
	{
		FilterData filterData = new FilterData();
		filterData.displayData = data;
		filterData.page = page;
		filterData.displayData.shouldShowRedDot = () => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(page);
		return filterData;
	}
}
