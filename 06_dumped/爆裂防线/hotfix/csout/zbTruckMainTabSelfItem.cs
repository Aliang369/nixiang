using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckMainTabSelfItem
{
	private zbTruckMainTabSelf m_wndParent;

	private VisualElement m_rootElement;

	private VisualElement m_rootEmpty;

	private VisualElement m_rootAdd;

	private VisualElement m_rootLock;

	private VisualElement m_rootChe;

	private Button m_rootAward;

	private Label m_lbOpenMoney;

	private SpineVisualElement m_spineSlot;

	private Button m_btnTouch;

	private int TruckId;

	private TruckDetail truckDetail;

	public void OnEnable(zbTruckMainTabSelf wndParent, VisualElement root, int index)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		m_wndParent = wndParent;
		m_rootElement = root;
		TruckId = index;
		m_btnTouch = (Button)m_rootElement;
		m_rootEmpty = UQueryExtensions.Q<VisualElement>(m_rootElement, "self_empty", (string)null);
		m_rootAdd = UQueryExtensions.Q<VisualElement>(m_rootElement, "truck_add", (string)null);
		m_rootLock = UQueryExtensions.Q<VisualElement>(m_rootElement, "truck_lock", (string)null);
		m_rootChe = UQueryExtensions.Q<VisualElement>(m_rootElement, "truck_che", (string)null);
		m_spineSlot = UQueryExtensions.Q<SpineVisualElement>(m_rootElement, "spine_che", (string)null);
		m_rootAward = UQueryExtensions.Q<Button>(m_rootElement, "truck_award", (string)null);
		m_lbOpenMoney = UQueryExtensions.Q<Label>(m_rootElement, "lb_openMoney", (string)null);
		m_spineSlot.AnimationState.SetAnimation(0, "che_1", true);
		if (m_lbOpenMoney != null)
		{
			((TextElement)m_lbOpenMoney).text = zbSelf.Instance.truckHandler.unlockDiamond.ToString();
		}
		wndParent.m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		wndParent.m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_rootAward, (Action)OnDetailGet, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		m_rootAdd.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_rootLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_rootChe.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_rootAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_rootEmpty.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		truckDetail = zbSelf.Instance.truckHandler.GetTruckDetail(TruckId);
		if (m_lbOpenMoney != null)
		{
			((VisualElement)m_lbOpenMoney).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (truckDetail != null)
		{
			if (truckDetail.TransportInfo.TransportId == 0)
			{
				m_rootAdd.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				m_rootEmpty.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_rootChe.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				string text = "che_" + (int)truckDetail.TransportInfo.Quality;
				TrackEntry current = m_spineSlot.AnimationState.GetCurrent(0);
				if (current != null && current.Animation != null && text != current.Animation.Name)
				{
					m_spineSlot.AnimationState.SetAnimation(0, text, true);
				}
				int num = Math.Min(((IEnumerable<int>)truckDetail.TransportInfo.RobList).Count(), 3);
				if (num > 0)
				{
					m_spineSlot.AnimationState.SetAnimation(1, "fire" + num, true);
				}
				else
				{
					m_spineSlot.AnimationState.SetEmptyAnimation(1, 0f);
				}
				if (truckDetail.TransportInfo.ArriveTm <= zbSelf.Instance.timeHandler.GetServerTime())
				{
					((VisualElement)m_rootAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					((VisualElement)m_rootAward).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_bubble.png"));
				}
			}
		}
		else
		{
			m_rootLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_rootAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (TruckId == 2)
			{
				((VisualElement)m_lbOpenMoney).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_rootAward).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_bubbleDiamond.png"));
			}
			else
			{
				((VisualElement)m_rootAward).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_bubblevip.png"));
			}
		}
		if (zbSelf.Instance.truckHandler.HaveTruckTransport())
		{
			m_rootAdd.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/guard/guard_add.png"));
		}
		else
		{
			m_rootAdd.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/guard/guard_add1.png"));
		}
	}

	public void UpdatePerSecond()
	{
		if (truckDetail != null)
		{
			if (truckDetail.TransportInfo.TransportId > 0 && truckDetail.TransportInfo.ArriveTm <= zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_rootAward, (DisplayStyle)0);
			}
			else
			{
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_rootAward, (DisplayStyle)1);
			}
		}
	}

	public void OnDetailGet()
	{
		TruckDetail truckDetail = zbSelf.Instance.truckHandler.GetTruckDetail(TruckId);
		if (truckDetail != null)
		{
			if (truckDetail.TransportInfo.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime() <= 0)
			{
				zbWsMsgHandler.Instance.Truck_TransportComplete_Req(TruckId);
			}
		}
		else
		{
			OnTouch(null);
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		TruckDetail truckDetail = zbSelf.Instance.truckHandler.GetTruckDetail(TruckId);
		if (truckDetail != null)
		{
			if (truckDetail.TransportInfo.TransportId == 0)
			{
				if (zbSelf.Instance.truckHandler.GetTransportCombatLimit() - zbServerData.sGameData.TruckSelfInfo.TransportUsed <= 0)
				{
					zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.Truck, 2);
				}
				else if (truckDetail.TransportInfo.Quality > EQuality.EmptyValue)
				{
					zbUIMgr.Instance.ShowTruckStartWnd(TruckId);
				}
				else
				{
					zbWsMsgHandler.Instance.Truck_TransportOpen_Req(TruckId);
				}
			}
			else
			{
				m_wndParent.m_wndParent.ShowDetail(2, null, truckDetail.TransportInfo, m_rootElement, TruckId);
			}
		}
		else if (TruckId == 2)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), string.Format(zbUtil.Language("lang.11123"), zbSelf.Instance.truckHandler.unlockDiamond), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Truck_SlotUnlock_Req();
				}
			});
		}
		else
		{
			zbUIMgr.Instance.ShowTruckHelpCardWnd();
		}
	}
}
