using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTimeCatcher : zbBaseWnd
{
	private VisualElement m_nodeHero;

	private Label m_lblTime;

	private SpineVisualElement m_spineBg;

	private SpineVisualElement m_spineYaogan;

	private SpineVisualElement m_spineButton;

	private SpineVisualElement m_spineZhuazi;

	private SpineVisualElement m_spineToubi;

	private SpineVisualElement m_spineCatch;

	private Bone m_zhuaziBone;

	private float m_zhuaziBoneInitialY;

	private float m_catcherRectBottomStart;

	private SpineVisualElement m_catcherhero;

	private VisualElement m_catcherRect;

	private Label m_lbName;

	private Button m_btnClose;

	private VisualElement m_btnYaogan;

	private VisualElement m_btnToubi;

	private VisualElement m_btnStart;

	private zbTimeCatcherHeroMgr m_catcherHeroMgr;

	private LimitTimeCatcherDetail m_catcherDetail;

	private Vector2 m_touchStartPos;

	private bool m_isDragging;

	private int m_yaoganDir;

	private float m_initZhuaziPosX;

	private bool m_isStart;

	private float m_zhuaziSpeed = 250f;

	private float m_zhuaziPosX;

	private int m_catcherHeroId;

	private zbItem m_targetItem;

	private VisualElement[] m_targetList = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_targetMask = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_flyItems = (VisualElement[])(object)new VisualElement[3];

	private Coroutine[] m_flyCoroutines = (Coroutine[])(object)new Coroutine[3];

	private bool[] m_targetCountChanges = new bool[3];

	private VisualElement m_biCountProgress;

	private Label m_lblbiCount;

	private VisualElement m_toubiTip;

	private Label m_lblToubiTip;

	private Label m_lblToubiDesc;

	private int m_MaxToubiCount = 20;

	private bool bFirstHalfHourToubiTip;

	protected override void Awake()
	{
		base.Awake();
		m_catcherHeroMgr = new zbTimeCatcherHeroMgr(this);
	}

	protected override void OnEnable()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootbg", (string)null);
		m_lblTime = UQueryExtensions.Q<Label>(val, "lbltime", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(val, "lblname", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(val, "exchange", (string)null);
		m_targetItem = UQueryExtensions.Q<zbItem>(val2, "itemget", (string)null);
		m_targetItem.OnEnable(this);
		for (int i = 0; i < 3; i++)
		{
			m_targetList[i] = UQueryExtensions.Q<VisualElement>(val2, $"item{i + 1}", (string)null);
			m_targetMask[i] = UQueryExtensions.Q<VisualElement>(m_targetList[i], "mask", (string)null);
			m_flyItems[i] = UQueryExtensions.Q<VisualElement>(val2, $"flyitem{i + 1}", (string)null);
			m_flyItems[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(val, "content", (string)null);
		m_nodeHero = UQueryExtensions.Q<VisualElement>(val3, "nodehero", (string)null);
		m_spineBg = UQueryExtensions.Q<SpineVisualElement>(val3, "spine_bg", (string)null);
		m_spineYaogan = UQueryExtensions.Q<SpineVisualElement>(val3, "spine_yaogan", (string)null);
		m_spineButton = UQueryExtensions.Q<SpineVisualElement>(val3, "spine_button", (string)null);
		m_spineZhuazi = UQueryExtensions.Q<SpineVisualElement>(val3, "spine_zhuazi", (string)null);
		m_spineToubi = UQueryExtensions.Q<SpineVisualElement>(val3, "spine_toubi", (string)null);
		m_lblToubiTip = UQueryExtensions.Q<Label>(val3, "lbltoubitip", (string)null);
		m_lblToubiDesc = UQueryExtensions.Q<Label>(val3, "lbltoubidesc", (string)null);
		VisualElement val4 = UQueryExtensions.Q<VisualElement>(val, "bi_info", (string)null);
		m_lblbiCount = UQueryExtensions.Q<Label>(val4, "lblcount", (string)null);
		m_toubiTip = UQueryExtensions.Q<VisualElement>(val4, "sprite_toubi", (string)null);
		m_biCountProgress = UQueryExtensions.Q<VisualElement>(val4, "progress", (string)null);
		m_catcherhero = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_spineZhuazi, "catcherhero", (string)null);
		((VisualElement)m_catcherhero).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_catcherRect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_spineZhuazi, "catcherrect", (string)null);
		((VisualElement)m_spineZhuazi).transform.position = new Vector3(m_initZhuaziPosX, 0f, 0f);
		m_zhuaziPosX = m_initZhuaziPosX;
		m_spineCatch = UQueryExtensions.Q<SpineVisualElement>(val, "spine_catch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btnclose", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_catcherHeroMgr.OnEnable(m_nodeHero);
		m_btnYaogan = UQueryExtensions.Q<VisualElement>(val3, "btnyaogan", (string)null);
		m_btnToubi = UQueryExtensions.Q<VisualElement>(val3, "btntoubi", (string)null);
		m_btnStart = UQueryExtensions.Q<VisualElement>(val3, "btnstart", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnStart, (Action)OnStartCatcher, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnToubi, (Action)OnToubi, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerDownEvent>(m_btnYaogan, (Action<PointerDownEvent>)OnYaoGanTouchDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerMoveEvent>(m_btnYaogan, (Action<PointerMoveEvent>)OnYaoGanTouchMove, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerUpEvent>(m_btnYaogan, (Action<PointerUpEvent>)OnYaoGanTouchUp, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.LimitTimeCatcher });
		m_gameEevent.RegisterEvent(EGameEvent.Event_GmLimitTimeCatcher_CoinAck, OnToubiAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_GmLimitTimeCatcher_GrabAck, OnCatcherGrabAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateOperate);
		InitSpineState();
	}

	public void InitSpineState()
	{
		PlaySpineAnimation(m_spineBg, "animation", loop: true);
		PlaySpineAnimation(m_spineYaogan, "animation", loop: true);
		PlaySpineAnimation(m_spineButton, "animation", loop: true);
		PlaySpineAnimation(m_spineToubi, "animation", loop: true);
		PlaySpineAnimation(m_spineZhuazi, "animation", loop: true);
	}

	public void RefreshCustom(LimitTimeCatcherDetail limitTimeCatcherDetail)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (m_spineZhuazi != null && m_spineZhuazi.skeleton != null)
		{
			m_zhuaziBone = m_spineZhuazi.skeleton.FindBone("zhuazi");
		}
		m_catcherRectBottomStart = 655f;
		m_isStart = false;
		if (m_catcherDetail != null && limitTimeCatcherDetail != null)
		{
			for (int i = 0; i < m_catcherDetail.TargetList.Count; i++)
			{
				LimitTimeCatcherTargetDetail limitTimeCatcherTargetDetail = m_catcherDetail.TargetList[i];
				LimitTimeCatcherTargetDetail limitTimeCatcherTargetDetail2 = limitTimeCatcherDetail.TargetList[i];
				m_targetCountChanges[i] = limitTimeCatcherTargetDetail2.Count > limitTimeCatcherTargetDetail.Count;
			}
		}
		else
		{
			m_targetCountChanges = new bool[3];
		}
		m_catcherDetail = limitTimeCatcherDetail;
		((VisualElement)m_catcherhero).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_catcherDetail != null)
		{
			RefreshBiInfo();
			m_catcherHeroMgr.CreatehHero(m_catcherDetail);
			RefreshTarget();
			RefreshLeftTime();
		}
	}

	private void RefreshBiInfo()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lblbiCount).text = $"x{m_catcherDetail.CoinCount}";
		m_toubiTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_catcherDetail.CoinCount > 0));
		zbUIEffect.CreateUpDownAction(m_toubiTip, 5f, 1.5f);
		((VisualElement)m_lblToubiTip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_catcherDetail.CoinCount > 0));
		((VisualElement)m_lblToubiDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_catcherDetail.CoinCount <= 0));
		m_biCountProgress.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_catcherDetail.CoinCount <= 0));
		if (m_catcherDetail.CoinCount >= m_MaxToubiCount)
		{
			m_biCountProgress.style.width = StyleLength.op_Implicit(84f);
			m_biCountProgress.style.backgroundColor = new StyleColor(Color.green);
			return;
		}
		float num = 0f;
		if (m_MaxToubiCount > 0)
		{
			num = (float)m_catcherDetail.CoinCount / (float)m_MaxToubiCount;
		}
		num = Mathf.Clamp01(num);
		m_biCountProgress.style.width = StyleLength.op_Implicit(84f * num);
		m_biCountProgress.style.backgroundColor = new StyleColor(new Color(1f, 0.8f, 0f));
	}

	private IEnumerator FlyItemToTargetCoroutine(VisualElement flyItem, VisualElement targetItem, VisualElement targetMask, float duration = 0.5f)
	{
		yield return null;
		flyItem.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		float startX = 320f;
		float startY = 420f;
		float endX = targetItem.resolvedStyle.left;
		float endY = targetItem.resolvedStyle.top;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			flyItem.style.left = StyleLength.op_Implicit(Mathf.Lerp(startX, endX, num));
			flyItem.style.top = StyleLength.op_Implicit(Mathf.Lerp(startY, endY, num));
			yield return null;
		}
		flyItem.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		flyItem.style.left = StyleLength.op_Implicit(320f);
		flyItem.style.top = StyleLength.op_Implicit(420f);
		targetMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void RefreshTarget()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_catcherDetail.TargetList.Count; i++)
		{
			if (i >= 3)
			{
				continue;
			}
			m_targetMask[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			LimitTimeCatcherTargetDetail limitTimeCatcherTargetDetail = m_catcherDetail.TargetList[i];
			if (limitTimeCatcherTargetDetail == null)
			{
				continue;
			}
			if (m_targetCountChanges[i])
			{
				if (m_flyCoroutines[i] != null)
				{
					((MonoBehaviour)this).StopCoroutine(m_flyCoroutines[i]);
					m_flyCoroutines[i] = null;
				}
				m_flyItems[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_flyItems[i].style.left = StyleLength.op_Implicit(320f);
				m_flyItems[i].style.top = StyleLength.op_Implicit(420f);
				m_flyCoroutines[i] = ((MonoBehaviour)this).StartCoroutine(FlyItemToTargetCoroutine(m_flyItems[i], m_targetList[i], m_targetMask[i]));
			}
			else
			{
				m_targetMask[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(limitTimeCatcherTargetDetail.Count > 0));
			}
		}
		AwardDetail targetAward = m_catcherDetail.AwardList[0];
		if (targetAward != null)
		{
			m_targetItem.Refresh(new zbItemData(targetAward.Type, targetAward.Tid, targetAward.Count));
			m_targetItem.SetTouchCallback(delegate
			{
				if (m_catcherDetail.AwardState != ECommonState.Yes)
				{
					zbUIMgr.Instance.ShowTip(targetAward.Type, targetAward.Tid, 0L);
				}
				else
				{
					zbWsMsgHandler.Instance.Operate_LimitTimeCatcherAward_Req(m_catcherDetail.BaseInfo.UUID);
				}
			});
		}
		m_targetItem.SetGray(m_catcherDetail.AwardState == ECommonState.No);
		m_targetItem.SetSpineVisible(m_catcherDetail.AwardState == ECommonState.Yes);
		m_targetItem.SetGettedVisible(m_catcherDetail.AwardState == ECommonState.End);
	}

	private void RefreshLeftTime()
	{
		if (m_catcherDetail != null && m_catcherDetail.BaseInfo != null)
		{
			long seconds = m_catcherDetail.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lblTime).text = zbUtil.FormatTimeHHMMSS(seconds);
		}
	}

	private void OnToubi()
	{
		if (m_isStart || m_catcherDetail == null)
		{
			return;
		}
		if (m_catcherDetail.CoinCount >= m_MaxToubiCount)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11037"));
			return;
		}
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(33, 1L))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 33L, 1L);
			return;
		}
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long endTm = m_catcherDetail.BaseInfo.EndTm;
		if (endTm > serverTime && endTm - serverTime <= 1800 && !bFirstHalfHourToubiTip)
		{
			bFirstHalfHourToubiTip = true;
			string title = zbUtil.Language("lang.10143");
			string message = string.Format(zbUtil.Language("lang.11038"), Array.Empty<object>());
			zbUIMgr.Instance.ShowMessageBox(title, message, OnClickToubiConfirm);
		}
		else
		{
			OnClickToubiConfirm(isConfirm: true);
		}
	}

	private void OnClickToubiConfirm(bool isConfirm)
	{
		if (isConfirm)
		{
			PlaySpineAnimation(m_spineToubi, "animation2", loop: false, "animation");
			zbWsMsgHandler.Instance.Operate_LimitTimeCatcherCoin_Req(m_catcherDetail.BaseInfo.UUID, 1);
		}
	}

	private void OnToubiAck(object data)
	{
		if (m_catcherDetail != null)
		{
			LimitTimeCatcherDetail timeCatcherInfoByUUID = zbSelf.Instance.operateHandler.GetTimeCatcherInfoByUUID(m_catcherDetail.BaseInfo.UUID);
			if (timeCatcherInfoByUUID != null)
			{
				RefreshCustom(timeCatcherInfoByUUID);
			}
		}
	}

	private void OnStartCatcher()
	{
		if (!m_isStart && m_catcherDetail != null)
		{
			if (m_catcherDetail != null && m_catcherDetail.CoinCount == 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10329"));
				return;
			}
			PlaySpineAnimation(m_spineButton, "animation2", loop: false, "animation");
			PlaySpineAnimation(m_spineZhuazi, "animation1");
			m_zhuaziBoneInitialY = m_zhuaziBone.WorldY;
			m_isStart = true;
		}
	}

	private void OnCatcherGrabAck(object data)
	{
		if (m_catcherDetail != null)
		{
			LimitTimeCatcherDetail timeCatcherInfoByUUID = zbSelf.Instance.operateHandler.GetTimeCatcherInfoByUUID(m_catcherDetail.BaseInfo.UUID);
			if (timeCatcherInfoByUUID != null)
			{
				RefreshCustom(timeCatcherInfoByUUID);
			}
		}
	}

	private void OnUpdateOperate(object data)
	{
		if (m_catcherDetail != null)
		{
			LimitTimeCatcherDetail timeCatcherInfoByUUID = zbSelf.Instance.operateHandler.GetTimeCatcherInfoByUUID(m_catcherDetail.BaseInfo.UUID);
			if (timeCatcherInfoByUUID != null)
			{
				RefreshCustom(timeCatcherInfoByUUID);
			}
		}
	}

	private void OnYaoGanTouchDown(PointerDownEvent evt)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		m_isDragging = true;
		m_touchStartPos = Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).position);
		PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_btnYaogan, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
		SetYaoGanState(0);
	}

	private void OnYaoGanTouchMove(PointerMoveEvent evt)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (m_isDragging && !m_isStart)
		{
			Vector2 val = Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position);
			float num = val.x - m_touchStartPos.x;
			if (Mathf.Abs(num) > 2f)
			{
				int yaoGanState = ((!(num < 0f)) ? 1 : (-1));
				SetYaoGanState(yaoGanState);
			}
			m_touchStartPos = val;
		}
	}

	private void OnYaoGanTouchUp(PointerUpEvent evt)
	{
		if (PointerCaptureHelper.HasPointerCapture((IEventHandler)(object)m_btnYaogan, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId))
		{
			SetYaoGanState(0);
			m_isDragging = false;
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_btnYaogan, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
		}
	}

	private void PlaySpineAnimation(SpineVisualElement spineElement, string animationName, bool loop = false, string nextAnimName = null, float delay = 0f)
	{
		if (spineElement != null)
		{
			AnimationState animationState = spineElement.AnimationState;
			animationState.SetAnimation(0, animationName, loop);
			if (!string.IsNullOrEmpty(nextAnimName))
			{
				animationState.AddAnimation(0, nextAnimName, true, delay);
			}
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_catcherHeroMgr != null)
		{
			m_catcherHeroMgr.Update();
		}
		UpdateZhuaZiState();
		UpdateZhuaziRectState();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void SetYaoGanState(int dir)
	{
		if (m_yaoganDir != dir)
		{
			m_yaoganDir = dir;
			switch (m_yaoganDir)
			{
			case 1:
				PlaySpineAnimation(m_spineYaogan, "animation3");
				break;
			case -1:
				PlaySpineAnimation(m_spineYaogan, "animation2");
				break;
			default:
				PlaySpineAnimation(m_spineYaogan, "animation", loop: true);
				break;
			}
		}
	}

	private void UpdateZhuaZiState()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (m_yaoganDir != 0)
		{
			m_zhuaziPosX = ((VisualElement)m_spineZhuazi).transform.position.x;
			m_zhuaziPosX += (float)m_yaoganDir * m_zhuaziSpeed * Time.unscaledDeltaTime;
			float num = -236f;
			float num2 = 250f;
			m_zhuaziPosX = Mathf.Clamp(m_zhuaziPosX, num, num2);
			((VisualElement)m_spineZhuazi).transform.position = new Vector3(m_zhuaziPosX, 0f, 0f);
		}
	}

	private void UpdateZhuaziRectState()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (m_isStart && m_zhuaziBone != null && m_catcherRect != null)
		{
			float num = m_zhuaziBone.WorldY - m_zhuaziBoneInitialY;
			m_catcherRect.style.bottom = StyleLength.op_Implicit(m_catcherRectBottomStart - num * 100f);
		}
	}

	public void OnCatcherHero(int heroId)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroId);
		if (recordByKey != null)
		{
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				m_catcherHeroId = heroId;
				zbUtil.RefreshSpineUI(m_catcherhero, recordByKey2.frontBone + "_SkeletonData.asset", "idle", loop: true);
				((VisualElement)m_catcherhero).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				PlaySpineAnimation(m_spineZhuazi, "animation2");
				((MonoBehaviour)this).StartCoroutine(OnCatcherHeroEnd());
			}
		}
	}

	public IEnumerator OnCatcherHeroEnd()
	{
		yield return (object)new WaitForSeconds(1f);
		yield return MoveZhuaziPosition(m_initZhuaziPosX);
		zbWsMsgHandler.Instance.Operate_LimitTimeCatcherGrab_Req(m_catcherDetail.BaseInfo.UUID, m_catcherHeroId);
		yield return (object)new WaitForSeconds(0.5f);
		((VisualElement)m_catcherhero).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		PlaySpineAnimation(m_spineZhuazi, "animation", loop: true);
		m_isStart = false;
		m_catcherHeroMgr.isCatcherHero = false;
	}

	private IEnumerator MoveZhuaziPosition(float targetX)
	{
		float startX = ((VisualElement)m_spineZhuazi).transform.position.x;
		float num = Mathf.Abs(targetX - startX);
		float zhuaziSpeed = m_zhuaziSpeed;
		float duration = num / zhuaziSpeed;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float num2 = Mathf.Clamp01(elapsed / duration);
			float num3 = Mathf.Lerp(startX, targetX, num2);
			Vector3 position = ((VisualElement)m_spineZhuazi).transform.position;
			((VisualElement)m_spineZhuazi).transform.position = new Vector3(num3, position.y, position.z);
			yield return null;
		}
		Vector3 position2 = ((VisualElement)m_spineZhuazi).transform.position;
		((VisualElement)m_spineZhuazi).transform.position = new Vector3(targetX, position2.y, position2.z);
	}

	public VisualElement GetCatcherRect()
	{
		return m_catcherRect;
	}

	public void PlayCatchEffect(zbTimeCatcherHero catcherHero)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = ((VisualElement)catcherHero).resolvedStyle.left + 62f - 250f;
		((VisualElement)m_spineCatch).style.left = StyleLength.op_Implicit(num);
		((VisualElement)m_spineCatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineCatch.AnimationState.SetAnimation(0, "animation", false);
	}

	private void OnClose()
	{
		if (m_catcherDetail != null && m_catcherDetail.CoinCount > 0)
		{
			string title = zbUtil.Language("lang.10143");
			string message = string.Format(zbUtil.Language("lang.11039"), Array.Empty<object>());
			zbUIMgr.Instance.ShowMessageBox(title, message, OnCloseConfirm);
		}
		else
		{
			Hide();
		}
	}

	private void OnCloseConfirm(bool isConfirm)
	{
		if (isConfirm)
		{
			Hide();
		}
	}
}
