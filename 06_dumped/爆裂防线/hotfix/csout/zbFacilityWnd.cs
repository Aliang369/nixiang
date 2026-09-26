using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityWnd : zbBaseWnd
{
	public VisualElement hudRoot;

	private zbFacilityPanel m_panel = new zbFacilityPanel();

	private VisualElement m_spriteBanner;

	private Label m_lbCount;

	private Label m_lbTitle;

	private Label m_lbTime;

	private VisualElement m_nodeTarget;

	private VisualElement m_spriteProgress;

	private Label m_lbProgress;

	private VisualElement[] m_nodeTargets = (VisualElement[])(object)new VisualElement[3];

	private zbItem[] m_items = new zbItem[3];

	private Label[] m_lbTargets = (Label[])(object)new Label[3];

	private Button m_btnResult;

	private Label m_lbDesc;

	private Button m_btnProgress;

	private VisualElement m_tip;

	private VisualElement m_doubleAward;

	private VisualElement m_nodeCreater;

	private VisualElement m_spriteHead;

	private VisualElement m_spriteFrame;

	private Label m_lbLevel;

	private Label m_lbCreateName;

	private Label m_lbShareRoom;

	private Button m_btnCreater;

	private SpineVisualElement m_spineStair;

	private Button m_btnRule;

	private Button m_btnClose;

	private VisualElement m_notifyResult;

	public zbFacilityCsvRecord m_record;

	public FacilityMapInfo m_facilityMapInfo;

	private IVisualElementScheduledItem m_hideSchedule;

	protected override void OnEnable()
	{
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		hudRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "hudRoot", (string)null);
		m_panel.OnEnable(this, m_RootElement, m_GameObjectSceneRoot);
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_progress", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_nodeTargets[i] = UQueryExtensions.Q<VisualElement>(m_nodeTarget, $"target{i}", (string)null);
			m_lbTargets[i] = UQueryExtensions.Q<Label>(m_nodeTargets[i], "lb_target", (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_nodeTargets[i], "item", (string)null);
			m_items[i].OnEnable(this);
		}
		m_btnResult = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_result", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_desc", (string)null);
		m_btnProgress = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_progress", (string)null);
		m_tip = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "tip", (string)null);
		m_doubleAward = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "double_award", (string)null);
		m_nodeCreater = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_creater", (string)null);
		m_spriteHead = UQueryExtensions.Q<VisualElement>(m_nodeCreater, "head", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_nodeCreater, "frame", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_nodeCreater, "lb_level", (string)null);
		m_lbCreateName = UQueryExtensions.Q<Label>(m_nodeCreater, "lb_createrName", (string)null);
		m_lbShareRoom = UQueryExtensions.Q<Label>(m_nodeCreater, "lb_shareRoom", (string)null);
		m_btnCreater = UQueryExtensions.Q<Button>(m_nodeCreater, "btn_creater", (string)null);
		m_spineStair = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineStair", (string)null);
		((VisualElement)m_spineStair).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnResult, (Action)OnResult, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnProgress, (Action<ClickEvent>)OnShowTip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_lbShareRoom, (Action)OnShareRoom, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCreater, (Action)OnCreater, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notifyResult = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnResult, (Vector2?)new Vector2(3f, 3f), 1f, true);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.FacilityChestKey,
			EMoneyType.Tili
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Facility, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_FacilityStairTeleport_Ack, OnEventUpdateStair);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
	}

	public void RefreshCustom(bool isFirstIn = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.facilityHandler.facilityMapInfo == null)
		{
			return;
		}
		m_facilityMapInfo = zbSelf.Instance.facilityHandler.facilityMapInfo;
		m_record = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(m_facilityMapInfo.BaseInfo.FacilityType);
		((TextElement)m_lbTitle).text = m_record.name;
		m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/facility/facility_banner{m_record.type}.jpg"));
		((TextElement)m_lbCount).text = zbUtil.Language("lang.11297") + zbSelf.Instance.facilityHandler.GetCanExploreCount();
		if (m_facilityMapInfo.BaseInfo.FacilityCreator.RoleId == zbSelf.Instance.GetRoleId())
		{
			m_nodeTarget.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeCreater.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			RefreshTarget();
		}
		else
		{
			m_nodeTarget.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeCreater.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshCreater();
			if (isFirstIn && m_facilityMapInfo.BaseInfo.ExploreShare >= m_record.shareCount)
			{
				zbUIMgr.Instance.ShowFacilityMessage(zbUtil.Language("lang.10143"), zbUtil.Language("lang.11306"), needTili: false, ExitFacility, 0L);
			}
		}
		m_panel.Refresh();
		RefreshLeftTime();
	}

	private void RefreshTarget()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		m_notifyResult.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_facilityMapInfo.BaseInfo.ExploreTotal < m_record.tierTarget3) ? 1 : 0));
		((TextElement)m_lbProgress).text = m_facilityMapInfo.BaseInfo.ExploreTotal.ToString();
		((TextElement)m_lbTargets[0]).text = m_record.tierTarget1.ToString();
		((TextElement)m_lbTargets[1]).text = m_record.tierTarget2.ToString();
		((TextElement)m_lbTargets[2]).text = m_record.tierTarget3.ToString();
		m_doubleAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_facilityMapInfo.BaseInfo.DoubleAward != EBoolState.True));
		int num = ((m_facilityMapInfo.BaseInfo.DoubleAward != EBoolState.True) ? 1 : 2);
		int[][] tierAwardList = m_record.tierAwardList1;
		int num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array = tierAwardList[num2];
			m_items[0].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2] * num));
		}
		tierAwardList = m_record.tierAwardList2;
		num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array2 = tierAwardList[num2];
			m_items[1].Refresh(new zbItemData((EResourceType)array2[0], array2[1], array2[2] * num));
		}
		tierAwardList = m_record.tierAwardList3;
		num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array3 = tierAwardList[num2];
			m_items[2].Refresh(new zbItemData((EResourceType)array3[0], array3[1], array3[2] * num));
		}
		float progress = GetProgress();
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(progress * 400f, (LengthUnit)0));
	}

	private float GetProgress()
	{
		int[] array = new int[3] { m_record.tierTarget1, m_record.tierTarget2, m_record.tierTarget3 };
		float num = 1f / 3f;
		for (int i = 0; i < 3; i++)
		{
			int num2 = array[i];
			if (m_facilityMapInfo.BaseInfo.ExploreTotal < num2)
			{
				int num3 = ((i != 0) ? array[i - 1] : 0);
				float num4 = (float)(m_facilityMapInfo.BaseInfo.ExploreTotal - num3) / (float)(num2 - num3);
				return Mathf.Clamp01((float)i * num + num4 * num);
			}
		}
		return 1f;
	}

	private void RefreshCreater()
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (m_facilityMapInfo.BaseInfo.FacilityCreator != null)
		{
			zbUtil.InitPlayerInfo(m_spriteHead, m_facilityMapInfo.BaseInfo.FacilityCreator.HeadId, m_spriteFrame, m_facilityMapInfo.BaseInfo.FacilityCreator.HeadFrameId, m_lbLevel, m_facilityMapInfo.BaseInfo.FacilityCreator.Level);
			((TextElement)m_lbCreateName).text = m_facilityMapInfo.BaseInfo.FacilityCreator.RoleName;
		}
		int leftShareRoomCount = zbSelf.Instance.facilityHandler.GetLeftShareRoomCount(m_record, m_facilityMapInfo.BaseInfo);
		if (leftShareRoomCount <= 5)
		{
			((VisualElement)m_lbShareRoom).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbShareRoom).text = zbUtil.Language("lang.11296") + $"<color=#C50000>{leftShareRoomCount}</color>";
		}
		else
		{
			((VisualElement)m_lbShareRoom).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnResult()
	{
		zbUIMgr.Instance.ShowFacilityResult();
	}

	private void OnShowTip(ClickEvent evt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_hideSchedule != null)
		{
			m_hideSchedule.Pause();
			m_hideSchedule = null;
		}
		m_hideSchedule = ((VisualElement)m_btnProgress).schedule.Execute((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}).StartingIn(5000L);
	}

	private void OnShareRoom()
	{
		int leftShareRoomCount = zbSelf.Instance.facilityHandler.GetLeftShareRoomCount(m_record, m_facilityMapInfo.BaseInfo);
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11336") + " " + leftShareRoomCount);
	}

	private void OnCreater()
	{
		if (m_facilityMapInfo.BaseInfo.FacilityCreator != null)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_facilityMapInfo.BaseInfo.FacilityCreator.RoleId);
		}
	}

	private void OnRule()
	{
		string[] langKeys = new string[28]
		{
			"lang.11313", "lang.11314", "lang.11315", "lang.11316", "", "lang.11317", "lang.11318", "lang.11319", "lang.11320", "",
			"lang.11321", "lang.11322", "lang.11323", "lang.11324", "", "lang.11325", "lang.11326", "lang.11327", "lang.11328", "lang.11329",
			"lang.11330", "", "lang.11331", "lang.11332", "lang.11333", "lang.11334", "lang.11335", ""
		};
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnClose()
	{
		if (m_facilityMapInfo.BaseInfo.FacilityCreator.RoleId != zbSelf.Instance.GetRoleId())
		{
			if (zbSelf.Instance.facilityHandler.GetCanExploreCount() > 0 && m_facilityMapInfo.ExploreCount > 0)
			{
				zbUIMgr.Instance.ShowFacilityMessage(zbUtil.Language("lang.10143"), zbUtil.Language("lang.11294"), needTili: false, ExitFacility, 0L);
			}
			else
			{
				ExitFacility(isOk: true);
			}
		}
		else
		{
			HideAndResetFacilityMapInfo();
		}
	}

	private void ExitFacility(bool isOk)
	{
		if (isOk)
		{
			zbWsMsgHandler.Instance.Facility_Quit_Req(m_facilityMapInfo.BaseInfo.FacilityId);
			HideAndResetFacilityMapInfo();
		}
	}

	private void RefreshLeftTime()
	{
		if (m_facilityMapInfo != null)
		{
			long num = m_facilityMapInfo.BaseInfo.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
				return;
			}
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11280"));
			zbSelf.Instance.facilityHandler.facilityMapInfo = null;
			HideAndResetFacilityMapInfo();
		}
	}

	private void HideAndResetFacilityMapInfo()
	{
		zbSelf.Instance.facilityHandler.facilityMapInfo = null;
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnEventUpdate(object data)
	{
		if (IsShow())
		{
			RefreshCustom();
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		RefreshCustom();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		RefreshCustom();
	}

	private void OnEventUpdateStair(object data)
	{
		if (data is FacilityStairTeleportAck ack)
		{
			((MonoBehaviour)this).StartCoroutine(SpineAnimation(ack));
		}
	}

	private IEnumerator SpineAnimation(FacilityStairTeleportAck ack)
	{
		zbUIMgr.Instance.ShowModel();
		((VisualElement)m_spineStair).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineStair.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(0.5f);
		zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(ack.FacilityMapInfo);
		yield return (object)new WaitForSeconds(0.5f);
		((VisualElement)m_spineStair).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbUIMgr.Instance.HideModel();
	}
}
