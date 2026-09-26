using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonHudBase
{
	protected VisualElement m_root;

	protected VisualElement m_iconFlag;

	protected Label m_lbLv;

	protected Label m_lbTitle;

	public bool active;

	protected Transform m_orderTF;

	protected Camera m_camera;

	protected float offsetY = (((float)Screen.height >= 1680f) ? 20f : (-50f));

	protected bool needUpdatePos;

	private Vector3 m_lastPos;

	protected SeasonAreaInfo m_seasonArea;

	protected int m_buildId;

	public zbSeasonHudBase(Transform orderTF)
	{
		m_orderTF = orderTF;
		m_seasonArea = zbSelf.Instance.seasonHandler.GetAreaInfoById(((Component)m_orderTF.parent.parent).GetComponent<zbSeasonArea>().areaId);
		m_camera = zbCameraHelper.Instance.mainCamera;
	}

	public VisualElement GetRoot()
	{
		return m_root;
	}

	public virtual void Init()
	{
	}

	public virtual void Update()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (needUpdatePos && active)
		{
			Vector3 position = m_orderTF.position;
			if (!(position == m_lastPos))
			{
				m_lastPos = position;
				UpdateHudPos();
			}
		}
	}

	public virtual void UpdateHudPos()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (m_root != null && !((Object)(object)m_orderTF == (Object)null))
		{
			Vector3 position = m_orderTF.position;
			Vector2 val = new Vector2(position.x * zbSeasonHudMgr.Instance.WorldToPanelScale.x, position.y * zbSeasonHudMgr.Instance.WorldToPanelScale.y) + zbSeasonHudMgr.Instance.WorldToPanelOffset;
			m_root.style.left = StyleLength.op_Implicit(val.x);
			m_root.style.top = StyleLength.op_Implicit(val.y - offsetY);
		}
	}

	public virtual void Show()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		active = true;
		UpdateHudPos();
	}

	public virtual void Hide()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		active = false;
	}

	public virtual void Destroy()
	{
		if (m_root != null)
		{
			m_root.RemoveFromHierarchy();
		}
	}
}
