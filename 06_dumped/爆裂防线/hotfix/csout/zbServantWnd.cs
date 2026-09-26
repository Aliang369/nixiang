using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantWnd : zbTabWnd
{
	private SkeletonAnimation m_spineLevelUp;

	private VisualElement m_iconSkillType;

	private Label m_lbName;

	private Button m_btnLock;

	private Label m_lbLockState;

	private Button m_btnSet;

	private Label m_lbSet;

	private VisualElement m_spriteOk;

	private Label m_lbCapacity;

	private SkeletonAnimation m_servantAnimation;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	public Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_tabButtonsNotify = (VisualElement[])(object)new VisualElement[3];

	private VisualElement m_spriteTabSel;

	private VisualElement m_selectTabBg;

	private ScrollView m_scrollView;

	public bool m_needScroll = true;

	private zbServantTabUpgrade m_modulUpgrade;

	public zbServantTabModule m_modulModule;

	private zbServantTabDevelop m_modulDevelop;

	private Button m_btnClose;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Button m_btnIncubate;

	private Button m_btnSubSkillHandBook;

	public ServantDetail m_detail;

	public zbServantCsvRecord m_record;

	private Sprite m_spriteLock;

	private Sprite m_spriteUnlock;

	private ServantDetail[] m_servantList;

	protected override void OnEnable()
	{
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_servantAnimation = ((Component)m_GameObjectSceneRoot.transform.Find("servant")).GetComponent<SkeletonAnimation>();
		m_spineLevelUp = ((Component)m_GameObjectSceneRoot.transform.Find("levelup")).GetComponent<SkeletonAnimation>();
		((Component)m_spineLevelUp).gameObject.SetActive(false);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_selectTabBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		m_iconSkillType = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_skillType", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_btnLock = UQueryExtensions.Q<Button>(m_RootElement, "btn_lock", (string)null);
		m_lbLockState = UQueryExtensions.Q<Label>(m_RootElement, "lb_lockState", (string)null);
		m_btnSet = UQueryExtensions.Q<Button>(m_RootElement, "btn_set", (string)null);
		m_lbSet = UQueryExtensions.Q<Label>(m_RootElement, "lb_set", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_lbCapacity = UQueryExtensions.Q<Label>(m_RootElement, "lb_capacity", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_btnIncubate = UQueryExtensions.Q<Button>(m_RootElement, "btn_incubate", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnIncubate, (Action)OnIncubate, "", (TrickleDown)0);
		m_btnSubSkillHandBook = UQueryExtensions.Q<Button>(m_RootElement, "btn_subSkillHandBook", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubSkillHandBook, (Action)OnSubSkillHandBook, "", (TrickleDown)0);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_selectTabBg, "tab_btn" + i, (string)null);
			m_tabButtonsNotify[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i], (Vector2?)new Vector2(0f, 0f), 1f, true);
		}
		m_spriteLock = zbAssetLoader.Instance.LoadSpriteAsset("ui/image/button/btn_lock.png");
		m_spriteUnlock = zbAssetLoader.Instance.LoadSpriteAsset("ui/image/button/btn_unLock.png");
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLock, (Action)OnLock, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSet, (Action)OnSet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Servant, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ServantUpgrade, OnEventUpdatePlaySpine);
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
	}

	public void RefreshCustom(ServantDetail detail = null, bool isFirstIn = true)
	{
		ServantInfo servantInfo = zbServerData.sGameData.ServantInfo;
		if (servantInfo?.ServantList != null && servantInfo.ServantList.Count > 0)
		{
			m_servantList = (from s in (IEnumerable<ServantDetail>)servantInfo.ServantList
				orderby s.Deploy == EBoolState.True descending, (int)s.Quality descending, zbDataConfig.Instance.servantCsvTable.GetRecordByKey(s.ServantTid).sortId descending
				select s).ToArray();
		}
		else
		{
			m_servantList = Array.Empty<ServantDetail>();
		}
		if (detail != null)
		{
			m_detail = detail;
		}
		else if (m_detail == null && m_servantList.Length != 0)
		{
			m_detail = m_servantList[0];
		}
		if (m_detail != null)
		{
			m_record = zbDataConfig.Instance.servantCsvTable.GetRecordByKey(m_detail.ServantTid);
			RefreshServant();
			RefreshNodeDetail();
			RefreshListView(isFirstIn);
			DoSelectTab();
		}
	}

	private void RefreshServant()
	{
		if (m_detail != null)
		{
			zbServantCsvRecord servantRecord = zbSelf.Instance.servantHandler.GetServantRecord(m_detail.ServantTid);
			string spinePath = zbDataConfig.Instance.servantAvatarCsvTable.GetRecordByKey(servantRecord.avatarId).frontBone + "_SkeletonData.asset";
			zbUtil.RefreshSpine(m_servantAnimation, spinePath, "idle", loop: true);
		}
	}

	private void RefreshNodeDetail()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		string skillTypeIconPathByType = zbResourceHelper.GetSkillTypeIconPathByType((ESkillType)m_record.skillType);
		m_iconSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPathByType));
		Color val = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(m_detail.Quality));
		((VisualElement)m_lbName).style.color = StyleColor.op_Implicit(val);
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		bool flag = m_detail.Lock == EBoolState.True;
		((VisualElement)m_btnLock).style.backgroundImage = new StyleBackground(flag ? m_spriteLock : m_spriteUnlock);
		bool flag2 = m_detail.Deploy == EBoolState.True;
		((TextElement)m_lbSet).text = zbUtil.Language(flag2 ? "lang.10825" : "lang.10826");
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		string arg = $"{zbServerData.sGameData.ServantInfo.ServantList.Count}/{zbServerData.sGameData.ServantInfo.BagLimit}";
		((TextElement)m_lbCapacity).text = string.Format(zbUtil.Language("lang.11047"), arg);
	}

	private void RefreshListView(bool isFisrtIn)
	{
		((VisualElement)m_scrollView).Clear();
		int num = -1;
		for (int i = 0; i < m_servantList.Length; i++)
		{
			zbServantItem zbServantItem2 = new zbServantItem();
			zbServantItem2.OnEnable(this);
			zbServantItem2.Refresh(m_servantList[i]);
			if (m_detail.ServantId == m_servantList[i].ServantId)
			{
				num = i;
			}
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbServantItem2);
		}
		if (num >= 0 && (m_needScroll | isFisrtIn))
		{
			int indexToScroll = num;
			((VisualElement)m_scrollView).schedule.Execute((Action)delegate
			{
				zbUtil.ScrollHorizontalToIndex(m_scrollView, 4, ((VisualElement)m_scrollView).childCount, 156f, indexToScroll);
			}).ExecuteLater(10L);
		}
	}

	public override void DoSelectTab()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		switch (m_currTab)
		{
		case 0:
			if (m_modulUpgrade == null)
			{
				m_modulUpgrade = new zbServantTabUpgrade();
				m_modulUpgrade.OnEnable(this, m_tabLayers[m_currTab]);
			}
			num = 0f;
			m_modulUpgrade.Refresh();
			break;
		case 1:
			if (m_modulModule == null)
			{
				m_modulModule = new zbServantTabModule();
				m_modulModule.OnEnable(this, m_tabLayers[m_currTab]);
			}
			num = 240f;
			m_modulModule.Refresh();
			break;
		case 2:
			if (m_modulDevelop == null)
			{
				m_modulDevelop = new zbServantTabDevelop();
				m_modulDevelop.OnEnable(this, m_tabLayers[m_currTab]);
			}
			num = 480f;
			m_modulDevelop.Refresh();
			break;
		}
		m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(num, (LengthUnit)0));
		RefreshNotifys();
	}

	private void RefreshNotifys()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		m_tabButtonsNotify[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 0 || !zbSelf.Instance.servantHandler.CanServantUpgrade(m_detail)) ? 1 : 0));
		m_tabButtonsNotify[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 2 || (!zbSelf.Instance.servantHandler.CanServantUpgradeAttr(m_detail) && !zbSelf.Instance.servantHandler.CanServantUpgradeMainSkill(m_detail) && !zbSelf.Instance.servantHandler.CanServantUpgradeSubSkill(m_detail))) ? 1 : 0));
	}

	private void OnLock()
	{
		if (m_detail != null)
		{
			zbWsMsgHandler.Instance.Servant_Lock_Req(m_detail.ServantId, (m_detail.Lock != EBoolState.True) ? EBoolState.True : EBoolState.False);
		}
	}

	private void OnIncubate()
	{
		zbUIMgr.Instance.ShowServantIncubate();
	}

	private void OnSubSkillHandBook()
	{
		zbUIMgr.Instance.ShowServantSubSkillHandBook();
	}

	private void OnSet()
	{
		if (m_detail != null && m_detail.Deploy != EBoolState.True)
		{
			zbWsMsgHandler.Instance.Servant_Deploy_Req(m_detail.ServantId);
		}
	}

	private void OnLeft()
	{
		int num = m_servantList.ToList().IndexOf(m_detail);
		ServantDetail detail = ((num != 0) ? m_servantList[num - 1] : m_servantList[m_servantList.Length - 1]);
		RefreshCustom(detail);
	}

	private void OnRight()
	{
		int num = m_servantList.ToList().IndexOf(m_detail);
		ServantDetail detail = ((num != m_servantList.Length - 1) ? m_servantList[num + 1] : m_servantList[0]);
		RefreshCustom(detail);
	}

	private void PlaySpineLevelUp()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		((Component)m_spineLevelUp).gameObject.SetActive(true);
		TrackEntry val = m_spineLevelUp.AnimationState.SetAnimation(0, "animation", false);
		if (val != null)
		{
			val.Complete += (TrackEntryDelegate)delegate
			{
				((Component)m_spineLevelUp).gameObject.SetActive(false);
			};
		}
	}

	private void OnEventUpdatePlaySpine(object data)
	{
		ServantDetail detail = null;
		if (m_detail != null)
		{
			detail = ((IEnumerable<ServantDetail>)zbServerData.sGameData.ServantInfo.ServantList).FirstOrDefault((ServantDetail s) => s.ServantId == m_detail.ServantId);
		}
		RefreshCustom(detail);
		PlaySpineLevelUp();
	}

	private void OnEventUpdate(object data)
	{
		ServantDetail detail = null;
		if (m_detail != null)
		{
			detail = ((IEnumerable<ServantDetail>)zbServerData.sGameData.ServantInfo.ServantList).FirstOrDefault((ServantDetail s) => s.ServantId == m_detail.ServantId);
		}
		RefreshCustom(detail);
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		ServantDetail detail = null;
		if (m_detail != null)
		{
			detail = ((IEnumerable<ServantDetail>)zbServerData.sGameData.ServantInfo.ServantList).FirstOrDefault((ServantDetail s) => s.ServantId == m_detail.ServantId);
		}
		RefreshCustom(detail);
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		ServantDetail detail = null;
		if (m_detail != null)
		{
			detail = ((IEnumerable<ServantDetail>)zbServerData.sGameData.ServantInfo.ServantList).FirstOrDefault((ServantDetail s) => s.ServantId == m_detail.ServantId);
		}
		RefreshCustom(detail);
	}

	public void OnClose()
	{
		Hide();
	}
}
