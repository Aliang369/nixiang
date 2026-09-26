using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarScoreItem : zbViewBaseItem
{
	private zbBarScoreWnd m_wndParent;

	private VisualElement m_nodeNotify;

	private VisualElement m_spriteNotifyOk;

	private zbItem[] m_items = new zbItem[3];

	private Label m_lbCount;

	private Button m_btnExchange;

	private Button m_btnBatch;

	private VisualElement m_notify;

	private LimitTimeHeroScoreDetail m_scoreDetail;

	private int m_leftScore;

	private string m_uuid;

	private readonly int[] BATCHCOUNT = new int[4] { 1, 2, 5, 10 };

	private int m_batchCount = 1;

	public zbBarScoreItem(zbBarScoreWnd wndParent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = wndParent;
		Init();
	}

	public void Init()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		m_nodeNotify = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_notify", (string)null);
		m_spriteNotifyOk = UQueryExtensions.Q<VisualElement>(m_nodeNotify, "sprite_ok", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnExchange = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_exchange", (string)null);
		m_btnBatch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_batch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnExchange, (Vector2?)new Vector2(2f, 2f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_nodeNotify, (Action)OnNotify, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action)OnExchange, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBatch, (Action)OnBatch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(LimitTimeHeroScoreDetail scoreDetail, int leftScore, string uuid)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		m_scoreDetail = scoreDetail;
		m_leftScore = leftScore;
		m_uuid = uuid;
		if (m_scoreDetail.LimitNum == 0)
		{
			((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((TextElement)m_lbCount).text = zbUtil.Language("lang.11073") + ": " + (m_scoreDetail.LimitNum - m_scoreDetail.Count) + "/" + m_scoreDetail.LimitNum;
			((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		m_spriteNotifyOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.newTagHandler.IsNotNotifyBarScore(m_scoreDetail.Index) ? 1 : 0));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasTimeHeroScoreGiftByIndex(m_uuid, m_scoreDetail.Index)));
		m_batchCount = m_wndParent.GetBatchCount(m_scoreDetail.Index);
		RefreshBatch();
	}

	private void RefreshBatch()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		int canExchangeCount = GetCanExchangeCount();
		if (canExchangeCount > 1 && (m_scoreDetail.LimitNum == 0 || m_scoreDetail.LimitNum >= 10))
		{
			if (m_batchCount > canExchangeCount)
			{
				m_batchCount = 1;
				for (int num = BATCHCOUNT.Length - 1; num >= 0; num--)
				{
					if (BATCHCOUNT[num] <= canExchangeCount)
					{
						m_batchCount = BATCHCOUNT[num];
						m_wndParent.SetBatchCount(m_scoreDetail.Index, m_batchCount);
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
		long num2 = m_scoreDetail.Score * m_batchCount;
		((TextElement)m_btnExchange).text = zbUtil.Language("lang.10900") + " x" + num2;
		for (int i = 0; i < m_items.Length; i++)
		{
			if (i < m_scoreDetail.AwardList.Count)
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[i].Refresh(new zbItemData(m_scoreDetail.AwardList[i].Type, m_scoreDetail.AwardList[i].Tid, m_scoreDetail.AwardList[i].Count * m_batchCount));
			}
			else
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private int GetCanExchangeCount()
	{
		int num = m_leftScore / m_scoreDetail.Score;
		if (m_scoreDetail.LimitNum > 0)
		{
			int num2 = m_scoreDetail.LimitNum - m_scoreDetail.Count;
			num = Mathf.Min(num, num2);
		}
		return num;
	}

	private void OnNotify()
	{
		if (!zbSelf.Instance.newTagHandler.IsNotNotifyBarScore(m_scoreDetail.Index))
		{
			zbSelf.Instance.newTagHandler.AddNotNotifyBarScore(m_scoreDetail.Index);
		}
		else
		{
			zbSelf.Instance.newTagHandler.RemoveNotNotifyBarScore(m_scoreDetail.Index);
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
	}

	private void OnExchange()
	{
		if (m_scoreDetail.LimitNum > 0 && m_scoreDetail.Count >= m_scoreDetail.LimitNum)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11072"));
		}
		else if (m_scoreDetail.Score > m_leftScore)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10292"));
		}
		else
		{
			zbWsMsgHandler.Instance.Operate_LimitTimeHeroScore_Req(m_uuid, m_scoreDetail.Index, m_batchCount);
		}
	}

	private void OnBatch()
	{
		int num = GetCanExchangeCount();
		if (m_scoreDetail.LimitNum > 0)
		{
			num = Mathf.Min(num, m_scoreDetail.LimitNum);
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
				m_wndParent.SetBatchCount(m_scoreDetail.Index, m_batchCount);
				break;
			}
		}
		RefreshBatch();
	}
}
