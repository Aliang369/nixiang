using System;
using Proto.Types;
using UnityEngine;

public class zbLegionWarMapHero : MonoBehaviour
{
	private float m_spineAvatarScale = 0.35f;

	public bool moveFinish = true;

	private float duration = 12f;

	private Vector2 startPos;

	private Vector2 endPos;

	private float elapsed;

	private zbPlayerAvatar m_playerAvatar { get; set; }

	public LegionWarMoveDetail legionWarMoveDetail { get; private set; }

	public void Init(LegionWarMoveDetail _legionWarMoveDetail)
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		((Component)((Component)this).transform).gameObject.SetActive(true);
		legionWarMoveDetail = _legionWarMoveDetail;
		m_playerAvatar = ((Component)((Component)this).transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			m_playerAvatar.RefreshCustom(legionWarMoveDetail.RoleBaseInfo.AvatarId, legionWarMoveDetail.RoleBaseInfo.GunId);
			m_playerAvatar.SetAnimation(0, "run", loop: true);
		}
		duration = Math.Max(5L, legionWarMoveDetail.ArrivalTm - zbSelf.Instance.timeHandler.GetServerTime());
		zbLegionWarPoint point = zbLegionWarMgr.Instance.GetPoint(legionWarMoveDetail.FromPointId);
		zbLegionWarPoint point2 = zbLegionWarMgr.Instance.GetPoint(legionWarMoveDetail.ToPointId);
		startPos = new Vector2(point.GetPositionXY().x, point.GetPositionXY().y);
		endPos = new Vector2(point2.GetPositionXY().x, point2.GetPositionXY().y);
		if (((Component)point2).transform.position.x - ((Component)point).transform.position.x > 0f)
		{
			((Component)this).transform.localScale = new Vector3(0f - m_spineAvatarScale, m_spineAvatarScale, m_spineAvatarScale);
		}
		else
		{
			((Component)this).transform.localScale = new Vector3(m_spineAvatarScale, m_spineAvatarScale, m_spineAvatarScale);
		}
		elapsed = 0f;
		moveFinish = false;
	}

	private void Update()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!moveFinish)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			Vector2 val = Vector2.Lerp(startPos, endPos, num);
			((Component)this).transform.position = new Vector3(val.x, val.y, val.y / 10f - 0.1f);
			if (num >= 1f)
			{
				moveFinish = true;
				((Component)((Component)this).transform).gameObject.SetActive(false);
			}
		}
	}
}
