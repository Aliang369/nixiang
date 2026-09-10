using System;

public static class DailyChallengeWillPowerMetrics
{
	public static int UpdateMaximum(int recordedMaximum, int currentUsedWillPower)
	{
		return Math.Max(Math.Max(recordedMaximum, 0), Math.Max(currentUsedWillPower, 0));
	}

	public static int ResolveMaximum(bool hasRecordedMaximum, int recordedMaximum, int legacyRemainingWillPower)
	{
		return Math.Max(hasRecordedMaximum ? recordedMaximum : legacyRemainingWillPower, 0);
	}
}
