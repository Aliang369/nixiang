using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasurePrivilegeWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Button m_btnBuy;

	private Label m_lbTime;

	private Label[] m_lbRules = (Label[])(object)new Label[6];

	private VisualElement m_nodeDaily;

	private VisualElement m_spriteDaily;

	private Label m_lbDaily;

	private VisualElement m_btnDaily;

	private VisualElement m_notify;

	private int m_goodsId;

	protected override void OnEnable()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_nodeDaily = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_daily", (string)null);
		m_spriteDaily = UQueryExtensions.Q<VisualElement>(m_nodeDaily, "bg_daily", (string)null);
		m_lbDaily = UQueryExtensions.Q<Label>(m_nodeDaily, "lb_daily", (string)null);
		m_btnDaily = UQueryExtensions.Q<VisualElement>(m_nodeDaily, "btn_daily", (string)null);
		m_notify = zbUtil.CreateNotifyTag(m_btnDaily, (Vector2?)new Vector2(20f, 15f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnDaily, (Action)OnDaily, "", (TrickleDown)0);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		for (int i = 0; i < 6; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "rule" + i, (string)null);
			m_lbRules[i] = UQueryExtensions.Q<Label>(val, "lb_text", (string)null);
		}
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, OnEventUpdate);
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		zbFreeGiftCsvRecord recordByKey = zbDataConfig.Instance.freeGiftCsvTable.GetRecordByKey(8);
		if (recordByKey != null)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)recordByKey.award[0], recordByKey.award[1]);
			m_spriteDaily.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
			((TextElement)m_lbDaily).text = "x" + recordByKey.award[2];
		}
	}

	public override void Refresh()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		m_nodeDaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasFreePackRedDot()));
		if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True)
		{
			((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_260_88_gray.png"));
			((TextElement)m_btnBuy).text = zbUtil.Language("lang.10721");
		}
		else
		{
			((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_260_88_yellow.png"));
			m_goodsId = zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.GoodsId;
			((TextElement)m_btnBuy).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_goodsId);
		}
		((TextElement)m_lbTime).text = zbUtil.Language("lang.10977");
		List<zbTreasurePrivilegeCsvRecord> list = zbDataConfig.Instance.treasurePrivilegeCsvTable.GetRecords().records.Values.ToList();
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < list.Count;
			((VisualElement)m_lbRules[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				((TextElement)m_lbRules[i]).text = zbUtil.Language(list[i].desc);
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnBuy()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10721"));
		}
		else if (m_goodsId > 0)
		{
			zbChannel_Pay.OnPay(m_goodsId);
		}
	}

	private void OnDaily()
	{
		if (zbSelf.Instance.treasureHandler.HasFreePackRedDot())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(8);
		}
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
