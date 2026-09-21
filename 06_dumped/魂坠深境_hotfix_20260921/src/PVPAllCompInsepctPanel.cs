using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class PVPAllCompInsepctPanel : UIBase
{
	public ResetAllPVPCompConfirmPanel resetAllPVPCompConfirmPanel;

	public GameButton resetPVPCompButton;

	public TextMeshProUGUI remainTimeForAutoResetText;

	public GameButton backButton;

	public TMP_Dropdown storageTypeFilterDropDown;

	public TMP_InputField storageSearchInputField;

	public TextMeshProUGUI allCompCardCountText;

	public TextMeshProUGUI allCompRuneCountText;

	public TextMeshProUGUI allCompInfluenceCountText;

	public DeckPresetCompStorageCell compStorageCellPrefab;

	public Transform compStorageContent;

	[NonSerialized]
	public DeckPresetCompType storageTypeFilter;

	[NonSerialized]
	private CellUIPrefabPool<DeckPresetCompStorageCell, DeckPresetCompDef> compStorageCellPool;

	protected override void Init()
	{
		InitUI(resetAllPVPCompConfirmPanel);
		compStorageCellPool = GetPrefabPool<DeckPresetCompStorageCell, DeckPresetCompDef>(compStorageCellPrefab, compStorageContent);
		storageTypeFilterDropDown.options = DeckPresetEditPanel.StorageTypeFilterOptions.Select(((DeckPresetCompType, string) s) => new TMP_Dropdown.OptionData(s.Item2)).ToList();
		storageTypeFilterDropDown.onValueChanged.AddListener(_OnOptionFilterChanged);
		storageSearchInputField.onValueChanged.AddListener(delegate
		{
			SetDirty();
		});
		backButton.onClick.AddListener(delegate
		{
			SetActiveAndDirty(active: false);
		});
		resetPVPCompButton.onClick.AddListener(delegate
		{
			resetAllPVPCompConfirmPanel.SetActiveAndDirty(active: true);
		});
	}

	protected override void UpdateDisplay()
	{
		List<DeckPresetCompDef> list = (from c in (from storage in UIBase.applicationState.pvpModeManager.storagePVPDeckPresetComps
				select storage.def into def
				where (storageTypeFilter == DeckPresetCompType.None || def.type == storageTypeFilter) && (string.IsNullOrEmpty(storageSearchInputField.text) || def.DisplayName.Contains(storageSearchInputField.text, StringComparison.OrdinalIgnoreCase))
				select def).ToList()
			orderby c.type, c.DIYPoint, c.item?.GetMajorTag().FullID, c.DisplayName
			select c).ToList();
		compStorageCellPool.DespawnAll();
		compStorageCellPool.Sync(list);
		TimeSpan span = ScriptableObjectSingleton<CalendarDataManager>.instance.NextDailyResetTime - DateTime.UtcNow;
		remainTimeForAutoResetText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_defender_deck_auto_reset_after_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(span));
		int num = UIBase.applicationState.pvpModeManager.storagePVPDeckPresetComps.Where((PVPModeManager.DeckPresetCompStorage c) => c.def.type == DeckPresetCompType.Item).Sum((PVPModeManager.DeckPresetCompStorage t) => t.count);
		int num2 = UIBase.applicationState.pvpModeManager.storagePVPDeckPresetComps.Where((PVPModeManager.DeckPresetCompStorage c) => c.def.type == DeckPresetCompType.Rune).Sum((PVPModeManager.DeckPresetCompStorage t) => t.count);
		int num3 = UIBase.applicationState.pvpModeManager.storagePVPDeckPresetComps.Where((PVPModeManager.DeckPresetCompStorage c) => c.def.type == DeckPresetCompType.Influence).Sum((PVPModeManager.DeckPresetCompStorage t) => t.count);
		allCompCardCountText.text = num.ToString();
		allCompRuneCountText.text = num2.ToString();
		allCompInfluenceCountText.text = num3.ToString();
	}

	private void _OnOptionFilterChanged(int index)
	{
		if (index >= 0 && index < DeckPresetEditPanel.StorageTypeFilterOptions.Count)
		{
			storageTypeFilter = DeckPresetEditPanel.StorageTypeFilterOptions[index].Item1;
			SetDirty();
		}
	}
}
