using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbShopWnd : zbTabWnd
{
	private zbConsumeTargetItem m_consumeTarget;

	private readonly Button[] m_tabButtons = (Button[])(object)new Button[5];

	private readonly VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[5];

	private readonly Sprite[] m_tabSpritesNormal = (Sprite[])(object)new Sprite[5];

	private readonly Sprite[] m_tabSpritesSelected = (Sprite[])(object)new Sprite[5];

	private VisualElement[] m_news = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_notifyTabPuTong;

	private VisualElement m_notify;

	private Button m_btnToVipTequan;

	private zbSpineScene m_spineScene;

	private Button m_btnClose;

	private zbShopTabPuTong m_modulePuTong;

	private zbShopTabHun m_moduleHun;

	private zbShopTabRongYu m_moduleRongYu;

	private zbShopTabVip m_moduleVip;

	private zbShopTab5 m_module5;

	private VisualElement m_shopContentRoot;

	private VisualElement m_shopListPnl;

	private VisualElement m_vipInfo;

	private EShopType m_currentShopType;

	private readonly Dictionary<EShopType, EMoneyType[]> m_shopCurrencyCache = new Dictionary<EShopType, EMoneyType[]>();

	private readonly EShopType[] m_tabShopTypes = new EShopType[5]
	{
		EShopType.Shop1,
		EShopType.Shop2,
		EShopType.Shop3,
		EShopType.Shop4,
		EShopType.Shop5
	};

	private readonly string[] m_tabIcons = new string[5] { "shop1.png", "shop2.png", "shop3.png", "shop4.png", "shop5.png" };

	private readonly string[] m_tabIconsSelected = new string[5] { "shop1Sel.png", "shop2Sel.png", "shop3Sel.png", "shop4Sel.png", "shop5Sel.png" };

	private readonly string[] m_shopSpineAnimation = new string[5] { "animation/ui/shop/shop_putong/shop_putong_SkeletonData.asset", "animation/ui/shop/shop_hun/shop_shenmi_SkeletonData.asset", "animation/ui/shop/shop_rongyu/shop_rongyu_SkeletonData.asset", "animation/ui/shop/shop_vip/shop_vip_SkeletonData.asset", "animation/ui/shop/zhanbushop_bg/shop_zhanbu_SkeletonData.asset" };

	protected override void Awake()
	{
		base.Awake();
		m_consumeTarget = new zbConsumeTargetItem();
	}

	protected override void OnEnable()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_shopListPnl = UQueryExtensions.Q<VisualElement>(m_RootElement, "shoplistpnl", (string)null);
		m_vipInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "vipshopinfo", (string)null);
		m_btnToVipTequan = UQueryExtensions.Q<Button>(m_vipInfo, "btn_toVipTequan", (string)null);
		m_consumeTarget = UQueryExtensions.Q<zbConsumeTargetItem>(m_RootElement, "consumeTarget", (string)null);
		m_consumeTarget.OnEnable(this);
		((VisualElement)m_consumeTarget).style.height = StyleLength.op_Implicit(160f);
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn_tab{i}", (string)null);
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"shop{i}", (string)null);
			m_news[i] = zbUtil.CreateNewTag((VisualElement)(object)m_tabButtons[i]);
		}
		m_notifyTabPuTong = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		VisualElement[] tabLayers = m_tabLayers;
		for (int j = 0; j < tabLayers.Length; j++)
		{
			MultiColumnListView val = UQueryExtensions.Q<MultiColumnListView>(tabLayers[j], "list", (string)null);
			if (val != null)
			{
				UQueryExtensions.Q<ScrollView>((VisualElement)(object)val, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
				UQueryExtensions.Q<ScrollView>((VisualElement)(object)val, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
			}
		}
		m_shopContentRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "ShopTitleContent", (string)null);
		LoadTabIcons();
		InitTab(m_tabButtons, m_tabLayers, null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnToVipTequan, (Action)OnVipTeQuan, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Shop, OnUpdateShop);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, delegate
		{
			RefreshOperate();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			RefreshOperate();
		});
	}

	private void OnUpdateShop(object data)
	{
		RefreshCustom(m_currTab);
		OperateConsumTargetData consumeTargetDetail = zbSelf.Instance.shopHandler.GetConsumeTargetDetail();
		if (m_currTab == 0 && consumeTargetDetail != null)
		{
			if (consumeTargetDetail.UUID == "")
			{
				zbWsMsgHandler.Instance.Journey_Data_Req();
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_Data_Req();
			}
		}
	}

	public void RefreshCustom(int index)
	{
		m_shopCurrencyCache.Clear();
		if (zbServerData.sGameData.ShopList == null)
		{
			zbWsMsgHandler.Instance.Shop_Data_Req();
		}
		else
		{
			SelectTab(index);
		}
		CheckFromGetWayBuyPanel();
	}

	public void CheckFromGetWayBuyPanel()
	{
		if (zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId >= 0 && m_currTab == 0)
		{
			m_modulePuTong.ScrollToSlotAndGuide();
		}
	}

	private void LoadTabIcons()
	{
		for (int i = 0; i < 5; i++)
		{
			m_tabSpritesNormal[i] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/shop/" + m_tabIcons[i]);
			m_tabSpritesSelected[i] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/shop/" + m_tabIconsSelected[i]);
		}
	}

	private void UpdateTabHighlight()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		m_notifyTabPuTong.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.shopHandler.HasPuTongNotify()));
		int num = 0;
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			bool flag = m_tabShopTypes[i] == m_currentShopType;
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(flag ? m_tabSpritesSelected[i] : m_tabSpritesNormal[i]);
			bool flag2 = zbSelf.Instance.shopHandler.IsShopOpen(i);
			if (flag2)
			{
				num++;
			}
			((VisualElement)m_tabButtons[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (i == 0 && zbSelf.Instance.shopHandler.HasPuTongNotify())
			{
				m_news[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				continue;
			}
			EFuncType funcTypeByTab = zbSelf.Instance.shopHandler.GetFuncTypeByTab(i);
			m_news[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.newTagHandler.isNewFunc(funcTypeByTab)));
		}
		((VisualElement)m_tabButtons[0]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num <= 1));
	}

	private void UpdateBannerAndAnimation()
	{
		int num = Array.IndexOf(m_tabShopTypes, m_currentShopType);
		if (num >= 0 && num < m_shopSpineAnimation.Length)
		{
			string spinePath = m_shopSpineAnimation[num] ?? "";
			m_spineScene.SetSkeletonDataAsset(spinePath);
			m_spineScene.SetAnimation("animation", isLoop: true);
		}
	}

	private EMoneyType[] GetCurrencyTypesByShop(EShopType shopType)
	{
		if (m_shopCurrencyCache.TryGetValue(shopType, out var value))
		{
			return value;
		}
		ShopDetail shopDetail = ((IEnumerable<ShopDetail>)zbServerData.sGameData.ShopList).FirstOrDefault((ShopDetail s) => s.ShopType == shopType);
		if (shopDetail == null)
		{
			return Array.Empty<EMoneyType>();
		}
		HashSet<EMoneyType> hashSet = new HashSet<EMoneyType>();
		foreach (ShopSlotDetail slot in shopDetail.SlotList)
		{
			hashSet.Add(slot.MoneyType);
		}
		EMoneyType[] array = hashSet.ToArray();
		m_shopCurrencyCache[shopType] = array;
		return array;
	}

	private void UpdateMoneyPanel()
	{
		EMoneyType[] currencyTypesByShop = GetCurrencyTypesByShop(m_currentShopType);
		CreateMoneyPanel(currencyTypesByShop);
	}

	private void OnVipTeQuan()
	{
		zbUIMgr.Instance.ShowVipTequanWnd();
	}

	private void RefreshOperate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		m_consumeTarget.Refresh(m_currentShopType == EShopType.Shop1);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.shopHandler.HasConsumeTargetDetailNotify()));
	}

	public override void DoSelectTab()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		m_shopContentRoot.Clear();
		m_vipInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_currTab != 3));
		switch (m_currTab)
		{
		case 0:
			if (m_modulePuTong == null)
			{
				m_modulePuTong = new zbShopTabPuTong();
				m_modulePuTong.OnEnable(this, m_tabLayers[0]);
			}
			m_modulePuTong.Refresh();
			m_currentShopType = EShopType.Shop1;
			break;
		case 1:
			if (m_moduleHun == null)
			{
				m_moduleHun = new zbShopTabHun();
				m_moduleHun.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleHun.Refresh();
			m_currentShopType = EShopType.Shop2;
			break;
		case 2:
			if (m_moduleRongYu == null)
			{
				m_moduleRongYu = new zbShopTabRongYu();
				m_moduleRongYu.OnEnable(this, m_tabLayers[2]);
			}
			m_moduleRongYu.Refresh();
			m_currentShopType = EShopType.Shop3;
			break;
		case 3:
			if (m_moduleVip == null)
			{
				m_moduleVip = new zbShopTabVip();
				m_moduleVip.OnEnable(this, m_tabLayers[3], m_vipInfo);
			}
			m_moduleVip.Refresh();
			m_currentShopType = EShopType.Shop4;
			break;
		case 4:
			if (m_module5 == null)
			{
				m_module5 = new zbShopTab5();
				m_module5.OnEnable(this, m_tabLayers[4]);
			}
			m_module5.Refresh();
			m_currentShopType = EShopType.Shop5;
			break;
		}
		if (m_currTab != 3)
		{
			m_shopContentRoot.Add((VisualElement)(object)new zbVipItemTitle(m_currentShopType));
		}
		EFuncType funcTypeByTab = zbSelf.Instance.shopHandler.GetFuncTypeByTab(m_currTab);
		zbSelf.Instance.newTagHandler.SetNewFuncViewed(funcTypeByTab);
		UpdateTabHighlight();
		UpdateBannerAndAnimation();
		UpdateMoneyPanel();
		RefreshOperate();
	}

	public override bool IsCanOpen(int tabIndex)
	{
		EFuncType funcTypeByTab = zbSelf.Instance.shopHandler.GetFuncTypeByTab(tabIndex);
		if (!zbSelf.Instance.funHandler.IsLockNoTip(funcTypeByTab))
		{
			EShopType eShopType = (EShopType)(0 + tabIndex + 1);
			foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
			{
				if (shop.ShopType == eShopType && shop.SlotList.Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnClose(ClickEvent evt)
	{
		zbSelf.Instance.shopHandler.getWayItem = null;
		Hide();
	}
}
