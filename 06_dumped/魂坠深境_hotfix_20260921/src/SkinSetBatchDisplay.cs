using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSetBatchDisplay : CellUI<ItemSkinSetDef.SkinSetBatchConfig>
{
	[SerializeField]
	private ItemDisplayBase prefab;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI batchCollectProgressText;

	[SerializeField]
	private Slider collectProgressSlider;

	[SerializeField]
	private TextMeshProUGUI currentSetSkinCoinCostText;

	[SerializeField]
	private TextMeshProUGUI nextCollectReduceCostHintText;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> pool;

	private List<ItemSkinDef> skins => base.setupData.skinDefs;

	protected override void Init()
	{
		pool = GetPrefabPool<ItemDisplayBase, Item>(prefab);
		pool.ConfigInfoPanel(GetComponentInParent<ItemSkinSetPanel>().itemInfoPanel);
	}

	protected override void OnSetup(ItemSkinSetDef.SkinSetBatchConfig _)
	{
		title.text = base.setupData.GetDisplayName(skins[0].attachSkinSet, withDotSplit: true);
		batchCollectProgressText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(base.setupData.GetUnlockedCount(CellUI<ItemSkinSetDef.SkinSetBatchConfig>.applicationState), base.setupData.BatchSize);
		pool.Sync(skins.ConvertAll((ItemSkinDef s) => new Item(s.attachItem).SetSkin(s)));
		RefreshSetSkinCost();
	}

	private void RefreshSetSkinCost()
	{
		int unlockedCount = base.setupData.GetUnlockedCount(CellUI<ItemSkinSetDef.SkinSetBatchConfig>.applicationState);
		collectProgressSlider.value = ((base.setupData.BatchSize > 0) ? Mathf.Clamp01((float)unlockedCount / (float)base.setupData.BatchSize) : 0f);
		string text = ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText();
		currentSetSkinCoinCostText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_current_skin_ascension_cost_arg0_icon_arg1.FormatWith(base.setupData.GetCurrentSetSkinCoinCost(CellUI<ItemSkinSetDef.SkinSetBatchConfig>.applicationState), text);
		int? needCollectCountForNextLayer = base.setupData.GetNeedCollectCountForNextLayer(CellUI<ItemSkinSetDef.SkinSetBatchConfig>.applicationState);
		nextCollectReduceCostHintText.text = ((!needCollectCountForNextLayer.HasValue) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_skin_ascension_cost_reduction_max : ScriptableObjectSingleton<TextDataPreset>.instance.term_collect_skin_reduce_ascension_cost_arg0_icon_arg1.FormatWith(needCollectCountForNextLayer.Value, text));
	}
}
