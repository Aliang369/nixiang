using System;
using TMPro;

public class RecordRankingScoreDetailPanel : UIBase
{
	public GameButton backButton;

	public TextMeshProUGUI totalScoreText;

	public TextMeshProUGUI scoreDetailText;

	[NonSerialized]
	private GameRecord record;

	protected override void Init()
	{
		backButton.onClick.AddListener(_OnBackButtonClicked);
	}

	public void Setup(GameRecord record)
	{
		this.record = record;
		totalScoreText.text = record.rankScore.ToString();
		DailyChallengeRunContext dailyChallengeContext = record.dailyChallengeContext;
		if (dailyChallengeContext != null && dailyChallengeContext.isDailyChallenge && record.dailyChallengeSettlement != null)
		{
			scoreDetailText.text = DailyChallengeDisplayText.GetScoreBreakdownText(record.dailyChallengeSettlement);
			SetDirty();
			return;
		}
		string text = "";
		foreach (RankingScorePoint item in record.GenerateRecordRankScorePoints())
		{
			text = text + TextDisplayHelper.NumToStringWithSign(item.score) + "     " + item.GetDescription(UIBase.applicationState, record) + Environment.NewLine;
		}
		scoreDetailText.text = text;
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		base.gameObject.SetActive(value: false);
	}
}
