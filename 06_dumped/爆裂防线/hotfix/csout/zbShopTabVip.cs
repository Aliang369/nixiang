using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbShopTabVip
{
	private MultiColumnListView m_listView;

	private zbShopWnd m_parent;

	private VisualElement m_viplv;

	private SpineVisualElement m_viplvSpine;

	private SpineVisualElement m_topPnlSpine;

	private VisualElement m_vipProgress;

	private Label m_vipExp;

	private Label m_lblvipLim;

	private ShopDetail m_shopDetail;

	public void OnEnable(zbShopWnd parent, VisualElement root, VisualElement vipInfoRoot)
	{
		m_parent = parent;
		m_viplv = UQueryExtensions.Q<VisualElement>(vipInfoRoot, "viplv", (string)null);
		m_topPnlSpine = UQueryExtensions.Q<SpineVisualElement>(vipInfoRoot, "topspine", (string)null);
		m_viplvSpine = UQueryExtensions.Q<SpineVisualElement>(vipInfoRoot, "viplvspine", (string)null);
		m_vipProgress = UQueryExtensions.Q<VisualElement>(vipInfoRoot, "progress", (string)null);
		m_vipExp = UQueryExtensions.Q<Label>(vipInfoRoot, "vip_exp", (string)null);
		m_lblvipLim = UQueryExtensions.Q<Label>(vipInfoRoot, "lblviplim", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(root, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		Refresh();
	}

	public void Refresh()
	{
		((VisualElement)m_listView).Clear();
		foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
		{
			if (shop.ShopType == EShopType.Shop4)
			{
				m_shopDetail = shop;
				break;
			}
		}
		RefreshVipInfo();
		RefreshList(zbSelf.Instance.shopHandler.GetSortedShopDetail(m_shopDetail, EShopType.Shop4));
	}

	private void RefreshVipInfo()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lblvipLim).text = $"VIP{m_shopDetail.VipLevel}解锁";
		int vipLevel = zbSelf.Instance.GetVipLevel();
		if (vipLevel == 0)
		{
			m_viplv.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/vip/vipshop/viplv0.png"));
		}
		else
		{
			m_viplv.style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			zbUtil.RefreshSpineUI(m_viplvSpine, $"animation/ui/vipdengji/ui_vipicon{vipLevel}/ui_vipicon{vipLevel}_SkeletonData.asset", "loop", loop: true);
		}
		((VisualElement)m_viplvSpine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vipLevel <= 0));
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		float num = Mathf.Clamp01((float)vipInfo.VipExpInfo.VipExp / (float)vipInfo.VipExpInfo.NextExp) * 620f;
		m_vipProgress.style.width = new StyleLength(num);
		((TextElement)m_vipExp).text = $"{vipInfo.VipExpInfo.VipExp}/{vipInfo.VipExpInfo.NextExp}";
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
					makeCell = () => (VisualElement)(object)new zbShopItemVip(m_parent),
					bindCell = delegate(VisualElement element, int row)
					{
						//IL_0068: Unknown result type (might be due to invalid IL or missing references)
						//IL_0056: Unknown result type (might be due to invalid IL or missing references)
						int num = row * 3 + columnIndex;
						if (num < totalItems)
						{
							(element as zbShopItemVip).Refresh(detail.SlotList[num].Index, detail.ShopType);
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
