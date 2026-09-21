using UnityEngine;

public sealed class DailyChallengeRecordSettlementPanel : MonoBehaviour
{
	[SerializeField]
	private DailyChallengeTextPanel summaryPanel;

	[SerializeField]
	private GameObject legacyContractArea;

	[SerializeField]
	private GameObject legacyRankingScoreArea;

	public bool Setup(GameRecord record)
	{
		bool flag = record != null && record.dailyChallengeContext?.isDailyChallenge == true && record.dailyChallengeSettlement != null;
		base.gameObject.SetActive(flag);
		legacyContractArea?.SetActive(!flag);
		legacyRankingScoreArea?.SetActive(!flag);
		if (!flag)
		{
			return false;
		}
		DailyChallengeSettlement dailyChallengeSettlement = record.dailyChallengeSettlement;
		DailyChallengeScoreContractPresentation contract = dailyChallengeSettlement.scoreContract ?? record.dailyChallengeContext.scoreCompetitionContract;
		summaryPanel.Setup(DailyChallengeDisplayText.GetSettlementTitleText(contract), DailyChallengeDisplayText.GetGameOverSettlementText(dailyChallengeSettlement), active: true);
		return true;
	}
}
