using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHolidayTaskExchangeItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbHolidayTaskExchangeItem>
	{
	}

	private zbHolidayTaskWnd m_wndParent;

	private Label m_lbName;

	private Label m_lbExchange;

	private zbItem m_item;

	private Label m_lbScore;

	private Label m_lbLimit;

	private VisualElement m_nodeNotify;

	private VisualElement m_spriteNotifyOk;

	private Button m_btnExchange;

	private VisualElement m_notify;

	private HolidayTaskExchangeDetail m_exchangeDetail;

	private int m_canExchangeCount;

	public zbHolidayTaskExchangeItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_holidayExchangeItem.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbHolidayTaskWnd wndParent)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbExchange = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_exchange", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(wndParent);
		m_lbScore = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_score", (string)null);
		m_lbLimit = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_limit", (string)null);
		m_nodeNotify = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_notify", (string)null);
		m_spriteNotifyOk = UQueryExtensions.Q<VisualElement>(m_nodeNotify, "sprite_ok", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_nodeNotify, (Action)OnNotify, "", (TrickleDown)0);
		m_btnExchange = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_exchange", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_nodeNotify, (Action)OnExchange, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnExchange, (Vector2?)new Vector2(10f, 10f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(HolidayTaskExchangeDetail exchangeDetail)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		m_exchangeDetail = exchangeDetail;
		AwardDetail awardDetail = m_exchangeDetail.AwardList[0];
		if (awardDetail != null)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(awardDetail.Type, awardDetail.Tid);
			((TextElement)m_lbName).text = resorceIconPath.strName;
			m_item.Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
		}
		m_nodeNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_data.Level < zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecords().records.Count || m_wndParent.m_data.ActiveState != 1) ? 1 : 0));
		((TextElement)m_lbScore).text = string.Format(zbUtil.Language("lang.10291"), m_exchangeDetail.ScoreCost);
		((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.11032"), zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecords().records.Count);
		((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_data.Level >= zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecords().records.Count && m_wndParent.m_data.ActiveState != 0) ? 1 : 0));
		((VisualElement)m_btnExchange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_data.Level < zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecords().records.Count || m_wndParent.m_data.ActiveState != 1) ? 1 : 0));
		m_canExchangeCount = (m_wndParent.m_data.TotalScore - m_wndParent.m_data.ExchangeScore) / m_exchangeDetail.ScoreCost;
		if (m_exchangeDetail.LimitCount > 0)
		{
			int val = m_exchangeDetail.LimitCount - m_exchangeDetail.ExchangeCount;
			m_canExchangeCount = Math.Min(m_canExchangeCount, val);
			((VisualElement)m_lbExchange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbExchange).text = zbUtil.Language("11030") + ": " + val + "/" + m_exchangeDetail.LimitCount;
		}
		else
		{
			((VisualElement)m_lbExchange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (m_canExchangeCount > 0)
		{
			((VisualElement)m_btnExchange).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_180_80_green.png"));
		}
		else
		{
			((VisualElement)m_btnExchange).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png"));
		}
		m_spriteNotifyOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.newTagHandler.IsNotNotifyHolidayTask(m_exchangeDetail.Index) ? 1 : 0));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasScoreExchangeGiftByIndex(m_wndParent.m_data.BaseInfo.UUID, m_exchangeDetail.Index)));
	}

	private void OnNotify()
	{
		if (!zbSelf.Instance.newTagHandler.IsNotNotifyHolidayTask(m_exchangeDetail.Index))
		{
			zbSelf.Instance.newTagHandler.AddNotNotifyHolidayTask(m_exchangeDetail.Index);
		}
		else
		{
			zbSelf.Instance.newTagHandler.RemoveNotNotifyHolidayTask(m_exchangeDetail.Index);
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HolidayTask_Ack);
	}

	private void OnExchange()
	{
		if (m_exchangeDetail.LimitCount > 0 && m_exchangeDetail.ExchangeCount >= m_exchangeDetail.LimitCount)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11031"));
		}
		else if (m_exchangeDetail.LimitCount > 0 && m_exchangeDetail.ExchangeCount >= m_exchangeDetail.LimitCount)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11031"));
		}
	}
}
