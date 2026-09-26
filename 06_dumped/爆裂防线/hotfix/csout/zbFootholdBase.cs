using UnityEngine;
using zbType;

public class zbFootholdBase : zbAttackerBase
{
	public zbFootholdCsvRecord footholdRecord;

	public zbFootholdEffectCsvRecord footholdEffectRecord;

	public virtual void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		combatData = _combatData;
		team = _team;
		SetFightPeopleType(EFightPeopleType.Foothold);
		footholdRecord = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(team.battleTeamInfo.FootholdInfo.FootholdId);
		footholdEffectRecord = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(team.battleTeamInfo.FootholdInfo.FootholdId, team.battleTeamInfo.FootholdInfo.Star);
		Init();
	}

	public virtual void Init()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = new Vector3(-20f, -20f, 0f);
		m_sortType = ESortLayerType.Hero;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		heroAttr = new zbHeroAttr();
		heroAttr.Init(this);
		heroAttr.RefreshFootholdAttr();
	}

	public virtual void RefreshParam()
	{
	}

	public override int GetSkillType()
	{
		return 201;
	}

	protected override void Update()
	{
	}
}
