using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbMainLevelRightPanel
{
	private zbMainLevelWnd m_wndParent;

	private ScrollView m_scrollView;

	private Button m_btnDown;

	private Button m_btnDownTouch;

	private Button m_btnUp;

	private Button m_btnUpTouch;

	private Button m_btnOperate;

	private Button m_btnInfinite;

	private Button m_btnRoulette;

	private Button m_btnTreasure;

	private Button m_btnVendor;

	private Button m_btnTimeCatcher;

	private Button m_btnFusion;

	private Button m_btnScratchCard;

	private Button m_btnPropExchange;

	private Button m_btnJizi;

	private Button m_btnPrayLantern;

	private Button m_btnZhanLingKill;

	private VisualElement m_notifyOperate;

	private VisualElement m_notifyInfinite;

	private VisualElement m_notifyRoulette;

	private VisualElement m_notifyTreasure;

	private VisualElement m_notifyVendor;

	private VisualElement m_notifyTimeCatcher;

	private VisualElement m_notifyFusion;

	private VisualElement m_notifyScratchCard;

	private VisualElement m_notifyPropExchange;

	private VisualElement m_notifyJizi;

	private VisualElement m_notifyPrayLantern;

	private VisualElement m_notifyZhanLingKill;

	private VisualElement m_notifyUp;

	private VisualElement m_notifyDown;

	private bool m_isOpen = true;

	private float _lastHeight = -1f;

	public void OnEnable(zbMainLevelWnd wndParent, ScrollView scrollView, Button btnUp, Button btnDown)
	{
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_scrollView = scrollView;
		m_btnUp = btnUp;
		m_btnUpTouch = UQueryExtensions.Q<Button>((VisualElement)(object)m_btnUp, "btn_upTouch", (string)null);
		m_btnDown = btnDown;
		m_btnDownTouch = UQueryExtensions.Q<Button>((VisualElement)(object)m_btnDown, "btn_downTouch", (string)null);
		m_btnOperate = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_operate", (string)null);
		m_btnInfinite = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_infinite", (string)null);
		m_btnRoulette = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_roulette", (string)null);
		m_btnTreasure = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_treasure", (string)null);
		m_btnVendor = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_vendor", (string)null);
		m_btnTimeCatcher = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_timeCatcher", (string)null);
		m_btnFusion = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_fusion", (string)null);
		m_btnScratchCard = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_scratchCard", (string)null);
		m_btnPropExchange = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_propExchange", (string)null);
		m_btnJizi = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_jizi", (string)null);
		m_btnPrayLantern = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_prayLantern", (string)null);
		m_btnZhanLingKill = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_zhanLingKill", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDownTouch, (Action)OnDown, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpTouch, (Action)OnUp, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOperate, (Action)OnOperate, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnInfinite, (Action)OnInfinite, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRoulette, (Action)OnRoulette, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTreasure, (Action)OnTreasure, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVendor, (Action)OnVendor, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTimeCatcher, (Action)OnTimeCatcher, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFusion, (Action)OnFusion, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnScratchCard, (Action)OnScratchCard, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPropExchange, (Action)OnPropExchange, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnJizi, (Action)OnJizi, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPrayLantern, (Action)OnPrayLantern, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnZhanLingKill, (Action)OnZhanLingKill, "", (TrickleDown)0);
		m_notifyOperate = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOperate, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyInfinite = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnInfinite, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyRoulette = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRoulette, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyTreasure = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTreasure, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyVendor = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnVendor, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyTimeCatcher = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTimeCatcher, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyFusion = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFusion, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyScratchCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnScratchCard, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyPropExchange = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPropExchange, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyJizi = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnJizi, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyPrayLantern = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPrayLantern, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyZhanLingKill = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnZhanLingKill, (Vector2?)new Vector2(15f, 8f), 1f, true);
		m_notifyUp = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUp, (Vector2?)new Vector2(-10f, 3f), 1f, true);
		m_notifyDown = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnDown, (Vector2?)new Vector2(-10f, 0f), 1f, true);
		InitOperateName();
	}

	private void InitOperateName()
	{
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnOperate, (string)null, (string)null)).text = zbUtil.Language("HuoDong");
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnInfinite, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(24).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRoulette, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(25).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTreasure, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(9).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnVendor, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(36).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTimeCatcher, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(19).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnFusion, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(23).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnScratchCard, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(15).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnPropExchange, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(14).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnJizi, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(16).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnPrayLantern, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(21).shortName;
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnZhanLingKill, (string)null, (string)null)).text = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(39).shortName;
	}

	public void Refresh()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (RefreshDetail())
		{
			RefreshUpDown();
			return;
		}
		OnUp();
		((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	public bool RefreshDetail()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_notifyOperate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Operate)));
		InfiniteGiftDetail infiniteDetail = GetInfiniteDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnInfinite, (DisplayStyle)(infiniteDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyInfinite, (DisplayStyle)((infiniteDetail == null || !zbSelf.Instance.operateHandler.HasInfiniteGift(infiniteDetail.BaseInfo.UUID)) ? 1 : 0));
		LimitTimeRouletteDetail rouletteDetail = GetRouletteDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnRoulette, (DisplayStyle)(rouletteDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyRoulette, (DisplayStyle)((rouletteDetail == null || !zbSelf.Instance.operateHandler.HasLimitTimeRouletteGift(rouletteDetail.BaseInfo.UUID)) ? 1 : 0));
		LimitTimeRouletteDetail treasureDetail = GetTreasureDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnTreasure, (DisplayStyle)(treasureDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyTreasure, (DisplayStyle)((treasureDetail == null || !zbSelf.Instance.operateHandler.HasLimitTimeTreasure(treasureDetail.BaseInfo.UUID)) ? 1 : 0));
		LimitTimeRouletteDetail vendorDetail = GetVendorDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnVendor, (DisplayStyle)(vendorDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyVendor, (DisplayStyle)((vendorDetail == null || !zbSelf.Instance.operateHandler.HasLimitTimeVendor(vendorDetail.BaseInfo.UUID)) ? 1 : 0));
		LimitTimeCatcherDetail catcherDetail = GetCatcherDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnTimeCatcher, (DisplayStyle)(catcherDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyTimeCatcher, (DisplayStyle)((catcherDetail == null || !zbSelf.Instance.operateHandler.HasTimeCatcherNotify(catcherDetail.BaseInfo.UUID)) ? 1 : 0));
		FusionInfo fusionDetail = GetFusionDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnFusion, (DisplayStyle)(fusionDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyFusion, (DisplayStyle)((fusionDetail == null || !zbSelf.Instance.operateHandler.HasFusionGift()) ? 1 : 0));
		ScratchCardDetail scratchCardDetail = GetScratchCardDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnScratchCard, (DisplayStyle)(scratchCardDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyScratchCard, (DisplayStyle)((scratchCardDetail == null || !zbSelf.Instance.operateHandler.HasScratchCardGift(scratchCardDetail.BaseInfo.UUID)) ? 1 : 0));
		PropExchangeDetail propExchangeDetail = GetPropExchangeDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnPropExchange, (DisplayStyle)(propExchangeDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyPropExchange, (DisplayStyle)((propExchangeDetail == null || !zbSelf.Instance.operateHandler.HasExchangeGift(propExchangeDetail.BaseInfo.UUID)) ? 1 : 0));
		JiZiDetail jiziDetail = GetJiziDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnJizi, (DisplayStyle)(jiziDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyJizi, (DisplayStyle)((jiziDetail == null || !zbSelf.Instance.operateHandler.HasJiZiGift(jiziDetail.BaseInfo.UUID)) ? 1 : 0));
		PrayLanternDetail prayLanternDetail = GetPrayLanternDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnPrayLantern, (DisplayStyle)(prayLanternDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyPrayLantern, (DisplayStyle)((prayLanternDetail == null || !zbSelf.Instance.operateHandler.HasPrayLanternGift(prayLanternDetail.BaseInfo.UUID)) ? 1 : 0));
		ZhanLingKillDetail zhanLingKillDetail = GetZhanLingKillDetail();
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnZhanLingKill, (DisplayStyle)(zhanLingKillDetail == null));
		zbUtil.SetDisplayIfChanged(m_notifyZhanLingKill, (DisplayStyle)((zhanLingKillDetail == null || !zbSelf.Instance.operateHandler.HasZhanLingKillNotify(zhanLingKillDetail.BaseInfo.UUID)) ? 1 : 0));
		int num = 1;
		num += ((infiniteDetail != null) ? 1 : 0);
		num += ((rouletteDetail != null) ? 1 : 0);
		num += ((treasureDetail != null) ? 1 : 0);
		num += ((vendorDetail != null) ? 1 : 0);
		num += ((catcherDetail != null) ? 1 : 0);
		num += ((fusionDetail != null) ? 1 : 0);
		num += ((scratchCardDetail != null) ? 1 : 0);
		num += ((propExchangeDetail != null) ? 1 : 0);
		num += ((jiziDetail != null) ? 1 : 0);
		num += ((prayLanternDetail != null) ? 1 : 0);
		num += ((zhanLingKillDetail != null) ? 1 : 0);
		zbUtil.SetStyleIfChanged(ref _lastHeight, Math.Min(num * 120, 352), delegate(float val)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			IStyle style = ((VisualElement)m_scrollView).style;
			BackgroundSize val2 = default(BackgroundSize);
			((BackgroundSize)(ref val2)).x = new Length(92f, (LengthUnit)0);
			((BackgroundSize)(ref val2)).y = new Length(val, (LengthUnit)0);
			style.backgroundSize = StyleBackgroundSize.op_Implicit(val2);
		});
		zbUtil.SetDisplayIfChanged(m_notifyUp, (DisplayStyle)(((!IsBelowScrollViewVisible(m_btnRoulette) || rouletteDetail == null || !zbSelf.Instance.operateHandler.HasLimitTimeRouletteGift(rouletteDetail.BaseInfo.UUID)) && (!IsBelowScrollViewVisible(m_btnTreasure) || treasureDetail == null || !zbSelf.Instance.operateHandler.HasLimitTimeTreasure(treasureDetail.BaseInfo.UUID)) && (!IsBelowScrollViewVisible(m_btnVendor) || vendorDetail == null || !zbSelf.Instance.operateHandler.HasLimitTimeVendor(vendorDetail.BaseInfo.UUID)) && (!IsBelowScrollViewVisible(m_btnTimeCatcher) || catcherDetail == null || !zbSelf.Instance.moneyHandler.HasEnoughMoney(33, 1L)) && (!IsBelowScrollViewVisible(m_btnFusion) || fusionDetail == null || !zbSelf.Instance.operateHandler.HasFusionGift()) && (!IsBelowScrollViewVisible(m_btnScratchCard) || scratchCardDetail == null || !zbSelf.Instance.operateHandler.HasScratchCardGift(scratchCardDetail.BaseInfo.UUID)) && (!IsBelowScrollViewVisible(m_btnPropExchange) || propExchangeDetail == null || !zbSelf.Instance.operateHandler.HasExchangeGift(propExchangeDetail.BaseInfo.UUID)) && (!IsBelowScrollViewVisible(m_btnJizi) || jiziDetail == null || !zbSelf.Instance.operateHandler.HasJiZiGift(jiziDetail.BaseInfo.UUID)) && (!IsBelowScrollViewVisible(m_btnPrayLantern) || prayLanternDetail == null || !zbSelf.Instance.operateHandler.HasPrayLanternGift(prayLanternDetail.BaseInfo.UUID)) && (!IsBelowScrollViewVisible(m_btnZhanLingKill) || zhanLingKillDetail == null || !zbSelf.Instance.operateHandler.HasZhanLingKillNotify(zhanLingKillDetail.BaseInfo.UUID))) ? 1 : 0));
		zbUtil.SetDisplayIfChanged(m_notifyDown, (DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Operate)));
		return num > 0;
	}

	private bool IsBelowScrollViewVisible(Button button)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		float num = m_scrollView.scrollOffset.y + ((VisualElement)m_scrollView).resolvedStyle.height;
		VisualElement contentContainer = ((VisualElement)m_scrollView).contentContainer;
		Rect worldBound = ((VisualElement)button).worldBound;
		return VisualElementExtensions.WorldToLocal(contentContainer, ((Rect)(ref worldBound)).position).y + ((VisualElement)button).resolvedStyle.height / 2f > num;
	}

	private InfiniteGiftDetail GetInfiniteDetail()
	{
		if (zbServerData.sGameData.OperateInfo.InfiniteGiftList != null)
		{
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(infiniteGift.BaseInfo) && infiniteGift.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return infiniteGift;
				}
			}
		}
		return null;
	}

	private LimitTimeRouletteDetail GetRouletteDetail()
	{
		if (zbServerData.sGameData.OperateInfo.LimitTimeRouletteList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeRoulette in zbServerData.sGameData.OperateInfo.LimitTimeRouletteList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeRoulette.BaseInfo) && limitTimeRoulette.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return limitTimeRoulette;
				}
			}
		}
		return null;
	}

	private LimitTimeRouletteDetail GetTreasureDetail()
	{
		if (zbServerData.sGameData.OperateInfo.LimitTimeTreasureList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeTreasure in zbServerData.sGameData.OperateInfo.LimitTimeTreasureList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeTreasure.BaseInfo) && limitTimeTreasure.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return limitTimeTreasure;
				}
			}
		}
		return null;
	}

	private LimitTimeRouletteDetail GetVendorDetail()
	{
		if (zbServerData.sGameData.OperateInfo.LimitTimeVendorList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeVendor in zbServerData.sGameData.OperateInfo.LimitTimeVendorList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeVendor.BaseInfo) && limitTimeVendor.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return limitTimeVendor;
				}
			}
		}
		return null;
	}

	private LimitTimeCatcherDetail GetCatcherDetail()
	{
		if (zbServerData.sGameData.OperateInfo.LimitTimeCatcherList != null)
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeCatcher.BaseInfo) && limitTimeCatcher.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return limitTimeCatcher;
				}
			}
		}
		return null;
	}

	private FusionInfo GetFusionDetail()
	{
		if (zbServerData.sGameData.OperateInfo.FusionInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo) && zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo.Type != EOperatePlanType.Holiday)
		{
			return zbServerData.sGameData.OperateInfo.FusionInfo;
		}
		return null;
	}

	private ScratchCardDetail GetScratchCardDetail()
	{
		if (zbServerData.sGameData.OperateInfo.ScratchCardList != null)
		{
			foreach (ScratchCardDetail scratchCard in zbServerData.sGameData.OperateInfo.ScratchCardList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(scratchCard.BaseInfo) && scratchCard.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return scratchCard;
				}
			}
		}
		return null;
	}

	private PropExchangeDetail GetPropExchangeDetail()
	{
		if (zbServerData.sGameData.OperateInfo.PropExchangeList != null)
		{
			foreach (PropExchangeDetail propExchange in zbServerData.sGameData.OperateInfo.PropExchangeList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(propExchange.BaseInfo) && propExchange.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return propExchange;
				}
			}
		}
		return null;
	}

	private JiZiDetail GetJiziDetail()
	{
		if (zbServerData.sGameData.OperateInfo.JiZiList != null)
		{
			foreach (JiZiDetail jiZi in zbServerData.sGameData.OperateInfo.JiZiList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(jiZi.BaseInfo) && jiZi.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return jiZi;
				}
			}
		}
		return null;
	}

	private PrayLanternDetail GetPrayLanternDetail()
	{
		if (zbServerData.sGameData.OperateInfo.PrayLanternList != null)
		{
			foreach (PrayLanternDetail prayLantern in zbServerData.sGameData.OperateInfo.PrayLanternList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(prayLantern.BaseInfo) && prayLantern.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return prayLantern;
				}
			}
		}
		return null;
	}

	private ZhanLingKillDetail GetZhanLingKillDetail()
	{
		if (zbServerData.sGameData.OperateInfo.ZhanLingKillList != null)
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(zhanLingKill.BaseInfo) && zhanLingKill.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					return zhanLingKill;
				}
			}
		}
		return null;
	}

	public void Update()
	{
		RefreshDetail();
	}

	public void OnDown()
	{
		m_isOpen = true;
		RefreshUpDown();
	}

	public void OnUp()
	{
		m_isOpen = false;
		RefreshUpDown();
	}

	private void RefreshUpDown()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (m_isOpen)
		{
			((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			RefreshDetail();
		}
		else
		{
			((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private void OnOperate()
	{
		zbLinkHelper.OpenOperate();
	}

	private void OnInfinite()
	{
		InfiniteGiftDetail infiniteDetail = GetInfiniteDetail();
		if (infiniteDetail != null)
		{
			zbUIMgr.Instance.ShowInfiniteGift(infiniteDetail);
		}
	}

	private void OnRoulette()
	{
		LimitTimeRouletteDetail rouletteDetail = GetRouletteDetail();
		if (rouletteDetail != null)
		{
			zbUIMgr.Instance.ShowLimitTimeRoulette(rouletteDetail);
		}
	}

	private void OnTreasure()
	{
		LimitTimeRouletteDetail treasureDetail = GetTreasureDetail();
		if (treasureDetail != null)
		{
			zbUIMgr.Instance.ShowLimitTimeTreasure(treasureDetail);
		}
	}

	private void OnVendor()
	{
		LimitTimeRouletteDetail vendorDetail = GetVendorDetail();
		if (vendorDetail != null)
		{
			zbUIMgr.Instance.ShowLimitTimeVendor(vendorDetail);
		}
	}

	private void OnTimeCatcher()
	{
		LimitTimeCatcherDetail catcherDetail = GetCatcherDetail();
		if (catcherDetail != null)
		{
			zbUIMgr.Instance.ShowTimeCatcher(catcherDetail);
		}
	}

	private void OnFusion()
	{
		zbWsMsgHandler.Instance.Operate_FusionData_Req();
	}

	private void OnScratchCard()
	{
		ScratchCardDetail scratchCardDetail = GetScratchCardDetail();
		if (scratchCardDetail != null)
		{
			zbUIMgr.Instance.ShowScratchCard(scratchCardDetail);
		}
	}

	private void OnPropExchange()
	{
		PropExchangeDetail propExchangeDetail = GetPropExchangeDetail();
		if (propExchangeDetail != null)
		{
			zbUIMgr.Instance.ShowPropExchange(propExchangeDetail);
		}
	}

	private void OnJizi()
	{
		JiZiDetail jiziDetail = GetJiziDetail();
		if (jiziDetail != null)
		{
			zbUIMgr.Instance.ShowJizi(jiziDetail);
		}
	}

	private void OnPrayLantern()
	{
		PrayLanternDetail prayLanternDetail = GetPrayLanternDetail();
		if (prayLanternDetail != null)
		{
			zbUIMgr.Instance.ShowPrayLantern(prayLanternDetail);
		}
	}

	private void OnZhanLingKill()
	{
		ZhanLingKillDetail zhanLingKillDetail = GetZhanLingKillDetail();
		if (zhanLingKillDetail != null)
		{
			zbUIMgr.Instance.ShowZhanLingKill(zhanLingKillDetail);
		}
	}
}
