using System;
using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderWnd : zbTabWnd
{
	private zbPlayerAvatar m_spineAvatar;

	private static readonly bool EnableHeroSpeech;

	private zbHeroSpeechController m_speechController;

	private VisualElement m_speechAnchor;

	private zbLeaderEquipSlotItem[] m_itemEquipSlots = new zbLeaderEquipSlotItem[3];

	private VisualElement m_spriteFv;

	private Label m_lbFv;

	private Button m_btnAttrTip;

	private Button m_btnGaozao;

	private Button m_btnPlayerSuit;

	private Button m_btnPlayerGun;

	private Button m_btnGemSuit;

	private VisualElement m_gemAttr;

	private VisualElement m_iconGemSuit;

	private Label m_lbGemAttr;

	private zbPieProgress m_gemProgress;

	private Button m_btnEquipSuit;

	private VisualElement m_equipAttr;

	private VisualElement m_iconEquipSuit;

	private Label m_lbEquipAttr;

	private zbPieProgress m_equipProgress;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	public zbLeaderTabEquip m_moduleYiti;

	public zbLeaderTabGem m_moduleJingti;

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_notifyEquipSlot;

	private VisualElement m_notifySuit;

	private VisualElement m_notifyGun;

	private zbTopLeftPlayerAvatar m_playerAvatar;

	private float playActionName;

	private VisualElement m_nodePlan;

	private Button[] m_planButtons = (Button[])(object)new Button[5];

	private EQuality m_curEquipQuality = EQuality.White;

	private EQuality m_curGemQuality = EQuality.White;

	private EQuality m_lastGemQuality = EQuality.White;

	private EQuality m_lastEquipQuality = EQuality.White;

	private bool m_isGemSpinePlaying;

	private bool m_isEquipSpinePlaying;

	private SpineVisualElement m_spineGemUp;

	private SpineVisualElement m_spineEquipUp;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i] = new zbLeaderEquipSlotItem();
		}
	}

	protected override void OnEnable()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_spriteFv = UQueryExtensions.Q<VisualElement>(m_RootElement, "power", (string)null);
		m_lbFv = UQueryExtensions.Q<Label>(m_RootElement, "lb_fv", (string)null);
		m_btnAttrTip = UQueryExtensions.Q<Button>(m_RootElement, "btn_attrTip", (string)null);
		m_btnGaozao = UQueryExtensions.Q<Button>(m_RootElement, "button_gaizao", (string)null);
		m_notifyEquipSlot = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGaozao, (Vector2?)new Vector2(3f, 3f), 1f, true);
		m_btnPlayerSuit = UQueryExtensions.Q<Button>(m_RootElement, "button_playerSuit", (string)null);
		m_btnPlayerGun = UQueryExtensions.Q<Button>(m_RootElement, "button_playerGun", (string)null);
		m_btnGemSuit = UQueryExtensions.Q<Button>(m_RootElement, "btn_gemSuit", (string)null);
		m_gemAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "gemAttr", (string)null);
		m_lbGemAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_gemAttr", (string)null);
		m_iconGemSuit = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_gemSuit", (string)null);
		m_gemProgress = UQueryExtensions.Q<zbPieProgress>(m_RootElement, "gemProgress", (string)null);
		m_btnEquipSuit = UQueryExtensions.Q<Button>(m_RootElement, "btn_equipSuit", (string)null);
		m_iconEquipSuit = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_equipSuit", (string)null);
		m_lbEquipAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_equipAttr", (string)null);
		m_equipAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "equipAttr", (string)null);
		m_equipProgress = UQueryExtensions.Q<zbPieProgress>(m_RootElement, "equipProgress", (string)null);
		m_notifySuit = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPlayerSuit, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyGun = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPlayerGun, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_playerAvatar = new zbTopLeftPlayerAvatar();
		m_playerAvatar.OnEnable(this);
		Button visualElement = UQueryExtensions.Q<Button>(m_RootElement, "button_show", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i] = UQueryExtensions.Q<zbLeaderEquipSlotItem>(m_RootElement, "equipSlot" + i, (string)null);
			m_itemEquipSlots[i].OnEnable(this, (EEquipType)(i + 1));
		}
		m_nodePlan = UQueryExtensions.Q<VisualElement>(m_RootElement, "gemPlan", (string)null);
		for (int j = 0; j < 5; j++)
		{
			int temp = j + 1;
			m_planButtons[j] = UQueryExtensions.Q<Button>(m_RootElement, "plan" + temp, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_planButtons[j], (Action<ClickEvent>)delegate
			{
				OnPlanGem(temp);
			}, "", (TrickleDown)0);
		}
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		m_tabButtons[0] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn0", (string)null);
		m_tabButtons[1] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn1", (string)null);
		m_notifys[0] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0]);
		m_notifys[1] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1]);
		m_tabLayers[0] = UQueryExtensions.Q<VisualElement>(m_RootElement, "jingti", (string)null);
		m_tabLayers[1] = UQueryExtensions.Q<VisualElement>(m_RootElement, "yiti", (string)null);
		m_spineGemUp = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "gemUpSpine", (string)null);
		m_spineEquipUp = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "equipUpSpine", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_spineGemUp, (Action)OnGemSuit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_spineEquipUp, (Action)OnEquipSuit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_spriteFv, (Action<ClickEvent>)OnAttrTip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttrTip, (Action<ClickEvent>)OnAttrTip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGaozao, (Action)OnGaozao, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayerSuit, (Action)OnPlayerSuit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayerGun, (Action)OnPlayerGun, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGemSuit, (Action)OnGemSuit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEquipSuit, (Action)OnEquipSuit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action)OnShow, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		CreateMainBottom(0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EquipSlotInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gem, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Equip, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleCommonInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Suit_Refresh, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Gun_Refresh, OnEventUpdate);
		InitializeLastQualities();
		if (EnableHeroSpeech)
		{
			m_speechAnchor = UQueryExtensions.Q<VisualElement>(m_RootElement, "speech_anchor", (string)null);
			VisualElement layer = UQueryExtensions.Q<VisualElement>(m_RootElement, "speech_layer", (string)null);
			VisualTreeAsset template = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/formation/tb_formation_speech.uxml");
			m_speechController = new zbHeroSpeechController(layer, template, 1, (int index) => zbDataConfig.Instance.heroCsvTable.GetRecordByKey(10100), delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				Rect worldBound = m_speechAnchor.worldBound;
				return ((Rect)(ref worldBound)).center;
			})
			{
				AutoPlay = false,
				TailLeft = 25f
			};
		}
	}

	protected override void OnDisable()
	{
		m_speechController?.Clear();
		m_speechController = null;
		base.OnDisable();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		m_speechController?.Clear();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		m_speechController?.Clear();
	}

	private void InitializeLastQualities()
	{
		EQuality curGemQuality = ((!zbSelf.Instance.equipHandler.HasGemSuit()) ? EQuality.White : zbSelf.Instance.equipHandler.GetGemSuitQuality());
		EQuality equipSuitQuality = zbSelf.Instance.equipHandler.GetEquipSuitQuality();
		m_lastGemQuality = (m_curGemQuality = curGemQuality);
		m_lastEquipQuality = (m_curEquipQuality = equipSuitQuality);
	}

	public void RefreshCustom(int tabIndex)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		m_nodePlan.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.GemPlan) ? 1 : 0));
		m_playerAvatar.Refresh();
		m_spineAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		m_spineAvatar.SetAnimation(1, "ui", loop: true);
		((TextElement)m_lbFv).text = zbSelf.Instance.GetFightValue().ToString();
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i].Refresh();
		}
		SelectTab(tabIndex);
		m_notifyEquipSlot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.HasEquipSlotNotify()));
		m_notifys[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.HasGemNotify()));
		m_notifys[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.HasEquipNotify()));
		m_notifySuit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllSuitActiveOrUpgrade()));
		m_notifyGun.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllGunActiveOrUpgrade()));
		RefreshNewIcon();
		RefreshSuit();
		for (int j = 0; j < 5; j++)
		{
			Texture2D val = ((j + 1 == zbServerData.sGameData.EquipSlotInfo.GemPlanId) ? zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/leader/gemcast/gemPlan_1.png") : zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/leader/gemcast/gemPlan_0.png"));
			if ((Object)(object)val != (Object)null)
			{
				((VisualElement)m_planButtons[j]).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				((VisualElement)m_planButtons[j]).MarkDirtyRepaint();
			}
		}
		zbUtil.FixAllBackgrounds(m_RootElement);
	}

	private void RefreshSuit()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.equipHandler.HasGemSuit();
		EQuality eQuality = ((!flag) ? EQuality.White : zbSelf.Instance.equipHandler.GetGemSuitQuality());
		if (!m_isGemSpinePlaying && eQuality > m_curGemQuality)
		{
			((VisualElement)m_spineGemUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_gemAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((MonoBehaviour)this).StartCoroutine(StartUp(m_spineGemUp));
			m_isGemSpinePlaying = true;
		}
		else if (eQuality < m_curGemQuality)
		{
			m_gemAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_spineGemUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_isGemSpinePlaying = false;
		}
		EQuality gemSuitQuality = zbSelf.Instance.equipHandler.GetGemSuitQuality();
		zbGemQualityCsvRecord recordByKey = zbDataConfig.Instance.gemQualityCsvTable.GetRecordByKey((int)gemSuitQuality);
		float progress = zbSelf.Instance.equipHandler.RemainingGemSetActivationPercent();
		m_gemProgress.progress = progress;
		((TextElement)m_lbGemAttr).text = "+" + ((!flag) ? "0%" : (recordByKey.critDamage * 100f + "%"));
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? $"ui/image/frame/gemSuit{(int)eQuality}.png" : "ui/image/frame/gemSuit1.png");
		m_iconGemSuit.style.backgroundImage = (((Object)(object)val != (Object)null && (Object)(object)val.texture != (Object)null) ? new StyleBackground(Background.FromTexture2D(val.texture)) : default(StyleBackground));
		m_iconGemSuit.MarkDirtyRepaint();
		m_curGemQuality = eQuality;
		EQuality equipSuitQuality = zbSelf.Instance.equipHandler.GetEquipSuitQuality();
		if (!m_isEquipSpinePlaying && equipSuitQuality > m_curEquipQuality)
		{
			((VisualElement)m_spineEquipUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_equipAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((MonoBehaviour)this).StartCoroutine(StartUp(m_spineEquipUp));
			m_isEquipSpinePlaying = true;
		}
		else if (equipSuitQuality < m_curEquipQuality)
		{
			((VisualElement)m_spineEquipUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_equipAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_isEquipSpinePlaying = false;
		}
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/frame/equipSuit{(int)equipSuitQuality}.png");
		m_iconEquipSuit.style.backgroundImage = (((Object)(object)val2 != (Object)null && (Object)(object)val2.texture != (Object)null) ? new StyleBackground(Background.FromTexture2D(val2.texture)) : default(StyleBackground));
		m_iconEquipSuit.MarkDirtyRepaint();
		EQuality equipSuitQuality2 = zbSelf.Instance.equipHandler.GetEquipSuitQuality();
		zbEquipQualityCsvRecord recordByKey2 = zbDataConfig.Instance.equipQualityCsvTable.GetRecordByKey((int)equipSuitQuality2);
		float progress2 = zbSelf.Instance.equipHandler.RemainingEquipSetActivationPercent();
		m_equipProgress.progress = progress2;
		((TextElement)m_lbEquipAttr).text = "+" + recordByKey2.damagePct * 100f + "%";
		m_curEquipQuality = equipSuitQuality;
	}

	private IEnumerator StartUp(SpineVisualElement sv)
	{
		TrackEntry val = sv.AnimationState.SetAnimation(0, "in", false);
		if (val != null)
		{
			float num = val.Animation.Duration / sv.AnimationState.TimeScale;
			yield return (object)new WaitForSecondsRealtime(num);
		}
		sv.AnimationState.SetAnimation(0, "loop", true);
		yield return null;
	}

	public override void DoSelectTab()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			if (m_moduleJingti == null)
			{
				m_moduleJingti = new zbLeaderTabGem();
				m_moduleJingti.OnEnable(this, m_tabLayers[0]);
			}
			m_moduleJingti.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_moduleYiti == null)
			{
				m_moduleYiti = new zbLeaderTabEquip();
				m_moduleYiti.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleYiti.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			break;
		}
	}

	public void RefreshNewIcon()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_tabButtons[0], "icon_new", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.equipHandler.HasNewGemNotify() || zbSelf.Instance.equipHandler.HasGemNotify()) ? 1 : 0));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_tabButtons[1], "icon_new", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.equipHandler.HasNewEquipNotify() || zbSelf.Instance.equipHandler.HasEquipNotify()) ? 1 : 0));
	}

	public void OnHead(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSelf();
	}

	public void OnAttrTip(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowAttrTip();
	}

	private void OnGaozao()
	{
		zbUIMgr.Instance.ShowEquipUpgrade();
	}

	private void OnPlayerSuit()
	{
		zbUIMgr.Instance.ShowPlayerSuit();
	}

	private void OnPlayerGun()
	{
		zbUIMgr.Instance.ShowPlayerGun();
	}

	private void OnGemSuit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spineGemUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_gemAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_isGemSpinePlaying = false;
		zbUIMgr.Instance.ShowGemSuit();
		RefreshSuit();
	}

	private void OnEquipSuit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spineEquipUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_equipAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_isEquipSpinePlaying = false;
		zbUIMgr.Instance.ShowEquipSuit();
		RefreshSuit();
	}

	private void OnPlanGem(int planId)
	{
		zbWsMsgHandler.Instance.EquipSlot_GemPlan_Req(planId);
	}

	private void OnShow()
	{
		m_speechController?.TryShow(0);
		if (!(m_spineAvatar.GetAnimationName(0) == "show"))
		{
			m_spineAvatar.SetAnimation(0, "show", loop: false);
			m_spineAvatar.AddAnimation(0, "idle", loop: true);
		}
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_currTab);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_speechController?.Tick(Time.unscaledDeltaTime);
		m_playerAvatar.UpdateRedPoint();
		if (m_spineAvatar.GetAnimationName(0) == "show")
		{
			playActionName = 0f;
			return;
		}
		playActionName += Time.deltaTime;
		if (playActionName > 5f)
		{
			m_spineAvatar.SetAnimation(0, "action", loop: false);
			m_spineAvatar.AddAnimation(0, "idle", loop: true);
			playActionName = 0f;
		}
	}
}
