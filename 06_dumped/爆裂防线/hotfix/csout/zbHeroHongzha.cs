using UnityEngine;

public class zbHeroHongzha : zbHeroMain
{
	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		zbSkillSystem.Instance.CreateSkill(this, targetPosition, Vector2.up, targetMonsterId);
		zbAudioMgr.Instance.PlayAudio("audio/combat/ranyoudan_fire.ogg");
	}
}
