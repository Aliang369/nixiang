using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionHeroHud
{
	private zbLegionHero m_hero;

	private VisualElement m_rootThis;

	private VisualElement m_nodeCenter;

	private VisualElement m_leaderIcon;

	private VisualElement m_adviserIcon;

	private Label m_lblName;

	private VisualElement m_rootHud;

	public zbLegionHeroHud(VisualElement root, zbLegionHero legionHero)
	{
		m_rootHud = root;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legion/legionheroHud.uxml");
		m_rootThis = (VisualElement)(object)val.Instantiate();
		m_rootHud.Add(m_rootThis);
		m_hero = legionHero;
	}

	public void Init(LegionMemberDetail legionMemberDetail)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		m_nodeCenter = UQueryExtensions.Q<VisualElement>(m_rootThis, "nodeCenter", (string)null);
		m_lblName = UQueryExtensions.Q<Label>(m_nodeCenter, "lblname", (string)null);
		m_leaderIcon = UQueryExtensions.Q<VisualElement>(m_nodeCenter, "leader", (string)null);
		m_adviserIcon = UQueryExtensions.Q<VisualElement>(m_nodeCenter, "adviser", (string)null);
		ELegionOfficial officialById = zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail.RoleBaseInfo.RoleId);
		m_leaderIcon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(officialById != ELegionOfficial.Leader));
		m_adviserIcon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(officialById != ELegionOfficial.Adviser));
		((TextElement)m_lblName).text = legionMemberDetail.RoleBaseInfo.RoleName;
	}

	public void SetHudVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_rootThis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}

	public void RemoveHud()
	{
		m_rootThis.RemoveFromHierarchy();
	}

	public void UpdatePos()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_hero == (Object)null) && (int)m_rootThis.resolvedStyle.display != 1)
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(((Component)m_hero).transform.position.x, ((Component)m_hero).transform.position.y + 1.3f, ((Component)m_hero).transform.position.z);
			Vector2 val2 = RuntimePanelUtils.CameraTransformWorldToPanel(m_nodeCenter.panel, val, zbCameraHelper.Instance.mainCamera);
			Rect worldBound = m_rootHud.worldBound;
			Vector2 position = ((Rect)(ref worldBound)).position;
			Vector2 val3 = val2 - position;
			m_nodeCenter.style.left = StyleLength.op_Implicit(val3.x);
			m_nodeCenter.style.top = StyleLength.op_Implicit(val3.y);
		}
	}
}
