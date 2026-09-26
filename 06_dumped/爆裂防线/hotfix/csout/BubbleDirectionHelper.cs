using System;
using UnityEngine;

public static class BubbleDirectionHelper
{
	private static readonly Vector2[] DirectionTable;

	static BubbleDirectionHelper()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		DirectionTable = (Vector2[])(object)new Vector2[361];
		for (int i = 0; i <= 360; i++)
		{
			float num = (float)i * ((float)Math.PI / 180f);
			DirectionTable[i] = Vector2.op_Implicit(new Vector3(Mathf.Cos(num), Mathf.Sin(num), 0f));
		}
	}

	public static Vector2 GetRandomDirectionalVector()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int num = ((!(Rand.value < 0.5f)) ? Rand.Range(210, 361) : Rand.Range(0, 151));
		return DirectionTable[num];
	}
}
