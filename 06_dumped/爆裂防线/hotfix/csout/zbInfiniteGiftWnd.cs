using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbInfiniteGiftWnd : zbBaseWnd
{
	private VisualElement m_dailyFree;

	private Label m_lblDailyFree;

	private VisualElement m_btnDailyFree;

	private VisualElement m_redDot;

	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lblTime;

	private Label m_lbDesc;

	private VisualElement m_targetProgress;

	private Label m_lblProgress;

	private zbItem m_awardItem;

	private Label m_awardCount;

	private Label m_lbTitle;

	private VisualElement m_btnAward;

	public VisualElement icon_flyTarget;

	private VisualElement m_nodeGift;

	private List<zbInfiniteGiftItem> m_infiniteGiftItems = new List<zbInfiniteGiftItem>();

	private InfiniteGiftDetail m_infiniteGiftDetail;

	private int m_targetIndex;

	private bool m_bFirsetIn = true;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(val, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(val, "btn_close", (string)null);
		m_dailyFree = UQueryExtensions.Q<VisualElement>(val, "nodedaily", (string)null);
		m_lblDailyFree = UQueryExtensions.Q<Label>(m_dailyFree, "lbldaily", (string)null);
		m_btnDailyFree = UQueryExtensions.Q<VisualElement>(m_dailyFree, "btndaily", (string)null);
		zbUIEffect.CreateUpDownAction(m_dailyFree, 20f, 1.5f);
		m_lblTime = UQueryExtensions.Q<Label>(val, "lbltime", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(val, "lb_desc", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(val, "lb_title", (string)null);
		m_targetProgress = UQueryExtensions.Q<VisualElement>(val, "progressval", (string)null);
		m_lblProgress = UQueryExtensions.Q<Label>(val, "lblscore", (string)null);
		m_redDot = UQueryExtensions.Q<VisualElement>(val, "red_dot", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(val, "award", (string)null);
		m_awardCount = UQueryExtensions.Q<Label>(val2, "lb_count", (string)null);
		m_awardItem = UQueryExtensions.Q<zbItem>(val2, "awarditem", (string)null);
		m_awardItem.OnEnable(this);
		m_btnAward = UQueryExtensions.Q<VisualElement>(val2, "btnaward", (string)null);
		m_nodeGift = UQueryExtensions.Q<VisualElement>(val, "nodegift", (string)null);
		icon_flyTarget = UQueryExtensions.Q<VisualElement>(val, "icon_flyTarget", (string)null);
		m_infiniteGiftItems.Clear();
		for (int i = 0; i < 5; i++)
		{
			VisualElement val3 = UQueryExtensions.Q<VisualElement>(m_nodeGift, $"nodeitem{i + 1}", (string)null);
			for (int j = 0; j < 2; j++)
			{
				VisualElement obj = UQueryExtensions.Q<VisualElement>(val3, $"item{j + 1}", (string)null);
				obj.Clear();
				zbInfiniteGiftItem zbInfiniteGiftItem2 = new zbInfiniteGiftItem(this);
				obj.Add((VisualElement)(object)zbInfiniteGiftItem2);
				m_infiniteGiftItems.Add(zbInfiniteGiftItem2);
			}
			VisualElement element = UQueryExtensions.Q<VisualElement>(val3, "arrow1", (string)null);
			VisualElement element2 = UQueryExtensions.Q<VisualElement>(val3, "arrow2", (string)null);
			zbUtil.CreateRowMoveAction(element);
			zbUtil.CreateRowUpDownAction(element2);
		}
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnDailyFree, (Action)OnDailyFree, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnAward, (Action)OnAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_InfiniteGiftFree_Ack, OnGiftFreeAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_InfiniteGiftTarget_Ack, RefreshTarget);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnGiftAwardAck);
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	private void OnGiftAwardAck(object data)
	{
		if (m_infiniteGiftDetail != null)
		{
			m_infiniteGiftDetail = zbSelf.Instance.operateHandler.GetInfiniteGiftInfoByUUID(m_infiniteGiftDetail.BaseInfo.UUID);
			if (m_infiniteGiftDetail != null)
			{
				RefreshCustom(m_infiniteGiftDetail, m_callback);
			}
		}
	}

	private void OnGiftFreeAck(object data)
	{
		if (m_infiniteGiftDetail != null)
		{
			m_infiniteGiftDetail = zbSelf.Instance.operateHandler.GetInfiniteGiftInfoByUUID(m_infiniteGiftDetail.BaseInfo.UUID);
			if (m_infiniteGiftDetail != null)
			{
				RefreshCustom(m_infiniteGiftDetail, m_callback);
			}
		}
	}

	public void RefreshCustom(InfiniteGiftDetail infiniteGiftDetail, Action<bool> callback)
	{
		m_infiniteGiftDetail = infiniteGiftDetail;
		m_callback = callback;
		if (m_infiniteGiftDetail != null)
		{
			RefreshGoodsList();
			RefreshTarget();
			RefreshDailyFree();
			RefreshTime();
		}
	}

	private int GetStartRow()
	{
		int result = 1;
		if (((m_infiniteGiftDetail.GoodsList[0].Index - 1) / 2 + 1) % 2 == 0)
		{
			result = 2;
		}
		return result;
	}

	private void RefreshGoodsList()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (m_infiniteGiftDetail.GoodsList.Count != 0)
		{
			int startRow = GetStartRow();
			float num = (startRow - 1) * 199;
			StyleLength top = m_nodeGift.style.top;
			Length value = ((StyleLength)(ref top)).value;
			float value2 = ((Length)(ref value)).value;
			if (m_bFirsetIn)
			{
				m_bFirsetIn = false;
				m_nodeGift.style.top = StyleLength.op_Implicit(0f - num);
				RefreshGoodsData(startRow);
			}
			else if (!Mathf.Approximately(value2, 0f - num))
			{
				((MonoBehaviour)this).StartCoroutine(AnimateScoll(value2, 0f - num, 0.5f, startRow));
			}
			else
			{
				RefreshGoodsData(startRow);
			}
		}
	}

	private IEnumerator AnimateScoll(float from, float to, float duration, int tmpRow)
	{
		float offset = 201f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			float num2 = Mathf.Lerp(from, from - offset, num);
			m_nodeGift.style.top = StyleLength.op_Implicit(num2);
			yield return null;
		}
		m_nodeGift.style.top = StyleLength.op_Implicit(to);
		RefreshGoodsData(tmpRow);
	}

	private void RefreshGoodsData(int tmpRow)
	{
		int num = ((tmpRow != 1) ? 2 : 0);
		for (int i = 0; i < m_infiniteGiftDetail.GoodsList.Count; i++)
		{
			InfiniteGiftGoodsDetail infiniteGiftGoodsDetail = m_infiniteGiftDetail.GoodsList[i];
			if (num < m_infiniteGiftItems.Count && infiniteGiftGoodsDetail != null)
			{
				m_infiniteGiftItems[num].Init(infiniteGiftGoodsDetail, m_infiniteGiftDetail.BaseInfo.UUID);
			}
			num++;
		}
	}

	private void RefreshDailyFree()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (m_infiniteGiftDetail.DailyFreeAward != null)
		{
			AwardDetail awardDetail = m_infiniteGiftDetail.DailyFreeAward[0];
			((TextElement)m_lblDailyFree).text = $"x{awardDetail.Count}";
			bool flag = m_infiniteGiftDetail.DailyFreeState == EBoolState.False;
			m_dailyFree.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			m_redDot.visible = flag;
		}
	}

	private void RefreshTarget(object data = null)
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (m_infiniteGiftDetail != null)
		{
			(OperateTargetDetail, OperateTargetDetail, bool) infiniteGiftLastCurTarget = zbSelf.Instance.operateHandler.GetInfiniteGiftLastCurTarget(m_infiniteGiftDetail.BaseInfo.UUID);
			int num = m_infiniteGiftDetail.TotalScore;
			int num2 = infiniteGiftLastCurTarget.Item2.Target;
			if (infiniteGiftLastCurTarget.Item1 != null)
			{
				num = m_infiniteGiftDetail.TotalScore - infiniteGiftLastCurTarget.Item1.Target;
				num2 = infiniteGiftLastCurTarget.Item2.Target - infiniteGiftLastCurTarget.Item1.Target;
			}
			if (infiniteGiftLastCurTarget.Item3)
			{
				((TextElement)m_lblProgress).text = "MAX";
				m_targetProgress.style.width = StyleLength.op_Implicit(new Length(608f, (LengthUnit)0));
			}
			else
			{
				float num3 = (float)num / (float)num2 * 608f;
				((TextElement)m_lblProgress).text = $"{num}/{num2}";
				m_targetProgress.style.width = StyleLength.op_Implicit(new Length(num3, (LengthUnit)0));
			}
			AwardDetail awardDetail = infiniteGiftLastCurTarget.Item2.AwardList[0];
			if (awardDetail != null)
			{
				m_awardItem.Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count), isNameVis: false, isCountVis: false);
				((TextElement)m_awardCount).text = "X" + awardDetail.Count;
			}
			m_awardItem.SetGettedVisible(infiniteGiftLastCurTarget.Item2.State == ECommonState.End);
			m_awardItem.SetSpineVisible(infiniteGiftLastCurTarget.Item2.State == ECommonState.Yes);
			m_targetIndex = infiniteGiftLastCurTarget.Item2.Index;
		}
	}

	private void RefreshTime()
	{
		if (m_infiniteGiftDetail != null)
		{
			long timeInSeconds = m_infiniteGiftDetail.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lblTime).text = zbUtil.FormatTimeSmart(timeInSeconds);
		}
	}

	public void OnDailyFree()
	{
		if (m_infiniteGiftDetail != null)
		{
			if (m_infiniteGiftDetail.DailyFreeState == EBoolState.True)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10148"));
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_InfiniteGiftFree_Req(m_infiniteGiftDetail.BaseInfo.UUID);
			}
		}
	}

	public void OnAward()
	{
		if (m_infiniteGiftDetail != null)
		{
			if (CheckCanReceiveAward())
			{
				zbWsMsgHandler.Instance.Operate_InfiniteGiftTarget_Req(m_infiniteGiftDetail.BaseInfo.UUID, m_targetIndex);
				return;
			}
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10280"));
			(OperateTargetDetail, OperateTargetDetail, bool) infiniteGiftLastCurTarget = zbSelf.Instance.operateHandler.GetInfiniteGiftLastCurTarget(m_infiniteGiftDetail.BaseInfo.UUID);
			zbUIMgr.Instance.ShowTip(infiniteGiftLastCurTarget.Item2.AwardList[0].Type, infiniteGiftLastCurTarget.Item2.AwardList[0].Tid, 0L);
		}
	}

	private bool CheckCanReceiveAward()
	{
		(OperateTargetDetail, OperateTargetDetail, bool) infiniteGiftLastCurTarget = zbSelf.Instance.operateHandler.GetInfiniteGiftLastCurTarget(m_infiniteGiftDetail.BaseInfo.UUID);
		int totalScore = m_infiniteGiftDetail.TotalScore;
		int target = infiniteGiftLastCurTarget.Item2.Target;
		return totalScore / target >= 1;
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnClose()
	{
		Hide();
		m_callback?.Invoke(obj: true);
	}
}
