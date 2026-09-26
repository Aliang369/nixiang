using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJiziItem : zbViewBaseItem
{
	private zbJiziWnd m_wndParent;

	private Label m_lbRemind;

	private Button m_btnRemind;

	private zbItem[] m_items;

	private zbItem[] m_exChangeTargets;

	private Label m_lbCount;

	private Button m_btnExchange;

	private OperateJiZiExchangeDetail m_detail;

	private JiZiDetail m_jiZiDetail;

	private Button m_btnBatch;

	private VisualElement m_notify;

	private bool m_remind;

	private readonly int[] BATCHCOUNT;

	private int m_batchCount;

	public zbJiziItem(zbJiziWnd wndParent, VisualTreeAsset tAsset)
	{
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[5];
		m_exChangeTargets = new zbItem[2];
		m_remind = true;
		BATCHCOUNT = new int[4] { 1, 2, 5, 10 };
		m_batchCount = 1;
		base._002Ector();
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_lbRemind = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_remind", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnRemind = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_remind", (string)null);
		m_btnExchange = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_exchange", (string)null);
		m_btnBatch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_batch", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		for (int j = 0; j < 2; j++)
		{
			m_exChangeTargets[j] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item_target" + j, (string)null);
			m_exChangeTargets[j].OnEnable(m_wndParent);
		}
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnExchange, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action)OnExchange, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRemind, (Action)OnRemind, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBatch, (Action)OnBatch, "", (TrickleDown)0);
	}

	public void Init(OperateJiZiExchangeDetail detail, JiZiDetail jiZiDetail)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		m_jiZiDetail = jiZiDetail;
		string key = $"Jizi{zbSelf.Instance.GetRoleId()}{m_jiZiDetail.BaseInfo.UUID}{m_detail.Index}";
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
		bool flag = CheckCanExchange();
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || !m_remind) ? 1 : 0));
		((TextElement)m_lbCount).text = $"{detail.Count}/{detail.LimitNum}";
		m_batchCount = m_wndParent.GetBatchCount(m_detail.Index);
		RefreshBatch();
	}

	private void RefreshBatch()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
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
		((TextElement)m_btnExchange).text = zbUtil.Language("lang.10053") + " x" + m_batchCount;
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < m_detail.PropList.Count;
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				long countByPropTid = zbSelf.Instance.itemHandler.GetCountByPropTid(m_detail.PropList[i]);
				zbItemData data = new zbItemData(EResourceType.Prop, m_detail.PropList[i], m_batchCount);
				m_items[i].Refresh(data, isNameVis: false, countByPropTid > 0);
				m_items[i].SetGray(countByPropTid == 0);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			bool flag2 = j < m_detail.AwardList.Count;
			((VisualElement)m_exChangeTargets[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				zbItemData data2 = new zbItemData(m_detail.AwardList[j].Type, m_detail.AwardList[j].Tid, m_detail.AwardList[j].Count * m_batchCount);
				m_exChangeTargets[j].Refresh(data2);
			}
		}
	}

	private int GetCanExchangeCount()
	{
		int num = ((IEnumerable<int>)m_detail.PropList).Select((int propId) => (int)zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, propId)).DefaultIfEmpty(int.MaxValue).Min();
		if (m_detail.LimitNum > 0)
		{
			int num2 = m_detail.LimitNum - m_detail.Count;
			num = Mathf.Min(num, num2);
		}
		return num;
	}

	private void OnExchange()
	{
		zbWsMsgHandler.Instance.Operate_JiZi_Req(m_jiZiDetail.BaseInfo.UUID, m_detail.Index, m_batchCount);
	}

	private bool CheckCanExchange()
	{
		bool result = true;
		foreach (int prop in m_detail.PropList)
		{
			if (zbSelf.Instance.itemHandler.GetCountByPropTid(prop) == 0L)
			{
				result = false;
				break;
			}
		}
		if (m_detail.LimitNum <= m_detail.Count)
		{
			result = false;
		}
		return result;
	}

	private void OnRemind()
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		string key = $"Jizi{zbSelf.Instance.GetRoleId()}{m_jiZiDetail.BaseInfo.UUID}{m_detail.Index}";
		bool visible = ((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRemind, "lb_isChecked", (string)null)).visible;
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRemind, "lb_isChecked", (string)null)).visible = !visible;
		m_remind = !visible;
		zbGameSave.Instance.SetInt(key, m_remind ? 1 : 2);
		bool flag = CheckCanExchange();
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || !m_remind) ? 1 : 0));
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
