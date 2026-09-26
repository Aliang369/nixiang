using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10352 : zbMonsterSkillBase
{
	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	private int monsterUUID;

	private Vector2 vector;

	public override void RefreshProperty()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshProperty();
		monster.monsterIndex10352.everyTime = monsterSkillRecord.param[0];
		monster.monsterIndex10352.subSkillId = (int)monsterSkillRecord.param[1];
		monster.monsterIndex10352.everyTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
		vector = Vector2.op_Implicit(((Component)monster).transform.position);
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
	}

	public override void Update()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				ActionCoroutine();
			}
		}
		if (IsBuffStop())
		{
			return;
		}
		monster.monsterIndex10352.everyTimeKeep += Time.deltaTime;
		if (monster.monsterIndex10352.everyTimeKeep >= monster.monsterIndex10352.everyTime)
		{
			CreateEffectSpine();
			monster.monsterIndex10352.everyTimeKeep = 0f;
			monster.canMoveOrAttack = false;
			vector = Vector2.op_Implicit(((Component)monster).transform.position);
			TrackEntry val = PlayTackAnimation(1, flag: false);
			if (val != null)
			{
				m_DurationMax = val.AnimationEnd;
			}
			else
			{
				m_DurationMax = 5f / 6f;
			}
			m_bExecReturn = true;
			m_DurationTime = 0f;
		}
	}

	public void ActionCoroutine()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			FreeEffectSpine();
			return;
		}
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) != (Object)null)
		{
			vector = Vector2.op_Implicit(((Component)monster).transform.position);
		}
		zbHero zbHero2 = monster.combatData.masterTeam.heroList[0];
		zbSkillSystem.Instance.CreateSubSkill(monster.monsterIndex10352.subSkillId, zbHero2, null, zbHero2.heroAttr.skillAttrList, Vector2.op_Implicit(vector), Vector2.up, monster.GetUUID());
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
	}
}
