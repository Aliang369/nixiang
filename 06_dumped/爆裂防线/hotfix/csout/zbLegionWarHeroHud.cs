using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarHeroHud : VisualElement
{
	private MonoBehaviour m_hero;

	private VisualElement m_rootThis;

	private VisualElement m_nodeCenter;

	private Vector2 m_lastPos;

	private float m_lastOrthoSize;

	private const float m_positionThreshold = 0.001f;

	private Vector3 _lastScale;

	private float _lastLeft;

	private float _lastTop;

	private float m_initOrthographicSize;

	private Label m_lblName;

	private VisualElement m_rootHud;

	private Label m_lblTime;

	private Vector2 m_lastHeroPos;

	private LegionWarPointPlayerDetail legionMemberDetail;

	private VisualElement m_progressOverFlow;

	private Label m_lbAction;

	private float offsetX;

	public zbLegionWarHeroHud(VisualElement root, MonoBehaviour legionHero)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		m_lastPos = Vector2.left * 10f;
		_lastScale = Vector3.zero;
		_lastLeft = -1f;
		_lastTop = -1f;
		m_initOrthographicSize = 7.4f;
		m_lastHeroPos = Vector2.left * 10f;
		((VisualElement)this)._002Ector();
		m_rootHud = root;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/legionWarHeroHud.uxml");
		m_rootThis = (VisualElement)(object)val.Instantiate();
		m_rootHud.Add(m_rootThis);
		m_hero = legionHero;
	}

	public void Init(LegionWarPointPlayerDetail _legionMemberDetail, float _offsetX = 0f)
	{
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		offsetX = _offsetX;
		legionMemberDetail = _legionMemberDetail;
		m_initOrthographicSize = zbWorldScaler.cameraOrthographicSize;
		m_nodeCenter = UQueryExtensions.Q<VisualElement>(m_rootThis, "nodeCenter", (string)null);
		m_lblName = UQueryExtensions.Q<Label>(m_nodeCenter, "lb_name", (string)null);
		m_lblTime = UQueryExtensions.Q<Label>(m_nodeCenter, "lb_time", (string)null);
		m_progressOverFlow = UQueryExtensions.Q<VisualElement>(m_rootThis, "progress_actionover", (string)null);
		m_lbAction = UQueryExtensions.Q<Label>(m_nodeCenter, "lb_action", (string)null);
		if (legionMemberDetail.RoleStateInfo.ActionLimit == 0)
		{
			legionMemberDetail.RoleStateInfo.ActionLimit = 1;
		}
		((TextElement)m_lbAction).text = legionMemberDetail.RoleStateInfo.ActionPoints + "/" + legionMemberDetail.RoleStateInfo.ActionLimit;
		m_progressOverFlow.style.width = StyleLength.op_Implicit((float)legionMemberDetail.RoleStateInfo.ActionPoints / (float)legionMemberDetail.RoleStateInfo.ActionLimit * 124f);
		zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail.RoleBaseInfo.RoleId);
		((TextElement)m_lblName).text = legionMemberDetail.RoleBaseInfo.RoleName;
		zbFontReplacer.Instance.ReplaceFontInUI(m_rootThis);
	}

	public void SetHudVisible(bool visible)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!visible || legionMemberDetail.RoleBaseInfo.RoleId < 100000)
		{
			m_rootThis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_rootThis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
		}
	}

	public void RemoveHud()
	{
		m_rootThis.RemoveFromHierarchy();
	}

	public void Update()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (m_nodeCenter.panel == null || (Object)(object)m_hero == (Object)null || !((Component)m_hero).gameObject.activeSelf)
		{
			zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)1);
			return;
		}
		zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)0);
		Vector2 val = Vector2.op_Implicit(((Component)zbCameraHelper.Instance.mainCamera).transform.position);
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		bool flag = false;
		if (Vector2.Distance(val, m_lastPos) > 0.001f || !Mathf.Approximately(orthographicSize, m_lastOrthoSize) || Vector2.Distance(Vector2.op_Implicit(((Component)m_hero).transform.position), m_lastHeroPos) > 0.001f)
		{
			m_lastPos = val;
			m_lastHeroPos = Vector2.op_Implicit(((Component)m_hero).transform.position);
			m_lastOrthoSize = orthographicSize;
			flag = true;
		}
		if (flag)
		{
			UpdateHudTransform();
		}
		if (legionMemberDetail.RoleStateInfo.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_lblTime).text = zbUtil.FormatTimeMMSS(legionMemberDetail.RoleStateInfo.ProtectTm - zbSelf.Instance.timeHandler.GetServerTime());
			zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lblTime, (DisplayStyle)0);
		}
		else
		{
			zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lblTime, (DisplayStyle)1);
		}
	}

	private void UpdateHudTransform()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		float num = m_initOrthographicSize / orthographicSize;
		Vector3 newScale = Vector3.one * num;
		zbUtil.SetScaleIfChanged(ref _lastScale, newScale, m_nodeCenter);
		float num2 = 190f * num;
		Vector2 val = WorldToToolKit(((Component)m_hero).transform.position);
		Rect worldBound = m_rootHud.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		Vector2 val2 = val - position;
		float newValue = val2.x - offsetX * 100f;
		float num3 = val2.y - num2 / 2f - 30f;
		if (legionMemberDetail.RoleBaseInfo.RoleId < 100000)
		{
			num3 += 20f;
		}
		zbUtil.SetStyleIfChanged(ref _lastLeft, newValue, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_nodeCenter.style.left = StyleLength.op_Implicit(num4);
		});
		zbUtil.SetStyleIfChanged(ref _lastTop, num3, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_nodeCenter.style.top = StyleLength.op_Implicit(num4);
		});
	}

	public Vector2 WorldToToolKit(Vector3 worldPos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return RuntimePanelUtils.CameraTransformWorldToPanel(m_nodeCenter.panel, worldPos, zbCameraHelper.Instance.mainCamera);
	}
}
