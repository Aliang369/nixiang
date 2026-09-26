using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneSelectTab
{
	private zbGeneSelectWnd m_wndParent;

	private VisualElement m_RootElement;

	public int m_tabIndex;

	public VisualElement m_listNo;

	private MultiColumnListView m_listView;

	private List<ResourceDetail> m_listItemDatas = new List<ResourceDetail>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private bool columnsInited;

	public void OnEnable(zbGeneSelectWnd wndParent, VisualElement root, int tabIndex)
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
				width = Length.op_Implicit(130f),
				makeCell = () => (VisualElement)(object)new zbGeneSelectItem(m_wndParent, this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbGeneSelectItem)?.Refresh(m_listItemDatas[num]);
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
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		GenerateData();
		if (m_listItemDatas.Count == 0)
		{
			zbGeneComposeWnd zbGeneComposeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.GeneCompose) as zbGeneComposeWnd;
			if ((Object)(object)zbGeneComposeWnd2 != (Object)null && zbGeneComposeWnd2.IsShow())
			{
				zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(m_wndParent.m_geneTid);
				if (recordByKey == null)
				{
					return;
				}
				int num = 0;
				for (int i = 0; i < 4; i++)
				{
					if (zbGeneComposeWnd2.m_geneTids[i] != 0)
					{
						num++;
					}
				}
				if (zbGeneComposeWnd2.m_skillType == ESkillType.EmptyValue)
				{
					num++;
				}
				if (zbSelf.Instance.geneHandler.HasGeneNotify(recordByKey.type, recordByKey.geneShape, num + 1))
				{
					zbWsMsgHandler.Instance.Bag_GeneComposeCalc_Req((EGeneType)recordByKey.type, (EGeneShape)recordByKey.geneShape, m_wndParent.m_index);
					m_wndParent.OnClose(null);
				}
			}
		}
		int num2 = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		rowIndexes.Clear();
		for (int j = 0; j < num2; j++)
		{
			rowIndexes.Add(j);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listItemDatas.Count > 0));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void GenerateData()
	{
		m_listItemDatas.Clear();
		if (m_tabIndex == m_wndParent.m_selectTabIndex && m_wndParent.m_geneTid > 0)
		{
			List<ResourceDetail> list = (from v in zbSelf.Instance.geneHandler.GetGenesByTid(m_wndParent.m_geneTid)
				select v.Clone()).ToList();
			if (zbUIMgr.Instance.IsShow(EWndId.GeneCompose))
			{
				zbGeneComposeWnd zbGeneComposeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.GeneCompose) as zbGeneComposeWnd;
				if (zbGeneComposeWnd2.m_skillType == ESkillType.EmptyValue)
				{
					RemoveSameGene(list, zbGeneComposeWnd2.m_geneTid);
				}
				for (int num = 0; num < 4; num++)
				{
					int num2 = zbGeneComposeWnd2.m_geneTids[num];
					if (num2 != 0)
					{
						RemoveSameGene(list, num2);
					}
				}
			}
			m_listItemDatas = list;
		}
		else
		{
			foreach (ResourceDetail gene in zbServerData.sGameData.GeneList)
			{
				if (zbSelf.Instance.geneHandler.getBagGeneCount(gene.Tid) > 0 && zbDataConfig.Instance.geneCsvTable.GetRecordByKey(gene.Tid).geneShape == m_tabIndex + 1)
				{
					m_listItemDatas.Add(gene);
				}
			}
		}
		m_listItemDatas = (from item in m_listItemDatas
			orderby zbDataConfig.Instance.geneCsvTable.GetRecordByKey(item.Tid).type descending, item.Tid
			select item).ToList();
	}

	private void RemoveSameGene(List<ResourceDetail> geneList, int geneTid)
	{
		ResourceDetail resourceDetail = geneList.FirstOrDefault((ResourceDetail v) => v.Tid == geneTid);
		if (resourceDetail != null)
		{
			resourceDetail.Count--;
			if (resourceDetail.Count <= 0)
			{
				geneList.Remove(resourceDetail);
			}
		}
	}
}
