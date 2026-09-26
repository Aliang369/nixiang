using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWnd : zbBaseWnd
{
	private Button m_btnOther;

	private Button m_btnMember;

	private Button m_btnToShop;

	private Button m_btnToChallenge;

	private Button m_btnToDonate;

	private Button m_btnToDinner;

	private Button m_btnToApproval;

	private VisualElement m_RedDotDinner;

	private VisualElement m_RedDotDonate;

	private VisualElement m_RedDotCallenge;

	private VisualElement m_iconDetail;

	private VisualElement m_bgPick;

	public VisualElement rootLegionHeroHud;

	private Button m_btnLegionWar;

	private VisualElement m_legionWarBanner;

	private Label m_lbLegionWar;

	private Label m_lbLegionWarState;

	private VisualElement m_legionWarTime;

	private Label m_lbLegionWarLeftTime;

	private VisualElement legionWarFinger;

	private Label m_lbName;

	private Label m_lbMember;

	private Label m_lbMemberVal;

	private Label m_lbLvVal;

	private Button m_btnFlag;

	private List<Button> m_btns;

	private VisualElement m_iconArrow;

	private bool m_isLegionWarOpen;

	private IVisualElementScheduledItem m_arrowAnim;

	private float m_arrowAnimTime;

	private ELegionWarState lastState;

	protected override void OnEnable()
	{
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		rootLegionHeroHud = UQueryExtensions.Q<VisualElement>(m_RootElement, "legionherohud", (string)null);
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		m_btnOther = UQueryExtensions.Q<Button>(m_RootElement, "btn_other", (string)null);
		m_btnMember = UQueryExtensions.Q<Button>(m_RootElement, "btn_member", (string)null);
		m_btnToShop = UQueryExtensions.Q<Button>(m_RootElement, "btn_shop", (string)null);
		m_btnToChallenge = UQueryExtensions.Q<Button>(m_RootElement, "btn_challenge", (string)null);
		m_btnToDonate = UQueryExtensions.Q<Button>(m_RootElement, "btn_donate", (string)null);
		m_btnToDinner = UQueryExtensions.Q<Button>(m_RootElement, "btn_dinner", (string)null);
		m_btnToApproval = UQueryExtensions.Q<Button>(m_RootElement, "btn_approval", (string)null);
		m_btnFlag = UQueryExtensions.Q<Button>(m_RootElement, "btn_flag", (string)null);
		m_btns = new List<Button> { m_btnMember, m_btnOther, m_btnToApproval };
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbMember = UQueryExtensions.Q<Label>(m_RootElement, "lb_member", (string)null);
		m_lbMemberVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_memberVal", (string)null);
		m_lbLvVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvVal", (string)null);
		m_iconDetail = (VisualElement)(object)UQueryExtensions.Q<Button>(m_RootElement, "icon_detail", (string)null);
		m_btnLegionWar = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWar", (string)null);
		m_lbLegionWar = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnLegionWar, "lb_legionWar", (string)null);
		m_lbLegionWarState = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnLegionWar, "lb_legionWarState", (string)null);
		m_legionWarTime = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnLegionWar, "legionWarTime", (string)null);
		m_legionWarBanner = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnLegionWar, "legionWarBanner", (string)null);
		m_lbLegionWarLeftTime = UQueryExtensions.Q<Label>(m_legionWarTime, "lb_legionWarLeftTime", (string)null);
		legionWarFinger = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnLegionWar, "legionWarFinger", (string)null);
		m_iconArrow = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_arrow", (string)null);
		m_RedDotDinner = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnToDinner, "red_dot", (string)null);
		m_RedDotDonate = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnToDonate, "red_dot", (string)null);
		m_RedDotCallenge = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnToChallenge, "red_dot", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgPick, (Action<ClickEvent>)OnLegionHeroView, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMember, (Action<ClickEvent>)OnClickMember, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOther, (Action<ClickEvent>)OnClickOther, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnToDonate, (Action<ClickEvent>)OnClickDonate, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnToDinner, (Action<ClickEvent>)OnClickDinner, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFlag, (Action<ClickEvent>)OnClickFlag, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnToApproval, (Action<ClickEvent>)OnClickApproval, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnToShop, (Action<ClickEvent>)OnClickToShop, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnToChallenge, (Action)OnClickToCallenge, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_iconDetail, (Action<ClickEvent>)OnClickDetailBtn, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWar, (Action)OnLegionWar, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Data, OnUpdateEventLegionData);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Members, OnUpdateEventLegionMember);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarData, OnUpdateEventLegionWarData);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarMapInfo, OnUpdateEventLegionWarMapInfo);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Honor
		});
		m_isLegionWarOpen = zbSelf.Instance.legionWarHandler.IsLegionWarOpen();
		((VisualElement)m_btnLegionWar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_isLegionWarOpen));
		CreateMainBottom(4);
	}

	public override void Refresh()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.legionWarHandler.NeedShowLegionWarBubble())
		{
			zbSelf.Instance.legionWarHandler.showLegionWarBubble = false;
			m_MainBottom.RefreshLegionWarBubble();
		}
		zbUIMgr.Instance.m_lastTouchTime = 99f;
		LegionBaseInfo legionBaseInfo = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo;
		int level = legionBaseInfo.Level;
		((TextElement)m_lbLvVal).text = $"Lv.{level}";
		((TextElement)m_lbName).text = legionBaseInfo.LegionName;
		((TextElement)m_lbMemberVal).text = legionBaseInfo.MemberCount + "/" + legionBaseInfo.MemberMax;
		((VisualElement)m_btnFlag).style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(legionBaseInfo.Icon));
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionWar) && zbSelf.Instance.legionWarHandler.CanReq)
		{
			zbWsMsgHandler.Instance.LegionWar_Data_Req();
		}
		RefreshLegionWar();
		UpdateRedPoint();
		ShowElementsByOfficial();
		RefreshLegionMember();
		UpdatePerSecond();
	}

	private void RefreshLegionWar()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		if (m_isLegionWarOpen)
		{
			bool flag = zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionWar);
			switch (lastState = zbSelf.Instance.legionWarHandler.GetLegionWarState())
			{
			case ELegionWarState.Preview:
				((TextElement)m_lbLegionWarState).text = zbUtil.Language("lang.11226");
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry0.png"));
				m_iconArrow.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/arrow_blue.png"));
				PlayArrowAnim(play: false);
				break;
			case ELegionWarState.SignUp:
				((TextElement)m_lbLegionWarState).text = zbUtil.Language("lang.11157");
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("fff1ae"));
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("fff1ae"));
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry1.png"));
				m_iconArrow.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/arrow_yellow.png"));
				PlayArrowAnim(play: true);
				break;
			case ELegionWarState.Prepare:
				((TextElement)m_lbLegionWarState).text = zbUtil.Language("lang.11165");
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("fff1ae"));
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("fff1ae"));
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry1.png"));
				m_iconArrow.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/arrow_yellow.png"));
				PlayArrowAnim(play: true);
				break;
			case ELegionWarState.Fighting:
				((TextElement)m_lbLegionWarState).text = zbUtil.Language("lang.11156");
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("ffd4d4"));
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("ffd4d4"));
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry2.png"));
				m_iconArrow.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/arrow_red.png"));
				PlayArrowAnim(play: true);
				break;
			case ELegionWarState.Settling:
				((TextElement)m_lbLegionWarState).text = ((zbServerData.sGameData.LegionWarBaseInfo != null && zbServerData.sGameData.LegionWarBaseInfo.Season == 0) ? zbUtil.Language("lang.11265") : zbUtil.Language("lang.11258"));
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry0.png"));
				m_iconArrow.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/arrow_blue.png"));
				PlayArrowAnim(play: true);
				break;
			case ELegionWarState.Finished:
				((TextElement)m_lbLegionWarState).text = ((zbServerData.sGameData.LegionWarBaseInfo != null && zbServerData.sGameData.LegionWarBaseInfo.Season == 0) ? zbUtil.Language("lang.11265") : zbUtil.Language("lang.11259"));
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry0.png"));
				m_iconArrow.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/arrow_blue.png"));
				PlayArrowAnim(play: false);
				break;
			case ELegionWarState.WaitNext:
				((TextElement)m_lbLegionWarState).text = zbUtil.Language("lang.10942");
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(zbUtil.ParseColor("71fdff"));
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry0.png"));
				m_iconArrow.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/arrow_blue.png"));
				PlayArrowAnim(play: false);
				break;
			}
			if (flag)
			{
				m_legionWarBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWarEntry3.png"));
				((VisualElement)m_lbLegionWar).style.color = StyleColor.op_Implicit(Color.gray);
				((VisualElement)m_lbLegionWarState).style.color = StyleColor.op_Implicit(Color.gray);
			}
			m_iconArrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		}
	}

	private void PlayArrowAnim(bool play)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!play)
		{
			IVisualElementScheduledItem arrowAnim = m_arrowAnim;
			if (arrowAnim != null)
			{
				arrowAnim.Pause();
			}
			m_iconArrow.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(0f)));
			return;
		}
		IVisualElementScheduledItem arrowAnim2 = m_arrowAnim;
		if (arrowAnim2 != null)
		{
			arrowAnim2.Pause();
		}
		m_arrowAnimTime = 0f;
		m_arrowAnim = m_iconArrow.schedule.Execute((Action)delegate
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			m_arrowAnimTime += 0.03f;
			float num = Mathf.Sin(m_arrowAnimTime * 2f) * 8f;
			m_iconArrow.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(num), Length.op_Implicit(0f)));
		}).Every(16L);
	}

	private void OnLegionWar()
	{
		if (zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.LegionWar) || !m_isLegionWarOpen)
		{
			return;
		}
		switch (zbSelf.Instance.legionWarHandler.GetLegionWarState())
		{
		case ELegionWarState.Preview:
			zbUIMgr.Instance.ShowLegionWarPreview();
			break;
		case ELegionWarState.SignUp:
		case ELegionWarState.Prepare:
			zbUIMgr.Instance.ShowLegionWarApply();
			break;
		case ELegionWarState.Fighting:
			if (zbServerData.sGameData.LegionWarBaseInfo.Season != zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo.Season)
			{
				zbUIMgr.Instance.ShowLegionWarEnter();
			}
			else if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionZone == 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11261"));
			}
			else
			{
				zbWsMsgHandler.Instance.LegionWar_EnterWar_Req(openLegionWar: true);
			}
			break;
		case ELegionWarState.Settling:
			zbUIMgr.Instance.BubbleInfo((zbServerData.sGameData.LegionWarBaseInfo != null && zbServerData.sGameData.LegionWarBaseInfo.Season == 0) ? zbUtil.Language("lang.11265") : zbUtil.Language("lang.11233"));
			break;
		case ELegionWarState.Finished:
		case ELegionWarState.WaitNext:
			zbWsMsgHandler.Instance.LegionWar_SettlePlayerInfo_Req(0, openWnd: true);
			break;
		}
	}

	private void RefreshLegionMember()
	{
		if (zbSelf.Instance.legionHandler.memberList == null || zbSelf.Instance.legionHandler.memberList.Count == 0)
		{
			zbWsMsgHandler.Instance.Legion_Members_Req();
		}
		else
		{
			zbLegionSceneMgr.Instance.InitHeroList();
		}
	}

	private void OnUpdateEventLegionMember(object data)
	{
		RefreshLegionMember();
	}

	private void OnClickDetailBtn(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionPrivilege();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateRedPoint();
		if (m_isLegionWarOpen)
		{
			ELegionWarState legionWarState = zbSelf.Instance.legionWarHandler.GetLegionWarState();
			if (lastState != legionWarState)
			{
				RefreshLegionWar();
			}
			lastState = legionWarState;
			long leftTimeByState = zbSelf.Instance.legionWarHandler.GetLeftTimeByState();
			((TextElement)m_lbLegionWarLeftTime).text = zbUtil.FormatTimeHHMMSS(leftTimeByState);
			if (legionWarState == ELegionWarState.Fighting && zbUIMgr.Instance.m_lastTouchTime > 20f && !zbUIMgr.Instance.IsShow(EWndId.GuideFinger) && !zbGuide.Instance.IsGuiding() && zbUIMgr.Instance.IsTopCascadWnd(EWndId.Legion))
			{
				zbUIMgr.Instance.ShowGuideFinger(legionWarFinger);
			}
		}
	}

	protected void UpdateRedPoint()
	{
		m_RedDotDinner.visible = zbSelf.Instance.legionHandler.HasDinnerRedPoint();
		m_RedDotDonate.visible = zbSelf.Instance.legionHandler.HasDonateRedPoint();
		m_RedDotCallenge.visible = zbSelf.Instance.legionHandler.HasCallengeCountRedPoint() || zbSelf.Instance.legionHandler.HasCallengeAwardRedPoint();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
	}

	private void ShowElementsByOfficial()
	{
		bool visible = zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) == ELegionOfficial.Leader || zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser;
		((VisualElement)m_btnToApproval).visible = visible;
	}

	private void OnLegionHeroView(ClickEvent evt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<ClickEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		Collider2D val2 = Physics2D.OverlapPoint(new Vector2(val.x, val.y));
		if ((Object)(object)val2 != (Object)null)
		{
			zbLegionHero component = ((Component)val2).gameObject.GetComponent<zbLegionHero>();
			if ((Object)(object)component != (Object)null)
			{
				component.OnClick();
			}
		}
	}

	private void OnClickMember(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Legion_Members_Req(bOpenLegionMembersWnd: true);
	}

	private void OnClickOther(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionOther();
	}

	private void OnClickDonate(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionDonate();
	}

	private void OnClickToShop(ClickEvent evt)
	{
		zbLinkHelper.OpenShop(2);
	}

	private void OnClickApproval(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionApply();
	}

	private void OnClickDinner(ClickEvent evt)
	{
		zbLinkHelper.OpenLegionDinner();
	}

	private void OnClickFlag(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionDetail();
	}

	private void OnUpdateEventLegionData(object data)
	{
		if (IsShow())
		{
			Refresh();
		}
	}

	private void OnUpdateEventLegionWarData(object data)
	{
		RefreshLegionWar();
	}

	private void OnUpdateEventLegionWarMapInfo(object data)
	{
		if (zbSelf.Instance.legionWarHandler.GetLegionWarState() >= ELegionWarState.Finished)
		{
			RefreshLegionWar();
		}
	}

	private void OnClickToCallenge()
	{
		zbLinkHelper.OpenLegionChallenge();
	}
}
