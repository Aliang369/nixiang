using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroRaritySelectList
{
	private zbHeroRaritySelectWnd m_wndParent;

	private MultiColumnListView m_listView;

	private List<HeroDetail> m_listItemDatas = new List<HeroDetail>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 4;

	private bool columnsInited;

	public void OnEnable(zbHeroRaritySelectWnd wndParent, MultiColumnListView listView)
	{
		m_wndParent = wndParent;
		m_listView = listView;
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
				width = Length.op_Implicit(160f),
				makeCell = () => (VisualElement)(object)new zbHeroRaritySelectItem(m_wndParent),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbHeroRaritySelectItem)?.Refresh(m_listItemDatas[num]);
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
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		GenerateData();
		int num = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		rowIndexes.Clear();
		for (int i = 0; i < num; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		m_wndParent.listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listItemDatas.Count > 0));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void GenerateData()
	{
		m_listItemDatas.Clear();
		foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
		{
			if (hero.Rarity == (ERarityType)(m_wndParent.m_currTab + 1) && hero.Quality >= EQuality.Orange)
			{
				m_listItemDatas.Add(hero);
			}
		}
	}
}
