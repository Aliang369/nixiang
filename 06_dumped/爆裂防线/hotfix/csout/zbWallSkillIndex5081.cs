using System.Collections;
using UnityEngine;

public class zbWallSkillIndex5081 : zbWallSkillHandleBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		wallSkillDataBase = new stWallSkillIndex5081();
		stWallSkillIndex5081 obj = (stWallSkillIndex5081)wallSkillDataBase;
		int num = 0;
		obj.actionInterval = wallSkillRecord.param[num];
		num++;
		obj.subSkillNumber = (int)wallSkillRecord.param[num];
		num++;
		obj.damageInterval = wallSkillRecord.param[num];
		num++;
		obj.damagePrecent = wallSkillRecord.param[num];
		num++;
		obj.slowDumpPrecent = wallSkillRecord.param[num];
		num++;
		obj.slowDumpTime = wallSkillRecord.param[num];
		num++;
		obj.moveSpeed = wallSkillRecord.param[num] / 100f;
		num++;
		obj.damageRange = wallSkillRecord.param[num] / 100f;
		num++;
		obj.slowDumpPrecent2 = wallSkillRecord.param[num];
		num++;
		obj.slowDumpTime2 = wallSkillRecord.param[num];
		num++;
		obj.duration = wallSkillRecord.param[num];
		num++;
		obj.defenseY = wallSkillRecord.param[num] / 100f;
		num++;
		obj.wallSkillIndex5081 = this;
		obj.lastCallTime = 0f;
	}

	public override void Update()
	{
		base.Update();
		stWallSkillIndex5081 stWallSkillIndex5082 = (stWallSkillIndex5081)wallSkillDataBase;
		stWallSkillIndex5082.lastCallTime += Time.deltaTime;
		if (stWallSkillIndex5082.lastCallTime >= stWallSkillIndex5082.actionInterval)
		{
			zbMonster closestMonster = combatData.monsters.GetClosestMonster(12f);
			if ((Object)(object)closestMonster != (Object)null)
			{
				stWallSkillIndex5082.lastCallTime = 0f;
				((MonoBehaviour)combatData.defense).StartCoroutine(ActionEnter5081(closestMonster));
			}
		}
	}

	public IEnumerator ActionEnter5081(zbMonster targetMonster)
	{
		stWallSkillIndex5081 data = (stWallSkillIndex5081)wallSkillDataBase;
		zbAudioMgr.Instance.PlayAudio("audio/wall/wall_5081.ogg");
		zbWall5081 obj = (zbWall5081)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5081.prefab", combatData.defense, targetMonster.GetCurPosition(), Vector2.up);
		obj.wallSkillIndex5081Data = data;
		obj.SetWallAtr(data.damagePrecent * (float)(int)data.wallSkillIndex5081.team.teamAttack, data.wallSkillIndex5081.wallSkillRecord);
		yield return (object)new WaitForSeconds(1f);
		for (int i = 1; i < data.subSkillNumber; i++)
		{
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			Vector3 spawnPostion = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
			if ((Object)(object)randomMonster != (Object)null)
			{
				spawnPostion = randomMonster.GetCurPosition();
			}
			zbAudioMgr.Instance.PlayAudio("audio/wall/wall_5081.ogg");
			zbWall5081 obj2 = (zbWall5081)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5081.prefab", combatData.defense, spawnPostion, Vector2.up);
			obj2.wallSkillIndex5081Data = data;
			obj2.SetWallAtr(data.damagePrecent * (float)(int)data.wallSkillIndex5081.team.teamAttack, data.wallSkillIndex5081.wallSkillRecord);
			yield return (object)new WaitForSeconds(1f);
		}
	}
}
