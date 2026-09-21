using System;

public class ContractState : IValidate
{
	[SaveField(null, null)]
	public ContractDef contractDef;

	[SaveField(null, null)]
	public bool finishedThisCycle;

	[SaveField(null, null)]
	public DifficultyLevel finishedMaxDifficultyThisCycle;

	[SaveField(null, null)]
	public long lastFinishedDataTimeTicks;

	[SaveField(null, null)]
	public SeasonDef lastFinishedSeason;

	public UnityDateTime? LastFinishedDateTime
	{
		get
		{
			if (lastFinishedDataTimeTicks < 0)
			{
				return null;
			}
			return new UnityDateTime(new DateTime(lastFinishedDataTimeTicks));
		}
	}

	public UnityDateTime? NextCycleResetTime
	{
		get
		{
			if (contractDef.contractType == ContractDef.ContractType.WeekShuffle)
			{
				return ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeekPeriod);
			}
			return null;
		}
	}

	public bool IsDifficultyFinished(DifficultyLevel difficultyLevel)
	{
		return finishedMaxDifficultyThisCycle >= difficultyLevel;
	}

	private ContractState()
	{
	}

	public ContractState(ContractDef contractDef)
	{
		this.contractDef = contractDef;
		finishedThisCycle = false;
		finishedMaxDifficultyThisCycle = DifficultyLevel.Not;
		lastFinishedDataTimeTicks = -1L;
		lastFinishedSeason = null;
	}

	public bool IsValidate()
	{
		return contractDef != null;
	}
}
