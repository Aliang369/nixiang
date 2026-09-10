using TMPro;

public class RitualPrayCell : CellUI<Pray>
{
	public TextMeshProUGUI descriptionText;

	private Pray pray;

	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<Pray>.applicationState, descriptionText, base.rectTransform);
	}

	protected override void OnSetup(Pray t)
	{
		pray = t;
	}

	protected override void UpdateDisplay()
	{
		descriptionText.text = pray.GetDescription(CellUI<Pray>.gameState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
	}
}
