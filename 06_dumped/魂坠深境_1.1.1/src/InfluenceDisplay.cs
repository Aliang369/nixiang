using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfluenceDisplay : CellUI<Influence>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		protected InfluenceDisplay display => parent as InfluenceDisplay;

		protected Influence influence => display.influence;
	}

	[SerializeField]
	public Image image;

	[SerializeField]
	private bool discoverDisplay;

	[SerializeField]
	private TextMeshProUGUI influenceValueText;

	[SerializeField]
	private InfluenceAttachIdentityBadge attachIdentityBadge;

	public Influence influence => base.setupData;

	public bool DisplayNotDiscovered
	{
		get
		{
			if (discoverDisplay)
			{
				return !influence.def.IsDiscovered(CellUI<Influence>.applicationState);
			}
			return false;
		}
	}

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(Influence _)
	{
		image.sprite = influence.def.Icon;
		image.color = (DisplayNotDiscovered ? ColorHelper.IndexUnDiscoverColor : Color.white);
		if (DisplayNotDiscovered)
		{
			attachIdentityBadge?.Hide();
		}
		else
		{
			attachIdentityBadge?.Setup(influence.def);
		}
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnset()
	{
		attachIdentityBadge?.Hide();
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		if (influence != null && influenceValueText != null)
		{
			if (influence.def.enableInfluenceValue)
			{
				influenceValueText.gameObject.SetActive(value: true);
				influenceValueText.text = influence.influenceValue.ToString();
			}
			else
			{
				influenceValueText.gameObject.SetActive(value: false);
			}
		}
	}
}
