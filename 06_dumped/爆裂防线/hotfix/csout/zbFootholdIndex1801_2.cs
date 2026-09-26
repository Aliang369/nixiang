public class zbFootholdIndex1801_2 : zbFootholdSkillBase
{
	public stFootholdIndex1801 footholdIndex1801_2Data;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.MonsterDie;
		base.OnInited();
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1801_2Data = (stFootholdIndex1801)_footholdIndex;
	}
}
