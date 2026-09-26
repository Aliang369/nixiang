using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderHelpCardWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_nodeDedaily;

	private VisualElement m_iconDaily;

	private Label m_lbDaily;

	private VisualElement m_btnDaily;

	private VisualElement m_notify;

	private VisualElement m_processContent;

	private Label m_lbProgress;

	private VisualElement m_progress;

	private Label m_lbDesc;

	private Button m_btnBuy;

	private JToken m_record;

	private Label[] m_lbRules = (Label[])(object)new Label[6];

	protected override void OnEnable()
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_nodeDedaily = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodedaily", (string)null);
		m_iconDaily = UQueryExtensions.Q<VisualElement>(m_nodeDedaily, "diamond", (string)null);
		m_lbDaily = UQueryExtensions.Q<Label>(m_nodeDedaily, "lbldaily", (string)null);
		m_btnDaily = UQueryExtensions.Q<VisualElement>(m_RootElement, "btndaily", (string)null);
		m_processContent = UQueryExtensions.Q<VisualElement>(m_RootElement, "progressContent", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_processContent, "lb_progress", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_processContent, "progress", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_notify = zbUtil.CreateNotifyTag(m_btnDaily, (Vector2?)new Vector2(20f, 20f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnDaily, (Action)OnDaily, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		for (int i = 0; i < 6; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "rule" + i, (string)null);
			m_lbRules[i] = UQueryExtensions.Q<Label>(val, "lb_text", (string)null);
		}
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, OnUpdateEventFreeGiftAward);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		m_record = (JToken)(object)zbDataConfig.Instance.vipBaseJsonTable.GetRecord();
		List<zbVipCrossLadderCsvRecord> list = zbDataConfig.Instance.vipCrossLadderCsvTable.GetRecords().records.Values.ToList();
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < list.Count;
			((VisualElement)m_lbRules[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				((TextElement)m_lbRules[i]).text = zbUtil.Language(list[i].desc);
			}
		}
		int goodId = m_record[(object)"vipCard"][(object)"crossLadder"].Value<int>((object)"goodsId");
		int num = (int)m_record[(object)"vipCard"][(object)"crossLadder"][(object)"cardDays"];
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuy, "lb_price", (string)null)).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(goodId);
		m_nodeDedaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.HasFreePackRedDot()));
		bool flag2 = zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard();
		((TextElement)m_lbDesc).text = string.Format(flag2 ? zbUtil.Language("lang.10917") : zbUtil.Language("lang.10916"), num);
		m_processContent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		RefreshDailyFree();
		if (flag2)
		{
			RefreshProcess();
		}
	}

	private void RefreshDailyFree()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		zbFreeGiftCsvRecord recordByKey = zbDataConfig.Instance.freeGiftCsvTable.GetRecordByKey(7);
		string strIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)recordByKey.award[0], recordByKey.award[1]).strIconPath;
		m_iconDaily.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(strIconPath));
		((TextElement)m_lbDaily).text = "X" + recordByKey.award[2];
	}

	private void RefreshProcess()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)Math.Floor((double)(zbSelf.Instance.crossLadderHandler.GetCrossLadderCardDetail().ExpireTm - zbSelf.Instance.timeHandler.GetServerTime()) / 86400.0);
		int num2 = (int)m_record[(object)"vipCard"][(object)"crossLadder"][(object)"cardDays"];
		float num3 = Mathf.Clamp((float)num / (float)num2, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num3 * 564f, (LengthUnit)0));
		((TextElement)m_lbProgress).text = string.Format(zbUtil.Language("lang.10604"), num);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnDaily()
	{
		if (zbSelf.Instance.crossLadderHandler.HasFreePackRedDot())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(7);
		}
	}

	private void OnBuy()
	{
		int num = m_record[(object)"vipCard"][(object)"crossLadder"].Value<int>((object)"goodsId");
		if (m_record != null && num > 0)
		{
			zbChannel_Pay.OnPay(num);
		}
	}

	private void OnUpdateEventFreeGiftAward(object data)
	{
		Refresh();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
