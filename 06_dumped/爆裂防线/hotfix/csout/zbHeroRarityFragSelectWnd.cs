using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroRarityFragSelectWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private VisualElement m_listNo;

	private Button m_btnConfirm;

	private MultiColumnListView m_listView;

	private List<ResourceDetail> m_listItemDatas = new List<ResourceDetail>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private bool columnsInited;

	public int m_needCount;

	private Action<bool> m_callback;

	public List<BagSelectDetail> m_selectList = new List<BagSelectDetail>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
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
				makeCell = () => (VisualElement)(object)new zbHeroRarityFragSelectItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbHeroRarityFragSelectItem)?.Refresh(m_listItemDatas[num]);
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

	public void RefreshCustom(int needCount, Action<bool> callback)
	{
		m_needCount = needCount;
		m_callback = callback;
		RefreshListView();
		m_selectList.Clear();
		m_selectList.AddRange(zbSelf.Instance.heroHandler.selectList.Where((BagSelectDetail x) => x != null));
	}

	private void RefreshListView()
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
		foreach (ResourceDetail heroFrag in zbServerData.sGameData.HeroFragList)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroFrag.Tid);
			HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(heroFrag.Tid);
			if (recordByKey != null && recordByKey.rarity == 4 && heroDetail != null && heroDetail.Quality >= EQuality.Orange)
			{
				m_listItemDatas.Add(heroFrag);
			}
		}
		m_listItemDatas.Sort((ResourceDetail v1, ResourceDetail v2) => v2.Count.CompareTo(v1.Count));
	}

	public void SetSelect(long tid, long count)
	{
		m_selectList.RemoveAll((BagSelectDetail x) => x.Tid == tid);
		if (count > 0)
		{
			m_selectList.Add(new BagSelectDetail
			{
				Tid = tid,
				Count = count
			});
		}
	}

	private void OnConfirm(ClickEvent evt)
	{
		zbSelf.Instance.heroHandler.selectList.Clear();
		zbSelf.Instance.heroHandler.selectList.AddRange(from x in m_selectList
			where x != null && x.Count > 0
			group x by x.Tid into g
			select g.First());
		m_callback?.Invoke(obj: true);
		Hide();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
