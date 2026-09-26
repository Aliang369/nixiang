using System.Collections.Generic;
using Proto.Enums;

public class zbWallSkillBase : zbSkillBase
{
	public HashSet<int> ignoreList = new HashSet<int>();

	public int SkillId { get; protected set; }

	public float damage { get; protected set; }

	protected override void OnInited()
	{
		base.skillParentType = zbSkillParentType.WallSkill;
		base.OnInited();
	}

	public virtual void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		wallSkillRecord = _wallSkillRecord;
		damage = _damage;
		skillAttrList[EAttrSkillType.Damage] = damage;
	}

	protected override void Update()
	{
		base.Update();
	}
}
