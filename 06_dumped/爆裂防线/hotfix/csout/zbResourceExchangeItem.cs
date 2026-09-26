using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbResourceExchangeItem : zbViewBaseItem
{
	private Label m_lbExchange;

	private Label m_icon;

	private Label m_lbCount;

	private Button m_btnExchange;

	private Button m_btnRemind;

	private zbItem[] m_items;

	private zbItem[] m_targets;

	private ExchangeItemDetail m_detail;

	private ResourceExchangeDetail m_resourceExchangeDetail;

	private VisualElement m_notify;

	private bool m_remind;

	public zbResourceExchangeItem(zbResourceExchangeWnd parent, VisualTreeAsset tAsset)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[4];
		m_targets = new zbItem[2];
		m_remind = true;
		base._002Ector();
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_lbExchange = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_exchange", (string)null);
		m_icon = UQueryExtensions.Q<Label>((VisualElement)(object)this, "icon", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnExchange = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_exchange", (string)null);
		m_btnRemind = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_remind", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(parent);
		}
		for (int j = 0; j < 2; j++)
		{
			m_targets[j] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item_target" + j, (string)null);
			m_targets[j].OnEnable(parent);
		}
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnExchange, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRemind, (Action)OnRemind, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action)OnExchange, "", (TrickleDown)0);
	}

	public void Init(ExchangeItemDetail detail, ResourceExchangeDetail resourceExchangeDetail)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		m_resourceExchangeDetail = resourceExchangeDetail;
		RefreshRemind();
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < m_detail.SrcList.Count;
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData(m_detail.SrcList[i].Type, m_detail.SrcList[i].Tid, m_detail.SrcList[i].Count);
				m_items[i].Refresh(data);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			bool flag2 = j < m_detail.DstList.Count;
			((VisualElement)m_targets[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				zbItemData data2 = new zbItemData(m_detail.DstList[j].Type, m_detail.DstList[j].Tid, m_detail.DstList[j].Count);
				m_targets[j].Refresh(data2);
			}
		}
		((TextElement)m_lbCount).text = $"{detail.Count}/{detail.Limit}";
	}

	private void RefreshRemind()
	{
		string key = $"ResourceExchange{zbSelf.Instance.GetRoleId()}{m_resourceExchangeDetail.BaseInfo.UUID}{m_detail.Index}";
		if (zbGameSave.Instance.GetInt(key) == 0)
		{
			zbGameSave.Instance.SetInt(key, 1);
			m_remind = true;
		}
		else
		{
			m_remind = zbGameSave.Instance.GetInt(key) == 1;
		}
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRemind, "lb_isChecked", (string)null)).visible = m_remind;
		SetNotifyByRemind();
	}

	private void SetNotifyByRemind()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		bool flag = CheckCanExchange();
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || !m_remind) ? 1 : 0));
	}

	private bool CheckCanExchange()
	{
		if (m_detail.Limit > 0 && m_detail.Count >= m_detail.Limit)
		{
			return false;
		}
		foreach (AwardDetail src in m_detail.SrcList)
		{
			if (zbSelf.Instance.itemHandler.GetCountByItemTid((int)src.Tid) < src.Count)
			{
				return false;
			}
		}
		return true;
	}

	private void OnExchange()
	{
		if (!CheckIsLimited())
		{
			zbWsMsgHandler.Instance.Operate_ResourceExchange_Req(m_resourceExchangeDetail.BaseInfo.UUID, m_detail.Index, 1);
		}
	}

	private bool CheckIsLimited()
	{
		bool num = m_detail.Count == m_detail.Limit;
		if (num)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10263"));
		}
		return num;
	}

	private void OnRemind()
	{
		string key = $"ResourceExchange{zbSelf.Instance.GetRoleId()}{m_resourceExchangeDetail.BaseInfo.UUID}{m_detail.Index}";
		bool visible = ((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRemind, "lb_isChecked", (string)null)).visible;
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRemind, "lb_isChecked", (string)null)).visible = !visible;
		m_remind = !visible;
		zbGameSave.Instance.SetInt(key, m_remind ? 1 : 2);
		SetNotifyByRemind();
	}
}
