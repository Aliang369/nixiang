using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBagTab
{
	private zbBagWnd m_wndParent;

	private VisualElement m_RootElement;

	private int m_tabIndex;

	public Label m_lbName;

	public VisualElement m_listNo;

	private MultiColumnListView m_listView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private bool columnsInited;

	public void OnEnable(zbBagWnd wndParent, VisualElement root, int tabIndex)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_tabIndex = tabIndex;
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		InitColumns();
	}

	private void InitColumns()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (columnsInited)
		{
			return;
		}
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(128f),
				makeCell = () => (VisualElement)(object)new zbBagItem(m_wndParent),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbBagItem)?.Refresh(m_listItemDatas[num]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		columnsInited = true;
	}

	public void Refresh()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		GenerateData();
		int num = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		rowIndexes.Clear();
		for (int i = 0; i < num; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listItemDatas.Count > 0));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void GenerateData()
	{
		m_listItemDatas.Clear();
		switch (m_tabIndex)
		{
		case 0:
			((TextElement)m_lbName).text = zbUtil.Language("lang.10028");
			foreach (ResourceDetail item in zbServerData.sGameData.ItemList)
			{
				m_listItemDatas.Add(new zbItemData(EResourceType.Item, item.Tid, item.Count));
			}
			m_listItemDatas = (from data in m_listItemDatas
				orderby zbDataConfig.Instance.itemCsvTable.GetRecordByKey((int)data.id)?.type ?? 0 descending, data.id descending
				select data).ToList();
			break;
		case 1:
			((TextElement)m_lbName).text = zbUtil.Language("lang.10026");
			foreach (ResourceDetail money in zbSelf.Instance.moneyHandler.GetMoneyList())
			{
				m_listItemDatas.Add(new zbItemData(EResourceType.Money, money.Tid, money.Count));
			}
			m_listItemDatas = m_listItemDatas.OrderBy((zbItemData item) => item.id).ToList();
			break;
		case 2:
			((TextElement)m_lbName).text = zbUtil.Language("lang.10027");
			foreach (PropDetail prop in zbServerData.sGameData.PropList)
			{
				m_listItemDatas.Add(new zbItemData(EResourceType.Prop, prop.Tid, prop.Count));
			}
			m_listItemDatas = m_listItemDatas.OrderBy((zbItemData item) => item.id).ToList();
			break;
		case 3:
			((TextElement)m_lbName).text = zbUtil.Language("lang.10030");
			foreach (ResourceDetail box in zbServerData.sGameData.BoxList)
			{
				m_listItemDatas.Add(new zbItemData(EResourceType.Box, box.Tid, box.Count));
			}
			m_listItemDatas = (from data in m_listItemDatas
				orderby zbDataConfig.Instance.boxCsvTable.GetRecordByKey((int)data.id)?.quality ?? 0 descending, data.id
				select data).ToList();
			break;
		}
	}
}
