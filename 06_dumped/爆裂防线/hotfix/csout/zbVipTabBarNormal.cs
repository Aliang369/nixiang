using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipTabBarNormal
{
	private zbLotteryWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_nodeOperate;

	private SpineVisualElement m_spineOperate;

	private VisualElement m_progressOperate;

	private Label m_lbProgressOperate;

	private Button m_btnOperate;

	private zbItem[] m_itemOperates = new zbItem[3];

	private ECommonState m_operateState = (ECommonState)(-1);

	private Button m_btnRate;

	private Label m_lbRareHit;

	private VisualElement m_target;

	private zbItem m_item = new zbItem();

	private VisualElement m_progress;

	private Label m_lbProgress;

	private Button m_btnTarget;

	private Button m_btnOne;

	private Label m_lbOne;

	private Label m_lbFree;

	private Button m_btnTen;

	private Label m_lbTen;

	private int m_barCost;

	private BarDetail m_barDetail;

	private VisualElement m_notifyFree;

	private VisualElement m_notifyTen;

	public void OnEnable(zbLotteryWnd wndParent, VisualElement root)
	{
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_nodeOperate = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_operate", (string)null);
		m_spineOperate = UQueryExtensions.Q<SpineVisualElement>(m_nodeOperate, "spine", (string)null);
		m_progressOperate = UQueryExtensions.Q<VisualElement>(m_nodeOperate, "progress", (string)null);
		m_lbProgressOperate = UQueryExtensions.Q<Label>(m_nodeOperate, "lb_progress", (string)null);
		m_btnOperate = UQueryExtensions.Q<Button>(m_nodeOperate, "btn_operate", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOperate, (Action)OnOperate, "", (TrickleDown)0);
		for (int i = 0; i < 3; i++)
		{
			m_itemOperates[i] = UQueryExtensions.Q<zbItem>(m_nodeOperate, "item" + i, (string)null);
			m_itemOperates[i].OnEnable(m_wndParent);
		}
		m_btnRate = UQueryExtensions.Q<Button>(m_RootElement, "btn_rate", (string)null);
		m_lbRareHit = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_target = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_target, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_progress = UQueryExtensions.Q<VisualElement>(m_target, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_target, "lb_progress", (string)null);
		m_btnTarget = UQueryExtensions.Q<Button>(m_target, "btn_target", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>(m_RootElement, "lb_one", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>(m_RootElement, "lb_free", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>(m_RootElement, "lb_ten", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRate, (Action<ClickEvent>)delegate
		{
			OnRate();
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTarget, (Action<ClickEvent>)delegate
		{
			OnTarget();
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action<ClickEvent>)delegate
		{
			OnBar(1);
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action<ClickEvent>)delegate
		{
			OnBar(10);
		}, "", (TrickleDown)0);
		m_notifyFree = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateMoney);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, delegate
		{
			RefreshOperate();
		});
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			RefreshOperate();
		});
	}

	public void Refresh()
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		m_barCost = zbSelf.Instance.barHandler.GetBarCost(EBarType.Normal);
		((TextElement)m_lbOne).text = "x" + m_barCost;
		((TextElement)m_lbTen).text = "x" + m_barCost * 10;
		m_barDetail = zbSelf.Instance.barHandler.GetBarDetail(EBarType.Normal);
		if (m_barDetail != null)
		{
			((VisualElement)m_lbOne).visible = m_barDetail.DailyFree == EBoolState.False;
			((VisualElement)m_lbFree).visible = m_barDetail.DailyFree == EBoolState.True;
			m_notifyFree.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_barDetail.DailyFree != EBoolState.True));
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(7, m_barCost * 10)));
			((TextElement)m_lbRareHit).text = $"<color=#3399FF>{m_barDetail.RareHit}</color>次抽取必出<color=#3399FF>晶体Ⅲ</color>";
			m_target.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 7, m_barCost);
		((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
		bool flag2 = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 7, m_barCost * 10);
		((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(flag2 ? Color.white : Color.red);
		RefreshOperate();
	}

	private void RefreshOperate()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		OperateTargetData barDrawTargetDetail = zbSelf.Instance.barHandler.GetBarDrawTargetDetail(EBarType.Normal);
		if (barDrawTargetDetail != null)
		{
			m_nodeOperate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int[][] awardList = ((IEnumerable<AwardDetail>)barDrawTargetDetail.TargetDetail.AwardList).Select((AwardDetail a) => new int[3]
			{
				(int)a.Type,
				(int)a.Tid,
				(int)a.Count
			}).ToArray();
			RefreshOperateDetail(awardList, barDrawTargetDetail.Count, barDrawTargetDetail.TargetDetail.Target, barDrawTargetDetail.TargetDetail.State);
		}
		else
		{
			m_nodeOperate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshOperateDetail(int[][] awardList, int count, int target, ECommonState state)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (m_operateState != state)
		{
			m_operateState = state;
			string text = ((state == ECommonState.Yes) ? "sel" : "unsel");
			m_spineOperate.AnimationState.SetAnimation(0, text, true);
		}
		float num = Mathf.Clamp((float)count / (float)target, 0f, 1f);
		m_progressOperate.style.width = StyleLength.op_Implicit(new Length(num * 288f, (LengthUnit)0));
		((TextElement)m_lbProgressOperate).text = count + "/" + target;
		for (int i = 0; i < 3; i++)
		{
			if (i < awardList.Length)
			{
				((VisualElement)m_itemOperates[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				int[] award = awardList[i];
				m_itemOperates[i].Refresh(new zbItemData((EResourceType)award[0], award[1], award[2]));
				m_itemOperates[i].SetTouchCallback(delegate
				{
					if (state != ECommonState.Yes)
					{
						zbUIMgr.Instance.ShowTip((EResourceType)award[0], award[1], 0L);
					}
					else
					{
						OnOperate();
					}
				});
			}
			else
			{
				((VisualElement)m_itemOperates[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private void OnOperate()
	{
		OperateTargetData barDrawTargetDetail = zbSelf.Instance.barHandler.GetBarDrawTargetDetail(EBarType.Normal);
		if (barDrawTargetDetail != null)
		{
			if (barDrawTargetDetail.UUID == "")
			{
				zbUIMgr.Instance.ShowJourneyThreeDrawCount();
				return;
			}
			zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(12);
			zbUIMgr.Instance.ShowBarDrawNormalSum(barDrawTargetDetail.UUID, recordByKey);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10156"));
			RefreshOperate();
		}
	}

	private void OnRate()
	{
		zbUIMgr.Instance.ShowBarRateNormal();
	}

	private void OnBar(int count)
	{
		if ((count == 1 && m_barDetail.DailyFree == EBoolState.True) || zbSelf.Instance.moneyHandler.HasEnoughMoney(7, m_barCost * count))
		{
			zbWsMsgHandler.Instance.Bar_Draw_Req(EBarType.Normal, count);
		}
		else
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 7L, m_barCost * count);
		}
	}

	private void OnTarget()
	{
		zbWsMsgHandler.Instance.Bar_TargetAward_Req(EBarType.Normal);
	}

	private void OnUpdateMoney(object obj)
	{
		Refresh();
	}
}
