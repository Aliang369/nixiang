using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDailyPackItem : zbViewBaseItem
{
	private Label m_lblGet;

	private Button m_btnBuy;

	private VisualElement m_itemgetted;

	private VisualElement m_nodePrice;

	private Label m_lblPrice;

	private VisualElement m_bg1;

	private VisualElement m_bg2;

	private Label m_lbltitle;

	private Label m_lbldays;

	private VisualElement m_bonus;

	private Label m_lblimited;

	private VisualElement m_notify;

	private zbItem[] m_items;

	private DailyPackDetail m_dailyPackDetail;

	private int m_dailyPackGoodId;

	public zbDailyPackItem(zbBaseWnd baseWnd)
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[3];
		base._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_dailypackitem.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.CloneTree());
		m_lbltitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbltitle", (string)null);
		m_lblGet = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lblget", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btnbuy", (string)null);
		m_nodePrice = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "nodeprice", (string)null);
		m_lblPrice = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lblprice", (string)null);
		m_lbldays = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbldays", (string)null);
		m_bg1 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg1", (string)null);
		m_bg2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg2", (string)null);
		m_itemgetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "itemgetted", (string)null);
		m_bonus = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bonus", (string)null);
		m_lblimited = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_limited", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "awarditems", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(val2, "item" + i, (string)null);
			m_items[i].OnEnable(baseWnd);
		}
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBuy, (Vector2?)new Vector2(5f, 5f), 1f, true);
		baseWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(DailyPackDetail dailyPackDetail)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_dailyPackDetail = dailyPackDetail;
		if (dailyPackDetail == null)
		{
			return;
		}
		zbVipDailyPackCsvRecord recordByKey = zbDataConfig.Instance.vipDailyPackCsvTable.GetRecordByKey(dailyPackDetail.Id);
		if (recordByKey == null)
		{
			return;
		}
		m_dailyPackGoodId = recordByKey.goodsId;
		bool flag = recordByKey.bonus != null;
		bool flag2 = recordByKey.type == 1;
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(dailyPackDetail.State != EActiveState.Active));
		m_bonus.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || flag2) ? 1 : 0));
		if (flag)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_bonus, "lb_bonus", (string)null)).text = recordByKey.bonus;
		}
		if (recordByKey != null)
		{
			for (int j = 0; j < recordByKey.awardList.Length; j++)
			{
				if (j < 3)
				{
					((VisualElement)m_items[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					int[] array = recordByKey.awardList[j];
					m_items[j].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
				}
			}
			((TextElement)m_lbltitle).text = zbUtil.Language(recordByKey.name);
		}
		m_bg1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
		m_bg2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		((TextElement)m_lblPrice).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(recordByKey.goodsId);
		((TextElement)m_lblGet).text = zbUtil.Language("lang.10090");
		m_nodePrice.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lblGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(dailyPackDetail.State == EActiveState.Done));
		m_itemgetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(dailyPackDetail.State != EActiveState.Done));
		((VisualElement)m_lblimited).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((dailyPackDetail.State != EActiveState.Pending || flag2) ? 1 : 0));
		if (dailyPackDetail.State == EActiveState.Pending)
		{
			m_nodePrice.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
		}
		else if (dailyPackDetail.State == EActiveState.Active)
		{
			((VisualElement)m_lblGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png"));
		}
		else if (dailyPackDetail.State == EActiveState.Done)
		{
			((VisualElement)m_btnBuy).SetEnabled(false);
			((VisualElement)m_lblGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((VisualElement)m_lbldays).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((dailyPackDetail.Count <= 0 || flag2) ? 1 : 0));
		((TextElement)m_lbldays).text = $"剩余:{dailyPackDetail.Count}天";
	}

	private void OnBuy()
	{
		if (m_dailyPackDetail != null)
		{
			if (m_dailyPackDetail.State == EActiveState.Active)
			{
				zbWsMsgHandler.Instance.RechargeActive_DailyPack_Req(m_dailyPackDetail.Id);
			}
			else
			{
				zbChannel_Pay.OnPay(m_dailyPackGoodId);
			}
		}
	}
}
