using System;
using System.Collections.Generic;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipLevelUpWnd : zbBaseWnd
{
	private VisualElement m_vipUpbg;

	private SpineVisualElement m_vipUpSpine;

	private Label m_lblUp;

	private SpineVisualElement m_viplvSpine;

	private VisualElement m_viplv;

	private VisualElement m_descbg;

	private Label m_tq_title;

	private ScrollView m_tqSv;

	private VisualElement m_gifttq_bg;

	private zbItem[] m_tqGiftAwards = new zbItem[4];

	private Label m_tqGiftTitle;

	private Button m_tqGiftBtnGet;

	private List<zbVipTequanDescItem> m_items = new List<zbVipTequanDescItem>();

	private Button m_btnSure;

	private Label m_lblDis;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_vipUpbg = UQueryExtensions.Q<VisualElement>(m_RootElement, "vipup", (string)null);
		m_vipUpSpine = UQueryExtensions.Q<SpineVisualElement>(m_vipUpbg, "vipupspine", (string)null);
		m_lblUp = UQueryExtensions.Q<Label>(m_vipUpbg, "lblup", (string)null);
		m_viplv = UQueryExtensions.Q<VisualElement>(m_RootElement, "viplv", (string)null);
		m_viplvSpine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "viplvspine", (string)null);
		m_lblDis = UQueryExtensions.Q<Label>(m_RootElement, "lbldis", (string)null);
		m_descbg = UQueryExtensions.Q<VisualElement>(m_RootElement, "tequan_bg", (string)null);
		m_tq_title = UQueryExtensions.Q<Label>(m_descbg, "tq_title", (string)null);
		m_tqSv = UQueryExtensions.Q<ScrollView>(m_descbg, "tq_sc", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "gift_tq", (string)null);
		m_gifttq_bg = UQueryExtensions.Q<VisualElement>(val, "gifttq_bg", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_tqGiftAwards[i] = UQueryExtensions.Q<zbItem>(val, $"item{i}", (string)null);
			m_tqGiftAwards[i].OnEnable(this);
		}
		m_tqGiftTitle = UQueryExtensions.Q<Label>(val, "gift_title", (string)null);
		m_tqGiftBtnGet = UQueryExtensions.Q<Button>(val, "btn_tqbuy", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tqGiftBtnGet, (Action)OnBuy, "", (TrickleDown)0);
		m_btnSure = UQueryExtensions.Q<Button>(m_RootElement, "btnsure", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSure, (Action)Hide, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnVipInfoUpdate);
		AddDark();
	}

	private void OnVipInfoUpdate(object data)
	{
		Refresh();
	}

	public override void Refresh()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
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
		string resName = "ui/image/vip/vipshop/vip_tqbg1.png";
		string resName2 = "ui/image/vip/vipshop/vip_cell1.png";
		Color val = default(Color);
		((Color)(ref val))._002Ector(0.325f, 0.125f, 0.38f);
		if (vipLevel >= 4 && vipLevel <= 7)
		{
			((Color)(ref val))._002Ector(0.4f, 0.149f, 0.047f);
			resName = "ui/image/vip/vipshop/vip_tqbg2.png";
			resName2 = "ui/image/vip/vipshop/vip_cell2.png";
		}
		else if (vipLevel >= 8 && vipLevel <= zbSelf.Instance.vipHandler.maxVipLv)
		{
			((Color)(ref val))._002Ector(0.506f, 0.071f, 0.067f);
			resName = "ui/image/vip/vipshop/vip_tqbg3.png";
			resName2 = "ui/image/vip/vipshop/vip_cell3.png";
		}
		((VisualElement)m_tqGiftTitle).style.color = StyleColor.op_Implicit(val);
		((TextElement)m_tqGiftTitle).text = $"VIP{vipLevel}特权礼包";
		m_descbg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		m_gifttq_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName2));
		zbItem[] tqGiftAwards = m_tqGiftAwards;
		for (int i = 0; i < tqGiftAwards.Length; i++)
		{
			((VisualElement)tqGiftAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		List<zbItemData> vipTqGiftAward = zbSelf.Instance.vipHandler.GetVipTqGiftAward(vipLevel);
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
		RefreshUpBg();
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		zbVipLevelPackCsvRecord recordByKey = zbDataConfig.Instance.vipLevelPackCsvTable.GetRecordByKey(vipLevel);
		if (vipInfo.VipLevelPack.Contains(vipLevel))
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
		if (recordByKey != null)
		{
			((TextElement)m_lblDis).text = recordByKey.offShow;
		}
		RefreshList(vipLevel);
	}

	private void SetBtnState(Button btn, bool enable)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		string resName = (enable ? "ui/image/button/btn_180_80_yellow.png" : "ui/image/button/btn_180_80_gray.png");
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(resName);
		if ((Object)(object)val != (Object)null)
		{
			((VisualElement)btn).style.backgroundImage = new StyleBackground(val);
		}
		((VisualElement)btn).SetEnabled(enable);
		((VisualElement)btn).style.opacity = StyleFloat.op_Implicit(1f);
	}

	private void RefreshUpBg()
	{
		((TextElement)m_lblUp).text = "VIP等级提升";
		zbUtil.RefreshSpineUI(m_vipUpSpine, "animation/ui/ui_vipup/ui_vipup_SkeletonData.asset", "animation", loop: true);
		float startTop = 10f;
		float endTop = -150f;
		float duration = 1f;
		float startTime = Time.realtimeSinceStartup;
		IVisualElementScheduledItem item = null;
		item = m_vipUpbg.schedule.Execute((Action)delegate
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / duration);
			if (num >= 1f)
			{
				m_vipUpbg.style.top = StyleLength.op_Implicit(endTop);
				item.Pause();
			}
			else if ((int)m_vipUpbg.resolvedStyle.display == 0)
			{
				float num2 = Mathf.SmoothStep(0f, 1f, num);
				m_vipUpbg.style.top = StyleLength.op_Implicit(Mathf.Lerp(startTop, endTop, num2));
			}
		}).Every(0L);
	}

	private void RefreshList(int curLv)
	{
		((TextElement)m_tq_title).text = $"VIP{curLv}特权";
		((VisualElement)m_tqSv).Clear();
		m_items.Clear();
		List<zbViptdescCsvRecord> vipTqDescByLv = zbSelf.Instance.vipHandler.GetVipTqDescByLv(curLv);
		if (vipTqDescByLv == null)
		{
			return;
		}
		int num = 1;
		foreach (zbViptdescCsvRecord item in vipTqDescByLv)
		{
			zbVipTequanDescItem zbVipTequanDescItem2 = new zbVipTequanDescItem();
			zbVipTequanDescItem2.Init(item, num);
			num++;
			m_items.Add(zbVipTequanDescItem2);
			((VisualElement)m_tqSv).Add((VisualElement)(object)zbVipTequanDescItem2);
		}
	}

	private void OnBuy()
	{
		zbVipLevelPackCsvRecord recordByKey = zbDataConfig.Instance.vipLevelPackCsvTable.GetRecordByKey(zbSelf.Instance.GetVipLevel());
		if (recordByKey != null)
		{
			zbChannel_Pay.OnPay(recordByKey.goodsId);
		}
	}
}
