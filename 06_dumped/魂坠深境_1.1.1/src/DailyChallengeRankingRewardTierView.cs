using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DailyChallengeRankingRewardTierView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Image rewardIcon;

	[SerializeField]
	private TextMeshProUGUI amount;

	public void Set(int maxRank, Sprite icon, int rewardAmount)
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		title.text = ((maxRank != 1) ? (instance?.term_daily_challenge_top_rank_arg0.FormatWith(maxRank) ?? string.Empty) : (instance?.term_daily_challenge_first_place ?? string.Empty));
		rewardIcon.sprite = icon;
		rewardIcon.color = ((icon == null) ? Color.clear : Color.white);
		amount.text = rewardAmount.ToString();
	}
}
