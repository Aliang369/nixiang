using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFusionAward : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_processVal;

	private Label m_lbScore;

	private zbItem[] m_items = new zbItem[4];

	private Label m_lbTarget;

	private Button m_btnTarget;

	private ScrollView m_scrollView;

	private zbFusionAwardItem[] m_awardItems;

	private OperateTargetDetail m_lastTargetDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_processVal = UQueryExtensions.Q<VisualElement>(m_RootElement, "processVal", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>(m_RootElement, "lb_score", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_lbTarget = UQueryExtensions.Q<Label>(m_RootElement, "lb_target", (string)null);
		m_btnTarget = UQueryExtensions.Q<Button>(m_RootElement, "btn_target", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		((CallbackEventHandler)m_scrollView).RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)delegate
		{
			RefreshProgress();
		}, (TrickleDown)0);
		m_scrollView.verticalScroller.valueChanged += delegate
		{
			RefreshProgress();
		};
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTarget, (Action)OnTarget, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_FusionAwardList, OnUpdateEvent);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (zbServerData.sGameData.OperateInfo != null && zbServerData.sGameData.OperateInfo.FusionInfo != null)
		{
			((TextElement)m_lbScore).text = zbUtil.ConvertToTBMK(zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.ActScore);
			UpdateListView();
			ScrollToCurrentTarget();
			RefreshLastTargetDetail();
		}
	}

	public void UpdateListView()
	{
		((VisualElement)m_scrollView).Clear();
		RepeatedField<OperateTargetDetail> targetList = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.TargetList;
		m_awardItems = new zbFusionAwardItem[targetList.Count - 1];
		int num = 0;
		for (int num2 = targetList.Count - 2; num2 >= 0; num2--)
		{
			zbFusionAwardItem zbFusionAwardItem2 = new zbFusionAwardItem(this);
			zbFusionAwardItem2.Refresh(targetList[num2]);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbFusionAwardItem2);
			m_awardItems[num++] = zbFusionAwardItem2;
		}
	}

	private void ScrollToCurrentTarget()
	{
		RepeatedField<OperateTargetDetail> targetList = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.TargetList;
		int currentTargetIndex = GetCurrentTargetIndex();
		currentTargetIndex = Mathf.Clamp(currentTargetIndex, 0, targetList.Count - 2);
		int uiIndex = targetList.Count - 2 - currentTargetIndex;
		float itemHeight = 160f;
		((VisualElement)m_scrollView).schedule.Execute((Action)delegate
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			float height = m_scrollView.contentViewport.resolvedStyle.height;
			float height2 = ((VisualElement)m_scrollView).contentContainer.resolvedStyle.height;
			float num = Mathf.Max(0f, height2 - height);
			float num2 = (float)uiIndex * itemHeight;
			num2 = Mathf.Clamp(num2, 0f, num);
			m_scrollView.scrollOffset = new Vector2(0f, num2);
			RefreshProgress();
		}).StartingIn(50L);
	}

	private int GetCurrentTargetIndex()
	{
		RepeatedField<OperateTargetDetail> targetList = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.TargetList;
		int actScore = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.ActScore;
		for (int i = 0; i < targetList.Count; i++)
		{
			if (actScore < targetList[i].Target)
			{
				return Mathf.Max(0, i - 1);
			}
		}
		return targetList.Count - 1;
	}

	private void RefreshLastTargetDetail()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		m_lastTargetDetail = ((IEnumerable<OperateTargetDetail>)zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.TargetList).LastOrDefault();
		if (m_lastTargetDetail == null)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (i < m_lastTargetDetail.AwardList.Count && m_lastTargetDetail.AwardList[i] != null)
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[i].Refresh(new zbItemData(m_lastTargetDetail.AwardList[i].Type, m_lastTargetDetail.AwardList[i].Tid, m_lastTargetDetail.AwardList[i].Count));
				m_items[i].SetSpineVisible(m_lastTargetDetail.State == ECommonState.Yes);
				m_items[i].SetGettedVisible(m_lastTargetDetail.State == ECommonState.End);
			}
		}
		((TextElement)m_lbTarget).text = zbUtil.ConvertToTBMK(m_lastTargetDetail.Target);
		((VisualElement)m_btnTarget).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_lastTargetDetail.State != ECommonState.Yes));
	}

	private void RefreshProgress()
	{
		m_processVal.schedule.Execute((Action)delegate
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			float height = m_scrollView.contentViewport.resolvedStyle.height;
			float height2 = ((VisualElement)m_scrollView).contentContainer.resolvedStyle.height;
			float y = m_scrollView.scrollOffset.y;
			float progressHeight = GetProgressHeight();
			float num = height2 - height - y;
			float num2 = progressHeight - num;
			num2 = Mathf.Clamp(num2, 0f, 640f);
			m_processVal.style.height = StyleLength.op_Implicit(num2);
		});
	}

	private float GetItemY(int index)
	{
		zbFusionAwardItem zbFusionAwardItem2 = m_awardItems[index];
		return ((VisualElement)zbFusionAwardItem2).resolvedStyle.top + ((VisualElement)zbFusionAwardItem2).resolvedStyle.height * 0.5f + 36f;
	}

	private float GetProgressHeight()
	{
		RepeatedField<OperateTargetDetail> targetList = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.TargetList;
		int actScore = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.ActScore;
		for (int i = 0; i < targetList.Count - 1; i++)
		{
			if (actScore <= targetList[i].Target)
			{
				if (i == 0)
				{
					float itemY = GetItemY(0);
					float num = (float)actScore / (float)targetList[0].Target;
					return Mathf.Lerp(0f, itemY, num);
				}
				int target = targetList[i - 1].Target;
				int target2 = targetList[i].Target;
				float itemY2 = GetItemY(i - 1);
				float itemY3 = GetItemY(i);
				float num2 = (float)(actScore - target) / (float)(target2 - target);
				return Mathf.Lerp(itemY2, itemY3, num2);
			}
		}
		float itemY4 = GetItemY(m_awardItems.Length - 1);
		OperateTargetDetail operateTargetDetail = targetList[targetList.Count - 2];
		OperateTargetDetail operateTargetDetail2 = targetList[targetList.Count - 1];
		if (actScore >= operateTargetDetail.Target)
		{
			float num3 = (float)(actScore - operateTargetDetail.Target) / (float)(operateTargetDetail2.Target - operateTargetDetail.Target);
			return itemY4 + num3 * 82f;
		}
		return itemY4;
	}

	public void OnTarget()
	{
		if (m_lastTargetDetail != null)
		{
			zbWsMsgHandler.Instance.Operate_FusionTargetAward_Req(m_lastTargetDetail.Target);
		}
	}

	private void OnUpdateEvent(object data)
	{
		Refresh();
	}
}
