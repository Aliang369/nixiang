using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckStartWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnRefresh;

	private Button m_btnStart;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbRefresh;

	private Label m_lbRefreshNum;

	private Label m_lbFree;

	private VisualElement m_lbRefreshMoney;

	private zbItem[] m_items = new zbItem[10];

	private VisualElement m_mainBg;

	private VisualElement m_spriteIcon;

	private SpineVisualElement m_spineTruck;

	private SpineVisualElement m_spineShaizi;

	private TruckDetail truckDetail;

	private int TruckId;

	private int b_refresh;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_mainBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_Refesh", (string)null);
		m_btnStart = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_spineTruck = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_che", (string)null);
		m_spineShaizi = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_shaizi", (string)null);
		m_spineShaizi.AnimationState.SetAnimation(0, "animation", true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStart, (Action)OnStart, "", (TrickleDown)0);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>(m_RootElement, "lb_refresh", (string)null);
		m_lbRefreshNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_refreshNum", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>(m_RootElement, "lb_free", (string)null);
		m_lbRefreshMoney = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_refreshMoney", (string)null);
		((TextElement)m_lbTitle).text = "";
		b_refresh = 0;
		for (int i = 0; i < 10; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{i}", (string)null);
			m_items[i]?.OnEnable(this);
		}
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckTransportStart, OnClose);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckTransportRefresh, OnTransportRefresh);
	}

	private void OnTransportRefresh(object data)
	{
		((MonoBehaviour)this).StartCoroutine(OnTransportEnumerator());
	}

	public IEnumerator OnTransportEnumerator()
	{
		truckDetail = zbSelf.Instance.truckHandler.GetTruckDetail(TruckId);
		m_spineShaizi.AnimationState.SetAnimation(0, "act" + (int)truckDetail.TransportInfo.Quality, false);
		yield return (object)new WaitForSeconds(17f / 30f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/open_box.ogg");
		RefreshCustom(TruckId);
	}

	public void RefreshCustom(int _TruckId)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		b_refresh = 0;
		TruckId = _TruckId;
		truckDetail = zbSelf.Instance.truckHandler.GetTruckDetail(_TruckId);
		m_spineTruck.AnimationState.SetAnimation(0, "che_" + (int)truckDetail.TransportInfo.Quality, true);
		m_mainBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_qualitybg" + (int)truckDetail.TransportInfo.Quality + ".png"));
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_icon" + (int)truckDetail.TransportInfo.Quality + ".png"));
		zbTruckQualityCsvRecord recordByKey = zbDataConfig.Instance.truckQualityCsvTable.GetRecordByKey((int)truckDetail.TransportInfo.Quality);
		((TextElement)m_lbTime).text = zbUtil.Language("lang.11122") + ":" + zbUtil.FormatTimeHHMMSS(recordByKey.arriveTime * 60);
		m_spineShaizi.AnimationState.SetAnimation(0, "act" + (int)truckDetail.TransportInfo.Quality + "_1", false);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_title" + (int)truckDetail.TransportInfo.Quality + ".png");
		UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_quality", (string)null).style.backgroundImage = new StyleBackground(val);
		if (zbSelf.Instance.truckHandler.GetRefreshDiamond(truckDetail.RefreshCount) == 0)
		{
			((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_lbRefreshMoney.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_lbRefreshMoney.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((TextElement)m_lbRefresh).text = zbSelf.Instance.truckHandler.GetRefreshDiamond(truckDetail.RefreshCount).ToString();
		if (zbServerData.sGameData.TruckSelfInfo.Privilege == EBoolState.False)
		{
			((TextElement)m_lbRefreshNum).text = (zbSelf.Instance.truckHandler.GetTransportRefreshLimit() - truckDetail.RefreshCount).ToString();
			((VisualElement)m_lbRefreshNum).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_lbRefreshNum).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		RefreshPauseAward();
	}

	public void RefreshPauseAward()
	{
		int num = 0;
		foreach (AwardDetail award in truckDetail.TransportInfo.AwardList)
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
			num++;
		}
	}

	private void OnAdd()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.Truck, 2);
	}

	protected override void UpdatePerSecond()
	{
		b_refresh--;
	}

	private void OnStart()
	{
		if (b_refresh > 0)
		{
			return;
		}
		if (zbSelf.Instance.truckHandler.GetTransportCombatLimit() - zbServerData.sGameData.TruckSelfInfo.TransportUsed > 0)
		{
			if (zbSelf.Instance.truckHandler.GetRefreshDiamond(truckDetail.RefreshCount) == 0 && truckDetail.TransportInfo.Quality <= EQuality.Green)
			{
				zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11148"), OnConfirm);
			}
			else
			{
				zbWsMsgHandler.Instance.Truck_TransportStart_Req(TruckId);
			}
		}
		else
		{
			OnAdd();
		}
	}

	private void OnConfirm(bool isOK)
	{
		if (isOK)
		{
			zbWsMsgHandler.Instance.Truck_TransportStart_Req(TruckId);
		}
	}

	private void OnRefresh()
	{
		if (b_refresh <= 0)
		{
			if (zbServerData.sGameData.TruckSelfInfo.Privilege == EBoolState.False && zbSelf.Instance.truckHandler.GetTransportRefreshLimit() - truckDetail.RefreshCount <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10160"));
				zbUIMgr.Instance.ShowTruckHelpCardWnd();
			}
			else if (truckDetail.RefreshCount != 0 && !zbSelf.Instance.moneyHandler.HasEnoughMoney(2, zbSelf.Instance.truckHandler.GetRefreshDiamond(truckDetail.RefreshCount)))
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10275"));
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, 0L);
			}
			else
			{
				b_refresh = 5;
				zbWsMsgHandler.Instance.Truck_TransportRefresh_Req(TruckId);
			}
		}
	}

	private void OnClose(object data = null)
	{
		Hide();
	}
}
