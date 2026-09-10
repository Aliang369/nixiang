using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartGameBuffCell : CellUI<StartGameBuff>, IPointerClickHandler, IEventSystemHandler
{
	public Image image;

	public TextMeshProUGUI text;

	public TextMeshProUGUI description;

	public TextMeshProUGUI persistentTimeText;

	public Image outline;

	public Image grey;

	private StartGameBuffConfigPanel parentPanel;

	[NonSerialized]
	protected TextLinkObjectDisplayer textLinkObjectDisplayer;

	public StartGameBuff buff => base.setupData;

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<StartGameBuff>.applicationState, description, base.rectTransform);
		parentPanel = GetUIInParents<StartGameBuffConfigPanel>();
	}

	protected override void OnSetup(StartGameBuff _)
	{
		image.sprite = buff.def.icon;
		text.text = buff.def.buffName.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(buff.def.quality));
		description.text = TextDataPreset.FormatStartGameBuffDescription(buff, CellUI<StartGameBuff>.applicationState, withHintAndExpireData: false, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		persistentTimeText.text = buff.GetTimeExpireDescription(CellUI<StartGameBuff>.applicationState);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		grey.gameObject.SetActive(!buff.IsActivatable(CellUI<StartGameBuff>.applicationState));
		if (outline != null)
		{
			outline.gameObject.SetActive(parentPanel.configSlot.buff == buff);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		parentPanel.SetStartGameBuffCell(buff);
	}
}
