using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbLegionWarPoint : MonoBehaviour
{
	private SkeletonAnimation m_spineAnimation;

	protected ESortMainLayerType m_sortType = ESortMainLayerType.Elevator;

	[NonSerialized]
	public zbLegionWarHud legionWarHud;

	public LegionWarPointStateDetail legionWarPointStateDetail;

	public int pointId;

	public zbLegionWarMapCsvRecord legionWarMapRecord;

	private void Awake()
	{
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void Init(int _pointId)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		pointId = _pointId;
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y, ((Component)this).transform.position.y / 10f);
		legionWarMapRecord = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(pointId);
		Transform val = ((Component)this).transform.Find("visibleObject");
		m_spineAnimation = ((Component)val.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		zbUtil.RefreshSpine(m_spineAnimation, legionWarMapRecord.spineName + "_SkeletonData.asset", "red", loop: true);
		InitHud();
	}

	public void InitHud()
	{
		zbLegionWarWnd componentInParent = ((Component)this).GetComponentInParent<zbLegionWarWnd>();
		componentInParent.GetRootElement();
		if (legionWarHud == null)
		{
			legionWarHud = new zbLegionWarHud(componentInParent.hudRoot, this);
		}
		legionWarHud.Init();
	}

	public void Refresh(LegionWarPointStateDetail _legionWarPointStateDetail)
	{
		legionWarPointStateDetail = _legionWarPointStateDetail;
		if (legionWarPointStateDetail.OccupyForceId == ELegionWarForce.Red)
		{
			m_spineAnimation.state.SetAnimation(0, "red", true);
		}
		else if (legionWarPointStateDetail.OccupyForceId == ELegionWarForce.Blue)
		{
			m_spineAnimation.state.SetAnimation(0, "blue", true);
		}
		else
		{
			m_spineAnimation.state.SetAnimation(0, "gray", true);
		}
		legionWarHud.Refresh(((IEnumerable<LegionWarPointForceDetail>)_legionWarPointStateDetail.PointForceList).ToArray());
	}

	public void Pick()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (legionWarPointStateDetail.PointId == 1001 || legionWarPointStateDetail.PointId == 2001)
		{
			zbUIMgr.Instance.ShowLegionWarMainTip(legionWarPointStateDetail.PointId, Vector2.op_Implicit(((Component)this).transform.position));
		}
		else
		{
			zbUIMgr.Instance.ShowLegionWarPointTip(legionWarPointStateDetail, Vector2.op_Implicit(((Component)this).transform.position));
		}
	}

	public (float x, float y) GetPositionXY()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return (x: ((Component)this).transform.position.x, y: ((Component)this).transform.position.y - 1f);
	}

	public float GetPositionX()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position.x;
	}

	public float GetPositionY()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position.y - 1f;
	}

	public void BlinkTimes()
	{
	}

	private void Update()
	{
		if (legionWarHud != null)
		{
			legionWarHud.Update();
		}
	}
}
