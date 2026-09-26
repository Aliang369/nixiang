using UnityEngine;

public class zbFootholdIndex1701_2 : zbFootholdSkillBase
{
	public stFootholdIndex1701 footholdIndex1701_2Data;

	protected override void OnInited()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Background;
		base.OnInited();
		canOutWindow = true;
		m_direction = Vector3.up;
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1701_2Data = (stFootholdIndex1701)_footholdIndex;
	}
}
