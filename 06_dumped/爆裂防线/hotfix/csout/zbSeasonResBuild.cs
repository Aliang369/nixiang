using UnityEngine;

public class zbSeasonResBuild : zbSeasonBuild
{
	private ResourceData m_resourceData;

	private zbSeasonResStateMachine m_fsm;

	public void Init(ResourceData resourceData)
	{
		m_resourceData = resourceData;
		base.Init(resourceData.buildId, ESeasonBuildType.Resource, resourceData.areaId);
		m_fsm = new zbSeasonResStateMachine();
		m_fsm.Init();
		m_fsm.SetState(ESeasonResState.Locked);
		zbSeasonResourceCsvRecord recordByKey = zbDataConfig.Instance.seasonResourceCsvTable.GetRecordByKey(m_resourceData.buildId);
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
		zbUIMgr.Instance.ShowSeasonResourceDetail(m_resourceData, this);
	}
}
