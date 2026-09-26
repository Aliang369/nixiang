using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbShopTabPuTong
{
	private class ShopCell : VisualElement
	{
		public zbShopItem shopItem;

		public ShopCell(zbShopWnd parent)
		{
			shopItem = new zbShopItem(parent);
			((VisualElement)this).Add((VisualElement)(object)shopItem);
		}
	}

	private MultiColumnListView m_listView;

	private zbShopWnd m_parent;

	private ShopDetail m_shopDetail;

	private Dictionary<int, zbShopItem> m_slotItemMap = new Dictionary<int, zbShopItem>();

	private int m_targetSlotIndex;

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
		m_slotItemMap.Clear();
		m_shopDetail = null;
		foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
		{
			if (shop.ShopType == EShopType.Shop1)
			{
				m_shopDetail = shop;
				break;
			}
		}
		m_shopDetail = zbSelf.Instance.shopHandler.GetSortedShopDetail(m_shopDetail, EShopType.Shop1);
		RefreshList(m_shopDetail);
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
					makeCell = () => (VisualElement)(object)new ShopCell(m_parent),
					bindCell = delegate(VisualElement element, int row)
					{
						//IL_0039: Unknown result type (might be due to invalid IL or missing references)
						//IL_004f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0027: Unknown result type (might be due to invalid IL or missing references)
						ShopCell shopCell = element as ShopCell;
						int num = row * 3 + columnIndex;
						if (num >= totalItems)
						{
							((VisualElement)shopCell).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
						}
						else
						{
							((VisualElement)shopCell).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
							((VisualElement)shopCell.shopItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
							shopCell.shopItem.Refresh(detail.SlotList[num].Index, detail.ShopType);
							int index = detail.SlotList[num].Index;
							if (!m_slotItemMap.ContainsKey(index))
							{
								m_slotItemMap[index] = shopCell.shopItem;
							}
							else
							{
								m_slotItemMap[index] = shopCell.shopItem;
							}
						}
					}
				});
			}
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = new List<int>(Enumerable.Range(0, Mathf.CeilToInt((float)totalItems / 3f)));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.height = StyleLength.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	public void ScrollToSlotAndGuide()
	{
		m_targetSlotIndex = zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId;
		int num = 0;
		using (IEnumerator<ShopSlotDetail> enumerator = m_shopDetail.SlotList.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Index != m_targetSlotIndex)
			{
				num++;
			}
		}
		int row = num / 3;
		((MonoBehaviour)m_parent).StartCoroutine(DelayedScroll(row));
	}

	private IEnumerator DelayedScroll(int row)
	{
		yield return null;
		((BaseVerticalCollectionView)m_listView).ScrollToItem(row);
		yield return null;
		if (m_slotItemMap.TryGetValue(m_targetSlotIndex, out var value))
		{
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)value);
			zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId = -1;
		}
	}
}
