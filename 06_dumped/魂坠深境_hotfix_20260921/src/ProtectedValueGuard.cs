using System;
using System.Collections.Generic;

public static class ProtectedValueGuard
{
	private static readonly HashSet<int> _tamperedKeys = new HashSet<int>();

	public static Action OnFirstTamper;

	public static bool CheatSuspected { get; private set; }

	public static int TamperCount { get; private set; }

	public static int MismatchReadCount { get; private set; }

	public static void ReportTamper(int fieldKey)
	{
		checked
		{
			MismatchReadCount++;
			if (!_tamperedKeys.Add(fieldKey))
			{
				return;
			}
			TamperCount++;
			if (CheatSuspected)
			{
				return;
			}
			CheatSuspected = true;
			try
			{
				OnFirstTamper?.Invoke();
			}
			catch
			{
			}
		}
	}

	public static void Reset()
	{
		CheatSuspected = false;
		TamperCount = 0;
		MismatchReadCount = 0;
		_tamperedKeys.Clear();
	}

	public static void Restore(int tamperCount, int mismatchReadCount)
	{
		TamperCount = ((tamperCount >= 0) ? tamperCount : 0);
		MismatchReadCount = ((mismatchReadCount >= 0) ? mismatchReadCount : 0);
		CheatSuspected = TamperCount > 0;
	}
}
