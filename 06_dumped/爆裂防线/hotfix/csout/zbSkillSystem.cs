using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSkillSystem
{
	private static zbSkillSystem sInstance;

	public zbModRegister modRegister;

	public zbBuffRegister buffRegister;

	public zbMonsterBuffRegister monsterBuffRegister;

	public zbMonsterSkillRegister monsterIndexRegister;

	public zbWallSkillRegister defenseRegister;

	public static zbSkillSystem Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbSkillSystem();
				sInstance.Init();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		modRegister = new zbModRegister();
		modRegister.Init();
		buffRegister = new zbBuffRegister();
		buffRegister.Init();
		monsterBuffRegister = new zbMonsterBuffRegister();
		monsterBuffRegister.Init();
		monsterIndexRegister = new zbMonsterSkillRegister();
		monsterIndexRegister.Init();
		defenseRegister = new zbWallSkillRegister();
		defenseRegister.Init();
	}

	public zbSkillBase CreateSkill(string prefabPath, zbAttackerBase owner, Vector3 spawnPostion, Vector2 dir, int _mainMonsterUUID = 0, int _targetMonsterUUID = 0, int mechaSkillId = 0)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)owner == (Object)null || owner.combatData == null || !((Component)owner.combatData.nodeRoot).gameObject.activeInHierarchy)
		{
			return null;
		}
		if (prefabPath == "")
		{
			return null;
		}
		zbSkillBase skill = owner.combatData.skillPoolMgr.GetSkill(prefabPath);
		if ((Object)(object)skill != (Object)null)
		{
			skill.Init(prefabPath, owner, null, owner.heroAttr.skillAttrList, spawnPostion, dir, _mainMonsterUUID, 0, _targetMonsterUUID, mechaSkillId);
		}
		return skill;
	}

	public zbSkillBase CreateSkill(zbAttackerBase owner, Vector3 spawnPostion, Vector2 dir, int _mainMonsterUUID = 0)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		string bulletPrefab = owner.skillTypeRecord.bulletPrefab;
		return CreateSkill(bulletPrefab, owner, spawnPostion, dir, _mainMonsterUUID);
	}

	public zbSkillBase CreateSubSkill(int subBulletId, zbAttackerBase owner, zbSkillBase skillBase, Dictionary<EAttrSkillType, float> skillAttrList, Vector3 spawnPostion, Vector2 dir, int _mainMonsterUUID = 0, int _targetMonsterUUID = 0)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)owner == (Object)null || owner.combatData == null || !((Component)owner.combatData.nodeRoot).gameObject.activeInHierarchy)
		{
			return null;
		}
		string skillSubPrefab = stSkillSubPrefab.GetSkillSubPrefab(subBulletId);
		if (skillSubPrefab == "")
		{
			return null;
		}
		if ((Object)(object)skillBase != (Object)null && skillBase.hero.GetSkillType() == 11)
		{
			zbMonster monsterById = owner.combatData.monsters.GetMonsterById(_mainMonsterUUID);
			if ((Object)(object)monsterById != (Object)null)
			{
				spawnPostion = ((Component)monsterById).transform.position;
			}
			else
			{
				monsterById = owner.combatData.monsters.GetMonsterById(_targetMonsterUUID);
				if ((Object)(object)monsterById != (Object)null)
				{
					spawnPostion = ((Component)monsterById).transform.position;
				}
			}
		}
		switch (subBulletId)
		{
		case 10203:
		case 11302:
			zbSubNotAnimation.CalSubNotAnimation(subBulletId, owner, _mainMonsterUUID, spawnPostion, skillBase, dir);
			return null;
		case 10001:
		{
			zbMonster monsterById2 = owner.combatData.monsters.GetMonsterById(_mainMonsterUUID);
			if ((Object)(object)monsterById2 == (Object)null || monsterById2.isEliteBoss || monsterById2.GetAttrValueByType(EAttrMonsterType.Mass) > 1000f)
			{
				return null;
			}
			break;
		}
		}
		zbSkillBase skill = owner.combatData.skillPoolMgr.GetSkill(skillSubPrefab);
		if ((Object)(object)skill != (Object)null)
		{
			skill.Init(skillSubPrefab, owner, skillBase, skillAttrList, spawnPostion, dir, _mainMonsterUUID, subBulletId, _targetMonsterUUID);
		}
		return skill;
	}

	public float SafeCheckAttrNeedDiv100(EAttrSkillType attrSkillType, float value)
	{
		if (IsAttrNeedDiv100(attrSkillType))
		{
			return value / 100f;
		}
		return value;
	}

	private bool IsAttrNeedDiv100(EAttrSkillType attrSkillType)
	{
		if (attrSkillType != EAttrSkillType.AlertRange && attrSkillType != EAttrSkillType.DamageRange && attrSkillType != EAttrSkillType.ExplosionRange && attrSkillType != EAttrSkillType.BulletSpeed && attrSkillType != EAttrSkillType.MoveSpeed && attrSkillType != EAttrSkillType.Knockback)
		{
			return attrSkillType == EAttrSkillType.Tow;
		}
		return true;
	}
}
