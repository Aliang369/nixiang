using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbOperateItem : zbViewBaseItem
{
	private zbOperateWnd m_wndParent;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement m_spriteTime;

	private Label m_lbTime;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_new;

	private zbOperateCsvRecord m_record;

	private zbOperateData m_data;

	public zbOperateItem(zbOperateWnd wndParent, VisualTreeAsset tAsset)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteTime = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sprite_time", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag(m_spriteBg, (Vector2?)new Vector2(0f, -5f), 1f, true);
		m_notify.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/common/notifytag_i.png"));
		m_new = zbUtil.CreateNewTag(m_spriteBg, (Vector2?)new Vector2(-5f, -12f), 0.9f);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbOperateData data)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_record = zbDataConfig.Instance.operateCsvTable.GetRecordByKey((int)data.operateType);
		if (m_record != null)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(m_record.icon);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteBg.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteBg.MarkDirtyRepaint();
			}
			((TextElement)m_lbName).text = m_record.name;
		}
		m_spriteTime.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(data.baseInfo == null));
		RefreshLeftTime();
		RefreshNotify();
	}

	private void OnTouch(ClickEvent evt)
	{
		switch (m_data.operateType)
		{
		case EOperateType.RechargeDaily:
			zbUIMgr.Instance.ShowRechargeDaily(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.RechargeDailySum:
			zbUIMgr.Instance.ShowRechargeDailySum(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.RechargeSum:
			zbUIMgr.Instance.ShowRechargeSum(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.ConsumeDailySum:
			zbUIMgr.Instance.ShowConsumeDailySum(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.ConsumeSum:
			zbUIMgr.Instance.ShowConsumeSum(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.LimitTimeHero:
			zbUIMgr.Instance.ShowLottery(2);
			break;
		case EOperateType.LimitTimeShop:
		{
			LimitTimeShopDetail timeShopInfoByUUID = zbSelf.Instance.operateHandler.GetTimeShopInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowLimitTimeShop(timeShopInfoByUUID);
			break;
		}
		case EOperateType.LoginSum:
		{
			LoginSumDetail loginSumInfoByUUID = zbSelf.Instance.operateHandler.GetLoginSumInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowLoginSum(loginSumInfoByUUID);
			break;
		}
		case EOperateType.DailyActive:
			zbUIMgr.Instance.ShowDailyActive(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.BarDrawNormal:
		case EOperateType.BarDrawSpecial:
			zbUIMgr.Instance.ShowBarDrawNormalSum(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.PropExchange:
		{
			PropExchangeDetail exchangeInfoByUUID = zbSelf.Instance.operateHandler.GetExchangeInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowPropExchange(exchangeInfoByUUID);
			break;
		}
		case EOperateType.ScratchCard:
		{
			ScratchCardDetail scratchCardInfoByUUID = zbSelf.Instance.operateHandler.GetScratchCardInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowScratchCard(scratchCardInfoByUUID);
			break;
		}
		case EOperateType.JiZi:
			zbUIMgr.Instance.ShowJizi(zbSelf.Instance.operateHandler.GetJiZiYouLiByUUID(m_data.baseInfo.UUID));
			break;
		case EOperateType.CipherBox:
		{
			CipherBoxDetail cipherBoxInfoByUUID = zbSelf.Instance.operateHandler.GetCipherBoxInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowCipherBox(cipherBoxInfoByUUID);
			break;
		}
		case EOperateType.BarrageCmd:
			zbUIMgr.Instance.ShowBarrageCmd(m_data.baseInfo.UUID, m_record);
			break;
		case EOperateType.LimitTimeCatcher:
		{
			LimitTimeCatcherDetail timeCatcherInfoByUUID = zbSelf.Instance.operateHandler.GetTimeCatcherInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowTimeCatcher(timeCatcherInfoByUUID);
			break;
		}
		case EOperateType.HolidayTask:
			zbWsMsgHandler.Instance.Operate_HolidayTaskData_Req(openWnd: true);
			break;
		case EOperateType.PrayLantern:
		{
			PrayLanternDetail prayLanternInfoByUUID = zbSelf.Instance.operateHandler.GetPrayLanternInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowPrayLantern(prayLanternInfoByUUID);
			break;
		}
		case EOperateType.ResourceExchange:
			zbUIMgr.Instance.ShowResourceExchange(zbSelf.Instance.operateHandler.GetResourceExchangeInfoByUUID(m_data.baseInfo.UUID));
			break;
		case EOperateType.Fusion:
			zbWsMsgHandler.Instance.Operate_FusionData_Req();
			break;
		case EOperateType.InfiniteGift:
		{
			InfiniteGiftDetail infiniteGiftInfoByUUID = zbSelf.Instance.operateHandler.GetInfiniteGiftInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowInfiniteGift(infiniteGiftInfoByUUID);
			break;
		}
		case EOperateType.LimitTimeRoulette:
		{
			LimitTimeRouletteDetail limitTimeRouletteInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeRouletteInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowLimitTimeRoulette(limitTimeRouletteInfoByUUID);
			break;
		}
		case EOperateType.LimitTimeTreasure:
		{
			LimitTimeRouletteDetail limitTimeTreasureInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeTreasureInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowLimitTimeTreasure(limitTimeTreasureInfoByUUID);
			break;
		}
		case EOperateType.LimitTimeVendor:
		{
			LimitTimeRouletteDetail limitTimeVendorInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeVendorInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowLimitTimeVendor(limitTimeVendorInfoByUUID);
			break;
		}
		case EOperateType.Restaurant:
		{
			RestaurantDetail restaurantByUUID = zbSelf.Instance.operateHandler.GetRestaurantByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowRestaurant(restaurantByUUID);
			break;
		}
		case EOperateType.ThemeTask:
		{
			ThemeTaskDetail themeTaskInfoByUUID = zbSelf.Instance.operateHandler.GetThemeTaskInfoByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowThemeTask(themeTaskInfoByUUID);
			break;
		}
		case EOperateType.ZhanLingKill:
		{
			ZhanLingKillDetail zhanLingKillDetailByUUID = zbSelf.Instance.operateHandler.GetZhanLingKillDetailByUUID(m_data.baseInfo.UUID);
			zbUIMgr.Instance.ShowZhanLingKill(zhanLingKillDetailByUUID);
			break;
		}
		case EOperateType.DailySign:
			zbUIMgr.Instance.ShowDailySign();
			break;
		case EOperateType.TiliBank:
			zbUIMgr.Instance.ShowTiliBank();
			break;
		case EOperateType.Community:
			zbUIMgr.Instance.ShowHub(1);
			break;
		case EOperateType.RechargeGift:
		case EOperateType.EndlessMine:
		case EOperateType.SnowVeil:
		case (EOperateType)30:
		case EOperateType.BubbleGift:
		case (EOperateType)34:
		case (EOperateType)35:
		case EOperateType.Discord:
			break;
		}
	}

	private void RefreshNotify()
	{
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool flag2 = false;
		if (m_data != null)
		{
			switch (m_data.operateType)
			{
			case EOperateType.RechargeDaily:
				flag = zbSelf.Instance.operateHandler.HasRechargeDailyGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.RechargeDailySum:
				flag = zbSelf.Instance.operateHandler.HasRechargeDailySumGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.RechargeSum:
				flag = zbSelf.Instance.operateHandler.HasRechargeSumGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.ConsumeDailySum:
				flag = zbSelf.Instance.operateHandler.HasConsumeDailySumGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.ConsumeSum:
				flag = zbSelf.Instance.operateHandler.HasConsumeSumGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.LimitTimeHero:
				flag = zbSelf.Instance.operateHandler.HasLimitTimeHeroGift();
				break;
			case EOperateType.LimitTimeShop:
				flag = zbSelf.Instance.operateHandler.HasLimitTimeShopGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.LoginSum:
				flag = zbSelf.Instance.operateHandler.HasLoginSumGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.DailyActive:
				flag = zbSelf.Instance.operateHandler.HasDailyActivieGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.BarDrawNormal:
			case EOperateType.BarDrawSpecial:
				flag = zbSelf.Instance.operateHandler.HasBarDrawGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.PropExchange:
				flag = zbSelf.Instance.operateHandler.HasExchangeGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.ScratchCard:
				flag = zbSelf.Instance.operateHandler.HasScratchCardGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.JiZi:
				flag = zbSelf.Instance.operateHandler.HasJiZiGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.CipherBox:
				flag = zbSelf.Instance.operateHandler.HasCipherBoxGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.BarrageCmd:
				flag = zbSelf.Instance.operateHandler.HasBarrageCmdGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.LimitTimeCatcher:
				flag = zbSelf.Instance.operateHandler.HasTimeCatcherNotify(m_data.baseInfo.UUID);
				break;
			case EOperateType.HolidayTask:
				flag = zbSelf.Instance.operateHandler.HasHolidayTaskGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.PrayLantern:
				flag = zbSelf.Instance.operateHandler.HasPrayLanternGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.ResourceExchange:
				flag = zbSelf.Instance.operateHandler.HasResourceExchangeGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.Fusion:
				flag = zbSelf.Instance.operateHandler.HasFusionGift();
				break;
			case EOperateType.InfiniteGift:
				flag = zbSelf.Instance.operateHandler.HasInfiniteGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.LimitTimeRoulette:
				flag = zbSelf.Instance.operateHandler.HasLimitTimeRouletteGift(m_data.baseInfo.UUID);
				break;
			case EOperateType.LimitTimeTreasure:
				flag = zbSelf.Instance.operateHandler.HasLimitTimeTreasure(m_data.baseInfo.UUID);
				break;
			case EOperateType.LimitTimeVendor:
				flag = zbSelf.Instance.operateHandler.HasLimitTimeVendor(m_data.baseInfo.UUID);
				break;
			case EOperateType.ThemeTask:
				flag = zbSelf.Instance.operateHandler.HasThemeTaskNotify(m_data.baseInfo.UUID);
				break;
			case EOperateType.ZhanLingKill:
				flag = zbSelf.Instance.operateHandler.HasZhanLingKillNotify(m_data.baseInfo.UUID);
				break;
			case EOperateType.DailySign:
				flag = zbSelf.Instance.activityHandler.HasDailySignNotify();
				break;
			case EOperateType.TiliBank:
				flag = zbSelf.Instance.operateHandler.HasTiliNotify();
				break;
			}
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_new.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
	}

	private void RefreshLeftTime()
	{
		if (m_data != null && m_data.baseInfo != null)
		{
			long num = m_data.baseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(num);
			}
			else
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.10156");
			}
		}
	}

	public void UpdatePerSecond()
	{
		RefreshLeftTime();
		RefreshNotify();
	}
}
