using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;

public class zbSeasonMap
{
	private zbSeasonWnd m_wndParent;

	private Transform m_areaRoot;

	public zbSeasonMapController m_seasonMapCtrl;

	public List<zbSeasonArea> m_seasonAreas = new List<zbSeasonArea>();

	public Dictionary<Vector2, GameObject> m_buildDict = new Dictionary<Vector2, GameObject>();

	private GameObject m_areaPrefab;

	private zbSeasonMiniMap m_seasonMiniMap;

	public zbSeasonMap(zbSeasonWnd wndParent)
	{
		m_wndParent = wndParent;
		m_seasonMapCtrl = new zbSeasonMapController(m_wndParent);
		m_seasonMiniMap = new zbSeasonMiniMap(wndParent);
		m_areaRoot = ((Component)m_wndParent).transform.Find("GameObjectSceneRoot/cellroot");
		if ((Object)(object)m_areaPrefab == (Object)null)
		{
			m_areaPrefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/season/season_cell.prefab");
		}
	}

	public void OnEnable()
	{
		if (m_seasonMapCtrl != null)
		{
			m_seasonMapCtrl.OnEnable();
		}
		if (m_seasonMiniMap != null)
		{
			m_seasonMiniMap.OnEnable();
		}
	}

	public void InitAreas()
	{
		SeasonAreaInfo[] areaList = zbSelf.Instance.seasonHandler.areaList;
		List<ServerAreaData> list = new List<ServerAreaData>();
		SeasonAreaInfo[] array = areaList;
		foreach (SeasonAreaInfo seasonAreaInfo in array)
		{
			zbSeasonAreaCsvRecord recordByKey = zbDataConfig.Instance.seasonAreaCsvTable.GetRecordByKey(seasonAreaInfo.AreaId);
			if (recordByKey.areaType != 2 || !list.Any((ServerAreaData cell) => cell.isCenter))
			{
				ServerAreaData serverAreaData = new ServerAreaData
				{
					areaId = recordByKey.id,
					logicX = recordByKey.grid[0],
					logicY = recordByKey.grid[1],
					isCenter = (recordByKey.areaType == 2),
					areaType = recordByKey.areaType,
					force = seasonAreaInfo.ForceId,
					resources = new List<ResourceData>()
				};
				for (int num = 0; num < recordByKey.placeResId.Length; num++)
				{
					float[] array2 = recordByKey.validSpawnPos[num];
					serverAreaData.resources.Add(new ResourceData
					{
						buildId = recordByKey.placeResId[num],
						logicX = array2[0],
						logicY = array2[1],
						index = num,
						areaId = seasonAreaInfo.AreaId,
						cityId = recordByKey.cityId
					});
				}
				list.Add(serverAreaData);
			}
		}
		ClearAreas();
		foreach (ServerAreaData item in list)
		{
			GameObject val = Object.Instantiate<GameObject>(m_areaPrefab, m_areaRoot);
			if ((Object)(object)val != (Object)null)
			{
				zbSeasonArea component = val.GetComponent<zbSeasonArea>();
				m_seasonAreas.Add(component);
				component.Init(item);
			}
		}
		m_seasonMiniMap.InitAreas(list);
	}

	public void ClearAreas()
	{
		for (int num = m_seasonAreas.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)m_seasonAreas[num] != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)m_seasonAreas[num]).gameObject);
			}
		}
		m_seasonAreas.Clear();
	}

	public void TrackGameObject(GameObject gameObject, Action OnComplete = null)
	{
		if (OnComplete != null)
		{
			m_seasonMapCtrl.StartTracking(gameObject, OnComplete);
		}
		else if ((Object)(object)gameObject != (Object)null)
		{
			m_seasonMapCtrl.StartTracking(gameObject);
		}
	}

	public void TrackArea(int areaId)
	{
		foreach (zbSeasonArea seasonArea in m_seasonAreas)
		{
			if (seasonArea.areaData != null && seasonArea.areaData.areaId == areaId)
			{
				m_seasonMapCtrl.StartTracking(((Component)seasonArea).gameObject);
			}
		}
	}

	public void TrackCityByAreaId(int areaId, Action OnComplete = null)
	{
		zbSeasonBuild cityByAreaId = zbSelf.Instance.seasonHandler.GetCityByAreaId(areaId);
		if (OnComplete != null)
		{
			m_seasonMapCtrl.StartTracking(((Component)cityByAreaId).gameObject, OnComplete);
		}
		else if ((Object)(object)cityByAreaId != (Object)null)
		{
			m_seasonMapCtrl.StartTracking(((Component)cityByAreaId).gameObject);
		}
	}

	public zbSeasonMiniMap GetSeaSonMiniMap()
	{
		return m_seasonMiniMap;
	}

	public void Update()
	{
		if (m_seasonMapCtrl != null)
		{
			m_seasonMapCtrl.Update();
		}
	}
}
