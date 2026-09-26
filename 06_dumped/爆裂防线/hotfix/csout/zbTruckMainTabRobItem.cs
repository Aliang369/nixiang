using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckMainTabRobItem
{
	private zbTruckMainTabRob m_wndParent;

	public VisualElement m_rootElement;

	private VisualElement m_goldHead;

	private VisualElement m_robIcon;

	private SpineVisualElement m_spineSlot;

	private Button m_btnTouch;

	private TruckMatchDetail truckMatchDetail;

	private int TruckId;

	public void OnEnable(zbTruckMainTabRob wndParent, VisualElement root, int index, VisualElement _m_goldHead)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		m_wndParent = wndParent;
		m_rootElement = root;
		TruckId = index;
		m_goldHead = _m_goldHead;
		m_btnTouch = (Button)m_rootElement;
		m_spineSlot = UQueryExtensions.Q<SpineVisualElement>(m_rootElement, "spine_che", (string)null);
		m_robIcon = UQueryExtensions.Q<VisualElement>(m_goldHead, "robIcon", (string)null);
		m_spineSlot.AnimationState.SetAnimation(0, "che_1", true);
		wndParent.m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
	}

	public void Refresh(TruckMatchDetail _truckMatchDetail)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		truckMatchDetail = _truckMatchDetail;
		string text = "che_" + (int)truckMatchDetail.TransportInfo.Quality;
		TrackEntry current = m_spineSlot.AnimationState.GetCurrent(0);
		if (current != null && current.Animation != null && text != current.Animation.Name)
		{
			m_spineSlot.AnimationState.SetAnimation(0, "che_" + (int)truckMatchDetail.TransportInfo.Quality, true);
		}
		int num = Math.Min(((IEnumerable<int>)truckMatchDetail.TransportInfo.RobList).Count(), 3);
		if (num > 0)
		{
			m_spineSlot.AnimationState.SetAnimation(1, "fire" + num, true);
		}
		else
		{
			m_spineSlot.AnimationState.SetEmptyAnimation(1, 0f);
		}
		if (truckMatchDetail.TransportInfo.Quality >= EQuality.Golden || truckMatchDetail.WantedState == EBoolState.True)
		{
			m_goldHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			InitPlayerInfo();
		}
		else
		{
			m_goldHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void InitPlayerInfo()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)UQueryExtensions.Q<Label>(m_goldHead, "lb_name", (string)null)).text = truckMatchDetail.RoleBaseInfo.RoleName;
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(truckMatchDetail.RoleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_goldHead, "header", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(truckMatchDetail.RoleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_goldHead, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((VisualElement)UQueryExtensions.Q<Label>(m_goldHead, "lb_level", (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_robIcon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(truckMatchDetail.WantedState != EBoolState.True));
	}

	public void OnTouch(ClickEvent evt)
	{
		m_wndParent.m_wndParent.ShowDetail(1, truckMatchDetail, null, m_rootElement);
	}
}
