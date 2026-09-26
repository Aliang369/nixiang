using System.Collections;
using UnityEngine;

public class zbMonsterSkillIndex30092 : zbMonsterSkillBase
{
	private stMonsterSkillIndex30092 data = new stMonsterSkillIndex30092();

	private int monsterUUID;

	private long curHp;

	private bool beginJilu;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.everyTime = monsterSkillRecord.param[0];
		data.durationTime = monsterSkillRecord.param[1];
		beginJilu = false;
		data.everyTimeKeep = 0f;
		data.durationTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
	}

	public override void Update()
	{
		if (IsBuffStop())
		{
			return;
		}
		data.everyTimeKeep += Time.deltaTime;
		if (data.everyTimeKeep >= data.everyTime)
		{
			zbMonsterEffect zbMonsterEffect2 = CreateEffectSpine();
			if ((Object)(object)zbMonsterEffect2 != (Object)null)
			{
				zbMonsterEffect2.PlayEffectAnim("animation", loop: false);
			}
			monster.canMoveOrAttack = false;
			data.everyTimeKeep = 0f;
			data.durationTimeKeep = data.durationTime;
			curHp = monster.curHp;
			beginJilu = true;
			((MonoBehaviour)monster).StartCoroutine(CleanupSequence());
		}
		if (!beginJilu)
		{
			return;
		}
		data.durationTimeKeep -= Time.deltaTime;
		if (data.durationTimeKeep <= 0f)
		{
			zbMonsterEffect zbMonsterEffect3 = CreateEffectSpine();
			if ((Object)(object)zbMonsterEffect3 != (Object)null)
			{
				zbMonsterEffect3.PlayEffectAnim("animation2", loop: false);
			}
			monster.canMoveOrAttack = false;
			data.everyTimeKeep = 0f;
			data.durationTimeKeep = data.durationTime;
			monster.SetCurHp(curHp);
			beginJilu = false;
			((MonoBehaviour)monster).StartCoroutine(CleanupSequence());
		}
	}

	public IEnumerator CleanupSequence()
	{
		yield return (object)new WaitForSeconds(1.6666666f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
			FreeEffectSpine();
		}
	}

	public override void ClearList()
	{
	}
}
