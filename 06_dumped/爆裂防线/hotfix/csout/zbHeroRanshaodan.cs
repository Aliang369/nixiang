using UnityEngine;

public class zbHeroRanshaodan : zbHeroMain
{
	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 gunpointPosition = GetGunpointPosition();
		Vector3 val = targetPosition - gunpointPosition;
		Vector2 direction = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
		zbSkillSystem.Instance.CreateSkill(this, gunpointPosition, Vector2.up, targetMonsterId).SetDirection(direction);
		zbAudioMgr.Instance.PlayAudio("audio/combat/ranyoudan_fire.ogg");
	}
}
