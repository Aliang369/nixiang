using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class SkinSelectCell : CellUI<RandomItemSkinInSkinSetReward>, IDisplayModulesController
{
	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private TextMeshProUGUI hint;

	[SerializeField]
	private TextMeshProUGUI selected;

	[NonSerialized]
	public ItemSkinSetBatch batch;

	private RandomItemSkinInSkinSetReward reward => base.setupData;

	private ItemSkinSetDef.SkinSetBatchConfig batchConfig => reward.itemSkinSetDef.GetBatchConfig(reward.batch);

	protected override void Init()
	{
		InitUI(globalItemDisplay);
	}

	protected override void OnSetup(RandomItemSkinInSkinSetReward setupData)
	{
		globalItemDisplay.Setup(reward);
		title.text = reward.GlobalItemTitle;
		description.text = reward.GetGlobalItemDescription(CellUI<RandomItemSkinInSkinSetReward>.applicationState, out var _);
		batch = new ItemSkinSetBatch(reward.itemSkinSetDef, reward.batch);
		hint.text = batch.GetSkinUnlockProgressText();
	}
}
