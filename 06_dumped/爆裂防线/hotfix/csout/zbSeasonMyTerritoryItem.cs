using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonMyTerritoryItem : zbViewBaseItem
{
	private zbSeasonMyTerritoryWnd m_wndParent;

	private VisualElement m_res;

	private Label m_lbResName;

	private Label m_lbResNoProfit;

	private VisualElement m_city;

	private Label m_lbCityName;

	private Label m_lbCityNoProfit;

	private Button m_btnTo;

	private Button m_btnGiveUp;

	private SeasonMyTerritory m_territoryInfo;

	public zbSeasonMyTerritoryItem(zbSeasonMyTerritoryWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonMyTerritory.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_wndParent = wndParent;
		m_res = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "res", (string)null);
		m_lbResName = UQueryExtensions.Q<Label>(m_res, "lb_name", (string)null);
		m_lbResNoProfit = UQueryExtensions.Q<Label>(m_res, "lb_noProfit", (string)null);
		m_city = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "city", (string)null);
		m_lbCityName = UQueryExtensions.Q<Label>(m_city, "lb_name", (string)null);
		m_lbCityNoProfit = UQueryExtensions.Q<Label>(m_city, "lb_noProfit", (string)null);
		m_btnTo = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_to", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTo, (Action)OnTo, "", (TrickleDown)0);
		m_btnGiveUp = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_giveUp", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGiveUp, (Action)OnGiveUp, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(SeasonMyTerritory territoryInfo)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (territoryInfo != null)
		{
			m_territoryInfo = territoryInfo;
			zbSeasonResourceCsvRecord recordByKey = zbDataConfig.Instance.seasonResourceCsvTable.GetRecordByKey(m_territoryInfo.ResNodeId);
			zbSeasonCityCsvRecord recordByKey2 = zbDataConfig.Instance.seasonCityCsvTable.GetRecordByKey(m_territoryInfo.CityId);
			((TextElement)m_lbResName).text = zbUtil.Language(recordByKey.name);
			((TextElement)m_lbCityName).text = zbUtil.Language(recordByKey2.name);
			bool flag = m_territoryInfo.Active == 1;
			((VisualElement)m_lbCityNoProfit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			((VisualElement)m_lbResNoProfit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		}
	}

	private void OnTo()
	{
	}

	private void OnGiveUp()
	{
		zbWsMsgHandler.Instance.Season_Res_Abandon_Req(m_territoryInfo.CityId, m_territoryInfo.ResNodeId);
	}
}
