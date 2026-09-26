using System.Collections.Generic;

public class zbFootholdSkillBase : zbSkillBase
{
	public HashSet<int> ignoreList = new HashSet<int>();

	public stFootholdIndex footholdIndex;

	public int SkillId { get; protected set; }

	public float damage { get; protected set; }

	protected override void OnInited()
	{
		base.skillParentType = zbSkillParentType.FootholdSkill;
		base.OnInited();
	}

	public virtual void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		footholdIndex = _footholdIndex;
		footholdEffectRecord = _footholdEffectRecord;
	}

	protected override void Update()
	{
		base.Update();
	}
}
