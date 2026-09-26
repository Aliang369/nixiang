using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPropExchangeItem : zbViewBaseItem
{
	private zbPropExchangeWnd m_wndParent;

	private Label m_lbCount;

	private Label m_lbExCount;

	private Button m_btnExchange;

	private Button m_btnRemind;

	private zbItem m_propItem;

	private zbItem[] m_itemTargets;

	private Button m_btnBatch;

	private OperatePropExchangeDetail m_detail;

	private PropExchangeDetail m_propExchangeDetail;

	private VisualElement m_notify;

	private bool m_remind;

	private readonly int[] BATCHCOUNT;

	private int m_batchCount;

	public zbPropExchangeItem(zbPropExchangeWnd wndParent, VisualTreeAsset tAsset)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		m_itemTargets = new zbItem[3];
		m_remind = true;
		BATCHCOUNT = new int[4] { 1, 2, 5, 10 };
		m_batchCount = 1;
		base._002Ector();
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_lbExCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_exCount", (string)null);
		m_btnRemind = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_remind", (string)null);
		m_btnExchange = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_exchange", (string)null);
		m_propItem = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "propItem", (string)null);
		m_propItem.OnEnable(m_wndParent);
		for (int i = 0; i < 3; i++)
		{
			m_itemTargets[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item_target{i}", (string)null);
			m_itemTargets[i].OnEnable(m_wndParent);
		}
		m_btnBatch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_batch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnExchange, (Vector2?)new Vector2(2f, 2f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action)OnExchange, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRemind, (Action)OnRemind, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBatch, (Action)OnBatch, "", (TrickleDown)0);
	}

	public void Init(OperatePropExchangeDetail detail, PropExchangeDetail propExchangeDetail)
	{
		m_detail = detail;
		m_propExchangeDetail = propExchangeDetail;
		bool flag = detail.LimitNum == 0;
		((VisualElement)m_lbCount).visible = !flag;
		if (!flag)
		{
			int num = detail.LimitNum - detail.Count;
			((TextElement)m_lbCount).text = zbUtil.Language("lang.10159") + ":" + $"{num}/{detail.LimitNum}";
		}
		RefreshRemind();
		m_batchCount = m_wndParent.GetBatchCount(m_detail.Index);
		RefreshBatch();
	}

	private void RefreshBatch()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		int canExchangeCount = GetCanExchangeCount();
		if (canExchangeCount > 1 && (m_detail.LimitNum == 0 || m_detail.LimitNum >= 10))
		{
			if (m_batchCount > canExchangeCount)
			{
				m_batchCount = 1;
				for (int num = BATCHCOUNT.Length - 1; num >= 0; num--)
				{
					if (BATCHCOUNT[num] <= canExchangeCount)
					{
						m_batchCount = BATCHCOUNT[num];
						m_wndParent.SetBatchCount(m_detail.Index, m_batchCount);
						break;
					}
				}
			}
			((VisualElement)m_btnBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnBatch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/vip/bar/bar_score{m_batchCount}.png"));
		}
		else
		{
			((VisualElement)m_btnBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_batchCount = 1;
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_propExchangeDetail.PropId);
		long num2 = m_detail.PropCost * m_batchCount;
		((TextElement)m_lbExCount).text = resourceCount + "/" + num2;
		((VisualElement)m_lbExCount).style.color = StyleColor.op_Implicit((resourceCount >= num2) ? Color.green : Color.white);
		m_propItem.Refresh(new zbItemData(EResourceType.Prop, m_propExchangeDetail.PropId, num2));
		for (int i = 0; i < 3; i++)
		{
			bool flag = i < m_detail.AwardList.Count;
			((VisualElement)m_itemTargets[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData(m_detail.AwardList[i].Type, m_detail.AwardList[i].Tid, m_detail.AwardList[i].Count * m_batchCount);
				bool flag2 = zbSelf.Instance.itemHandler.GetCountByPropTid(m_propExchangeDetail.PropId) > 0;
				m_itemTargets[i].Refresh(data, isNameVis: false, flag2);
				m_itemTargets[i].SetGray(!flag2);
			}
		}
	}

	private void RefreshRemind()
	{
		string key = $"PropExchange{zbSelf.Instance.GetRoleId()}{m_propExchangeDetail.BaseInfo.UUID}{m_detail.Index}";
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
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((GetCanExchangeCount() <= 0 || !m_remind) ? 1 : 0));
	}

	private int GetCanExchangeCount()
	{
		int num = (int)zbSelf.Instance.itemHandler.GetCountByPropTid(m_propExchangeDetail.PropId) / m_detail.PropCost;
		if (m_detail.LimitNum > 0)
		{
			int num2 = m_detail.LimitNum - m_detail.Count;
			num = Mathf.Min(num, num2);
		}
		return num;
	}

	private void OnExchange()
	{
		if (m_detail.LimitNum > 0 && m_detail.Count >= m_detail.LimitNum)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10283"));
		}
		else if (GetCanExchangeCount() <= 0)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Prop, m_propExchangeDetail.PropId);
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10086"), resorceIconPath.strName));
		}
		else
		{
			zbWsMsgHandler.Instance.Operate_PropExchange_Req(m_propExchangeDetail.BaseInfo.UUID, m_detail.Index, m_batchCount);
		}
	}

	private void OnRemind()
	{
		string key = $"PropExchange{zbSelf.Instance.GetRoleId()}{m_propExchangeDetail.BaseInfo.UUID}{m_detail.Index}";
		bool visible = ((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRemind, "lb_isChecked", (string)null)).visible;
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRemind, "lb_isChecked", (string)null)).visible = !visible;
		m_remind = !visible;
		zbGameSave.Instance.SetInt(key, m_remind ? 1 : 2);
		SetNotifyByRemind();
	}

	private void OnBatch()
	{
		int num = GetCanExchangeCount();
		if (m_detail.LimitNum > 0)
		{
			num = Mathf.Min(num, m_detail.LimitNum);
		}
		int num2 = Array.IndexOf(BATCHCOUNT, m_batchCount);
		if (num2 < 0)
		{
			num2 = 0;
		}
		for (int i = 1; i <= BATCHCOUNT.Length; i++)
		{
			int num3 = (num2 + i) % BATCHCOUNT.Length;
			int num4 = BATCHCOUNT[num3];
			if (num4 <= num)
			{
				m_batchCount = num4;
				m_wndParent.SetBatchCount(m_detail.Index, m_batchCount);
				break;
			}
		}
		RefreshBatch();
	}
}
