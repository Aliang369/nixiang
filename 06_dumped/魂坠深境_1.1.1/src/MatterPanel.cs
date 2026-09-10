using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class MatterPanel : UIBase
{
	[SerializeField]
	private MatterCell matterCell;

	[SerializeField]
	public CommonInfoPanel matterHintPanel;

	[SerializeField]
	private IconObjectFilterPanel matterTypeFilterPanel;

	[SerializeField]
	private GameObject matterFilterTitleTextArea;

	[SerializeField]
	private TextMeshProUGUI matterFilterTitleText;

	[NonSerialized]
	private CellUIPrefabPool<MatterCell, MatterInfo> pool;

	[NonSerialized]
	private List<MatterDef.MatterDisplayGroup> externalMatterGroups;

	protected override void Init()
	{
		InitUI(matterHintPanel);
		InitUI(matterTypeFilterPanel);
		GetPrefabPool(ref pool, matterCell, null, matterHintPanel);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(SetDirty);
		matterTypeFilterPanel.Sync(GetAllMatterGroupFilters());
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		Dictionary<MatterInfo, int> allMatterOrder = UIBase.applicationState.matterManager.matterInfos.Select((MatterInfo m, int i) => (matter: m, index: i)).ToDictionary(((MatterInfo matter, int index) pair) => pair.matter, ((MatterInfo matter, int index) pair) => pair.index);
		List<MatterInfo> source = UIBase.applicationState.matterManager.matterInfos.Where((MatterInfo m) => m.count != 0).ToList();
		if (!externalMatterGroups.IsNullOrEmpty())
		{
			matterTypeFilterPanel.gameObject.SetActive(value: false);
			matterFilterTitleTextArea.SetActive(value: false);
			source = source.Where((MatterInfo m) => externalMatterGroups.Contains(m.matterDef.GetMatterGroup())).ToList();
		}
		else if (matterTypeFilterPanel.selected != null)
		{
			matterTypeFilterPanel.gameObject.SetActive(value: true);
			matterFilterTitleTextArea.SetActive(value: true);
			matterFilterTitleText.text = matterTypeFilterPanel.selected.setupData.title;
			source = source.Where((MatterInfo m) => m.matterDef.GetMatterGroup() == (MatterDef.MatterDisplayGroup)matterTypeFilterPanel.selected.setupData.target).ToList();
		}
		else
		{
			matterTypeFilterPanel.gameObject.SetActive(value: true);
			matterFilterTitleTextArea.SetActive(value: false);
		}
		source = (from m in source
			orderby m.matterDef.qualityLevel descending, allMatterOrder[m]
			select m).ToList();
		pool.Sync(source);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(SetDirty);
	}

	public List<IconObjectFilterPanel.IconFilterData> GetAllMatterGroupFilters()
	{
		List<IconObjectFilterPanel.IconFilterData> list = new List<IconObjectFilterPanel.IconFilterData>();
		foreach (KeyValuePair<MatterDef.MatterDisplayGroup, VFXDataPreset.MatterGroupDisplayInfo> matterGroupDisplayInfo in ScriptableObjectSingleton<VFXDataPreset>.instance.matterGroupDisplayInfos)
		{
			KeyValuePair<MatterDef.MatterDisplayGroup, VFXDataPreset.MatterGroupDisplayInfo> pair = matterGroupDisplayInfo;
			list.Add(new IconObjectFilterPanel.IconFilterData(pair.Key, pair.Value.icon, pair.Value.name, ShouldShowRedDot));
			bool? ShouldShowRedDot()
			{
				if (pair.Key != MatterDef.MatterDisplayGroup.Chest)
				{
					return false;
				}
				if (!UIBase.applicationState.playerInfoManager.newLoots.Any())
				{
					return false;
				}
				return true;
			}
		}
		return list;
	}

	public void SetExternalMatterFilter(params MatterDef.MatterDisplayGroup[] groups)
	{
		externalMatterGroups = groups?.ToList();
		SetDirty();
	}

	public void ClearExternalMatterFilter()
	{
		externalMatterGroups = null;
		SetDirty();
	}
}
