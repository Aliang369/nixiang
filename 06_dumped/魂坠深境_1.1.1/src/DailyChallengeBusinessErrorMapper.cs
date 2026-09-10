using System;

public static class DailyChallengeBusinessErrorMapper
{
	public static DailyChallengeBusinessError FromCode(int code)
	{
		if (!Enum.IsDefined(typeof(DailyChallengeBusinessError), code))
		{
			return DailyChallengeBusinessError.Unknown;
		}
		return (DailyChallengeBusinessError)code;
	}
}
