using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationWnd : zbBaseWnd
{
	private Button m_btnEditTouch;

	private zbFormationItem[] m_itemFormations = new zbFormationItem[14];

	private zbFormationMechaItem m_itemMecha = new zbFormationMechaItem();

	public Button m_btnUpgrade;

	private Button m_btnHeroList;

	public Button m_btnMecha;

	private VisualElement m_nodeEdit;

	private zbFormationEditPanel m_editPanel = new zbFormationEditPanel();

	private Button m_btnSuit;

	private Button m_btnGun;

	private Button m_btnDecompose;

	private VisualElement m_notifyUpgrade;

	private VisualElement m_notifyHeroList;

	private VisualElement m_notifyMecha;

	private VisualElement m_notifySuit;

	private VisualElement m_notifyGun;

	private VisualElement m_newUpgrade;

	private VisualElement m_newHeroList;

	private VisualElement m_starTagHeroList;

	private zbTopBar m_topBar;

	private zbTopLeftPlayerAvatar m_playerAvatar;

	public int m_skillType;

	public zbHeroCsvRecord m_record;

	private static readonly bool EnableHeroSpeech;

	private zbHeroSpeechController m_speechController;

	private readonly Button[] m_speechTargets = (Button[])(object)(EnableHeroSpeech ? new Button[14] : null);

	private bool m_isSpineRefreshing;

	private bool m_spineRefreshPending;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 14; i++)
		{
			m_itemFormations[i] = new zbFormationItem();
		}
	}

	protected override void OnEnable()
	{
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnEditTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_editTouch", (string)null);
		for (int i = 0; i < 14; i++)
		{
			m_itemFormations[i] = UQueryExtensions.Q<zbFormationItem>(m_RootElement, "item" + (i + 1), (string)null);
			m_itemFormations[i].OnEnable(this, i + 1);
			if (EnableHeroSpeech)
			{
				m_speechTargets[i] = UQueryExtensions.Q<Button>((VisualElement)(object)m_itemFormations[i], "btn_touch", (string)null);
			}
		}
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_mecha", (string)null);
		m_itemMecha.OnEnable(this, root);
		m_nodeEdit = UQueryExtensions.Q<VisualElement>(m_RootElement, "edit", (string)null);
		m_editPanel.OnEnable(this, m_nodeEdit);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_btnHeroList = UQueryExtensions.Q<Button>(m_RootElement, "btn_heroList", (string)null);
		m_btnMecha = UQueryExtensions.Q<Button>(m_RootElement, "btn_mecha", (string)null);
		m_btnSuit = UQueryExtensions.Q<Button>(m_RootElement, "btn_suit", (string)null);
		m_btnGun = UQueryExtensions.Q<Button>(m_RootElement, "btn_gun", (string)null);
		m_btnDecompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_decompose", (string)null);
		m_topBar = new zbTopBar();
		m_topBar.OnEnable(this);
		m_playerAvatar = new zbTopLeftPlayerAvatar();
		m_playerAvatar.OnEnable(this);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEditTouch, (Action)OnEditTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHeroList, (Action)OnHeroList, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMecha, (Action)OnMecha, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSuit, (Action<ClickEvent>)OnSuit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGun, (Action<ClickEvent>)OnGun, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompose, (Action<ClickEvent>)OnDecompose, "", (TrickleDown)0);
		m_notifyUpgrade = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(20f, 15f), 1f, true);
		m_notifyHeroList = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnHeroList, (Vector2?)new Vector2(20f, 15f), 1f, true);
		m_notifyMecha = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnMecha, (Vector2?)new Vector2(20f, 15f), 1f, true);
		m_notifySuit = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnSuit, (Vector2?)new Vector2(0f, 7f), 1f, true);
		m_notifyGun = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGun, (Vector2?)new Vector2(0f, 7f), 1f, true);
		m_newUpgrade = zbUtil.CreateNewTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(8f, 8f), 0.9f);
		m_newHeroList = zbUtil.CreateNewTag((VisualElement)(object)m_btnHeroList, (Vector2?)new Vector2(8f, 8f), 0.9f);
		m_starTagHeroList = zbUtil.CreateHeroStarTag((VisualElement)(object)m_btnHeroList, (Vector2?)new Vector2(10f, 5f));
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_HeroFrag, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MechaInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleBaseInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleCommonInfo, OnEventUpdate);
		SetEditPanelVisible(vis: false);
		CreateMainBottom(1);
		if (EnableHeroSpeech)
		{
			VisualElement layer = UQueryExtensions.Q<VisualElement>(m_RootElement, "speech_layer", (string)null);
			VisualTreeAsset template = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/formation/tb_formation_speech.uxml");
			m_speechController = new zbHeroSpeechController(layer, template, m_itemFormations.Length, GetSpeechHero, GetSpeechHeadPosition);
		}
	}

	private zbHeroCsvRecord GetSpeechHero(int index)
	{
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)(index + 1));
		if (warRoomDetailBySkillType == null || warRoomDetailBySkillType.HeroTid <= 0)
		{
			return null;
		}
		return zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
	}

	private Vector2? GetSpeechHeadPosition(int index)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Rect worldBound = ((VisualElement)m_speechTargets[index]).worldBound;
		return new Vector2(((Rect)(ref worldBound)).center.x, ((Rect)(ref worldBound)).yMin);
	}

	protected override void OnDisable()
	{
		m_speechController?.Clear();
		m_speechController = null;
		if (m_speechTargets != null)
		{
			Array.Clear(m_speechTargets, 0, m_speechTargets.Length);
		}
		base.OnDisable();
	}

	protected override void UpdateAtShow()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		base.UpdateAtShow();
		if (m_speechController != null && !m_isSpineRefreshing && (int)m_nodeEdit.resolvedStyle.display == 1)
		{
			m_speechController?.Tick(Time.unscaledDeltaTime);
		}
	}

	public override void Refresh()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha))
		{
			zbWsMsgHandler.Instance.Mecha_Data_Req();
		}
		SetEditPanelVisible(vis: false);
		m_topBar.Refresh();
		m_playerAvatar.Refresh();
		RefreshDetail();
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_HeroAwaken);
	}

	private void RefreshDetail()
	{
		for (int i = 0; i < 14; i++)
		{
			m_itemFormations[i].RefreshWithoutSpine();
		}
		m_itemMecha.RefreshWithoutSpine();
		m_editPanel.Refresh();
		RefreshNotify();
		if (!m_isSpineRefreshing)
		{
			((MonoBehaviour)this).StartCoroutine(RefreshDetailSpineCoroutine());
		}
		else
		{
			m_spineRefreshPending = true;
		}
	}

	private IEnumerator RefreshDetailSpineCoroutine()
	{
		m_isSpineRefreshing = true;
		try
		{
			for (int i = 0; i < 14; i += 3)
			{
				int num = Mathf.Min(i + 3, 14);
				for (int j = i; j < num; j++)
				{
					m_itemFormations[j].RefreshSpinePart();
				}
				yield return null;
			}
			m_itemMecha.RefreshSpinePart();
			yield return null;
		}
		finally
		{
			m_isSpineRefreshing = false;
		}
		if (m_spineRefreshPending)
		{
			m_spineRefreshPending = false;
			RefreshDetail();
		}
	}

	public void RefreshMechaDetail()
	{
		m_itemMecha.RefreshWithoutSpine();
		if (!m_isSpineRefreshing)
		{
			((MonoBehaviour)this).StartCoroutine(RefreshMechaSpineCoroutine());
		}
		else
		{
			m_spineRefreshPending = true;
		}
	}

	private IEnumerator RefreshMechaSpineCoroutine()
	{
		m_isSpineRefreshing = true;
		try
		{
			yield return null;
			m_itemMecha.RefreshSpinePart();
		}
		finally
		{
			m_isSpineRefreshing = false;
		}
		if (m_spineRefreshPending)
		{
			m_spineRefreshPending = false;
			RefreshDetail();
		}
	}

	public void RefreshSelectList(int skillType)
	{
		m_skillType = skillType;
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)m_skillType);
		if (warRoomDetailBySkillType != null && warRoomDetailBySkillType.HeroTid > 0)
		{
			m_record = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
		}
		for (int i = 0; i < 14; i++)
		{
			m_itemFormations[i].RefreshSelect(skillType);
		}
		m_itemMecha.RefreshSelect(skillType);
	}

	public void SetEditPanelVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnEditTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vis ? 1 : 0));
		((VisualElement)m_btnHeroList).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vis ? 1 : 0));
		((VisualElement)m_btnMecha).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((vis || zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha)) ? 1 : 0));
		((VisualElement)m_btnSuit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vis ? 1 : 0));
		((VisualElement)m_btnGun).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vis ? 1 : 0));
		((VisualElement)m_btnDecompose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vis ? 1 : 0));
		m_nodeEdit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		if (vis)
		{
			m_speechController?.Reset();
			m_editPanel.Refresh();
		}
		else
		{
			RefreshSelectList(0);
		}
	}

	private void RefreshNotify()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		m_topBar.UpdateRedPoint();
		m_playerAvatar.UpdateRedPoint();
		m_notifyUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.heroHandler.HasUpgradeEnterNotify() == null));
		m_notifyHeroList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroListEnterNotify()));
		m_notifyMecha.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasRedPoint()));
		m_notifySuit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllSuitActiveOrUpgrade()));
		m_notifyGun.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllGunActiveOrUpgrade()));
		m_newUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.heroHandler.HasUpgradeEnterNotify() != null || !zbSelf.Instance.newTagHandler.isNewFunc(EFuncType.GeneLock)) ? 1 : 0));
		m_newHeroList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_starTagHeroList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.heroHandler.HasHeroListEnterNotify() || zbSelf.Instance.heroHandler.HasNewPoint() || !zbSelf.Instance.heroHandler.HasHeroListEnterStarNotify()) ? 1 : 0));
	}

	private void OnEditTouch()
	{
		SetEditPanelVisible(vis: false);
	}

	private void OnUpgrade()
	{
		zbHeroCsvRecord zbHeroCsvRecord2 = zbSelf.Instance.heroHandler.HasUpgradeEnterNotify();
		if (zbHeroCsvRecord2 != null)
		{
			zbUIMgr.Instance.ShowHero(zbHeroCsvRecord2);
		}
		else if (m_record == null || m_record.id == 0)
		{
			WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType(ESkillType.Qiang);
			if (warRoomDetailBySkillType != null && warRoomDetailBySkillType.HeroTid > 0)
			{
				zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
				if (recordByKey != null)
				{
					zbUIMgr.Instance.ShowHero(recordByKey);
				}
			}
		}
		else
		{
			zbUIMgr.Instance.ShowHero(m_record);
		}
	}

	private void OnHeroList()
	{
		zbUIMgr.Instance.ShowHeroList();
	}

	private void OnMecha()
	{
		zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(102);
		if (zbSelf.Instance.GetMechaId() > 0)
		{
			mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(zbSelf.Instance.GetMechaId());
		}
		zbUIMgr.Instance.ShowMecha(mechaRecord);
	}

	public void OnSuit(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowPlayerSuit();
	}

	public void OnGun(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowPlayerGun();
	}

	public void OnDecompose(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHeroDecompose();
	}

	private void OnEventUpdate(object data)
	{
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)m_skillType);
		if (warRoomDetailBySkillType != null && warRoomDetailBySkillType.HeroTid > 0)
		{
			m_record = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
		}
		RefreshDetail();
		m_topBar.Refresh();
		m_playerAvatar.Refresh();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		m_speechController?.Reset();
		RefreshNotify();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		m_speechController?.Reset();
		RefreshNotify();
	}
}
