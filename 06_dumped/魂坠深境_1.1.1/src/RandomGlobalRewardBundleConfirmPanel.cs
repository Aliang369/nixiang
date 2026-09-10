using TMPro;
using UnityEngine;

public class RandomGlobalRewardBundleConfirmPanel : CellUI<MatterInfo>
{
	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private GlobalItemDisplay lootDisplay;

	[SerializeField]
	private TextMeshProUGUI storageCount;

	[SerializeField]
	private TextMeshProUGUI openCount;

	[SerializeField]
	private RangeSliderWithAdjustButton slider;

	private MatterInfo matterInfo => base.setupData;

	private FixedLootSetup setup => matterInfo?.matterDef?.lootSetup as FixedLootSetup;

	private int storageLootCount => CellUI<MatterInfo>.applicationState.matterManager.GetMatterCount(matterInfo.matterDef);

	private int maxCount => Mathf.Min(storageLootCount, 100);

	private int count => Mathf.Clamp(slider.CurrentValue, 1, maxCount);

	protected override void Init()
	{
		InitUI(lootDisplay);
		confirmButton.onClick.AddListener(OnConfirm);
		cancelButton.onClick.AddListener(base.Unset);
		slider.onValueChanged.AddListener(delegate
		{
			SetDirty();
		});
		void OnConfirm()
		{
			CellUI<MatterInfo>.applicationState.matterManager.OpenLoot(matterInfo.matterDef, count);
			Unset();
		}
	}

	protected override void OnSetup(MatterInfo setupData)
	{
		if (setup == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		lootDisplay.Setup(matterInfo);
		slider.MinValue = 1;
		slider.MaxValue = maxCount;
		storageCount.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_current_storage.FormatWith(storageLootCount);
		slider.CurrentValue = 1;
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		openCount.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_open_loot_count.FormatWith(count, matterInfo.GlobalItemTitle);
	}
}
