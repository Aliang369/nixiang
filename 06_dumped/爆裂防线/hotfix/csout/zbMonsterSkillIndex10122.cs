using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10122 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10122 data = new stMonsterSkillIndex10122();

	public float everyTimeKeep;

	private TrackEntry shootTrack;

	public override void RefreshProperty()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshProperty();
		data.distance_1 = monsterSkillRecord.param[0] / 100f;
		data.distance_2 = monsterSkillRecord.param[1] / 100f;
		everyTimeKeep = 0f;
		if (!monster.isRestoreData)
		{
			shootTrack = PlayTackAnimation(1, flag: false);
			if (shootTrack != null)
			{
				monster.monsterSkillIndex10122Hiding = true;
				monster.canMoveOrAttack = false;
			}
			float num = Rand.Range(data.distance_1 + combatData.defense.GetAlertPositionY(), data.distance_2 + combatData.defense.GetAlertPositionY());
			Vector3 position = default(Vector3);
			((Vector3)(ref position))._002Ector(((Component)monster).gameObject.transform.position.x, num, ((Component)monster).gameObject.transform.position.z);
			((Component)monster).gameObject.transform.position = position;
		}
	}

	public override void Update()
	{
		if (shootTrack != null)
		{
			everyTimeKeep += Time.deltaTime;
			if (everyTimeKeep > shootTrack.AnimationEnd)
			{
				monster.monsterSkillIndex10122Hiding = false;
				monster.canMoveOrAttack = true;
				monster.anim.spineAnimation.StopAnimation(1);
			}
		}
		else
		{
			monster.monsterSkillIndex10122Hiding = false;
			monster.canMoveOrAttack = true;
		}
	}
}
