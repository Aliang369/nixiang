using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartGameBuffConfigSlot : CellUI<StartGameBuffSlot>
{
	public Image icon;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI descriptionText;

	public TextMeshProUGUI wisdowmCountText;

	public MatterCell matterCellPrefab;

	public RectTransform matterCellParent;

	public CellUIPrefabPool<MatterCell, MatterInfo> matterCellPool;

	public StartGameBuffSlot data { get; private set; }

	protected override void Init()
	{
		matterCellPool = GetPrefabPool<MatterCell, MatterInfo>(matterCellPrefab, matterCellParent);
	}

	protected override void OnSetup(StartGameBuffSlot setupData)
	{
		data = setupData;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		matterCellPool.DespawnAll();
		StartGameBuffDef buffDef = data.buffDef;
		if (buffDef != null)
		{
			icon.sprite = buffDef.icon;
			titleText.text = buffDef.buffName.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(buffDef.quality));
			descriptionText.text = TextDataPreset.FormatStartGameBuffDescription(buffDef, CellUI<StartGameBuffSlot>.applicationState, out var _);
			wisdowmCountText.gameObject.SetActive(value: true);
			wisdowmCountText.text = buffDef.consumeWisdom.ToString();
		}
		else
		{
			icon.sprite = ScriptableObjectSingleton<GameDataManager>.instance.emptySkillWordSlotIcon;
			titleText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skillWord_empty_slot;
			descriptionText.text = "";
			wisdowmCountText.gameObject.SetActive(value: false);
		}
	}
}
