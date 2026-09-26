using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaWnd : zbTabWnd
{
	private SkeletonAnimation m_spineChuChang;

	private SkeletonAnimation m_spineMecha;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Button m_btnTouch;

	private Label m_lbName;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbAttr;

	private Button m_btnRule;

	private VisualElement m_nodeNotify;

	private VisualElement m_spriteNotifyOk;

	private VisualElement m_nodeSet;

	private VisualElement m_spriteOk;

	private Button m_btnReset;

	private Label m_lbDiamond;

	private Label m_lbTotalAttack;

	private ScrollView m_scrollView;

	private List<zbMechaItem> m_mechaItems = new List<zbMechaItem>();

	private Button m_btnClose;

	private VisualElement m_nodeLock;

	public zbMechaLockPanel m_lockPanel;

	private VisualElement m_nodeUnLock;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	public Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_notifyTabs = (VisualElement[])(object)new VisualElement[3];

	private zbMechaTabUpgrade m_moduleUpgrade;

	public zbMechaTabStar m_moduleStar;

	public zbMechaTabSkill m_moduleSkill;

	public zbMechaCsvRecord m_record;

	public MechaDetail m_mechaDetail;

	private zbMechaCsvRecord[] m_allRecords;

	public int m_selectTid;

	private int m_lastSelectTid;

	private int m_targetIndex = -1;

	private bool m_isReq;

	public bool m_needScroll = true;

	private VisualElement m_mechaSkillsWrapper;

	private VisualElement[] m_mechaSkills = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_btnsMechaSkills = (Button[])(object)new Button[5];

	private Button m_btnMask;

	private Label m_lbMechaSkill;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineChuChang = ((Component)m_GameObjectSceneRoot.transform.Find("chuChang")).GetComponent<SkeletonAnimation>();
		((Component)m_spineChuChang).gameObject.SetActive(false);
		m_spineMecha = ((Component)m_GameObjectSceneRoot.transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_mechaSkillsWrapper = UQueryExtensions.Q<VisualElement>(m_RootElement, "mechaSkills", (string)null);
		m_btnMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMask, (Action)OnMask, "", (TrickleDown)0);
		m_lbMechaSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_mechaSkill", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "detail", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(val, "lb_name", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(val, "star" + i, (string)null);
			m_mechaSkills[i] = UQueryExtensions.Q<VisualElement>(m_mechaSkillsWrapper, "skill" + i, (string)null);
			m_btnsMechaSkills[i] = UQueryExtensions.Q<Button>(m_mechaSkillsWrapper, "btn_skill" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsMechaSkills[i], (Action)delegate
			{
				OnMechaSkills(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_lbAttr = UQueryExtensions.Q<Label>(val, "lb_attr", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(val, "btn_rule", (string)null);
		m_lbTotalAttack = UQueryExtensions.Q<Label>(m_RootElement, "lb_totalAttack", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_nodeLock = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_lock", (string)null);
		m_lockPanel = new zbMechaLockPanel();
		m_lockPanel.OnEnable(this, m_nodeLock);
		m_nodeUnLock = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_unlock", (string)null);
		m_nodeNotify = UQueryExtensions.Q<VisualElement>(m_nodeUnLock, "node_notify", (string)null);
		m_spriteNotifyOk = UQueryExtensions.Q<VisualElement>(m_nodeNotify, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeNotify, (Action)OnNotify, "", (TrickleDown)0);
		m_nodeSet = UQueryExtensions.Q<VisualElement>(m_nodeUnLock, "node_set", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeSet, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeSet, (Action)OnSet, "", (TrickleDown)0);
		m_btnReset = UQueryExtensions.Q<Button>(m_nodeUnLock, "btn_reset", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnReset, "lb_diamond", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReset, (Action)OnReset, "", (TrickleDown)0);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_nodeUnLock, "selectTab", (string)null);
		for (int num = 0; num < 3; num++)
		{
			m_tabLayers[num] = UQueryExtensions.Q<VisualElement>(m_nodeUnLock, "panel" + num, (string)null);
			m_tabButtons[num] = UQueryExtensions.Q<Button>(m_nodeUnLock, "tab_btn" + num, (string)null);
			m_notifyTabs[num] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[num]);
		}
		InitListView();
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MechaInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
	}

	private void InitListView()
	{
		m_allRecords = zbSelf.Instance.mechaHandler.GetAllMechaRecords();
		zbMechaCsvRecord[] allRecords = m_allRecords;
		for (int i = 0; i < allRecords.Length; i++)
		{
			_ = allRecords[i];
			zbMechaItem zbMechaItem2 = new zbMechaItem(this);
			m_mechaItems.Add(zbMechaItem2);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbMechaItem2);
		}
		((TextElement)m_lbDiamond).text = "x" + zbSelf.Instance.mechaHandler.resetDiamond;
	}

	public void RefreshCustom(zbMechaCsvRecord record, bool isFirstIn = true)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.moneyHandler.HasEnoughMoney(2, zbSelf.Instance.mechaHandler.resetDiamond);
		((VisualElement)m_lbDiamond).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
		CheckShouldConvert();
		if (record != null)
		{
			m_lastSelectTid = m_selectTid;
			m_selectTid = record.id;
			m_record = record;
			m_mechaDetail = zbSelf.Instance.mechaHandler.GetMechaDetail(m_record.id);
			RefreshDetail();
			RefreshScrollTo(isFirstIn);
			if (m_mechaDetail == null)
			{
				m_nodeLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_nodeUnLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_lockPanel.Refresh();
				((TextElement)m_lbTotalAttack).text = zbUtil.Language("lang.10795") + 0;
			}
			else
			{
				m_nodeLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_nodeUnLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbTotalAttack).text = zbUtil.Language("lang.10795") + zbServerData.sGameData.MechaInfo.TotalAttack;
				RefreshNotify();
				SelectTab(m_currTab);
				AutoFontSize();
			}
		}
	}

	private void AutoFontSize()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		float num = 24f;
		((VisualElement)m_tabButtons[1]).style.fontSize = StyleLength.op_Implicit(num);
		num = ((((TextElement)m_tabButtons[1]).MeasureTextSize(((TextElement)m_tabButtons[1]).text, 9999f, (MeasureMode)0, 0f, (MeasureMode)0).x > 240f) ? 22f : 24f);
		((VisualElement)m_tabButtons[1]).style.fontSize = StyleLength.op_Implicit(num);
	}

	private void CheckShouldConvert()
	{
		List<MechaDetail> mechaShouldConvert = zbSelf.Instance.mechaHandler.GetMechaShouldConvert();
		if (mechaShouldConvert.Count > 0)
		{
			zbUIMgr.Instance.ShowMechaConvert(mechaShouldConvert.ToArray());
		}
	}

	private void RefreshScrollTo(bool isFirstIn)
	{
		int num = -1;
		for (int i = 0; i < m_mechaItems.Count; i++)
		{
			zbMechaCsvRecord zbMechaCsvRecord2 = m_allRecords[i];
			m_mechaItems[i].Refresh(zbMechaCsvRecord2);
			if (m_selectTid == zbMechaCsvRecord2.id)
			{
				num = i;
			}
		}
		if (num >= 0 && (m_needScroll | isFirstIn))
		{
			int indexToScroll = num;
			((VisualElement)m_scrollView).schedule.Execute((Action)delegate
			{
				zbUtil.ScrollHorizontalToIndex(m_scrollView, 4, ((VisualElement)m_scrollView).childCount, 156f, indexToScroll);
			}).ExecuteLater(50L);
		}
	}

	private void RefreshDetail()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			string spinePath = recordByKey.fightBone + "_SkeletonData.asset";
			zbUtil.RefreshSpine(m_spineMecha, spinePath, "idle", loop: true);
			((Component)m_spineMecha).transform.localScale = Vector3.one * m_record.scale1;
		}
		((TextElement)m_lbName).text = m_record.name;
		zbUIEffect.SetStars(m_stars, m_mechaDetail?.Star ?? 0);
		if (m_mechaDetail == null)
		{
			((TextElement)m_lbAttr).text = "";
		}
		else
		{
			((TextElement)m_lbAttr).text = zbUtil.Language("lang.10798") + "+" + m_mechaDetail.Attack;
		}
		m_nodeSet.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_mechaDetail == null));
		if (m_mechaDetail != null && zbSelf.Instance.GetMechaId() == m_record.id)
		{
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshNotify()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if (m_record.id > 0)
		{
			m_notifyTabs[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 0 || !zbSelf.Instance.mechaHandler.HasMechaUpgradeNotify(m_record.id)) ? 1 : 0));
			m_notifyTabs[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 1 || !zbSelf.Instance.mechaHandler.HasMechaStarNotify(m_record.id)) ? 1 : 0));
			m_notifyTabs[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 2 || !zbSelf.Instance.mechaHandler.HasMechaSkillNotify(m_record.id)) ? 1 : 0));
			m_spriteNotifyOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.newTagHandler.IsNotNotifyMecha(m_record.id) ? 1 : 0));
		}
	}

	public override void DoSelectTab()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		if (m_currTab == 0 || m_currTab == 1)
		{
			_ = 1;
		}
		else
			_ = m_mechaDetail == null;
		switch (m_currTab)
		{
		case 0:
			if (m_moduleUpgrade == null)
			{
				m_moduleUpgrade = new zbMechaTabUpgrade();
				m_moduleUpgrade.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleUpgrade.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_moduleStar == null)
			{
				m_moduleStar = new zbMechaTabStar();
				m_moduleStar.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleStar.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			break;
		case 2:
			if (m_moduleSkill == null)
			{
				m_moduleSkill = new zbMechaTabSkill();
				m_moduleSkill.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleSkill.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			break;
		}
		RefreshNotify();
	}

	private void OnRule()
	{
		string[] langKeys = new string[6] { "lang.10847", "lang.10848", "lang.10849", "lang.10850", "lang.10851", "lang.10852" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	public void RefreshMechaSkills()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			zbMechaSkillCsvRecord recordByKey = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(m_record.skillList[i]);
			m_mechaSkills[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
	}

	private void OnMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnMechaSkills(int index)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		string text = ((index == 0) ? GetMechaPowerSkillDescByIndex(index) : (zbUtil.Language("lang.11477") + GetMechaBaseSkillDescByIndex(index) + "\n" + string.Format(zbUtil.Language("lang.11478"), index + 1) + GetMechaPowerSkillDescByIndex(index)));
		((TextElement)m_lbMechaSkill).text = text;
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private string GetMechaBaseSkillDescByIndex(int index)
	{
		int num = 10011;
		switch (index)
		{
		case 0:
			num = m_record.skillList[0];
			break;
		case 1:
			num = m_record.skillList[1];
			break;
		case 2:
			num = m_record.skillList[2];
			break;
		case 3:
			num = m_record.skillList[3];
			break;
		case 4:
			num = m_record.skillList[4];
			break;
		}
		zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(num);
		return zbSelf.Instance.mechaHandler.GetMechaSkillDesc(num);
	}

	private string GetMechaPowerSkillDescByIndex(int index)
	{
		int num = 10011;
		switch (index)
		{
		case 0:
			num = m_record.skillList[0];
			break;
		case 1:
			num = m_record.skill2;
			break;
		case 2:
			num = m_record.skill3;
			break;
		case 3:
			num = m_record.skill4;
			break;
		case 4:
			num = m_record.skill5;
			break;
		}
		zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(num);
		return zbSelf.Instance.mechaHandler.GetMechaSkillDesc(num);
	}

	private void OnNotify()
	{
		if (!zbSelf.Instance.newTagHandler.IsNotNotifyMecha(m_record.id))
		{
			zbSelf.Instance.newTagHandler.AddNotNotifyMecha(m_record.id);
		}
		else
		{
			zbSelf.Instance.newTagHandler.RemoveNotNotifyMecha(m_record.id);
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MechaInfo);
	}

	private void OnSet()
	{
		if (m_mechaDetail != null && zbSelf.Instance.GetMechaId() != m_record.id)
		{
			m_isReq = true;
			zbWsMsgHandler.Instance.Mecha_Set_Req(m_record.id);
		}
	}

	private void OnReset()
	{
		if (!zbSelf.Instance.mechaHandler.CanMechaReset(m_record.id))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10954"));
		}
		else
		{
			zbWsMsgHandler.Instance.Mecha_ResetCalc_Req(m_record.id);
		}
	}

	private void OnRight()
	{
		m_needScroll = true;
		int num = Array.FindIndex(m_allRecords, (zbMechaCsvRecord r) => r.id == m_record.id);
		int num2 = m_allRecords.Length;
		if (num >= 0 && num2 > 0)
		{
			m_targetIndex = (num + 1 + num2) % num2;
		}
		if (m_targetIndex >= 0)
		{
			RefreshCustom(m_allRecords[m_targetIndex], isFirstIn: false);
			m_targetIndex = -1;
		}
	}

	private void OnLeft()
	{
		m_needScroll = true;
		int num = Array.FindIndex(m_allRecords, (zbMechaCsvRecord r) => r.id == m_record.id);
		int num2 = m_allRecords.Length;
		if (num >= 0 && num2 > 0)
		{
			m_targetIndex = (num - 1 + num2) % num2;
		}
		if (m_targetIndex >= 0)
		{
			RefreshCustom(m_allRecords[m_targetIndex], isFirstIn: false);
			m_targetIndex = -1;
		}
	}

	private void OnTouch()
	{
		TrackEntry current = m_spineMecha.AnimationState.GetCurrent(0);
		if (current != null && current.Animation.Name != "win")
		{
			m_spineMecha.AnimationState.SetAnimation(0, "win", false);
		}
		m_spineMecha.AnimationState.AddAnimation(0, "idle", true, 0f);
	}

	public void ActionChuchangAnim()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		((Component)m_spineChuChang).gameObject.SetActive(true);
		TrackEntry val = m_spineChuChang.AnimationState.SetAnimation(0, "animation", false);
		if (val != null)
		{
			val.Complete += (TrackEntryDelegate)delegate
			{
				OnTouch();
				((Component)m_spineChuChang).gameObject.SetActive(false);
			};
		}
	}

	public void OnClose()
	{
		OnMask();
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_record);
		if (m_isReq)
		{
			ActionChuchangAnim();
			m_isReq = false;
		}
	}
}
