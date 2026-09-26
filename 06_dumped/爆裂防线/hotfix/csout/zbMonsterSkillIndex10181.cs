using UnityEngine;

public class zbMonsterSkillIndex10181 : zbMonsterSkillBase
{
	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	public stMonsterSkillIndex10181 monsterIndex10181 = new stMonsterSkillIndex10181();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monsterIndex10181.everyTime = monsterSkillRecord.param[0];
		monsterIndex10181.distance = monsterSkillRecord.param[1] / 100f;
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
	}

	public override void Update()
	{
		if (monster.fsm.GetStateId() != EMonsterState.Move || IsBuffStop())
		{
			return;
		}
		if (Time.frameCount != monsterIndex10181.frameCount)
		{
			monsterIndex10181.everyTimeKeep += Time.deltaTime;
			if (monsterIndex10181.everyTimeKeep >= monsterIndex10181.everyTime)
			{
				monsterIndex10181.everyTimeKeep = 0f;
				ActionCoroutine();
				m_DurationMax = 1f;
				m_bExecReturn = true;
				m_DurationTime = 0f;
			}
			monsterIndex10181.frameCount = Time.frameCount;
		}
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				monster.anim.spineAnimation.StopAnimation(1);
			}
		}
	}

	public void ActionCoroutine()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		PlayTackAnimation(1, flag: false);
		zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
		if (((Component)monster).transform.position.x > combatData.nodeRootPositonX)
		{
			zbExtraDirection2.direction = Vector2.op_Implicit(Vector2.left);
		}
		else
		{
			zbExtraDirection2.direction = Vector2.op_Implicit(Vector2.right);
		}
		zbExtraDirection2.duration = 1f;
		zbExtraDirection2.speed = monsterIndex10181.distance / 1f;
		monster.AddExtraDirectionList(zbExtraDirection2);
	}

	public override void ClearList()
	{
	}
}
