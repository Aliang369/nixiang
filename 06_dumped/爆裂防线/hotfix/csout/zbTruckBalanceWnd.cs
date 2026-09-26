using System;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckBalanceWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTitle2;

	private Label m_lbPingan;

	private Label m_lbTime;

	private SpineVisualElement m_spineTruck;

	private zbItem[] m_items = new zbItem[10];

	private zbTruckBalanceItem[] truckBalanceItems = new zbTruckBalanceItem[10];

	private Label m_lbFirst;

	private ScrollView m_scroll;

	private Button m_btnClose;

	public TruckTransportInfo truckTransportInfo;

	private bool isRuning;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbFirst = UQueryExtensions.Q<Label>(m_RootElement, "lb_first", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11102");
		m_lbTitle2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_title2", (string)null);
		m_lbPingan = UQueryExtensions.Q<Label>(m_RootElement, "lb_pingan", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_spineTruck = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_che", (string)null);
		m_scroll = UQueryExtensions.Q<ScrollView>(m_RootElement, "listView", (string)null);
		m_scroll.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scroll.horizontalScrollerVisibility = (ScrollerVisibility)2;
		for (int i = 0; i < 10; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{i}", (string)null);
			m_items[i]?.OnEnable(this);
		}
		for (int j = 0; j < 10; j++)
		{
			truckBalanceItems[j] = new zbTruckBalanceItem();
			truckBalanceItems[j]?.OnEnable(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_player" + (j + 1), (string)null));
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckSelfInfo, OnEventUpdate);
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(truckTransportInfo, isRuning);
	}

	public void RefreshCustom(TruckTransportInfo _truckTransportInfo, bool _isRuning)
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		truckTransportInfo = _truckTransportInfo;
		isRuning = _isRuning;
		InitTruckInfo();
		int num = 0;
		foreach (AwardDetail award in truckTransportInfo.TransportInfo.AwardList)
		{
			zbItemData data = new zbItemData(award.Type, award.Tid, award.Count);
			m_items[num].Refresh(data);
			if (num == 0)
			{
				m_items[num].AddBestPrizeTag(vis: true);
			}
			else
			{
				m_items[num].AddBestPrizeTag(vis: false);
			}
			m_items[num].SetTruckLossItemVisible(truckTransportInfo.TransportInfo.RobList.Contains(num));
			num++;
		}
		if (isRuning)
		{
			((TextElement)m_lbTitle2).text = zbUtil.Language("lang.11147");
		}
		else
		{
			((TextElement)m_lbTitle2).text = zbUtil.Language("lang.11142");
		}
		int num2 = Math.Min(truckTransportInfo.RecordList.Count, 10);
		if (num2 > 0)
		{
			((TextElement)m_lbFirst).text = string.Format(zbUtil.Language("lang.11144"), num2);
			((VisualElement)m_lbPingan).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbFirst).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_lbPingan).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbFirst).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		for (int i = 0; i < 10; i++)
		{
			truckBalanceItems[i].m_rootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		for (int j = 0; j < num2; j++)
		{
			truckBalanceItems[j].Refresh(truckTransportInfo.RecordList[j]);
			truckBalanceItems[j].m_rootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		if (!isRuning)
		{
			((TextElement)m_lbTime).text = zbUtil.Language("lang.11475");
		}
		UpdatePerSecond();
		m_scroll.scrollOffset = new Vector2(m_scroll.scrollOffset.x, 0f);
	}

	private void InitTruckInfo()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		m_spineTruck.AnimationState.SetAnimation(0, "che_" + (int)truckTransportInfo.TransportInfo.Quality, true);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_title" + (int)truckTransportInfo.TransportInfo.Quality + ".png");
		UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_quality", (string)null).style.backgroundImage = new StyleBackground(val);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_smallbg" + (int)truckTransportInfo.TransportInfo.Quality + ".png");
		UQueryExtensions.Q<VisualElement>(m_RootElement, "small_quality", (string)null).style.backgroundImage = new StyleBackground(val2);
	}

	protected override void UpdatePerSecond()
	{
		if (isRuning)
		{
			if (truckTransportInfo.TransportInfo.ArriveTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11122") + ":" + zbUtil.FormatTimeHHMMSS(truckTransportInfo.TransportInfo.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime());
			}
			else
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11136");
			}
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
