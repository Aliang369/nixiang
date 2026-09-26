using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonMyTerritoryWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbMyTerritory;

	private ScrollView m_scrollView;

	private Button m_btnUpgrade;

	private VisualElement m_hasTerritoryVs;

	private Label m_lbNoTerritory;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbMyTerritory = UQueryExtensions.Q<Label>(m_RootElement, "lb_myTerritory", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_lbNoTerritory = UQueryExtensions.Q<Label>(m_RootElement, "lb_noTerritory", (string)null);
		m_hasTerritoryVs = UQueryExtensions.Q<VisualElement>(m_RootElement, "hasTerritory", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom()
	{
		RefreshListView();
	}

	private void RefreshListView()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_scrollView).Clear();
		if (zbSelf.Instance.seasonHandler.seasonMyTerritoryAck != null && zbSelf.Instance.seasonHandler.seasonMyTerritoryAck.TerritoryList != null)
		{
			RepeatedField<SeasonMyTerritory> territoryList = zbSelf.Instance.seasonHandler.seasonMyTerritoryAck.TerritoryList;
			bool flag = territoryList.Count > 0;
			m_hasTerritoryVs.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)m_lbNoTerritory).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			for (int i = 0; i < territoryList.Count; i++)
			{
				zbSeasonMyTerritoryItem zbSeasonMyTerritoryItem2 = new zbSeasonMyTerritoryItem(this);
				zbSeasonMyTerritoryItem2.Refresh(territoryList[i]);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbSeasonMyTerritoryItem2);
			}
			int num = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.seasonBaseJsonTable.GetRecord()["territoryMaxCount"][(object)"value"]);
			((TextElement)m_lbMyTerritory).text = string.Format(zbUtil.Language("lang.11479"), territoryList.Count + "/" + num);
		}
	}

	private void OnUpgrade()
	{
	}

	public void OnClose()
	{
		Hide();
	}
}
