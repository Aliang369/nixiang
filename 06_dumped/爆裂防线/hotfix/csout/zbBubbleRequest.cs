using UnityEngine;

public struct zbBubbleRequest
{
	public Vector3 WorldPos;

	public ECombatBubble BubbleType;

	public long Num;

	public float EnqueueTime;

	public zbBubbleRequest(Vector3 pos, ECombatBubble type, long num)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		WorldPos = pos;
		BubbleType = type;
		Num = num;
		EnqueueTime = Time.time;
	}
}
