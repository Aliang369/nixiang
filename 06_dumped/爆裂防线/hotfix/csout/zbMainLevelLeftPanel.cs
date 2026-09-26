using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainLevelLeftPanel
{
	private zbMainLevelWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnMonthCard;

	private Button m_btnFirstRecharge;

	private Button m_btnShop;

	private Button m_btnRecharge;

	private SpineVisualElement m_spineMonthCard;

	private VisualElement m_notifyMonthCard;

	private VisualElement m_notifyFirstRecharge;

	private VisualElement m_notifyRecharge;

	private VisualElement m_newShop;

	private VisualElement m_notifyShop;

	public void OnEnable(zbMainLevelWnd wndParent, VisualElement root)
	{
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnMonthCard = UQueryExtensions.Q<Button>(m_RootElement, "btn_monthCard", (string)null);
		m_btnFirstRecharge = UQueryExtensions.Q<Button>(m_RootElement, "btn_firstRecharge", (string)null);
		m_btnShop = UQueryExtensions.Q<Button>(m_RootElement, "btn_shop", (string)null);
		m_btnRecharge = UQueryExtensions.Q<Button>(m_RootElement, "btn_recharge", (string)null);
		m_spineMonthCard = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnMonthCard, "spine_effect", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMonthCard, (Action)OnMonthCard, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFirstRecharge, (Action)OnFirstRecharge, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShop, (Action)OnShop, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecharge, (Action)OnRecharge, "", (TrickleDown)0);
		m_notifyMonthCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnMonthCard, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyFirstRecharge = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFirstRecharge, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyRecharge = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRecharge, (Vector2?)new Vector2(-2f, -5f), 1f, true);
		m_newShop = zbUtil.CreateNewTag((VisualElement)(object)m_btnShop, (Vector2?)new Vector2(5f, 0f), 1f);
		m_notifyShop = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnShop, (Vector2?)new Vector2(15f, 8f), 1f, true);
	}

	public void Refresh()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMonthCard).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!(zbServerData.sGameData.RoleCommonInfo.RechargeAmount > 0f) || zbServerData.sGameData.MainQuestInfo.MainLevel < 5 || zbChannel_Login.IsOpenTSCheck()) ? 1 : 0));
		((VisualElement)m_btnFirstRecharge).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.activityHandler.IsFirstRechargeEnd() || zbServerData.sGameData.MainQuestInfo.MainLevel < 2) ? 1 : 0));
		((VisualElement)m_btnShop).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Shop1) || zbServerData.sGameData.MainQuestInfo.MainLevel < 5) ? 1 : 0));
		((VisualElement)m_btnRecharge).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.MainQuestInfo.MainLevel < 5));
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_spineMonthCard, (DisplayStyle)(zbSelf.Instance.mainQuestHandler.GetSpineEffectFunc() != 3));
		UpdateRedPoint();
	}

	public void UpdateRedPoint()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbServerData.sGameData.OperateInfo.RechargeGiftList != null && zbServerData.sGameData.OperateInfo.RechargeGiftList.Count > 0 && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.RechargeGiftList[0].BaseInfo);
		m_notifyMonthCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasVipCardShopNotify()));
		m_notifyFirstRecharge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.activityHandler.HasFirstRechargeNotify()));
		m_notifyRecharge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(((!flag || !zbSelf.Instance.operateHandler.HasRechargeGiftNotify(1)) && !zbSelf.Instance.vipHandler.HasDailyPackInfoNotify() && !zbSelf.Instance.vipHandler.HasVipCardShopNotify() && !zbSelf.Instance.passportHandler.HasPassportRedPoint() && !zbSelf.Instance.passportHandler.HasMainQuestRedPoint()) ? 1 : 0));
		m_newShop.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.shopHandler.HasNewPoint()));
		m_notifyShop.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.shopHandler.HasNewPoint() || (!zbSelf.Instance.shopHandler.HasConsumeTargetDetailNotify() && !zbSelf.Instance.shopHandler.HasPuTongNotify())) ? 1 : 0));
	}

	public void OnMonthCard()
	{
		zbUIMgr.Instance.ShowVip(3);
	}

	public void OnRecharge()
	{
		if (zbChannel_Login.IsOpenTSCheck())
		{
			zbUIMgr.Instance.ShowVipIosReviewShop();
			return;
		}
		bool flag = zbSelf.Instance.operateHandler.uuidRechargeGift.Count > 0;
		zbUIMgr.Instance.ShowVip((!flag) ? 2 : 0);
	}

	public void OnFirstRecharge()
	{
		zbUIMgr.Instance.ShowFirstRecharge();
	}

	public void OnShop()
	{
		zbUIMgr.Instance.ShowShop();
	}
}
