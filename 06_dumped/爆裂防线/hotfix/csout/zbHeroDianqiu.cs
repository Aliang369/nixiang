using UnityEngine;

public class zbHeroDianqiu : zbHeroMain
{
	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 gunpointPosition = GetGunpointPosition();
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_1.ogg");
		((zbBulletDianqiu)zbSkillSystem.Instance.CreateSkill(this, gunpointPosition, Vector2.up, targetMonsterId)).setDianqiuMonsterId(targetMonsterId, targetPosition, null, 0);
	}
}
