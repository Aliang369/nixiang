using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneTab
{
	private zbGeneWnd m_wndParent;

	private VisualElement m_RootElement;

	private int m_tabIndex;

	public VisualElement m_listNo;

	private MultiColumnListView m_listView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private bool columnsInited;

	public void OnEnable(zbGeneWnd wndParent, VisualElement root, int tabIndex)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_tabIndex = tabIndex;
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
				makeCell = () => (VisualElement)(object)new zbGeneItem(m_wndParent),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbGeneItem)?.Refresh(m_listItemDatas[num]);
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
		foreach (ResourceDetail gene in zbServerData.sGameData.GeneList)
		{
			long bagGeneCount = zbSelf.Instance.geneHandler.getBagGeneCount(gene.Tid);
			if (bagGeneCount > 0 && zbDataConfig.Instance.geneCsvTable.GetRecordByKey(gene.Tid).geneShape == m_tabIndex + 1)
			{
				m_listItemDatas.Add(new zbItemData(EResourceType.Gene, gene.Tid, bagGeneCount));
			}
		}
		m_listItemDatas = (from item in m_listItemDatas
			orderby zbDataConfig.Instance.geneCsvTable.GetRecordByKey((int)item.id).type descending, item.id
			select item).ToList();
	}
}
