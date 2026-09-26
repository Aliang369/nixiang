using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonSearchResource
{
	private zbSeasonSearchWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button[] m_btnsResource = (Button[])(object)new Button[2];

	private VisualElement[] m_iconsChoose = (VisualElement[])(object)new VisualElement[2];

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnSearch;

	private Label m_lbSearchLv;

	private VisualElement[] m_resources = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_resourcesIcon = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbsResourceOutput = (Label[])(object)new Label[3];

	private int m_index;

	private int m_searchLv = 1;

	private const int MAX_SEARCH_LV = 15;

	private zbSeasonResourceCsvRecord[] m_records;

	public void OnEnable(zbSeasonSearchWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		for (int i = 0; i < 2; i++)
		{
			m_btnsResource[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_resource" + i, (string)null);
			int tempIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsResource[i], (Action<ClickEvent>)delegate
			{
				OnSelectIndex(tempIndex);
			}, "", (TrickleDown)0);
			m_iconsChoose[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnsResource[i], "icon_choose", (string)null);
		}
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_lbSearchLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnSearch = UQueryExtensions.Q<Button>(m_RootElement, "btn_search", (string)null);
		for (int num = 0; num < 3; num++)
		{
			m_resources[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "resource" + num, (string)null);
			m_resourcesIcon[num] = UQueryExtensions.Q<VisualElement>(m_resources[num], "icon", (string)null);
			m_lbsResourceOutput[num] = UQueryExtensions.Q<Label>(m_resources[num], "lb_outPut", (string)null);
		}
		m_records = zbDataConfig.Instance.seasonResourceCsvTable.GetRecords().records.Values.ToArray();
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSearch, (Action)OnSearch, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		RefreshSelect();
		RefreshSearchLv();
		RefreshOutPutResource();
	}

	private void RefreshSearchLv()
	{
		((TextElement)m_lbSearchLv).text = m_searchLv.ToString();
	}

	private void RefreshOutPutResource()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		zbSeasonResourceCsvRecord zbSeasonResourceCsvRecord2 = m_records.FirstOrDefault((zbSeasonResourceCsvRecord r) => r.nodeType == m_index + 1 && r.level == m_searchLv);
		if (zbSeasonResourceCsvRecord2 == null)
		{
			return;
		}
		for (int num = 0; num < 3; num++)
		{
			bool flag = num < zbSeasonResourceCsvRecord2.produce.Length;
			m_resources[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				m_resourcesIcon[num].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/season/build/resource_00{zbSeasonResourceCsvRecord2.produce[num][0]}.png"));
				((TextElement)m_lbsResourceOutput[num]).text = "+" + zbSeasonResourceCsvRecord2.produce[num][1] + "/H";
			}
		}
	}

	private void OnSelectIndex(int index)
	{
		m_index = index;
		m_searchLv = 1;
		Refresh();
	}

	private void RefreshSelect()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 2; i++)
		{
			bool flag = i == m_index;
			m_iconsChoose[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	private void OnSubtract()
	{
		if (m_searchLv > 1)
		{
			m_searchLv--;
			Refresh();
		}
	}

	private void OnAdd()
	{
		if (m_searchLv < 15)
		{
			m_searchLv++;
			Refresh();
		}
	}

	private void OnSearch()
	{
	}
}
