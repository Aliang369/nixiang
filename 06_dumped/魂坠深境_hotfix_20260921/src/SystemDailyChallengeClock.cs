using System;

public sealed class SystemDailyChallengeClock : IDailyChallengeClock
{
	public DateTime UtcNow => DateTime.UtcNow;
}
