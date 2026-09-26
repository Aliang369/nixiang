using System.Collections.Generic;
using Proto.Enums;

public class zbMechaSkillBase : zbSkillBase
{
	public HashSet<int> ignoreList = new HashSet<int>();

	public float damage { get; protected set; }

	protected override void OnInited()
	{
		base.skillParentType = zbSkillParentType.MechaSkill;
		base.OnInited();
	}

	public virtual void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		mechaSkillRecord = _mechaSkillRecord;
		damage = _damage;
		skillAttrList[EAttrSkillType.Damage] = damage;
		skillAttrList[EAttrSkillType.PeripheralDamage] = damage;
		skillAttrList[EAttrSkillType.CentralDamage] = damage;
		skillAttrList[EAttrSkillType.ExplosionDamage] = damage;
		skillAttrList[EAttrSkillType.SplashDamage] = damage;
	}

	protected override void Update()
	{
		base.Update();
	}
}
