using UnityEngine;

public class zbHeroBingbao : zbHeroMain
{
	private bool isFirst;

	public int actionNum;

	public override void SetFirst(bool _isFirst)
	{
		isFirst = _isFirst;
		if (_isFirst)
		{
			actionNum++;
		}
	}

	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		zbSkillBase zbSkillBase2 = zbSkillSystem.Instance.CreateSkill(this, targetPosition, Vector2.up, targetMonsterId);
		if (!((Object)(object)zbSkillBase2 == (Object)null))
		{
			zbSkillBase2.SetFirst(isFirst);
			zbAudioMgr.Instance.PlayAudio("audio/combat/bingbaofashengqi_fire.ogg");
		}
	}
}
