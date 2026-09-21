public class FestivalCheckInResult
{
	public FestivalDef festivalDef;

	public int alreadyCheckInCount;

	public GlobalRewardInfo reward;

	public int CurrentCheckInIndex => alreadyCheckInCount - 1;
}
