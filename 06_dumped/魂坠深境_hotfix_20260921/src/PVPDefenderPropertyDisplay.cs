using System;
using TMPro;

public class PVPDefenderPropertyDisplay : UIBase
{
	public TextMeshProUGUI healthValueText;

	public TextMeshProUGUI turnDrawValueText;

	public TextMeshProUGUI pointValueText;

	public TextMeshProUGUI deckMinimalSizeValueText;

	[NonSerialized]
	private PVPDefenderProperty defenderProperty;

	public void Setup(PVPDefenderProperty defenderProperty)
	{
		this.defenderProperty = defenderProperty;
		UpdateDisplay();
	}

	protected override void UpdateDisplay()
	{
		if (defenderProperty != null)
		{
			healthValueText.text = defenderProperty.health.ToString();
			turnDrawValueText.text = defenderProperty.turnDrawCount.ToString();
			pointValueText.text = defenderProperty.diyPoint.ToString();
			deckMinimalSizeValueText.text = defenderProperty.minimalDeckSize.ToString();
		}
	}
}
