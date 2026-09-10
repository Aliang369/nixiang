using TMPro;
using UnityEngine.UI;

public class DifficultySelectionPanelRewardDisplay : CellUI<IGlobalItemDisplaySetup>
{
	public GlobalItemDisplay display;

	public Image gotImage;

	public TextMeshProUGUI notGotText;

	protected override void OnSetup(IGlobalItemDisplaySetup setupData)
	{
		display.Setup(setupData);
	}

	public void SetGot(bool got, string notGotHintText)
	{
		gotImage.gameObject.SetActive(got);
		notGotText.gameObject.SetActive(!got);
		notGotText.text = notGotHintText;
	}
}
