using System;
using System.Collections.Generic;

public static class DailyChallengeRecordMetrics
{
	public static int GetBaseExperience(List<GameRecord.ExpChangeRecord> records)
	{
		if (records == null)
		{
			return 0;
		}
		List<GameRecord.ExpChangeRecord> records2 = records.FindAll((GameRecord.ExpChangeRecord record) => record.source == GameRecord.ExpChangeRecord.ExpChangeSource.Progress || record.source == GameRecord.ExpChangeRecord.ExpChangeSource.Difficulty);
		return Math.Max(0, GameRecord.ExpChangeRecord.GetExpSum(records2));
	}
}
