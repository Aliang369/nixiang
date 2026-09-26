using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonLeftPanel
{
	private zbSeasonWnd m_wndParent;

	private VisualElement m_root;

	private Button m_btnCityWar;

	private Label m_lbCityWarLeftTm;

	private Button[] m_btnCuts = (Button[])(object)new Button[6];

	private ESeasonForce m_myForceId;

	public void OnEnable(zbSeasonWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_root = root;
		m_btnCityWar = UQueryExtensions.Q<Button>(m_root, "btn_cityWar", (string)null);
		m_lbCityWarLeftTm = UQueryExtensions.Q<Label>(m_root, "lb_cityWarLeftTm", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCityWar, (Action)OnCityWar, "", (TrickleDown)0);
		for (int i = 0; i < 6; i++)
		{
			m_btnCuts[i] = UQueryExtensions.Q<Button>(m_root, "btn_cut" + i, (string)null);
			int tempIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCuts[i], (Action)delegate
			{
				OnCut(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_CityWar_Ack, OnEventUpdateCityWar);
	}

	public void RefreshCityWar()
	{
	}

	public void RefreshDeclareList()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		m_myForceId = zbSelf.Instance.seasonHandler.GetMySeasonForce();
		SeasonDeclareInfo[] array = zbSelf.Instance.seasonHandler.seasonDeclareList.Where((SeasonDeclareInfo declarInfo) => declarInfo.AtkForce == m_myForceId).ToArray();
		if (array == null || array.Length == 0)
		{
			return;
		}
		for (int num = 0; num < 6; num++)
		{
			bool flag = num < array.Length;
			((VisualElement)m_btnCuts[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbSeasonAreaCsvRecord recordByKey = zbDataConfig.Instance.seasonAreaCsvTable.GetRecordByKey(array[num].CityId);
				zbSeasonCityCsvRecord recordByKey2 = zbDataConfig.Instance.seasonCityCsvTable.GetRecordByKey(recordByKey.cityId);
				((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnCuts[num], (string)null, (string)null)).text = string.Format(zbUtil.Language("lang.11480"), recordByKey2.name);
			}
		}
	}

	private void OnCut(int index)
	{
		int cityId = zbSelf.Instance.seasonHandler.seasonDeclareList.Where((SeasonDeclareInfo declarInfo) => declarInfo.AtkForce == m_myForceId).ToArray()[index].CityId;
		zbSeasonBuild cityInstance = zbSelf.Instance.seasonHandler.GetCityByAreaId(cityId);
		zbSeasonMgr.Instance.SeasonMap.TrackCityByAreaId(cityId, delegate
		{
			zbUIMgr.Instance.ShowSeasonBuildDetail(cityInstance);
		});
	}

	private void OnCityWar()
	{
		zbWsMsgHandler.Instance.Season_CityWar_Info_Req();
	}

	private void OnEventUpdateCityWar(object data)
	{
		zbUIMgr.Instance.ShowSeasonCityWar();
	}
}
