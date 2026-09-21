using UnityEngine;

[CreateAssetMenu(fileName = "得分竞赛契约", menuName = "每日挑战/得分竞赛契约")]
public class ScoreCompetitionContractDef : CTScriptableObject
{
	[LocalizeText("得分竞赛契约名字")]
	[CTSOAssetName]
	public string displayName;

	[LocalizeText("得分竞赛契约文本")]
	[TextArea]
	public string description;

	[LocalizeText("计分方式")]
	[TextArea]
	public string scoringMethod;

	public Sprite icon;

	public ScoreCompetitionRule rule;

	public override string CTAssetIDPrepend => "SCORECONTRA";

	public DailyChallengeScoreContractPresentation ToPresentation()
	{
		return DailyChallengeScoreContractPresentation.FromMockAsset(this);
	}
}
