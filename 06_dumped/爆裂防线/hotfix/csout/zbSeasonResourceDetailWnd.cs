using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonResourceDetailWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbOutputEfficiency;

	private Label m_lbCity;

	private VisualElement m_iconResource;

	private VisualElement m_iconFlag;

	private ResourceData m_resourceData;

	private zbSeasonResBuild m_build;

	private Button m_btnAttack;

	private Button m_btnGiveup;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbOutputEfficiency = UQueryExtensions.Q<Label>(m_RootElement, "lb_outputEfficiency", (string)null);
		m_lbCity = UQueryExtensions.Q<Label>(m_RootElement, "lb_city", (string)null);
		m_iconResource = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_resource", (string)null);
		m_iconFlag = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_flag", (string)null);
		m_btnAttack = UQueryExtensions.Q<Button>(m_RootElement, "btn_attack", (string)null);
		m_btnGiveup = UQueryExtensions.Q<Button>(m_RootElement, "btn_giveup", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttack, (Action)OnAttack, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(ResourceData resourceData, zbSeasonResBuild res)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		zbSeasonResourceCsvRecord recordByKey = zbDataConfig.Instance.seasonResourceCsvTable.GetRecordByKey(resourceData.buildId);
		m_build = res;
		m_resourceData = resourceData;
		((TextElement)m_lbTitle).text = recordByKey.name;
		((TextElement)m_lbOutputEfficiency).text = recordByKey.produce[0][1] + "/H";
		zbSeasonAreaCsvRecord recordByKey2 = zbDataConfig.Instance.seasonAreaCsvTable.GetRecordByKey(resourceData.areaId);
		zbSeasonCityCsvRecord recordByKey3 = zbDataConfig.Instance.seasonCityCsvTable.GetRecordByKey(recordByKey2.cityId);
		((TextElement)m_lbCity).text = recordByKey3.name;
		SeasonAreaInfo areaInfoById = zbSelf.Instance.seasonHandler.GetAreaInfoById(resourceData.areaId);
		m_iconFlag.style.backgroundImage = zbSelf.Instance.seasonHandler.GetForceBigFlagByForceId(areaInfoById.ForceId);
		m_iconResource.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/resource_" + recordByKey.nodeType + ".png"));
	}

	private void OnAttack()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		SeasonTeamPosInfo seasonTeamInfoByIndex = zbSelf.Instance.seasonHandler.GetSeasonTeamInfoByIndex(0);
		zbSeasonBuild cityByAreaId = zbSelf.Instance.seasonHandler.GetCityByAreaId(seasonTeamInfoByIndex.CityId);
		zbDataConfig.Instance.seasonAreaCsvTable.GetRecordByKey(cityByAreaId.m_areaId);
		zbWsMsgHandler.Instance.Season_March_Start_Req(seasonTeamInfoByIndex.TeamId, 3, ((Component)m_build).transform.position.x, ((Component)m_build).transform.position.y, m_resourceData.index);
		OnClose();
	}

	public void OnClose()
	{
		Hide();
	}
}
