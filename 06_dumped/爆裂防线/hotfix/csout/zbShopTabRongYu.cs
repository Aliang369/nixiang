using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbShopTabRongYu
{
	private MultiColumnListView m_listView;

	private zbShopWnd m_parent;

	public void OnEnable(zbShopWnd parent, VisualElement root)
	{
		m_parent = parent;
		m_listView = UQueryExtensions.Q<MultiColumnListView>(root, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		Refresh();
	}

	public void Refresh()
	{
		((VisualElement)m_listView).Clear();
		ShopDetail shopDetail = null;
		foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
		{
			if (shop.ShopType == EShopType.Shop3)
			{
				shopDetail = shop;
				break;
			}
		}
		RefreshList(zbSelf.Instance.shopHandler.GetSortedShopDetail(shopDetail, EShopType.Shop3));
	}

	private void RefreshList(ShopDetail detail)
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		int totalItems = detail.SlotList.Count;
		((VisualElement)m_listView).visible = totalItems > 0;
		if (m_listView.columns.Count == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				int columnIndex = i;
				m_listView.columns.Add(new Column
				{
					width = Length.op_Implicit(220f),
					makeCell = () => (VisualElement)(object)new zbShopItem(m_parent),
					bindCell = delegate(VisualElement element, int row)
					{
						//IL_0068: Unknown result type (might be due to invalid IL or missing references)
						//IL_0056: Unknown result type (might be due to invalid IL or missing references)
						int num = row * 3 + columnIndex;
						if (num < totalItems)
						{
							(element as zbShopItem).Refresh(detail.SlotList[num].Index, detail.ShopType);
							element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
						}
						else
						{
							element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
						}
					}
				});
			}
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = new List<int>(Enumerable.Range(0, Mathf.CeilToInt((float)totalItems / 3f)));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.height = StyleLength.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}
}
