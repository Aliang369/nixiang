using System;

public class ForerunnerLegacy
{
	[SaveField(null, null)]
	public string guid;

	[SaveField(null, null)]
	public string gameGuid;

	[SaveField(null, null)]
	public long sourcePlayerId;

	[SaveField(null, null)]
	public GameRecord.ItemRecord legacyItem;

	[SaveField(null, null)]
	public GameRecord.SkillRecord legacySkill;

	[SaveField(null, null)]
	public int stage;

	[SaveField(null, null)]
	public DraftMapRef sourceDraftMap;

	[SaveField(null, null)]
	public int locationPivotIndex;

	[SaveField(null, null)]
	public DateTime leaveTime;

	[SaveField(null, null)]
	public DateTime expireTime;

	[SaveField(null, null)]
	public string signature;

	[SaveField(null, null)]
	public string note;

	[SaveField(null, null)]
	public string presetMessageId;

	[SaveField(null, null)]
	public int shaderEncounterCount;

	[SaveField(null, null)]
	public int shareGetCount;

	[SaveField(null, null)]
	public int shareWinCount;

	[SaveField(null, null)]
	public int upvoteCount;

	[SaveField(null, null)]
	public int downvoteCount;

	[SaveField(null, null)]
	public bool shareLegacy;

	[SaveField(null, null)]
	public bool storyOnly;

	public int SumVoteCount => upvoteCount + downvoteCount;

	public void SetItem(Item it)
	{
		if (it == null)
		{
			legacyItem = null;
		}
		else
		{
			legacyItem = new GameRecord.ItemRecord(it);
		}
	}

	public void SetSkill(GameSkill sk)
	{
		if (sk == null)
		{
			legacySkill = null;
		}
		else
		{
			legacySkill = new GameRecord.SkillRecord(sk, activated: true, consumed: false);
		}
	}
}
