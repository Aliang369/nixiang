using UnityEngine;

public class zbSeasonCityBuild : zbSeasonBuild
{
	private zbSeasonCityStateMachine m_fsm;

	public override void Init(int buildId, ESeasonBuildType type, int areaId)
	{
		base.Init(buildId, type, areaId);
		m_fsm = new zbSeasonCityStateMachine();
		m_fsm.Init();
		m_fsm.SetState(ESeasonCityState.Unknown);
		zbSeasonCityCsvRecord recordByKey = zbDataConfig.Instance.seasonCityCsvTable.GetRecordByKey(buildId);
		m_spriteRenderer.sprite = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.image);
	}

	public override void Update()
	{
		base.Update();
		m_fsm.Update();
	}

	public override void OnClick()
	{
		base.OnClick();
		zbWsMsgHandler.Instance.Season_City_Detail_Req(m_areaId);
	}
}
