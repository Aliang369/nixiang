using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;

public class zbSeasonTopPanel
{
	private zbSeasonWnd m_wndParent;

	private VisualElement m_root;

	private Label m_lbMerit;

	private Label m_lbResource0;

	private Label m_lbResource1;

	private Button m_btnTerritory;

	private Label m_lbTerritory;

	public void OnEnable(zbSeasonWnd wnd, VisualElement root)
	{
		m_wndParent = wnd;
		m_root = root;
		m_btnTerritory = UQueryExtensions.Q<Button>(m_root, "btn_territory", (string)null);
		m_lbMerit = UQueryExtensions.Q<Label>(m_root, "lb_merit", (string)null);
		m_lbResource0 = UQueryExtensions.Q<Label>(m_root, "lb_resource0", (string)null);
		m_lbResource1 = UQueryExtensions.Q<Label>(m_root, "lb_resource1", (string)null);
		m_lbTerritory = UQueryExtensions.Q<Label>(m_root, "lb_territory", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTerritory, (Action)OnTerritory, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		((TextElement)m_lbMerit).text = zbUtil.ConvertToTBMK(zbSelf.Instance.seasonHandler.seasonEnterAck.Merit);
		((TextElement)m_lbResource0).text = zbUtil.ConvertToTBMK(zbSelf.Instance.seasonHandler.seasonEnterAck.Gas);
		((TextElement)m_lbResource1).text = zbUtil.ConvertToTBMK(zbSelf.Instance.seasonHandler.seasonEnterAck.TechPoint);
	}

	public void RefreshTerritory()
	{
		int num = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.seasonBaseJsonTable.GetRecord()["territoryMaxCount"][(object)"value"]);
		((TextElement)m_lbTerritory).text = zbSelf.Instance.seasonHandler.seasonMyTerritoryAck.TerritoryList.Count + "/" + num;
	}

	private void OnTerritory()
	{
		zbUIMgr.Instance.ShowSeasonMyTerritory();
	}
}
