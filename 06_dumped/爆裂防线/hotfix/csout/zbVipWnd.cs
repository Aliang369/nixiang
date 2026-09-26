using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipWnd : zbTabWnd
{
	public zbSpineScene m_spineScene;

	private Button m_btnClose;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_tabButtons = (Button[])(object)new Button[5];

	private zbVipTabLimited m_moduleLimited;

	private zbVipTabGift m_moduleGift;

	private zbVipTabVip m_moduleVip;

	private zbVipTabVipCard m_moduleVipCard;

	private zbVipPassport m_modulePassport;

	private VisualElement m_notifyLimited;

	private VisualElement m_notifyGift;

	private VisualElement m_notifyRecharge;

	private VisualElement m_notifyVipCard;

	private VisualElement m_notifyPassport;

	public int m_subTabIndex;

	public int m_diff;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 5; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		m_notifyGift = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_notifyRecharge = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[2], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_notifyVipCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[3], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_notifyPassport = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[4], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_notifyLimited = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.BarSpecial,
			EMoneyType.BarNormal
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DailyPack_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Shop, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckSelfInfo, OnEventUpdate);
	}

	public void RefreshCustom(int tabIndex, int subTabIndex = -1, int diff = 0)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		m_diff = diff;
		m_subTabIndex = subTabIndex;
		((VisualElement)m_tabButtons[0]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasRechargeGift()));
		((VisualElement)m_tabButtons[3]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
		SelectTab(tabIndex);
		RefreshNotify();
	}

	private void RefreshNotify()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		m_notifyLimited.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasRechargeGiftNotify(1)));
		m_notifyGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasDailyPackInfoNotify()));
		m_notifyRecharge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasRechargeNotify()));
		m_notifyVipCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasVipCardShopNotify()));
		m_notifyPassport.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.passportHandler.HasPassportRedPoint() && !zbSelf.Instance.passportHandler.HasMainQuestRedPoint()) ? 1 : 0));
	}

	private void OnEventUpdate(object data)
	{
		if (m_currTab != 4)
		{
			DoSelectTab();
		}
	}

	public override void DoSelectTab()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[5] { "ui/image/vip/limit.png", "ui/image/vip/pack.png", "ui/image/vip/recharge.png", "ui/image/vip/vip_tab4.png", "ui/image/vip/passport.png" };
		((Component)m_spineScene).gameObject.SetActive(true);
		((Component)m_spineScene).transform.position = Vector2.op_Implicit(new Vector2(0f, 7f));
		switch (m_currTab)
		{
		case 0:
			CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
			m_spineScene.SetSceneBg("ui/image/bg/bg_common3.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/shop/shop_xianshilibao/shop_xianshilibao_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			((Component)m_spineScene).transform.position = Vector2.op_Implicit(new Vector2(0f, 6.88f));
			if (m_moduleLimited == null)
			{
				m_moduleLimited = new zbVipTabLimited();
				m_moduleLimited.OnEnable(this, m_tabLayers[m_currTab]);
			}
			foreach (RechargeGiftDetail rechargeGift in zbServerData.sGameData.OperateInfo.RechargeGiftList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeGift.BaseInfo) && rechargeGift.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					m_moduleLimited.RefreshCustom(rechargeGift.BaseInfo.UUID);
					break;
				}
			}
			array[0] = "ui/image/vip/limit_selected.png";
			break;
		case 1:
			CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
			m_spineScene.SetSceneBg("ui/image/vip/dailypack/dailypack_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/shop/shop_meirilibao/shop_meirilibao_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			((Component)m_spineScene).transform.position = Vector2.op_Implicit(new Vector2(0f, 0f));
			if (m_moduleGift == null)
			{
				m_moduleGift = new zbVipTabGift();
				m_moduleGift.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleGift.Refresh();
			array[1] = "ui/image/vip/pack_selected.png";
			break;
		case 2:
			CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
			m_spineScene.SetSceneBg("ui/image/vip/recharge/recharge_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/shop/shop_chongzhi/shop_chongzhi_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			((Component)m_spineScene).transform.position = Vector2.op_Implicit(new Vector2(0f, 0.2f));
			if (m_moduleVip == null)
			{
				m_moduleVip = new zbVipTabVip();
				m_moduleVip.OnEnable(this, m_tabLayers[m_currTab]);
			}
			if (m_subTabIndex >= 0)
			{
				m_moduleVip.Refresh(m_subTabIndex);
				m_subTabIndex = -1;
			}
			else
			{
				m_moduleVip.Refresh();
			}
			m_moduleVip.Refresh(m_subTabIndex, m_diff);
			array[2] = "ui/image/vip/recharge_selected.png";
			break;
		case 3:
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Gold,
				EMoneyType.Diamond
			});
			m_spineScene.SetSceneBg("ui/image/vip/card/card_bg.jpg");
			((Component)m_spineScene).transform.position = Vector2.op_Implicit(new Vector2(0f, 0f));
			if (m_moduleVipCard == null)
			{
				m_moduleVipCard = new zbVipTabVipCard();
				m_moduleVipCard.OnEnable(this, m_tabLayers[3]);
			}
			if (m_subTabIndex >= 0)
			{
				m_moduleVipCard.Refresh(m_subTabIndex);
				m_subTabIndex = -1;
			}
			else
			{
				m_moduleVipCard.Refresh();
			}
			array[3] = "ui/image/vip/vip_tab4_sel.png";
			break;
		case 4:
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Gold,
				EMoneyType.Diamond
			});
			((Component)m_spineScene).transform.position = Vector2.op_Implicit(new Vector2(0f, 0f));
			if (m_modulePassport == null)
			{
				m_modulePassport = new zbVipPassport();
				m_modulePassport.OnEnable(this, m_tabLayers[4]);
			}
			if (m_subTabIndex >= 0)
			{
				m_modulePassport.RefreshCustom(m_subTabIndex);
				m_subTabIndex = -1;
			}
			else
			{
				m_modulePassport.RefreshCustom();
			}
			array[4] = "ui/image/vip/passport_selected.png";
			break;
		}
		for (int i = 0; i < 5; i++)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(array[i]);
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(val);
		}
	}

	public void OnClose()
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_currTab == 0)
		{
			m_moduleLimited.UpdateAtShow();
		}
		if (m_currTab == 4)
		{
			m_modulePassport.UpdateAtShow();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshNotify();
		if (m_currTab == 4)
		{
			m_modulePassport.UpdatePerSecond();
		}
		if (m_currTab == 1)
		{
			m_moduleGift.UpdatePerSecond();
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		OnEventUpdate(null);
	}
}
