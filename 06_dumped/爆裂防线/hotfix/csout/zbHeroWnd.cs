using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroWnd : zbTabWnd
{
	private SpriteRenderer m_spriteRoomBg;

	private SkeletonAnimation m_spineChuChang;

	private zbPlayerAvatar m_spineAvatar;

	private SkeletonAnimation m_skeletonAnimation;

	private SkeletonAnimation m_spineDoor;

	public SpriteRenderer m_spriteGeneBg;

	private Button m_btnLeft;

	private Button m_btnRight;

	private VisualElement m_nodeDetail;

	private Label m_lbName;

	private VisualElement m_spriteRarity;

	private VisualElement m_spriteSkillType;

	private VisualElement m_spriteSkill;

	private Label m_lbSkill;

	private Label m_lbDamage;

	private Button m_btnFollow;

	private Label m_lbDesc;

	private Button[] m_btnMods = (Button[])(object)new Button[6];

	private Button m_btnStar;

	private Button m_btnRarity;

	private ScrollView m_scrollView;

	private List<zbHeroItem> m_heroItems = new List<zbHeroItem>();

	private Button m_btnClose;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	public Button[] m_tabButtons = (Button[])(object)new Button[3];

	private zbHeroTabUpgrade m_moduleUpgrade;

	public zbHeroTabAwaken m_moduleAwaken;

	public zbHeroTabGene m_moduleGene;

	private VisualElement m_notifyFollow;

	private VisualElement m_notifyStar;

	private VisualElement m_notifyRarity;

	private VisualElement m_notifyUpgrade;

	private VisualElement m_notifyAwaken;

	private VisualElement m_notifyGene;

	private VisualElement m_starTag;

	private VisualElement m_newGene;

	public zbHeroCsvRecord m_record;

	public HeroDetail m_heroDetail;

	public WarRoomDetail m_warRoomDetail;

	public HomelandRoomDetail m_roomDetail;

	private List<zbHeroCsvRecord> m_allRecords;

	public int m_selectHeroTid;

	private int m_targetIndex = -1;

	public bool m_needScroll = true;

	private SkeletonAnimation m_servantAnimation;

	private Button m_btnServant;

	private Button m_btnServantBottom;

	private static readonly bool EnableHeroSpeech;

	private zbHeroSpeechController m_speechController;

	private VisualElement m_speechAnchor;

	protected override void OnEnable()
	{
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spriteRoomBg = ((Component)m_GameObjectSceneRoot.transform.Find("roomBg")).GetComponent<SpriteRenderer>();
		m_spineChuChang = ((Component)m_GameObjectSceneRoot.transform.Find("chuChang")).GetComponent<SkeletonAnimation>();
		((Component)m_spineChuChang).gameObject.SetActive(false);
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_skeletonAnimation = ((Component)m_GameObjectSceneRoot.transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_servantAnimation = ((Component)m_GameObjectSceneRoot.transform.Find("servant")).GetComponent<SkeletonAnimation>();
		m_spineDoor = ((Component)m_GameObjectSceneRoot.transform.Find("spineDoor")).GetComponent<SkeletonAnimation>();
		m_spriteGeneBg = ((Component)m_GameObjectSceneRoot.transform.Find("geneBg")).GetComponent<SpriteRenderer>();
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_nodeDetail = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_detail", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_rarity", (string)null);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_skill", (string)null);
		m_lbSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_skill_value", (string)null);
		m_lbDamage = UQueryExtensions.Q<Label>(m_RootElement, "lb_damage", (string)null);
		m_btnFollow = UQueryExtensions.Q<Button>(m_RootElement, "btn_follow", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnServant = UQueryExtensions.Q<Button>(m_RootElement, "btn_servant", (string)null);
		m_btnServantBottom = UQueryExtensions.Q<Button>(m_RootElement, "btn_servantBottom", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_btnMods[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_mod" + i, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMods[i], (Action)OnStar, "", (TrickleDown)0);
		}
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "btn_star", (string)null);
		m_btnRarity = UQueryExtensions.Q<Button>(m_RootElement, "btn_rarity", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action)OnStar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRarity, (Action)OnRarity, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFollow, (Action)OnFollow, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnServant, (Action)OnServant, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnServantBottom, (Action)OnServant, "", (TrickleDown)0);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int j = 0; j < 3; j++)
		{
			m_tabLayers[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + j, (string)null);
			m_tabButtons[j] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + j, (string)null);
		}
		m_notifyFollow = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFollow, (Vector2?)new Vector2(8f, 5f), 1f, true);
		m_notifyStar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar, (Vector2?)new Vector2(8f, 5f), 1f, true);
		m_notifyRarity = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRarity, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyUpgrade = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0]);
		m_notifyAwaken = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1]);
		m_notifyGene = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[2]);
		m_newGene = zbUtil.CreateNewTag((VisualElement)(object)m_tabButtons[2]);
		m_starTag = zbUtil.CreateHeroStarTag((VisualElement)(object)m_btnStar, (Vector2?)new Vector2(5f, -10f));
		InitTab(m_tabButtons, m_tabLayers, null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gene, OnEventUpdate);
		if (EnableHeroSpeech)
		{
			m_speechAnchor = UQueryExtensions.Q<VisualElement>(m_RootElement, "speech_anchor", (string)null);
			VisualElement layer = UQueryExtensions.Q<VisualElement>(m_RootElement, "speech_layer", (string)null);
			VisualTreeAsset template = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/formation/tb_formation_speech.uxml");
			m_speechController = new zbHeroSpeechController(layer, template, 1, (int index) => m_record, delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				Rect worldBound = m_speechAnchor.worldBound;
				return ((Rect)(ref worldBound)).center;
			});
		}
	}

	protected override void OnDisable()
	{
		m_speechController?.Clear();
		m_speechController = null;
		base.OnDisable();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_speechController?.Tick(Time.unscaledDeltaTime);
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		m_speechController?.Reset();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		m_speechController?.Reset();
	}

	public void RefreshCustom(zbHeroCsvRecord record, int tabIndex = 0, bool isFirstIn = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnRarity).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.IsHeroRarityUnlock()));
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.GeneLock))
		{
			((VisualElement)m_tabButtons[2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_tabButtons[0]).style.width = StyleLength.op_Implicit(360f);
			((VisualElement)m_tabButtons[1]).style.width = StyleLength.op_Implicit(360f);
			((VisualElement)m_tabButtons[1]).style.left = StyleLength.op_Implicit(360f);
			m_spriteTabSel.style.width = StyleLength.op_Implicit(362f);
		}
		else
		{
			((VisualElement)m_tabButtons[2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_tabButtons[0]).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_tabButtons[1]).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_tabButtons[1]).style.left = StyleLength.op_Implicit(240f);
			m_spriteTabSel.style.width = StyleLength.op_Implicit(242f);
		}
		if (record == null)
		{
			return;
		}
		m_selectHeroTid = record.id;
		if (m_record == null || m_record.id != record.id)
		{
			m_speechController?.Reset();
		}
		m_record = record;
		m_heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(m_record.id);
		if (m_heroDetail == null)
		{
			return;
		}
		zbSelf.Instance.newTagHandler.RemoveNewHero(m_selectHeroTid);
		m_warRoomDetail = zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(m_selectHeroTid);
		if (m_warRoomDetail != null)
		{
			m_roomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(m_warRoomDetail.RoomId);
			if (m_roomDetail != null)
			{
				((VisualElement)m_btnServantBottom).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Servant) ? 1 : 0));
				RefreshScrollTo(isFirstIn);
				RefreshDetail();
				RefreshNotify();
				RefreshServant();
				SelectTab(m_currTab);
				bool flag = m_heroDetail.Quality >= EQuality.Orange && m_heroDetail.Rarity < ERarityType.Ss;
				((VisualElement)m_btnRarity).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
				((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			}
		}
	}

	private void RefreshScrollTo(bool isFirstIn)
	{
		List<zbHeroCsvRecord> list = (m_allRecords = zbSelf.Instance.heroHandler.GetHeroList().roomList);
		if (list.Count != m_heroItems.Count)
		{
			m_heroItems.Clear();
			((VisualElement)m_scrollView).Clear();
			foreach (zbHeroCsvRecord item in list)
			{
				_ = item;
				zbHeroItem zbHeroItem2 = new zbHeroItem(this);
				m_heroItems.Add(zbHeroItem2);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbHeroItem2);
			}
		}
		int num = -1;
		for (int i = 0; i < m_heroItems.Count; i++)
		{
			zbHeroCsvRecord zbHeroCsvRecord2 = list[i];
			m_heroItems[i].Refresh(zbHeroCsvRecord2);
			if (m_selectHeroTid == zbHeroCsvRecord2.id)
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
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(m_roomDetail.TypeId);
		m_spriteRoomBg.sprite = zbAssetLoader.Instance.LoadSpriteAsset(recordByKey.icon);
		zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(m_heroDetail.AvatarId);
		if (recordByKey2 != null)
		{
			if (m_record.type == 1)
			{
				((Component)m_spineAvatar).gameObject.SetActive(true);
				((Component)m_skeletonAnimation).gameObject.SetActive(false);
				m_spineAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
			}
			else
			{
				((Component)m_spineAvatar).gameObject.SetActive(false);
				((Component)m_skeletonAnimation).gameObject.SetActive(true);
				string spinePath = recordByKey2.frontBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_skeletonAnimation, spinePath, "idle", loop: true);
			}
		}
		((TextElement)m_lbName).text = m_record.name;
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(m_heroDetail.Rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(m_record, m_heroDetail.Rarity);
		m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		zbSkillCsvRecord recordByKey3 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(m_record.skillId);
		if (recordByKey3 != null)
		{
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey3.icon));
			((TextElement)m_lbSkill).text = recordByKey3.name;
			zbLabelLinkUtil.BuildModLink(recordByKey3.desc, m_lbDesc);
		}
		float damage = zbSelf.Instance.heroHandler.GetDamage(m_heroDetail.Quality, m_heroDetail.Rarity, m_heroDetail.QualityStar);
		float followDamages = zbSelf.Instance.heroHandler.GetFollowDamages((ESkillType)m_record.type);
		string text = zbUtil.Language("ShangHaiXiShu") + ": " + $"{damage * 100f:0.##}%";
		if (followDamages > 0f)
		{
			text = text + "<color=#2DF421>+" + $"{followDamages * 100f:0.##}%" + "</color>";
		}
		((TextElement)m_lbDamage).text = text;
		((VisualElement)m_btnFollow).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HeroFollow) || m_record.type == 1) ? 1 : 0));
		zbUtil.SetElementGray((VisualElement)(object)m_btnFollow, !zbSelf.Instance.heroHandler.CanFollow((ESkillType)m_record.type));
		RefreshMods(m_heroDetail.Quality);
	}

	private void RefreshMods(EQuality quality)
	{
		List<int> list = new List<int> { m_record.mod1, m_record.mod2, m_record.mod3, m_record.mod4, m_record.mod5, m_record.mod6 };
		for (int i = 0; i < list.Count; i++)
		{
			if (zbDataConfig.Instance.modCsvTable.GetRecordByKey(list[i]) != null)
			{
				zbUtil.SetElementGray((VisualElement)(object)m_btnMods[i], (int)m_heroDetail.Quality < i + 2);
			}
		}
	}

	private void RefreshServant()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ServantDetail currentServantBySkillType = zbSelf.Instance.servantHandler.GetCurrentServantBySkillType((ESkillType)m_record.type);
		((VisualElement)m_btnServant).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(currentServantBySkillType == null));
		((Component)m_servantAnimation).gameObject.SetActive(currentServantBySkillType != null);
		if (currentServantBySkillType != null)
		{
			zbServantCsvRecord servantRecord = zbSelf.Instance.servantHandler.GetServantRecord(currentServantBySkillType.ServantTid);
			string spinePath = zbDataConfig.Instance.servantAvatarCsvTable.GetRecordByKey(servantRecord.avatarId).frontBone + "_SkeletonData.asset";
			zbUtil.RefreshSpine(m_servantAnimation, spinePath, "idle", loop: true);
		}
	}

	private void RefreshNotify()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		m_notifyFollow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroFollowNotify((ESkillType)m_record.type)));
		m_notifyStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id)));
		m_notifyRarity.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroRarityNotify(m_record.id)));
		m_notifyUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 0 || !zbSelf.Instance.heroHandler.HasHeroUpgradeNotify(m_record.id)) ? 1 : 0));
		m_notifyAwaken.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 1 || !zbSelf.Instance.heroHandler.HasHeroAwakenNotify(m_record.id)) ? 1 : 0));
		if (m_warRoomDetail != null)
		{
			m_notifyGene.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 2 || !zbSelf.Instance.geneHandler.HasHeroTabGeneNotify(m_warRoomDetail.SkillType)) ? 1 : 0));
		}
		m_newGene.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.newTagHandler.isNewFunc(EFuncType.GeneLock)));
		m_starTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id) || !zbSelf.Instance.heroHandler.HasHeroStarFragNotify(m_record.id)) ? 1 : 0));
	}

	public override bool IsCanOpen(int tabIndex)
	{
		return tabIndex switch
		{
			1 => !zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroAwaken), 
			2 => !zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.GeneLock), 
			_ => true, 
		};
	}

	public override void DoSelectTab()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		m_nodeDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_currTab == 2));
		((Component)m_spriteGeneBg).gameObject.SetActive(m_currTab == 2);
		switch (m_currTab)
		{
		case 0:
			if (m_moduleUpgrade == null)
			{
				m_moduleUpgrade = new zbHeroTabUpgrade();
				m_moduleUpgrade.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleUpgrade.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.Gold
			});
			break;
		case 1:
			if (m_moduleAwaken == null)
			{
				m_moduleAwaken = new zbHeroTabAwaken();
				m_moduleAwaken.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleAwaken.Refresh();
			if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.GeneLock))
			{
				m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			}
			else
			{
				m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			}
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.Gold
			});
			break;
		case 2:
			if (m_moduleGene == null)
			{
				m_moduleGene = new zbHeroTabGene();
				m_moduleGene.OnEnable(this, m_tabLayers[m_currTab]);
			}
			zbSelf.Instance.newTagHandler.SetNewFuncViewed(EFuncType.GeneLock);
			m_moduleGene.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.GeneCoin
			});
			break;
		}
		RefreshNotify();
	}

	private void OnStar()
	{
		zbUIMgr.Instance.ShowHeroStar(m_record);
	}

	private void OnRarity()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroRarity))
		{
			zbUIMgr.Instance.ShowHeroRarity(m_record.id);
		}
	}

	private void OnRight()
	{
		m_needScroll = true;
		PlayDoorAnimation(delegate
		{
			int num = m_allRecords.FindIndex((zbHeroCsvRecord r) => r.id == m_record.id);
			int count = m_allRecords.Count;
			if (num >= 0 && count > 0)
			{
				m_targetIndex = (num + 1 + count) % count;
			}
		});
	}

	private void OnLeft()
	{
		m_needScroll = true;
		PlayDoorAnimation(delegate
		{
			int num = m_allRecords.FindIndex((zbHeroCsvRecord r) => r.id == m_record.id);
			int count = m_allRecords.Count;
			if (num >= 0 && count > 0)
			{
				m_targetIndex = (num - 1 + count) % count;
			}
		});
	}

	public void PlayDoorAnimation(Action onCloseLogic, bool isLeftRight = true)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		TrackEntryDelegate val = default(TrackEntryDelegate);
		m_spineDoor.AnimationState.SetAnimation(0, "door_close", false).Complete += (TrackEntryDelegate)delegate
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_0050: Expected O, but got Unknown
			onCloseLogic?.Invoke();
			TrackEntry obj = m_spineDoor.AnimationState.AddAnimation(0, "door_open", false, 0.25f);
			TrackEntryDelegate obj2 = val;
			if (obj2 == null)
			{
				TrackEntryDelegate val2 = delegate
				{
					if (isLeftRight && m_targetIndex >= 0)
					{
						RefreshCustom(m_allRecords[m_targetIndex], m_currTab, isFirstIn: false);
						m_targetIndex = -1;
					}
				};
				TrackEntryDelegate val3 = val2;
				val = val2;
				obj2 = val3;
			}
			obj.Complete += obj2;
		};
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
				m_spineAvatar.SetAnimation(0, "show", loop: false);
				m_spineAvatar.AddAnimation(0, "idle", loop: true);
				m_skeletonAnimation.AnimationState.SetAnimation(0, "show", false);
				m_skeletonAnimation.AnimationState.AddAnimation(0, "idle", true, 0f);
				((Component)m_spineChuChang).gameObject.SetActive(false);
			};
		}
	}

	public void OnClose()
	{
		Hide();
	}

	private void OnFollow()
	{
		if (zbSelf.Instance.heroHandler.CanFollow((ESkillType)m_record.type))
		{
			zbUIMgr.Instance.ShowHeroFollow((ESkillType)m_record.type);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11060"));
		}
	}

	private void OnServant()
	{
		ServantDetail currentServantBySkillType = zbSelf.Instance.servantHandler.GetCurrentServantBySkillType((ESkillType)m_record.type);
		zbUIMgr.Instance.ShowServant(currentServantBySkillType);
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_record);
	}
}
