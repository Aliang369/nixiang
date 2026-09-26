using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSeasonArea : MonoBehaviour
{
	private Transform m_buildRoot;

	public Transform m_bordTF;

	private SpriteRenderer m_bordSp;

	private GameObject m_normalBuildPrefab;

	private GameObject m_forceBuildPrefab;

	private GameObject m_resourceBuildPrefab;

	[NonSerialized]
	public List<zbSeasonBuild> m_builds = new List<zbSeasonBuild>();

	public zbSeasonBuild cityBuild;

	public ServerAreaData areaData;

	public int areaId;

	private void Awake()
	{
		m_bordTF = ((Component)this).transform.Find("bord");
		m_bordSp = ((Component)m_bordTF).GetComponent<SpriteRenderer>();
		m_buildRoot = ((Component)this).transform.Find("buildroot");
		if ((Object)(object)m_normalBuildPrefab == (Object)null)
		{
			m_normalBuildPrefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/season/normalBuild.prefab");
		}
		if ((Object)(object)m_forceBuildPrefab == (Object)null)
		{
			m_forceBuildPrefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/season/forceBuild.prefab");
		}
		if ((Object)(object)m_resourceBuildPrefab == (Object)null)
		{
			m_resourceBuildPrefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/season/resourceBuild.prefab");
		}
	}

	public void Init(ServerAreaData data)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		ClearBuilds();
		areaData = data;
		areaId = areaData.areaId;
		if (data.isCenter)
		{
			((Component)this).transform.position = Vector3.zero;
			m_bordTF.localScale = new Vector3(2f, 2f, 1f);
		}
		else
		{
			Vector2 val = zbSelf.Instance.seasonHandler.TransformLogicPosToClientRenderWorldPos(data.logicX, data.logicY);
			((Component)this).transform.position = new Vector3(val.x, val.y, 0f);
		}
		SetColor(zbSeasonMapDefine.GetForceColor(data.force));
		CreateBuilds();
	}

	public void SetColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_bordSp.color = color;
	}

	private void CreateBuilds()
	{
		if ((Object)(object)m_normalBuildPrefab == (Object)null || (Object)(object)m_forceBuildPrefab == (Object)null || (Object)(object)m_resourceBuildPrefab == (Object)null || (Object)(object)m_buildRoot == (Object)null)
		{
			return;
		}
		CreateBuild();
		foreach (ResourceData resource in areaData.resources)
		{
			CreateResourceBuild(resource);
		}
	}

	private void CreateBuild()
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		zbSeasonAreaCsvRecord seasonMapRecord = zbSelf.Instance.seasonHandler.GetSeasonMapRecord(areaData.areaId, 1);
		GameObject val;
		ESeasonBuildType type;
		switch (zbSelf.Instance.seasonHandler.GetAreaType(seasonMapRecord.areaType))
		{
		case ESeasonAreaType.Birth:
			val = Object.Instantiate<GameObject>(m_forceBuildPrefab, m_buildRoot);
			type = ESeasonBuildType.Force;
			break;
		case ESeasonAreaType.Neutral:
			val = Object.Instantiate<GameObject>(m_normalBuildPrefab, m_buildRoot);
			type = ESeasonBuildType.Normal;
			break;
		case ESeasonAreaType.Center:
			val = Object.Instantiate<GameObject>(m_normalBuildPrefab, m_buildRoot);
			type = ESeasonBuildType.Center;
			break;
		default:
			val = Object.Instantiate<GameObject>(m_normalBuildPrefab, m_buildRoot);
			type = ESeasonBuildType.Normal;
			break;
		}
		zbSeasonBuild component = val.GetComponent<zbSeasonBuild>();
		val.transform.localPosition = Vector3.zero;
		if ((Object)(object)component != (Object)null)
		{
			component.Init(seasonMapRecord.cityId, type, areaId);
			cityBuild = component;
			m_builds.Add(component);
			if (!zbSeasonMgr.Instance.SeasonMap.m_buildDict.ContainsKey(Vector2.op_Implicit(((Component)cityBuild).transform.position)))
			{
				zbSeasonMgr.Instance.SeasonMap.m_buildDict[Vector2.op_Implicit(((Component)cityBuild).transform.position)] = ((Component)cityBuild).gameObject;
			}
		}
	}

	private void CreateResourceBuild(ResourceData resourceData)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(m_resourceBuildPrefab, m_buildRoot);
		val.transform.position = new Vector3(((Component)cityBuild).transform.position.x + resourceData.logicX, ((Component)cityBuild).transform.position.y + resourceData.logicY, val.transform.position.z);
		zbSeasonResBuild component = val.GetComponent<zbSeasonResBuild>();
		if ((Object)(object)component != (Object)null)
		{
			component.Init(resourceData);
			m_builds.Add(component);
			if (!zbSeasonMgr.Instance.SeasonMap.m_buildDict.ContainsKey(Vector2.op_Implicit(((Component)component).transform.position)))
			{
				zbSeasonMgr.Instance.SeasonMap.m_buildDict[Vector2.op_Implicit(((Component)component).transform.position)] = ((Component)component).gameObject;
			}
		}
	}

	private void ClearBuilds()
	{
		for (int num = m_builds.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)m_builds[num] != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)m_builds[num]).gameObject);
			}
		}
		m_builds.Clear();
	}

	private void OnDestroy()
	{
		ClearBuilds();
	}

	private void Update()
	{
		if (m_builds.Count <= 0)
		{
			return;
		}
		foreach (zbSeasonBuild build in m_builds)
		{
			build.Update();
		}
	}
}
