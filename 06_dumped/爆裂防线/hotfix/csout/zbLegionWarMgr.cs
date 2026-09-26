using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbLegionWarMgr : MonoBehaviour
{
	public Transform heroTF;

	public static zbLegionWarMgr Instance;

	private zbLegionWarWnd legionWarWnd;

	private float m_refreshTimer;

	private static readonly float m_refreshInterval = 1f;

	public Dictionary<int, zbLegionWarPoint> legionWarPointList { get; set; } = new Dictionary<int, zbLegionWarPoint>();

	public List<zbLegionWarMapHero> heroList { get; private set; }

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
		}
		legionWarPointList = new Dictionary<int, zbLegionWarPoint>();
		heroList = new List<zbLegionWarMapHero>();
	}

	public void Init(zbLegionWarWnd _legionWarWnd)
	{
		legionWarWnd = _legionWarWnd;
		if (legionWarPointList.Count > 0)
		{
			return;
		}
		foreach (KeyValuePair<int, zbLegionWarMapCsvRecord> record in zbDataConfig.Instance.legionWarMapCsvTable.GetRecords().records)
		{
			zbLegionWarPoint component = ((Component)heroTF.Find("legionWarPoint" + record.Value.pointId)).GetComponent<zbLegionWarPoint>();
			if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId == ELegionWarForce.Red)
			{
				legionWarPointList[record.Value.pointId] = component;
				component.Init(record.Value.pointId);
			}
			else
			{
				legionWarPointList[record.Value.mirrorId] = component;
				component.Init(record.Value.mirrorId);
			}
		}
	}

	public void Free()
	{
	}

	public void Refresh()
	{
		foreach (LegionWarPointStateDetail pointState in zbSelf.Instance.legionWarHandler.legionWarMapInfo.PointStateList)
		{
			legionWarPointList[pointState.PointId].Refresh(pointState);
		}
	}

	public zbLegionWarPoint GetPoint(int pointId)
	{
		return legionWarPointList[pointId];
	}

	public void Update()
	{
		m_refreshTimer += Time.unscaledDeltaTime;
		if (m_refreshTimer >= m_refreshInterval)
		{
			m_refreshTimer = 0f;
			UpdatePerSecond();
		}
		foreach (KeyValuePair<int, zbLegionWarPoint> legionWarPoint in legionWarPointList)
		{
			legionWarPoint.Value.legionWarHud.Update();
		}
	}

	public virtual void UpdatePerSecond()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ArrivalTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUtil.SetActive(((Component)legionWarWnd.fangxiangAnimation).gameObject, active: false);
			return;
		}
		if ((double)zbUtil.GetVector2Distance(((Component)legionWarWnd.fangxiangAnimation).transform.position, ((Component)legionWarWnd.m_spineAvatar).transform.position) > 0.1)
		{
			((Component)legionWarWnd.fangxiangAnimation).transform.position = new Vector3(((Component)legionWarWnd.m_spineAvatar).transform.position.x, ((Component)legionWarWnd.m_spineAvatar).transform.position.y, ((Component)legionWarWnd.m_spineAvatar).transform.position.z + 0.002f);
		}
		for (int i = 0; i < 3; i++)
		{
			zbUtil.SetActive(((Component)legionWarWnd.fangxiangArrow[i]).gameObject, active: false);
		}
		zbLegionWarPoint point = GetPoint(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId);
		zbLegionWarMapCsvRecord recordByKey = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId);
		if (recordByKey != null)
		{
			int num = 0;
			int[] connection = recordByKey.connection;
			foreach (int num2 in connection)
			{
				if (num > 2)
				{
					break;
				}
				if (num2 == 1001 || num2 == 2001)
				{
					continue;
				}
				zbLegionWarPoint point2 = GetPoint(num2);
				if (!((Object)(object)point2 != (Object)null))
				{
					continue;
				}
				zbUtil.SetActive(((Component)legionWarWnd.fangxiangArrow[num]).gameObject, active: true);
				Vector3 val = ((Component)legionWarWnd.fangxiangArrow[num]).transform.InverseTransformPoint(((Component)point2).transform.position);
				SkeletonExtensions.SetLocalPosition(legionWarWnd.fangxiangBone[num], val);
				if (((Object)(object)point != (Object)null && point.legionWarPointStateDetail.OccupyForceId == zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId) || point2.legionWarPointStateDetail.OccupyForceId == zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId)
				{
					TrackEntry current = legionWarWnd.fangxiangArrow[num].AnimationState.GetCurrent(0);
					if (current != null && current.Animation.Name != "animation")
					{
						legionWarWnd.fangxiangArrow[num].AnimationState.SetAnimation(0, "animation", true);
					}
				}
				else
				{
					TrackEntry current2 = legionWarWnd.fangxiangArrow[num].AnimationState.GetCurrent(0);
					if (current2 != null && current2.Animation.Name != "animation2")
					{
						legionWarWnd.fangxiangArrow[num].AnimationState.SetAnimation(0, "animation2", true);
					}
				}
				num++;
			}
		}
		zbUtil.SetActive(((Component)legionWarWnd.fangxiangAnimation).gameObject, active: true);
	}

	public bool IsNearPoint(int PointId, int myPoint)
	{
		return zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(PointId).connection.Contains(myPoint);
	}

	public void HeroMoveHandler()
	{
		foreach (LegionWarMoveDetail heroMove in zbSelf.Instance.legionWarHandler.HeroMoveList)
		{
			if (heroMove.RoleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
			{
				zbLegionWarMapHero zbLegionWarMapHero2 = FindHero();
				if ((Object)(object)zbLegionWarMapHero2 == (Object)null)
				{
					break;
				}
				zbLegionWarMapHero2.Init(heroMove);
			}
		}
	}

	public zbLegionWarMapHero FindHero()
	{
		bool flag = false;
		foreach (zbLegionWarMapHero hero in heroList)
		{
			if (hero.moveFinish)
			{
				return hero;
			}
		}
		if (!flag && heroList.Count < 10)
		{
			return CreateHero();
		}
		return null;
	}

	private zbLegionWarMapHero CreateHero()
	{
		GameObject obj = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/wnd/legionWar/legionWarMapHero.prefab"));
		obj.transform.SetParent(heroTF);
		zbLegionWarMapHero component = ((Component)obj.transform).GetComponent<zbLegionWarMapHero>();
		heroList.Add(component);
		return component;
	}
}
