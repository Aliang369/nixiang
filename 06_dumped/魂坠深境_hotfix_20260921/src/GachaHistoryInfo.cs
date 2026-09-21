using System;

public class GachaHistoryInfo
{
	[SaveField(null, null)]
	public GachaPoolDef gachaPoolDef;

	[SaveField(null, null)]
	public GachaPoolDef.GachaContentType contentType;

	[SaveField(null, null)]
	public CTScriptableObject content;

	[SaveField(null, null)]
	public QualityLevel qualityLevel;

	[SaveField(null, null)]
	public GlobalRewardInfo reward;

	[SaveField(null, null)]
	public bool wish;

	[SaveField(null, null)]
	public DateTime date;

	public bool isNew => ((IGlobalItemDisplaySetup)reward)?.isNew ?? false;
}
