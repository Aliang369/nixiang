using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipTequanWnd : zbBaseWnd
{
	private VisualElement m_topPnl;

	private VisualElement m_viplv;

	private SpineVisualElement m_viplvSpine;

	private VisualElement m_vipProgress;

	private Label m_vipExp;

	private Label m_vipExpNextGet;

	private VisualElement m_vipBox;

	private Label m_vipBoxTm;

	private VisualElement m_vipBoxNotify;

	private VisualElement m_bg;

	private VisualElement m_descbg;

	private zbVipTequanDesc m_tqDescPnl;

	private VisualElement m_giftdaily_bg;

	private VisualElement m_gifttq_bg;

	private VisualElement m_giftDaily;

	private zbItem[] m_dailyGiftAwards = new zbItem[4];

	private Label m_dailyGiftTitle;

	private Label m_dailyGiftTime;

	private Label m_dailyNorech;

	private Button m_dailyGiftBtnGet;

	private VisualElement m_dailyNotify;

	private VisualElement m_giftTq;

	private zbItem[] m_tqGiftAwards = new zbItem[4];

	private Label m_tqGiftTitle;

	private Button m_tqGiftBtnGet;

	private VisualElement m_tqdis;

	private Label m_lbltqDis;

	private Button m_btnVipShop;

	private Button m_btnVipUp;

	private Button m_btnClose;

	private int curTabVip;

	private Dictionary<string, Sprite> m_spriteCache = new Dictionary<string, Sprite>();

	public zbSpineScene m_spineScene;

	protected override void OnEnable()
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_topPnl = UQueryExtensions.Q<VisualElement>(m_RootElement, "top", (string)null);
		m_viplv = UQueryExtensions.Q<VisualElement>(m_topPnl, "viplv", (string)null);
		m_viplvSpine = UQueryExtensions.Q<SpineVisualElement>(m_topPnl, "viplvspine", (string)null);
		m_vipProgress = UQueryExtensions.Q<VisualElement>(m_topPnl, "progress", (string)null);
		m_vipExp = UQueryExtensions.Q<Label>(m_topPnl, "vip_exp", (string)null);
		m_vipExpNextGet = UQueryExtensions.Q<Label>(m_topPnl, "vip_next_exp", (string)null);
		m_vipBox = UQueryExtensions.Q<VisualElement>(m_topPnl, "vipbox", (string)null);
		m_vipBoxTm = UQueryExtensions.Q<Label>(m_topPnl, "vipbox_tm", (string)null);
		((TextElement)m_vipBoxTm).text = "";
		m_vipBoxNotify = zbUtil.CreateNotifyTag(m_vipBox, (Vector2?)new Vector2(-6f, -6f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>(m_vipBox, (Action)OnVipBoxClick, "", (TrickleDown)0);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "tequan_desc", (string)null);
		if (m_tqDescPnl == null)
		{
			m_tqDescPnl = new zbVipTequanDesc(this, root);
		}
		m_tqDescPnl.OnEnable();
		m_descbg = UQueryExtensions.Q<VisualElement>(m_RootElement, "tequan_bg", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "gift", (string)null);
		m_giftDaily = UQueryExtensions.Q<VisualElement>(val, "gift_daily", (string)null);
		m_giftTq = UQueryExtensions.Q<VisualElement>(val, "gift_tq", (string)null);
		m_giftdaily_bg = UQueryExtensions.Q<VisualElement>(m_giftDaily, "giftdaily_bg", (string)null);
		m_gifttq_bg = UQueryExtensions.Q<VisualElement>(m_giftTq, "gifttq_bg", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_dailyGiftAwards[i] = UQueryExtensions.Q<zbItem>(m_giftDaily, $"item{i}", (string)null);
			m_dailyGiftAwards[i].OnEnable(this);
		}
		m_dailyGiftTitle = UQueryExtensions.Q<Label>(m_giftDaily, "daily_title", (string)null);
		m_dailyGiftTime = UQueryExtensions.Q<Label>(m_giftDaily, "daily_time", (string)null);
		m_dailyNorech = UQueryExtensions.Q<Label>(m_giftDaily, "daily_norech", (string)null);
		m_dailyGiftBtnGet = UQueryExtensions.Q<Button>(m_giftDaily, "btn_dailybuy", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_dailyGiftBtnGet, (Action)OnDailyGiftGet, "", (TrickleDown)0);
		m_dailyNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_dailyGiftBtnGet, (Vector2?)new Vector2(0f, 0f), 1f, true);
		for (int j = 0; j < 4; j++)
		{
			m_tqGiftAwards[j] = UQueryExtensions.Q<zbItem>(m_giftTq, $"item{j}", (string)null);
			m_tqGiftAwards[j].OnEnable(this);
		}
		m_tqGiftTitle = UQueryExtensions.Q<Label>(m_giftTq, "tq_title", (string)null);
		m_tqGiftBtnGet = UQueryExtensions.Q<Button>(m_giftTq, "btn_tqbuy", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tqGiftBtnGet, (Action)OnTqGiftGet, "", (TrickleDown)0);
		m_tqdis = UQueryExtensions.Q<VisualElement>(m_giftTq, "tqdis", (string)null);
		m_lbltqDis = UQueryExtensions.Q<Label>(m_tqdis, "lbldis", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		m_btnVipUp = UQueryExtensions.Q<Button>(m_RootElement, "btn_vipup", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVipUp, (Action)OnGoVipUp, "", (TrickleDown)0);
		m_btnVipShop = UQueryExtensions.Q<Button>(m_RootElement, "btn_vipshop", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVipShop, (Action)OnVipTqShop, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_VipInfo_Update, OnVipInfoUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnVipInfoUpdate);
	}

	private void OnVipInfoUpdate(object data)
	{
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		RefreshVipInfo();
		m_tqDescPnl.Refresh();
	}

	private void RefreshVipInfo()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		int vipLevel = zbSelf.Instance.GetVipLevel();
		if (vipLevel == 0)
		{
			m_viplv.style.backgroundImage = new StyleBackground(LoadSpriteCached("ui/image/vip/vipshop/viplv0.png"));
		}
		else
		{
			m_viplv.style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			zbUtil.RefreshSpineUI(m_viplvSpine, $"animation/ui/vipdengji/ui_vipicon{vipLevel}/ui_vipicon{vipLevel}_SkeletonData.asset", "loop", loop: true);
		}
		((VisualElement)m_viplvSpine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vipLevel <= 0));
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		float num = Mathf.Clamp01((float)vipInfo.VipExpInfo.VipExp / (float)vipInfo.VipExpInfo.NextExp) * 564f;
		m_vipProgress.style.width = new StyleLength(num);
		((TextElement)m_vipExp).text = $"{vipInfo.VipExpInfo.VipExp}/{vipInfo.VipExpInfo.NextExp}";
		if (vipInfo.VipFreeExp != null)
		{
			if (vipInfo.VipFreeExp.State == EBoolState.False)
			{
				((TextElement)m_vipBoxTm).text = "领取";
				int dayFreeExp = zbSelf.Instance.vipHandler.GetDayFreeExp(vipInfo.VipFreeExp.Days);
				((TextElement)m_vipExpNextGet).text = $"今日可领取vip经验值:{dayFreeExp}";
			}
			else
			{
				RefreshFreeExpTime();
				int dayFreeExp2 = zbSelf.Instance.vipHandler.GetDayFreeExp(vipInfo.VipFreeExp.Days, bGetNextDay: true);
				((TextElement)m_vipExpNextGet).text = $"明日可领取vip经验值:{dayFreeExp2}";
			}
		}
		m_vipBoxNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.hasVipFreeExp()));
	}

	private Sprite LoadSpriteCached(string path)
	{
		if (m_spriteCache.TryGetValue(path, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(path);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteCache[path] = val;
		}
		return val;
	}

	private void SetBtnState(Button btn, bool enable)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		string path = (enable ? "ui/image/button/btn_180_80_yellow.png" : "ui/image/button/btn_180_80_gray.png");
		Sprite val = LoadSpriteCached(path);
		if ((Object)(object)val != (Object)null)
		{
			((VisualElement)btn).style.backgroundImage = new StyleBackground(val);
		}
		((VisualElement)btn).SetEnabled(enable);
		((VisualElement)btn).style.opacity = StyleFloat.op_Implicit(1f);
	}

	public void OnTabVipChange(int curTabVip)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		this.curTabVip = curTabVip;
		string path = "ui/image/vip/vipshop/vip_tequan_bg1.png";
		string path2 = "ui/image/vip/vipshop/vip_tqbg1.png";
		string spinePath = "animation/ui/vipdengji/viptequan_bg1/viptequan_bg1_SkeletonData.asset";
		string path3 = "ui/image/vip/vipshop/vip_cell1.png";
		string path4 = "ui/image/vip/vipshop/daily_cell1.png";
		Color val = default(Color);
		((Color)(ref val))._002Ector(0.325f, 0.125f, 0.38f);
		if (this.curTabVip >= 3 && this.curTabVip <= 7)
		{
			path = "ui/image/vip/vipshop/vip_tequan_bg2.png";
			spinePath = "animation/ui/vipdengji/viptequan_bg2/viptequan_bg2_SkeletonData.asset";
			path3 = "ui/image/vip/vipshop/vip_cell2.png";
			path2 = "ui/image/vip/vipshop/vip_tqbg2.png";
			path4 = "ui/image/vip/vipshop/daily_cell2.png";
			((Color)(ref val))._002Ector(0.4f, 0.149f, 0.047f);
		}
		else if (this.curTabVip >= 8 && this.curTabVip <= zbSelf.Instance.vipHandler.maxVipLv)
		{
			path = "ui/image/vip/vipshop/vip_tequan_bg3.png";
			spinePath = "animation/ui/vipdengji/viptequan_bg3/viptequan_bg3_SkeletonData.asset";
			path3 = "ui/image/vip/vipshop/vip_cell3.png";
			path2 = "ui/image/vip/vipshop/vip_tqbg3.png";
			path4 = "ui/image/vip/vipshop/daily_cell3.png";
			((Color)(ref val))._002Ector(0.506f, 0.071f, 0.067f);
		}
		m_spineScene.SetSkeletonDataAsset(spinePath);
		m_spineScene.SetAnimation("animation", isLoop: true);
		m_bg.style.backgroundImage = new StyleBackground(LoadSpriteCached(path));
		m_descbg.style.backgroundImage = new StyleBackground(LoadSpriteCached(path2));
		m_giftdaily_bg.style.backgroundImage = new StyleBackground(LoadSpriteCached(path4));
		m_gifttq_bg.style.backgroundImage = new StyleBackground(LoadSpriteCached(path3));
		((VisualElement)m_tqGiftTitle).style.color = StyleColor.op_Implicit(val);
		RefreshDailyGiftInfo();
		RefreshVipTqGiftInfo();
	}

	public void RefreshDailyGiftInfo()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		zbItem[] dailyGiftAwards = m_dailyGiftAwards;
		for (int i = 0; i < dailyGiftAwards.Length; i++)
		{
			((VisualElement)dailyGiftAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		List<zbItemData> dailyFreeGiftAward = zbSelf.Instance.vipHandler.GetDailyFreeGiftAward(0, curTabVip);
		int num = 0;
		foreach (zbItemData item in dailyFreeGiftAward)
		{
			if (num >= m_dailyGiftAwards.Length)
			{
				break;
			}
			zbItem zbItem2 = m_dailyGiftAwards[num];
			if (zbItem2 != null)
			{
				((VisualElement)zbItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItem2.Refresh(item);
			}
			num++;
		}
		((TextElement)m_dailyGiftTitle).text = $"VIP{curTabVip}每日礼包";
		RefreshNextDailyGiftTime();
		m_dailyNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.hasDailyFreeGift(curTabVip)));
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		if (vipInfo == null)
		{
			return;
		}
		((TextElement)m_dailyGiftBtnGet).text = "领取";
		SetBtnState(m_dailyGiftBtnGet, enable: true);
		int vipLevel = zbSelf.Instance.GetVipLevel();
		if (vipInfo.DailyFree > 0)
		{
			if (curTabVip <= vipInfo.DailyFree)
			{
				((TextElement)m_dailyGiftBtnGet).text = "已领取";
				SetBtnState(m_dailyGiftBtnGet, enable: false);
			}
			else if (curTabVip > vipInfo.DailyFree && vipLevel >= curTabVip)
			{
				((TextElement)m_dailyGiftBtnGet).text = "补领";
			}
		}
		((VisualElement)m_dailyGiftBtnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((vipLevel < curTabVip) ? 1 : 0));
		((VisualElement)m_dailyNorech).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((vipLevel >= curTabVip) ? 1 : 0));
	}

	public void RefreshVipTqGiftInfo()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		zbItem[] tqGiftAwards = m_tqGiftAwards;
		for (int i = 0; i < tqGiftAwards.Length; i++)
		{
			((VisualElement)tqGiftAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		List<zbItemData> vipTqGiftAward = zbSelf.Instance.vipHandler.GetVipTqGiftAward(curTabVip);
		int num = 0;
		foreach (zbItemData item in vipTqGiftAward)
		{
			if (num >= m_tqGiftAwards.Length)
			{
				break;
			}
			zbItem zbItem2 = m_tqGiftAwards[num];
			if (zbItem2 != null)
			{
				((VisualElement)zbItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItem2.Refresh(item);
			}
			num++;
		}
		((TextElement)m_tqGiftTitle).text = $"VIP{curTabVip}特权礼包";
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		if (vipInfo == null)
		{
			return;
		}
		zbVipLevelPackCsvRecord recordByKey = zbDataConfig.Instance.vipLevelPackCsvTable.GetRecordByKey(curTabVip);
		if (vipInfo.VipLevelPack.Contains(curTabVip))
		{
			((TextElement)m_tqGiftBtnGet).text = "已购买";
			SetBtnState(m_tqGiftBtnGet, enable: false);
		}
		else
		{
			if (recordByKey != null)
			{
				string priceByGoodsId = zbSelf.Instance.vipHandler.GetPriceByGoodsId(recordByKey.goodsId);
				((TextElement)m_tqGiftBtnGet).text = priceByGoodsId;
			}
			SetBtnState(m_tqGiftBtnGet, enable: true);
		}
		((VisualElement)m_tqGiftBtnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetVipLevel() < curTabVip) ? 1 : 0));
		m_tqdis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetVipLevel() < curTabVip) ? 1 : 0));
		if (recordByKey != null)
		{
			((TextElement)m_lbltqDis).text = recordByKey.offShow;
		}
	}

	private void RefreshFreeExpTime()
	{
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		if (vipInfo != null && vipInfo.VipFreeExp.State != EBoolState.False)
		{
			long num = zbServerData.sGameData.RoleCommonInfo.NextRefresh - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_vipBoxTm).text = zbUtil.FormatTimeHHMMSS(num);
			}
			else
			{
				((TextElement)m_vipBoxTm).text = "";
			}
		}
	}

	private void RefreshNextDailyGiftTime()
	{
		long num = zbServerData.sGameData.RoleCommonInfo.NextRefresh - zbSelf.Instance.timeHandler.GetServerTime();
		if (num > 0)
		{
			((TextElement)m_dailyGiftTime).text = zbUtil.FormatTimeHHMMSS(num);
		}
		else
		{
			((TextElement)m_dailyGiftTime).text = "";
		}
	}

	public void OnVipBoxClick()
	{
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		if (vipInfo != null)
		{
			if (vipInfo.VipFreeExp.State == EBoolState.False)
			{
				zbWsMsgHandler.Instance.Vip_FreeExp_Req();
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo("倒计时结束后再领取");
			}
		}
	}

	public void OnDailyGiftGet()
	{
		if (zbSelf.Instance.GetVipLevel() == 0 || zbSelf.Instance.GetVipLevel() < curTabVip)
		{
			zbUIMgr.Instance.BubbleInfo("请先提升VIP等级");
		}
		else if (zbServerData.sGameData.VipInfo.DailyFree == 0 && zbSelf.Instance.GetVipLevel() >= curTabVip)
		{
			zbWsMsgHandler.Instance.Vip_DailyFree_Req(curTabVip);
		}
		else
		{
			zbUIMgr.Instance.ShowVipTequanGiftGetPopWnd(curTabVip);
		}
	}

	public void OnTqGiftGet()
	{
		if (zbSelf.Instance.GetVipLevel() == 0 || zbSelf.Instance.GetVipLevel() < curTabVip)
		{
			zbUIMgr.Instance.BubbleInfo("请先提升VIP等级");
			return;
		}
		zbVipLevelPackCsvRecord recordByKey = zbDataConfig.Instance.vipLevelPackCsvTable.GetRecordByKey(curTabVip);
		if (recordByKey != null)
		{
			zbChannel_Pay.OnPay(recordByKey.goodsId);
		}
	}

	public void OnGoVipUp()
	{
		zbUIMgr.Instance.ShowVip(2);
	}

	public void OnVipTqShop()
	{
		zbUIMgr.Instance.ShowShop(3);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshFreeExpTime();
		RefreshNextDailyGiftTime();
	}

	protected override void OnDestroy()
	{
		m_spriteCache.Clear();
		base.OnDestroy();
	}
}
