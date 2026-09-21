using System.Collections;
using UnityEngine;

public static class EnumeratorExtends
{
	public static IEnumerator WaitFrame(float time)
	{
		while (time-- > 0f)
		{
			yield return null;
		}
	}

	public static IEnumerator WaitTime(float time)
	{
		while (time > 0f)
		{
			yield return null;
			time -= Time.deltaTime;
		}
	}
}
