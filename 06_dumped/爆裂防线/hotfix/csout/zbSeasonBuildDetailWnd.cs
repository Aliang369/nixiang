using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonBuildDetailWnd : zbBaseWnd
{
	private VisualElement m_bg;

	private Button m_btnTouch;

	private zbSeasonBuild m_build;

	private Label m_lbTitle;

	private Label m_lbLv;

	private VisualElement m_iconFlag;

	private VisualElement m_firstCapReward;

	private VisualElement m_firstCapExp;

	private VisualElement[] m_resources = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_resourcIcons = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbOutPuts = (Label[])(object)new Label[4];

	private zbItem[] zbItems = new zbItem[5];

	private Label m_lbExp;

	private VisualElement m_forceInfo;

	private Button m_btnMarch;

	private Button m_btnDraft;

	private Button m_btnDeclare;

	private Button m_btnGarrison;

	private Button m_btnRally;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_iconFlag = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_flag", (string)null);
		m_forceInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "forceInfo", (string)null);
		m_firstCapReward = UQueryExtensions.Q<VisualElement>(m_RootElement, "firstCapReward", (string)null);
		m_firstCapExp = UQueryExtensions.Q<VisualElement>(m_RootElement, "firstCapExp", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_resources[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "resource" + i, (string)null);
			m_resourcIcons[i] = UQueryExtensions.Q<VisualElement>(m_resources[i], "icon", (string)null);
			m_lbOutPuts[i] = UQueryExtensions.Q<Label>(m_resources[i], "lb_outPut", (string)null);
		}
		for (int j = 0; j < 5; j++)
		{
			zbItems[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + j, (string)null);
			zbItems[j].OnEnable(this);
		}
		m_lbExp = UQueryExtensions.Q<Label>(m_RootElement, "lb_exp", (string)null);
		m_btnMarch = UQueryExtensions.Q<Button>(m_RootElement, "btn_march", (string)null);
		m_btnDraft = UQueryExtensions.Q<Button>(m_RootElement, "btn_draft", (string)null);
		m_btnDeclare = UQueryExtensions.Q<Button>(m_RootElement, "btn_declare", (string)null);
		m_btnGarrison = UQueryExtensions.Q<Button>(m_RootElement, "btn_garrison", (string)null);
		m_btnRally = UQueryExtensions.Q<Button>(m_RootElement, "btn_rally", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMarch, (Action)OnMarch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDraft, (Action)OnDraft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDeclare, (Action)OnDeclare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGarrison, (Action)OnGarrison, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRally, (Action)OnRally, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	protected override void Start()
	{
		base.Start();
	}

	public void RefreshCustom(zbSeasonBuild build)
	{
		if (!((Object)(object)build == (Object)null) && m_bg != null)
		{
			m_build = build;
			RefreshPosition();
			RefreshContent();
			RefreshBtns();
		}
	}

	private void RefreshPosition()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Camera val = ((zbCameraHelper.Instance != null) ? zbCameraHelper.Instance.mainCamera : Camera.main);
		if (!((Object)(object)val == (Object)null))
		{
			Vector3 position = ((Component)m_build).transform.position;
			Vector3 val2 = val.WorldToScreenPoint(position);
			if (val2.z < 0f)
			{
				m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				return;
			}
			m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			val2.y = (float)Screen.height - val2.y;
			Vector2 val3 = RuntimePanelUtils.ScreenToPanel(m_RootElement.panel, new Vector2(val2.x, val2.y));
			m_bg.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			m_bg.style.left = StyleLength.op_Implicit(val3.x);
			m_bg.style.top = StyleLength.op_Implicit(val3.y);
			m_bg.style.translate = new StyleTranslate(new Translate(new Length(-50f, (LengthUnit)1), new Length(-50f, (LengthUnit)1)));
		}
	}

	private void RefreshContent()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		ESeasonForce forceByAreaId = zbSelf.Instance.seasonHandler.GetForceByAreaId(m_build.m_areaId);
		m_iconFlag.style.backgroundImage = zbSelf.Instance.seasonHandler.GetForceBigFlagByForceId(forceByAreaId);
		bool flag = m_build.m_buildType == ESeasonBuildType.Force;
		m_forceInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_firstCapReward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		m_firstCapExp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		zbSeasonCityCsvRecord recordByKey = zbDataConfig.Instance.seasonCityCsvTable.GetRecordByKey(m_build.m_buildId);
		if (!flag)
		{
			zbSeasonCityLevelCsvRecord recordByKey2 = zbDataConfig.Instance.seasonCityLevelCsvTable.GetRecordByKey(recordByKey.level);
			((TextElement)m_lbExp).text = string.Format(zbUtil.Language("lang.11469"), recordByKey2.occupyExp);
		}
		((TextElement)m_lbTitle).text = recordByKey.name;
		((TextElement)m_lbLv).text = "Lv." + recordByKey.level;
		for (int i = 0; i < 4; i++)
		{
			bool flag2 = i < recordByKey.produce.Length;
			m_resources[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				m_resourcIcons[i].style.backgroundImage = zbSelf.Instance.seasonHandler.GetResourceIconByType((ESeasonResType)recordByKey.produce[i][0]);
				((TextElement)m_lbOutPuts[i]).text = recordByKey.produce[i][1] + "/H";
			}
		}
		for (int j = 0; j < 5; j++)
		{
			bool flag3 = j < recordByKey.firstCapReward.Length;
			((VisualElement)zbItems[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			if (flag3)
			{
				zbItemData data = new zbItemData((EResourceType)recordByKey.firstCapReward[j][0], recordByKey.firstCapReward[j][1], recordByKey.firstCapReward[j][2]);
				zbItems[j].Refresh(data);
			}
		}
	}

	private void RefreshBtns()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.seasonHandler.areaList.FirstOrDefault((SeasonAreaInfo area) => area.AreaId == m_build.m_areaId).ForceId == zbSelf.Instance.seasonHandler.GetMySeasonForce();
		((VisualElement)m_btnDraft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void OnClose()
	{
		Hide();
		(zbUIMgr.Instance.GetWndById(EWndId.Season) as zbSeasonWnd).SetUiElementVisible(vis: true);
	}

	private void OnMarch()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		zbWsMsgHandler.Instance.Season_March_Start_Req(1, 0, ((Component)m_build).transform.position.x, ((Component)m_build).transform.position.y);
		OnClose();
	}

	private void OnDraft()
	{
	}

	private void OnDeclare()
	{
	}

	private void OnGarrison()
	{
		zbWsMsgHandler.Instance.Season_Guard_Info_Req(m_build.m_areaId);
	}

	private void OnRally()
	{
	}
}
