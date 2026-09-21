using TMPro;
using UnityEngine.UI;

public class GemWordCell : CellUI<GemWordCell.CellData>
{
	public class CellData
	{
		public SkillGem skillGem;

		public string description;

		public TextDataGenerator generator;
	}

	public TextMeshProUGUI text;

	public Image gemIcon;

	public TextMeshProUGUI levelText;

	private CommonTextLinkObjectDisplayer textLinkObjectDisplayer;

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<CellData>.applicationState, text, base.rectTransform);
	}

	protected override void OnSetup(CellData data)
	{
		text.text = data.description;
		textLinkObjectDisplayer?.SetGenerator(data.generator);
		gemIcon.sprite = data.skillGem?.def?.GemSprite;
		levelText.text = ((data.skillGem?.def != null) ? $"lv. {data.skillGem.def.level}" : "");
	}

	protected override void OnUnset()
	{
		if (text != null)
		{
			text.text = "";
		}
		if (gemIcon != null)
		{
			gemIcon.sprite = null;
		}
		if (levelText != null)
		{
			levelText.text = "";
		}
	}
}
