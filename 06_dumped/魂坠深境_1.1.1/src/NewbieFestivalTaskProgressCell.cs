using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewbieFestivalTaskProgressCell : CellUI<RoutineQuest>
{
	[SerializeField]
	private TextMeshProUGUI paidTaskText;

	[SerializeField]
	private Slider paidProgressSlider;

	[SerializeField]
	private TextMeshProUGUI paidProgressText;

	private RoutineQuest quest => base.setupData;

	protected override void UpdateDisplay()
	{
		if (quest != null)
		{
			string text = ScriptableObjectSingleton<GameDataManager>.instance.currencyIcon.ToTMPSpriteText();
			string text2 = ScriptableObjectSingleton<GameDataManager>.instance.ticketIcon.ToTMPSpriteText();
			int paidGiftProgress = CellUI<RoutineQuest>.applicationState.newbieFestivalManager.GetPaidGiftProgress(quest);
			paidTaskText.text = CellUI<RoutineQuest>.textData.term_real_money_ticket.FormatWith(text, text2);
			paidProgressText.text = $"{paidGiftProgress}/{60}";
			paidProgressSlider.value = Mathf.Clamp01((float)paidGiftProgress / 60f);
		}
	}
}
