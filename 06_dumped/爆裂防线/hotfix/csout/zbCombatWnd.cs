using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbCombatWnd : zbBaseWnd
{
	public zbCombatData combatData;

	private VisualElement m_eleTopUI;

	private bool m_topUiSkillVisible = true;

	private float maxWidth = 690f;

	private Label m_lbBarExp;

	private Button m_btnPause;

	public Button m_btnSpeed;

	private Button m_btnData;

	public Button m_btnFocus;

	private Label m_lbLevelName;

	private Label m_lbCountdown;

	private VisualElement m_gunObject;

	private Label m_lbGunLevel;

	private Label m_lbGunAmmo;

	private Button m_btnGun;

	private zbRectProgress m_gunProgress;

	private zbCombatDamagePanel m_dmgPanel;

	private zbCombatDamageHelp m_dmgHelp;

	private VisualElement m_panel;

	private VisualElement m_pvpVS;

	private Label m_lbPVPP1KillCount;

	private Label m_lbPVPP2KillCount;

	public VisualElement m_killCount;

	private VisualElement m_iconZombie;

	private Label m_lbKillCount;

	private Label m_lbKillCountAwardCount;

	private VisualElement m_killCountProgress;

	private Button m_btnKillCountAward;

	private Button m_awardMask;

	private VisualElement m_awards;

	private VisualElement m_roundNum;

	private Label m_lbRoundNum;

	private Button m_btnDef;

	private VisualElement m_DefMask;

	private VisualElement m_leftTime;

	private Label m_lbLeftTime;

	private VisualElement[] m_nodeDetail = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgress = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgress = new zbPieProgress[5];

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevel = (Label[])(object)new Label[5];

	private Button[] m_btnSkill = (Button[])(object)new Button[5];

	private Label m_lbImmune;

	private Label m_lbCurrHp;

	private Label m_lbCurrShield;

	private Button m_btnDefEnemy;

	private Label m_lbImmuneEnemy;

	private Label m_lbCurrHpEnemy;

	private Label m_lbCurrShieldEnemy;

	private GameObject bossCome;

	private VisualElement m_barExpPro;

	private SpineVisualElement spineLevelProgress;

	private VisualElement m_barJijia;

	private SpineVisualElement spineGunWuxian;

	private GameObject m_LoadBg1;

	private GameObject m_Load1;

	private SpriteRenderer m_Load1Renderer1;

	private GameObject m_LoadBg2;

	private GameObject m_Load2;

	private SpriteRenderer m_Load1Renderer2;

	private SpineVisualElement spineGunWuxianEnemy;

	private GameObject m_LoadBg1Enemy;

	private GameObject m_Load1Enemy;

	private SpriteRenderer m_Load1Renderer1Enemy;

	private GameObject m_LoadBg2Enemy;

	private GameObject m_Load2Enemy;

	private SpriteRenderer m_Load1Renderer2Enemy;

	private SpineVisualElement spineGunWuxianSelf;

	private Sprite m_iconZombies;

	private Sprite m_iconHurt;

	private float worldBoundFrame;

	private bool worldBoundFlag;

	private bool openEndWnd;

	private bool m_battleNotReadyLogged;

	private Button m_btnTouch;

	private Button m_btnMecha;

	private VisualElement m_mechaIcon;

	private SpineVisualElement spineMechaProgress;

	private Button m_btnMecha2;

	private VisualElement m_mechaIcon2;

	private SpineVisualElement spineMechaProgress2;

	private bool isIconAnimationPlaying;

	public zbCombatWnd_EndlessHorde m_endlessHordePanel;

	private zbCombatWnd_Legendary m_legendaryPanel;

	private zbCombatWnd_Speed m_speedPanel;

	private zbCombatCamera renderTarget1;

	private zbCombatCamera renderTarget2;

	private VisualElement m_camera1;

	private Image m_descImg1;

	private VisualElement m_camera2;

	private Image m_descImg2;

	private SpriteRenderer m_pvpBg;

	private VisualElement m_wallSkill;

	private VisualElement[] m_wallSkills = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_wallIcons = (VisualElement[])(object)new VisualElement[3];

	private VisualElement m_pvpUI;

	private VisualElement m_gunObjectEnemy;

	private Label m_lbGunLevelEnemy;

	private Label m_lbGunAmmoEnemy;

	private Button m_btnGunEnemy;

	private zbRectProgress m_gunProgressEnemy;

	private VisualElement m_spriteFrameEnemy;

	private VisualElement m_spriteIconEnemy;

	private Label m_levelEnemy;

	private VisualElement[] m_nodeDetailEnemy = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillModEnemy = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgressEnemy = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgressEnemy = new zbPieProgress[5];

	private VisualElement[] m_skillIconEnemy = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevelEnemy = (Label[])(object)new Label[5];

	private Button[] m_btnSkillEnemy = (Button[])(object)new Button[5];

	private VisualElement m_gunObjectSelf;

	private Label m_lbGunLevelSelf;

	private Label m_lbGunAmmoSelf;

	private Button m_btnGunSelf;

	private zbRectProgress m_gunProgressSelf;

	private VisualElement m_spriteFrameSelf;

	private VisualElement m_spriteIconSelf;

	private Label m_levelSelf;

	private VisualElement[] m_nodeDetailSelf = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillModSelf = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgressSelf = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgressSelf = new zbPieProgress[5];

	private VisualElement[] m_skillIconSelf = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevelSelf = (Label[])(object)new Label[5];

	private Button[] m_btnSkillSelf = (Button[])(object)new Button[5];

	private float lastOffsetY;

	private zbPvpMonsterDie m_PvpMonsterDieEnemy;

	private zbPvpMonsterDie m_PvpMonsterDieSelf;

	private VisualElement m_startFight;

	private SpineVisualElement spineStartFight;

	private Label m_lbStartFight;

	private int currActionPoints;

	protected override void Awake()
	{
		base.Awake();
		m_endlessHordePanel = new zbCombatWnd_EndlessHorde();
		m_legendaryPanel = new zbCombatWnd_Legendary();
		m_speedPanel = new zbCombatWnd_Speed();
	}

	protected override void OnEnable()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0680: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b42: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b63: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1059: Unknown result type (might be due to invalid IL or missing references)
		//IL_1063: Expected O, but got Unknown
		//IL_1073: Unknown result type (might be due to invalid IL or missing references)
		//IL_108d: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1128: Unknown result type (might be due to invalid IL or missing references)
		//IL_1132: Expected O, but got Unknown
		//IL_1142: Unknown result type (might be due to invalid IL or missing references)
		//IL_115c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1176: Unknown result type (might be due to invalid IL or missing references)
		//IL_1190: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a6: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_endlessHordePanel.OnEnable(this, m_RootElement, GetMyGameObjectSceneRoot());
		m_legendaryPanel.OnEnable(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "legendary", (string)null));
		m_speedPanel.OnEnable(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "node_speed", (string)null));
		m_startFight = UQueryExtensions.Q<VisualElement>(m_RootElement, "startFight", (string)null);
		spineStartFight = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_start", (string)null);
		m_lbStartFight = UQueryExtensions.Q<Label>(m_RootElement, "lbStartFight", (string)null);
		m_startFight.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_camera1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "CombatDataRoot1", (string)null);
		m_camera2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "CombatDataRoot2", (string)null);
		m_pvpBg = ((Component)GetMyGameObjectSceneRoot().transform.Find("pvp_bg")).GetComponent<SpriteRenderer>();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "commUI", (string)null);
		m_eleTopUI = UQueryExtensions.Q<VisualElement>(m_RootElement, "topUI", (string)null);
		m_pvpUI = UQueryExtensions.Q<VisualElement>(m_RootElement, "pvpUI", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_pvpUI, "enemySkill", (string)null);
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(m_pvpUI, "selfSkill", (string)null);
		m_btnPause = UQueryExtensions.Q<Button>(m_RootElement, "button_pause", (string)null);
		m_btnSpeed = UQueryExtensions.Q<Button>(m_RootElement, "button_speed", (string)null);
		m_btnData = UQueryExtensions.Q<Button>(m_RootElement, "button_data", (string)null);
		m_btnFocus = UQueryExtensions.Q<Button>(m_RootElement, "button_focus", (string)null);
		m_gunObject = UQueryExtensions.Q<VisualElement>(m_eleTopUI, "gunObject", (string)null);
		m_lbLevelName = UQueryExtensions.Q<Label>(m_RootElement, "lb_levelname", (string)null);
		m_lbCountdown = UQueryExtensions.Q<Label>(m_RootElement, "lb_countdown", (string)null);
		m_lbGunLevel = UQueryExtensions.Q<Label>(m_eleTopUI, "lb_gun", (string)null);
		m_lbGunAmmo = UQueryExtensions.Q<Label>(m_eleTopUI, "lb_ammo", (string)null);
		m_gunProgress = UQueryExtensions.Q<zbRectProgress>(m_eleTopUI, (string)null, (string)null);
		m_btnGun = UQueryExtensions.Q<Button>(m_eleTopUI, "button_gun", (string)null);
		m_barExpPro = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_gunObjectEnemy = UQueryExtensions.Q<VisualElement>(val2, "gunObject", (string)null);
		m_lbGunLevelEnemy = UQueryExtensions.Q<Label>(val2, "lb_gun", (string)null);
		m_lbGunAmmoEnemy = UQueryExtensions.Q<Label>(val2, "lb_ammo", (string)null);
		m_gunProgressEnemy = UQueryExtensions.Q<zbRectProgress>(val2, (string)null, (string)null);
		m_btnGunEnemy = UQueryExtensions.Q<Button>(val2, "button_gun", (string)null);
		m_gunObjectSelf = UQueryExtensions.Q<VisualElement>(val3, "gunObject", (string)null);
		m_lbGunLevelSelf = UQueryExtensions.Q<Label>(val3, "lb_gun", (string)null);
		m_lbGunAmmoSelf = UQueryExtensions.Q<Label>(val3, "lb_ammo", (string)null);
		m_gunProgressSelf = UQueryExtensions.Q<zbRectProgress>(val3, (string)null, (string)null);
		m_btnGunSelf = UQueryExtensions.Q<Button>(val3, "button_gun", (string)null);
		m_killCount = UQueryExtensions.Q<VisualElement>(m_RootElement, "killCount", (string)null);
		m_iconZombie = UQueryExtensions.Q<VisualElement>(m_killCount, "icon_zomble", (string)null);
		m_lbKillCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_killCount", (string)null);
		m_lbKillCountAwardCount = UQueryExtensions.Q<Label>(m_killCount, "lb_count", (string)null);
		m_killCountProgress = UQueryExtensions.Q<VisualElement>(m_killCount, "killCountProgress", (string)null);
		m_btnKillCountAward = UQueryExtensions.Q<Button>(m_killCount, "btn_award", (string)null);
		m_awardMask = UQueryExtensions.Q<Button>(m_RootElement, "award_mask", (string)null);
		m_awards = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_awardMask, "awards", (string)null);
		m_roundNum = UQueryExtensions.Q<VisualElement>(m_RootElement, "roundNum", (string)null);
		m_lbRoundNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_roundNum", (string)null);
		m_leftTime = UQueryExtensions.Q<VisualElement>(m_RootElement, "leftTime", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftTime", (string)null);
		m_lbBarExp = UQueryExtensions.Q<Label>(m_RootElement, "lbg_exp", (string)null);
		m_lbImmune = UQueryExtensions.Q<Label>(val, "lbImm", (string)null);
		m_lbCurrHp = UQueryExtensions.Q<Label>(val, "lbHp", (string)null);
		m_lbCurrShield = UQueryExtensions.Q<Label>(val, "lbShield", (string)null);
		m_lbImmuneEnemy = UQueryExtensions.Q<Label>(val2, "lbImm", (string)null);
		m_lbCurrHpEnemy = UQueryExtensions.Q<Label>(val2, "lbHp", (string)null);
		m_lbCurrShieldEnemy = UQueryExtensions.Q<Label>(val2, "lbShield", (string)null);
		m_btnDefEnemy = UQueryExtensions.Q<Button>(val2, "btn_def", (string)null);
		m_btnDef = UQueryExtensions.Q<Button>(val, "btn_def", (string)null);
		m_DefMask = (VisualElement)(object)UQueryExtensions.Q<Button>(m_RootElement, "def_mask", (string)null);
		((TextElement)UQueryExtensions.Q<Label>(m_DefMask, "lb_imm", (string)null)).text = zbUtil.Language("lang.10626");
		((TextElement)UQueryExtensions.Q<Label>(m_DefMask, "lb_shield", (string)null)).text = zbUtil.Language("lang.10627");
		((TextElement)UQueryExtensions.Q<Label>(m_DefMask, "lb_hp", (string)null)).text = zbUtil.Language("lang.10628");
		OnCloseDefMask();
		m_iconZombies = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_zombies.png");
		m_iconHurt = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_hurt.png");
		m_panel = UQueryExtensions.Q<VisualElement>(m_RootElement, "dmg_panel", (string)null);
		m_pvpVS = UQueryExtensions.Q<VisualElement>(m_RootElement, "pvpVS", (string)null);
		m_lbPVPP1KillCount = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_pvpVS, "selfPvp1", (string)null), "selfKill", (string)null);
		m_lbPVPP2KillCount = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_pvpVS, "selfPvp2", (string)null), "selfKill", (string)null);
		m_PvpMonsterDieEnemy = new zbPvpMonsterDie(UQueryExtensions.Q<VisualElement>(m_pvpVS, "selfPvp1", (string)null), this);
		m_PvpMonsterDieSelf = new zbPvpMonsterDie(UQueryExtensions.Q<VisualElement>(m_pvpVS, "selfPvp2", (string)null), this);
		spineLevelProgress = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_expFx", (string)null);
		m_barJijia = UQueryExtensions.Q<VisualElement>(m_RootElement, "bar_jijia", (string)null);
		m_barJijia.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		spineGunWuxian = UQueryExtensions.Q<SpineVisualElement>(m_eleTopUI, "spine_wuxian", (string)null);
		spineGunWuxianEnemy = UQueryExtensions.Q<SpineVisualElement>(val2, "spine_wuxian", (string)null);
		spineGunWuxianSelf = UQueryExtensions.Q<SpineVisualElement>(val3, "spine_wuxian", (string)null);
		m_btnMecha = UQueryExtensions.Q<Button>(m_RootElement, "btn_mecha1", (string)null);
		m_mechaIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMecha, "mechaIcon", (string)null);
		spineMechaProgress = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnMecha, "spine_mecha", (string)null);
		m_btnMecha2 = UQueryExtensions.Q<Button>(m_RootElement, "btn_mecha2", (string)null);
		m_mechaIcon2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMecha2, "mechaIcon", (string)null);
		spineMechaProgress2 = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnMecha2, "spine_mecha", (string)null);
		m_spriteFrameEnemy = UQueryExtensions.Q<VisualElement>(val2, "frame", (string)null);
		m_spriteIconEnemy = UQueryExtensions.Q<VisualElement>(val2, "header", (string)null);
		m_levelEnemy = UQueryExtensions.Q<Label>(val2, "lb_level", (string)null);
		m_spriteFrameSelf = UQueryExtensions.Q<VisualElement>(val3, "frame", (string)null);
		m_spriteIconSelf = UQueryExtensions.Q<VisualElement>(val3, "header", (string)null);
		m_levelSelf = UQueryExtensions.Q<Label>(val3, "lb_level", (string)null);
		for (int i = 0; i < 5; i++)
		{
			tb_skillMod[i] = UQueryExtensions.Q<TemplateContainer>(m_eleTopUI, "skill" + (i + 1), (string)null);
			m_nodeDetail[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "nodeDetail", (string)null);
			m_skillOutProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "outProgress", (string)null);
			m_skillOutProgress[i].progress = 100f;
			m_skillInProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "inProgress", (string)null);
			m_skillInProgress[i].progress = 100f;
			m_skillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "skillIcon", (string)null);
			m_lbSkillLevel[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillMod[i], "lb_skill", (string)null);
			m_btnSkill[i] = UQueryExtensions.Q<Button>((VisualElement)(object)tb_skillMod[i], "btn_skill", (string)null);
			int curIndex = i + 1;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkill[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillClick(evt, curIndex);
			}, "", (TrickleDown)0);
			tb_skillModEnemy[i] = UQueryExtensions.Q<TemplateContainer>(val2, "skill" + (i + 1), (string)null);
			m_nodeDetailEnemy[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillModEnemy[i], "nodeDetail", (string)null);
			m_skillOutProgressEnemy[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillModEnemy[i], "outProgress", (string)null);
			m_skillOutProgressEnemy[i].progress = 100f;
			m_skillInProgressEnemy[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillModEnemy[i], "inProgress", (string)null);
			m_skillInProgressEnemy[i].progress = 100f;
			m_skillIconEnemy[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillModEnemy[i], "skillIcon", (string)null);
			m_lbSkillLevelEnemy[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillModEnemy[i], "lb_skill", (string)null);
			m_btnSkillEnemy[i] = UQueryExtensions.Q<Button>((VisualElement)(object)tb_skillModEnemy[i], "btn_skill", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkillEnemy[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillClickEnemy(evt, curIndex - 1);
			}, "", (TrickleDown)0);
			tb_skillModSelf[i] = UQueryExtensions.Q<TemplateContainer>(val3, "skill" + (i + 1), (string)null);
			m_nodeDetailSelf[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillModSelf[i], "nodeDetail", (string)null);
			m_skillOutProgressSelf[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillModSelf[i], "outProgress", (string)null);
			m_skillOutProgressSelf[i].progress = 100f;
			m_skillInProgressSelf[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillModSelf[i], "inProgress", (string)null);
			m_skillInProgressSelf[i].progress = 100f;
			m_skillIconSelf[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillModSelf[i], "skillIcon", (string)null);
			m_lbSkillLevelSelf[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillModSelf[i], "lb_skill", (string)null);
			m_btnSkillSelf[i] = UQueryExtensions.Q<Button>((VisualElement)(object)tb_skillModSelf[i], "btn_skill", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkillSelf[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillClickSelf(evt, curIndex - 1);
			}, "", (TrickleDown)0);
		}
		((VisualElement)tb_skillMod[4]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_panel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		UQueryExtensions.Q<VisualElement>(m_RootElement, "dmg_team", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_dmgPanel = new zbCombatDamagePanel(this, m_panel);
		m_dmgHelp = new zbCombatDamageHelp(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "dmg_team", (string)null));
		m_wallSkill = UQueryExtensions.Q<VisualElement>(m_eleTopUI, "wallSkill", (string)null);
		for (int num = 0; num < 3; num++)
		{
			m_wallSkills[num] = UQueryExtensions.Q<VisualElement>(m_wallSkill, "wallSkill" + num, (string)null);
			m_wallIcons[num] = UQueryExtensions.Q<VisualElement>(m_wallSkills[num], "bg_skill", (string)null);
			Button visualElement = UQueryExtensions.Q<Button>(m_wallSkills[num], "btn_preview", (string)null);
			int curIndex2 = num;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillWall(evt, curIndex2);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<PointerDownEvent>(m_eleTopUI, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)1);
		m_uiEvent.RegisterCallback<PointerUpEvent>(m_eleTopUI, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPause, (Action)OnPause, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSpeed, (Action<ClickEvent>)OnSpeed, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnData, (Action<ClickEvent>)OnData, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFocus, (Action<ClickEvent>)OnFocus, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMecha, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnMecha(evt, 1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMecha2, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnMecha(evt, 2);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnKillCountAward, (Action)ShowKillCountAwards, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDef, (Action)OnDef, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_DefMask, (Action)OnCloseDefMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnChangeTarget, "", (TrickleDown)0);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGun, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnSkillClick(evt, 0);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGunEnemy, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnSkillClickEnemy(evt, 0);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGunSelf, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnSkillClickSelf(evt, 0);
		}, "", (TrickleDown)0);
		m_LoadBg1 = ((Component)GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot").Find("RootHero").Find("load_bg")).gameObject;
		m_Load1 = ((Component)m_LoadBg1.transform.Find("load_1")).gameObject;
		m_LoadBg2 = ((Component)GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot").Find("RootHero").Find("load_bg2")).gameObject;
		m_Load2 = ((Component)m_LoadBg2.transform.Find("load_2")).gameObject;
		m_Load1Renderer1 = m_Load1.GetComponent<SpriteRenderer>();
		m_Load1Renderer2 = m_Load2.GetComponent<SpriteRenderer>();
		m_LoadBg1Enemy = ((Component)GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot2").Find("RootHero").Find("load_bg")).gameObject;
		m_Load1Enemy = ((Component)m_LoadBg1.transform.Find("load_1")).gameObject;
		m_LoadBg2Enemy = ((Component)GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot2").Find("RootHero").Find("load_bg2")).gameObject;
		m_Load2Enemy = ((Component)m_LoadBg2Enemy.transform.Find("load_2")).gameObject;
		m_Load1Renderer1Enemy = m_Load1Enemy.GetComponent<SpriteRenderer>();
		m_Load1Renderer2Enemy = m_Load2Enemy.GetComponent<SpriteRenderer>();
		worldBoundFrame = 0f;
		worldBoundFlag = false;
		renderTarget1 = ((Component)GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot").Find("GOCamera")).GetComponent<zbCombatCamera>();
		if (m_descImg1 == null)
		{
			m_descImg1 = new Image();
			((VisualElement)m_descImg1).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg1).style.bottom = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg1).style.width = StyleLength.op_Implicit(720f);
			((VisualElement)m_descImg1).style.height = StyleLength.op_Implicit(1680f);
			((VisualElement)m_descImg1).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		m_camera1.Add((VisualElement)(object)m_descImg1);
		renderTarget2 = ((Component)GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot2").Find("GOCamera")).GetComponent<zbCombatCamera>();
		if (m_descImg2 == null)
		{
			m_descImg2 = new Image();
			((VisualElement)m_descImg2).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg2).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg2).style.width = StyleLength.op_Implicit(720f);
			((VisualElement)m_descImg2).style.height = StyleLength.op_Implicit(1680f);
			((VisualElement)m_descImg2).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		m_camera2.Add((VisualElement)(object)m_descImg2);
		m_gameEevent.RegisterEvent(EGameEvent.Event_FightForward_EndlessHorde, m_endlessHordePanel.RefreshFightForwardEndlessHorde);
		m_gameEevent.RegisterEvent(EGameEvent.Event_RankData_EndlessHorde, m_endlessHordePanel.RefreshRankDataEndlessHorde);
	}

	public void FreezeBattleCameras()
	{
		renderTarget1?.FreezeRender();
		renderTarget2?.FreezeRender();
	}

	public void StartBattle(BattleInfo battleInfo, int paramId, zbCombatRecorder restoreData, BattleResultInfo dataResult)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		if (battleInfo.BattleBaseInfo.BattleModel == EBattleModelType.Pvp)
		{
			renderTarget1.StartRender(m_descImg1);
			renderTarget2.StartRender(m_descImg2);
		}
		else
		{
			renderTarget1.StopRender();
			renderTarget2.StopRender();
			zbPerfLog.Log("[zbRT] PVE 战斗：战场 RT 相机未启动（RT 画面不显示，避免同战场渲染两遍）");
		}
		zbUIMgr.Instance.m_scrollBubble.Stop();
		openEndWnd = false;
		m_battleNotReadyLogged = false;
		((VisualElement)m_lbCurrHp).style.color = StyleColor.op_Implicit(Color.white);
		zbCombatSystem.FreeInstatnce();
		zbCombatSystem.CreateInstatnce();
		zbCombatSystem.Instance.Init(this, battleInfo, paramId, restoreData);
		zbCombatSystem.Instance.dataResult = dataResult;
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			m_camera1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_camera2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_pvpVS.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_pvpUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_eleTopUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_topUiSkillVisible = false;
			m_PvpMonsterDieEnemy.Refresh(zbCombatSystem.Instance.combatData);
			m_PvpMonsterDieSelf.Refresh(zbCombatSystem.Instance.combatData2);
			((VisualElement)m_btnPause).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(dataResult == null));
		}
		else
		{
			m_camera1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_camera2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_pvpVS.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_pvpUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_eleTopUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_topUiSkillVisible = true;
			((VisualElement)m_btnPause).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		try
		{
			zbCombatSystem.Instance.PreHeatBattleAssets();
		}
		catch (Exception)
		{
		}
		combatData = zbCombatSystem.Instance.combatData;
		bossCome = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SpineBossComing");
		bossCome.SetActive(false);
		m_LoadBg1.gameObject.SetActive(false);
		m_LoadBg2.gameObject.SetActive(false);
		m_legendaryPanel.Refresh();
		refreshSkill();
		m_speedPanel.SetVisible(vis: false);
		refreshSpeed();
		UpdateAll();
		UpdateLevelName();
		refreshFocus();
		refreshWallSkill();
		refreshPvpBg();
		m_killCount.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbCombatSystem.Instance.isLegionBoss() && !zbCombatSystem.Instance.IsWorldLair() && !zbCombatSystem.Instance.isWorldBoss()) ? 1 : 0));
		if (m_killCount.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			UQueryExtensions.Q<VisualElement>(m_killCount, "icon_zomble", (string)null).style.backgroundImage = new StyleBackground((zbCombatSystem.Instance.isLegionBoss() || zbCombatSystem.Instance.IsWorldLair()) ? m_iconZombies : m_iconHurt);
		}
		m_roundNum.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.GetBattleModel() != EBattleModelType.KillAll));
		m_leftTime.visible = false;
		if (restoreData == null)
		{
			int num = 0;
			float num2 = combatData.masterTeam.ExtraProperties[ETeamExtra.GemMod1050];
			num += (int)num2;
			num += battleInfo.BattleBaseInfo.InitLevel;
			if (num > 0 && !zbCombatSystem.Instance.isPVPBattleType)
			{
				zbUIMgr.Instance.ShowSelectMod(combatData.masterTeam, num);
			}
		}
		else if (restoreData.loadedData.curSelectMod.needSelectMod)
		{
			zbUIMgr.Instance.ShowModSelectRestoreData(combatData.masterTeam, restoreData.loadedData.curSelectMod.selectNum, restoreData.loadedData.curSelectMod.selectMax, restoreData.loadedData.curSelectMod.curRefreshNum, restoreData.loadedData.curSelectMod.selectMod);
		}
		else if (restoreData.loadedData.curModWheel.needModWheel)
		{
			zbUIMgr.Instance.ShowModWheelRestoreData(combatData.masterTeam, restoreData.loadedData.curModWheel);
		}
		else if (restoreData.loadedData.currFightForward.needFightForward)
		{
			zbUIMgr.Instance.ShowEndlessHordeFightForwardRestoreData(combatData.masterTeam, restoreData.loadedData.currFightForward.selectNum, restoreData.loadedData.currFightForward.selectMax, restoreData.loadedData.currFightForward.curRefreshNum, restoreData.loadedData.currFightForward.selectMod);
		}
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(combatData.masterTeam.battleTeamInfo.RoleBaseInfo.GunId);
		m_gunObject.style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.battleBanner));
		m_gunObjectSelf.style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.battleBanner));
		if (zbSelf.Instance.guideHandler.IsFirstMainQuestBattle())
		{
			zbCombatSystem.Instance.Pause();
			zbAudioMgr.Instance.PauseAllAudio();
			zbUIMgr.Instance.ShowGuideWeak(1001, zbSelf.Instance.guideHandler.OnResume);
		}
		zbMechaBase mecha = zbCombatSystem.Instance.combatData.masterTeam.mecha;
		if ((Object)(object)mecha != (Object)null)
		{
			((VisualElement)m_btnMecha).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(mecha.mechaAvatarRecord.battleIcon);
			if ((Object)(object)val != (Object)null)
			{
				m_mechaIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_mechaIcon.MarkDirtyRepaint();
			}
		}
		else
		{
			((VisualElement)m_btnMecha).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbCombatTeam helperTeam = zbCombatSystem.Instance.combatData.helperTeam;
		if (helperTeam != null && (Object)(object)helperTeam.mecha != (Object)null)
		{
			((VisualElement)m_btnMecha2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(helperTeam.mecha.mechaAvatarRecord.battleIcon);
			if ((Object)(object)val2 != (Object)null)
			{
				m_mechaIcon2.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
				m_mechaIcon2.MarkDirtyRepaint();
			}
		}
		else
		{
			((VisualElement)m_btnMecha2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_endlessHordePanel.StartBattle();
		m_startFight.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			((MonoBehaviour)this).StartCoroutine(refreshStartFight());
		}
		currActionPoints = 0;
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.LegionWar)
		{
			currActionPoints = zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ActionPoints - zbSelf.Instance.legionWarHandler.currActionPoints;
		}
		float num3 = (float)Math.Clamp(currActionPoints, -50, 50) * 0.8f;
		lastOffsetY = num3 / 10f;
	}

	private IEnumerator refreshStartFight()
	{
		Time.timeScale = zbCombatSpeed.GetSpeedByEnum(ECombatSpeed.X1);
		m_startFight.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbStartFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		TrackEntry enter = spineStartFight.AnimationState.SetAnimation(0, "animation", false);
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		zbAudioMgr.Instance.PlayAudio("audio/combat/bosscoming.ogg");
		yield return (object)new WaitForSecondsRealtime(1f / 3f);
		((VisualElement)m_lbStartFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return (object)new WaitForSecondsRealtime(1f);
		((VisualElement)m_lbStartFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (enter != null)
		{
			yield return (object)new WaitForSecondsRealtime(enter.AnimationEnd - 1.3333334f);
		}
		m_startFight.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbCombatSystem.Instance.Resume();
		zbAudioMgr.Instance.ResumeAllAudio();
		zbCombatSystem.Instance.SetTimeScaleOrDeferPreHeat(zbCombatSpeed.GetSpeedByEnum(ECombatSpeed.X3));
	}

	public void refreshSkill()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		List<zbModCsvRecord> source = (from r in combatData.masterTeam.modMgr.modList
			select zbDataConfig.Instance.modCsvTable.GetRecordByKey(r.modRecord.id) into m
			where m != null
			select m).ToList();
		List<zbHero> heroList = combatData.masterTeam.heroList;
		for (int num = 0; num < 5; num++)
		{
			m_nodeDetail[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool flag = false;
		int num2 = 0;
		foreach (zbHero item in heroList)
		{
			if (item.skillDetail.SkillType == ESkillType.Qiang)
			{
				flag = true;
				continue;
			}
			((VisualElement)tb_skillMod[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeDetail[num2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(item.skillDetail.SkillId);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
			m_skillIcon[num2].style.backgroundImage = StyleBackground.op_Implicit(val);
			m_skillIcon[num2].transform.scale = Vector3.one * 0.45f;
			ESkillType type = item.skillDetail.SkillType;
			int num3 = source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
			((TextElement)m_lbSkillLevel[num2]).text = num3.ToString();
			num2++;
		}
		m_gunObject.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_lbGunLevel).text = "Lv." + source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, 1));
		refreshEnemySkill();
		refreshSelfSkill();
	}

	public void refreshEnemySkill()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		if (!zbCombatSystem.Instance.isPVPBattleType)
		{
			return;
		}
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(zbCombatSystem.Instance.combatData2.masterTeam.battleTeamInfo.RoleBaseInfo.HeadFrameId);
		m_spriteFrameEnemy.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(zbCombatSystem.Instance.combatData2.masterTeam.battleTeamInfo.RoleBaseInfo.HeadId);
		m_spriteIconEnemy.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		((TextElement)m_levelEnemy).text = zbCombatSystem.Instance.combatData2.masterTeam.battleTeamInfo.RoleBaseInfo.Level.ToString();
		List<zbModCsvRecord> source = (from r in zbCombatSystem.Instance.combatData2.masterTeam.modMgr.modList
			select zbDataConfig.Instance.modCsvTable.GetRecordByKey(r.modRecord.id) into m
			where m != null
			select m).ToList();
		List<zbHero> heroList = zbCombatSystem.Instance.combatData2.masterTeam.heroList;
		for (int num = 0; num < 5; num++)
		{
			m_nodeDetailEnemy[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool flag = false;
		int num2 = 0;
		foreach (zbHero item in heroList)
		{
			if (item.skillDetail.SkillType == ESkillType.Qiang)
			{
				flag = true;
				continue;
			}
			((VisualElement)tb_skillModEnemy[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeDetailEnemy[num2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(item.skillDetail.SkillId);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
			m_skillIconEnemy[num2].style.backgroundImage = StyleBackground.op_Implicit(val);
			m_skillIconEnemy[num2].transform.scale = Vector3.one * 0.45f;
			ESkillType type = item.skillDetail.SkillType;
			int num3 = source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
			((TextElement)m_lbSkillLevelEnemy[num2]).text = num3.ToString();
			num2++;
		}
		zbPlayerGunCsvRecord recordByKey2 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(zbCombatSystem.Instance.combatData2.masterTeam.battleTeamInfo.RoleBaseInfo.GunId);
		m_gunObjectEnemy.style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey2.battleBanner));
		m_gunObjectEnemy.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_lbGunLevelEnemy).text = "Lv." + source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, 1));
	}

	public void refreshSelfSkill()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		if (!zbCombatSystem.Instance.isPVPBattleType)
		{
			return;
		}
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(zbCombatSystem.Instance.combatData.masterTeam.battleTeamInfo.RoleBaseInfo.HeadFrameId);
		m_spriteFrameSelf.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(zbCombatSystem.Instance.combatData.masterTeam.battleTeamInfo.RoleBaseInfo.HeadId);
		m_spriteIconSelf.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		((TextElement)m_levelSelf).text = zbCombatSystem.Instance.combatData.masterTeam.battleTeamInfo.RoleBaseInfo.Level.ToString();
		List<zbModCsvRecord> source = (from r in combatData.masterTeam.modMgr.modList
			select zbDataConfig.Instance.modCsvTable.GetRecordByKey(r.modRecord.id) into m
			where m != null
			select m).ToList();
		List<zbHero> heroList = combatData.masterTeam.heroList;
		for (int num = 0; num < 5; num++)
		{
			m_nodeDetailSelf[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool flag = false;
		int num2 = 0;
		foreach (zbHero item in heroList)
		{
			if (item.skillDetail.SkillType == ESkillType.Qiang)
			{
				flag = true;
				continue;
			}
			((VisualElement)tb_skillModSelf[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeDetailSelf[num2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(item.skillDetail.SkillId);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
			m_skillIconSelf[num2].style.backgroundImage = StyleBackground.op_Implicit(val);
			m_skillIconSelf[num2].transform.scale = Vector3.one * 0.45f;
			ESkillType type = item.skillDetail.SkillType;
			int num3 = source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
			((TextElement)m_lbSkillLevelSelf[num2]).text = num3.ToString();
			num2++;
		}
		m_gunObjectSelf.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_lbGunLevelSelf).text = "Lv." + source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, 1));
	}

	public void refreshWallSkill()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			m_wallSkills[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (int wallSkill in combatData.masterTeam.battleTeamInfo.DefenseInfo.WallSkillList)
		{
			if (num > 2)
			{
				break;
			}
			m_wallSkills[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbWallSkillCsvRecord recordByKey = zbDataConfig.Instance.wallSkillCsvTable.GetRecordByKey(wallSkill);
			zbWallCsvRecord recordByKey2 = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(recordByKey.wallId);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey2.skillIcon);
			m_wallIcons[num].style.backgroundImage = StyleBackground.op_Implicit(val);
			num++;
		}
	}

	public void refreshPvpBg()
	{
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			string text = "ui/image/combat/pvp/";
			switch (zbCombatSystem.Instance.GetBattleType())
			{
			case EBattleFuncType.Arena:
			case EBattleFuncType.PeakArena:
				text += "pvp_bg42.jpg";
				break;
			case EBattleFuncType.CrossLadderMatch:
				text += "pvp_bg43.jpg";
				break;
			case EBattleFuncType.CrossLadderUpgrade:
				text += "pvp_bg44.jpg";
				break;
			case EBattleFuncType.MineWar:
			case EBattleFuncType.CrossMine:
				text += "pvp_bg45.jpg";
				break;
			case EBattleFuncType.Truck:
			case EBattleFuncType.Train:
				text += "pvp_bg48.jpg";
				break;
			case EBattleFuncType.LegionWar:
				text += "pvp_bg49.jpg";
				break;
			}
			zbAssetLoader.Instance.LoadResouce<Texture2D>(text);
			Sprite val = zbAssetLoader.Instance.LoadSpriteAsset(text);
			if ((Object)(object)val != (Object)null)
			{
				m_pvpBg.sprite = val;
			}
		}
	}

	public override void Hide()
	{
		base.Hide();
	}

	protected override void OnDisable()
	{
		zbAudioMgr.Instance.StopAllAudio();
		zbCombatSystem.Instance.Free();
		base.OnDisable();
	}

	public void OnPause()
	{
		zbUIMgr.Instance.ShowCombatPause();
	}

	public void refreshSpeed()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		string text = zbCombatSystem.Instance.combatSpeed.GetSpeed().ToString().Substring(1);
		string text2 = "1";
		if (zbServerData.sGameData.RoleCommonInfo.RechargeAmount <= 0f)
		{
			text2 = ((text != "1") ? "1" : "");
		}
		string resName = "ui/image/button/button_speed" + text + text2 + ".png";
		((VisualElement)m_btnSpeed).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
	}

	public void OnSpeed(ClickEvent evt)
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.HighSpeed, zbSelf.Instance.GetVipLevel()) > 0)
		{
			m_speedPanel.SetVisible(vis: true);
			return;
		}
		ECombatSpeed speed = zbCombatSystem.Instance.combatSpeed.GetSpeed();
		ECombatSpeed eCombatSpeed = ECombatSpeed.X1;
		switch (speed)
		{
		case ECombatSpeed.X1:
			eCombatSpeed = ECombatSpeed.X2;
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10682"));
			break;
		case ECombatSpeed.X2:
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.HighSpeed, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				eCombatSpeed = ECombatSpeed.X1;
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10681"));
			}
			else
			{
				eCombatSpeed = ECombatSpeed.X3;
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10683"));
			}
			break;
		default:
			eCombatSpeed = ECombatSpeed.X1;
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10681"));
			break;
		}
		zbCombatSystem.Instance.combatSpeed.SetSpeed(eCombatSpeed);
		refreshSpeed();
		zbProgressEventHelper.Instance.SetProgressTrack(604);
	}

	public void OnData(ClickEvent evt)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (m_dmgHelp.m_root.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0) || m_dmgPanel.m_root.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			m_dmgPanel.Close();
			m_dmgHelp.Close();
		}
		else if (zbCombatSystem.Instance.combatData.helperTeam != null)
		{
			m_dmgHelp.Show();
		}
		else
		{
			m_dmgPanel.Show();
		}
	}

	public void refreshFocus()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.m_focusFlag == 1)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/button/button_focus2.png");
			if ((Object)(object)val != (Object)null)
			{
				((VisualElement)m_btnFocus).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				((VisualElement)m_btnFocus).MarkDirtyRepaint();
			}
		}
		else
		{
			Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/button/button_focus1.png");
			if ((Object)(object)val2 != (Object)null)
			{
				((VisualElement)m_btnFocus).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
				((VisualElement)m_btnFocus).MarkDirtyRepaint();
			}
		}
	}

	public void OnFocus(ClickEvent evt)
	{
		zbCombatSystem.Instance.m_focusFlag = 1 - zbCombatSystem.Instance.m_focusFlag;
		if (zbCombatSystem.Instance.m_focusFlag == 1)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10428"));
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10429"));
		}
		refreshFocus();
		zbHeroQiang zbHeroQiang2 = (zbHeroQiang)zbCombatSystem.Instance.combatData.masterTeam.GetHeroBySkillType(ESkillType.Qiang);
		if ((Object)(object)zbHeroQiang2 != (Object)null)
		{
			zbHeroQiang2.SetTargetMonsterId(0);
		}
		zbCombatSystem.Instance.SetFocus(zbCombatSystem.Instance.m_focusFlag);
		zbProgressEventHelper.Instance.SetProgressTrack(605);
	}

	private void ShowKillCountAwards()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!(((VisualElement)m_awardMask).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0)))
		{
			((MonoBehaviour)this).StartCoroutine(ShowKillCountAward());
		}
	}

	private IEnumerator ShowKillCountAward()
	{
		((VisualElement)m_awardMask).BringToFront();
		((VisualElement)m_awardMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		List<zbItemData> list = new List<zbItemData>();
		if (zbCombatSystem.Instance.IsWorldLair())
		{
			list = zbSelf.Instance.worldLairHandler.GetLairPauseAwardByKilled(list);
		}
		else if (zbCombatSystem.Instance.isLegionBoss())
		{
			list = zbSelf.Instance.legionHandler.GetLegionBossPauseAwardByKilled(list);
		}
		else if (zbCombatSystem.Instance.isWorldBoss())
		{
			list = zbSelf.Instance.worldBossHandler.GetWorldBossPauseAwardByHurt(list);
		}
		foreach (zbItemData item in list)
		{
			m_awards.Add((VisualElement)(object)zbSelf.Instance.combatHandler.CreatezbItemByAwardPreview(item));
		}
		yield return (object)new WaitForSecondsRealtime(3f);
		((VisualElement)m_awardMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_awards.Clear();
		yield return null;
	}

	private void OnDef()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_DefMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_DefMask.SetEnabled(true);
		m_DefMask.pickingMode = (PickingMode)0;
	}

	private void OnCloseDefMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_DefMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_DefMask.SetEnabled(false);
		m_DefMask.pickingMode = (PickingMode)1;
	}

	private void OnChangeTarget(ClickEvent evt)
	{
		zbHeroQiang zbHeroQiang2 = (zbHeroQiang)zbCombatSystem.Instance.combatData.masterTeam.GetHeroBySkillType(ESkillType.Qiang);
		if ((Object)(object)zbHeroQiang2 != (Object)null)
		{
			zbHeroQiang2.SetTargetMonsterId(0);
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (zbCombatSystem.Instance == null || combatData == null)
		{
			if (!m_battleNotReadyLogged)
			{
				m_battleNotReadyLogged = true;
			}
			return;
		}
		zbCombatSystem.Instance.Update();
		if (IsShow() && GetMyGameObjectSceneRoot().activeSelf)
		{
			m_dmgPanel.Update();
			m_dmgHelp.Update();
			UpdateAll();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (zbCombatSystem.Instance != null && combatData != null && zbCombatSystem.Instance.dataResult == null && zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.LegionWar && (float)(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime()) <= 0f && !openEndWnd && !zbUIMgr.Instance.IsShow(EWndId.CombatBalance) && !zbUIMgr.Instance.IsShow(EWndId.CrossLadderBalance))
		{
			openEndWnd = true;
			zbCombatSystem.Instance.Pause();
			zbAudioMgr.Instance.PauseAllAudio();
			zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11248"), delegate
			{
				Hide();
				DestroySelfWnd();
				zbUIMgr.Instance.DestroyWnd(EWndId.CombatPause);
				zbUIMgr.Instance.DestroyWnd(EWndId.SelectMod);
				zbUIMgr.Instance.DestroyWnd(EWndId.ModWheel);
				Time.timeScale = zbCombatSpeed.GetSpeedByEnum(ECombatSpeed.X1);
			}, "1", "", isTouchVis: false);
		}
	}

	private void UpdateAll()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		UpdateCountdown();
		UpdateGunAmmo();
		UpdateSkill();
		UpdateExpLevel();
		UpdateKillCount();
		UpdateLeftTime();
		UpdateRoundNum();
		m_endlessHordePanel.UpdateLandmark();
		worldBoundFrame += Time.deltaTime;
		if (worldBoundFrame >= 0.5f && !worldBoundFlag)
		{
			worldBoundFlag = true;
			Rect worldBound = m_killCount.worldBound;
			Vector2 center = ((Rect)(ref worldBound)).center;
			center.x = center.x / 720f * (float)Screen.width;
			center.y = (float)Screen.height - center.y / 1680f * (float)Screen.height;
		}
	}

	private void UpdateKillCount()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		zbCombatExp combatExp = this.combatData.combatExp;
		if (zbCombatSystem.Instance.isWorldBoss())
		{
			zbUtil.SetLabelIfChanged(m_lbKillCount, zbUtil.ConvertToTBMK(zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum));
			zbUtil.SetLabelIfChanged(m_lbKillCountAwardCount, "x" + zbSelf.Instance.worldBossHandler.ClaimedRewardCount());
			zbUtil.SetWidthIfChanged(m_killCountProgress, zbSelf.Instance.worldBossHandler.KilledProgressToNextReward());
		}
		else if (zbCombatSystem.Instance.IsWorldLair())
		{
			zbUtil.SetLabelIfChanged(m_lbKillCount, zbUtil.ConvertToTBMK((int)combatExp.monsterKilledCount));
			zbUtil.SetLabelIfChanged(m_lbKillCountAwardCount, "x" + zbSelf.Instance.worldLairHandler.ClaimedRewardCount());
			zbUtil.SetWidthIfChanged(m_killCountProgress, zbSelf.Instance.worldLairHandler.KilledProgressToNextReward());
		}
		else if (zbCombatSystem.Instance.isLegionBoss())
		{
			zbUtil.SetLabelIfChanged(m_lbKillCount, zbUtil.ConvertToTBMK((int)combatExp.monsterKilledCount));
			zbUtil.SetLabelIfChanged(m_lbKillCountAwardCount, "x" + zbSelf.Instance.legionHandler.ClaimedRewardCount());
			zbUtil.SetWidthIfChanged(m_killCountProgress, zbSelf.Instance.legionHandler.KilledProgressToNextReward());
		}
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			zbCombatData zbCombatData2 = zbCombatSystem.Instance.combatData;
			zbUtil.SetLabelIfChanged(m_lbPVPP1KillCount, zbCombatData2.combatExp.monsterKilledCount.ToString());
			zbCombatData combatData = zbCombatSystem.Instance.combatData2;
			zbUtil.SetLabelIfChanged(m_lbPVPP2KillCount, combatData.combatExp.monsterKilledCount.ToString());
			float num = (float)Math.Clamp((int)zbCombatData2.combatExp.monsterKilledCount - (int)combatData.combatExp.monsterKilledCount + currActionPoints, -50, 50) * 0.8f / 10f;
			if (num > lastOffsetY)
			{
				lastOffsetY = Math.Min(lastOffsetY + Time.deltaTime * 0.1f, num);
			}
			else
			{
				lastOffsetY = Math.Max(lastOffsetY - Time.deltaTime * 0.1f, num);
			}
			zbCombatData2.monsterSpawner.RefreshArea(lastOffsetY);
			combatData.monsterSpawner.RefreshArea(0f - lastOffsetY);
			zbUtil.SetTopIfChanged(m_pvpVS, 840f - lastOffsetY * 100f);
			zbUtil.SetHeightIfChanged(m_camera1, 1050f + lastOffsetY * 100f / 0.8f);
			zbUtil.SetHeightIfChanged(m_camera2, 1050f - lastOffsetY * 100f / 0.8f);
			zbCombatData2.combatScene.RefreshTopPositionY(lastOffsetY);
			combatData.combatScene.RefreshTopPositionY(0f - lastOffsetY);
			m_PvpMonsterDieEnemy.TriggerCall();
			m_PvpMonsterDieSelf.TriggerCall();
			if ((int)zbCombatData2.combatExp.monsterKilledCount >= 100 && (Object)(object)zbCombatData2.masterTeam.mecha != (Object)null && zbCombatData2.masterTeam.mechaCallCount < zbCombatData2.masterTeam.mechaMaxCount)
			{
				OnMecha(null, 1, zbCombatData2);
			}
			if ((int)combatData.combatExp.monsterKilledCount >= 100 && (Object)(object)combatData.masterTeam.mecha != (Object)null && combatData.masterTeam.mechaCallCount < combatData.masterTeam.mechaMaxCount)
			{
				OnMecha(null, 1, combatData);
			}
		}
	}

	private void UpdateLeftTime()
	{
		if (zbCombatSystem.Instance.IsTrialBattle(zbCombatSystem.Instance.GetBattleType()))
		{
			long trialShowEndTmByBattleFuncType = zbSelf.Instance.trialHandler.GetTrialShowEndTmByBattleFuncType(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType);
			zbUtil.SetLabelIfChanged(m_lbLeftTime, zbUtil.FormatTimeHHMMSS(Math.Max(trialShowEndTmByBattleFuncType - zbSelf.Instance.timeHandler.GetServerTime(), 0L)));
		}
	}

	private void UpdateRoundNum()
	{
		if (zbCombatSystem.Instance.GetBattleModel() == EBattleModelType.KillAll)
		{
			int num = ((IEnumerable<BattleMonsterRoundDetail>)zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList).Count();
			zbUtil.SetLabelIfChanged(m_lbRoundNum, (int)zbCombatSystem.Instance.combatRecorder.curRound + 1 + "/" + num);
		}
	}

	public void UpdateCountdown()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)Mathf.Ceil(Mathf.Max((float)zbCombatSystem.Instance.countDown, 0f));
		zbUtil.SetLabelIfChanged(m_lbCountdown, zbUtil.FormatTimeMMSS(num));
		if (zbCombatSystem.Instance.maxTime > 0 && num < 60)
		{
			if (((VisualElement)m_lbCountdown).style.color != Color.red)
			{
				((VisualElement)m_lbCountdown).style.color = StyleColor.op_Implicit(Color.red);
			}
		}
		else if (((VisualElement)m_lbCountdown).style.color != Color.white)
		{
			((VisualElement)m_lbCountdown).style.color = StyleColor.op_Implicit(Color.white);
		}
	}

	public void UpdateExpLevel()
	{
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		float val = 1f;
		if (zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.EndlessHorde)
		{
			zbCombatExp combatExp = combatData.combatExp;
			if (combatExp.curLevelMaxExp != 0)
			{
				val = (float)(int)combatExp.curLevelExp / (float)combatExp.curLevelMaxExp;
			}
			string text = "";
			int num = (int)combatExp.curLevel + combatExp.initLevel;
			if ((int)combatExp.curLevel + combatExp.initLevel <= combatExp.maxLevel)
			{
				zbUtil.SetLabelIfChanged(m_lbBarExp, "Lv." + num + text);
			}
			else
			{
				val = 1f;
				zbUtil.SetLabelIfChanged(m_lbBarExp, "Lv." + combatExp.maxLevel + " MAX" + text);
			}
		}
		else
		{
			val = zbCombatSystem.Instance.scrollDistance / (float)(zbCombatSystem.Instance.endlessHordeNextDistance - zbCombatSystem.Instance.endlessHordeCurrDistance);
			float num2 = zbCombatSystem.Instance.scrollDistance + (float)zbCombatSystem.Instance.endlessHordeCurrDistance;
			zbUtil.SetLabelIfChanged(m_lbBarExp, (num2 / 1000f).ToString("0.#") + "km/" + ((float)zbCombatSystem.Instance.endlessHordeNextDistance / 1000f).ToString("0.#") + "km");
		}
		float num3 = Math.Min(val, 1f) * maxWidth;
		zbUtil.SetWidthIfChanged(m_barExpPro, num3);
		((VisualElement)spineLevelProgress).transform.position = new Vector3(num3, ((VisualElement)spineLevelProgress).transform.position.y, ((VisualElement)spineLevelProgress).transform.position.z);
		zbUtil.SetLabelIfChanged(m_lbImmune, combatData.defense.damageImmunity.ToString());
		if (((VisualElement)m_lbCurrHp).style.color == Color.white && combatData.defense.curHp < combatData.defense.maxHp)
		{
			((VisualElement)m_lbCurrHp).style.color = StyleColor.op_Implicit(Color.red);
		}
		if (((VisualElement)m_lbCurrHp).style.color == Color.red && combatData.defense.curHp == combatData.defense.maxHp)
		{
			((VisualElement)m_lbCurrHp).style.color = StyleColor.op_Implicit(Color.white);
		}
		if ((int)combatData.defense.damageImmunity > 0)
		{
			zbUtil.SetLabelIfChanged(m_lbCurrHp, combatData.defense.curHp.ToString());
		}
		else
		{
			zbUtil.SetLabelIfChanged(m_lbCurrHp, combatData.defense.curHp.ToString());
		}
		zbUtil.SetLabelIfChanged(m_lbCurrShield, combatData.defense.shieldHp.ToString());
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			zbUtil.SetLabelIfChanged(m_lbImmuneEnemy, zbCombatSystem.Instance.combatData2.defense.damageImmunity.ToString());
			if (((VisualElement)m_lbCurrHpEnemy).style.color == Color.white && zbCombatSystem.Instance.combatData2.defense.curHp < zbCombatSystem.Instance.combatData2.defense.maxHp)
			{
				((VisualElement)m_lbCurrHpEnemy).style.color = StyleColor.op_Implicit(Color.red);
			}
			if (((VisualElement)m_lbCurrHpEnemy).style.color == Color.red && zbCombatSystem.Instance.combatData2.defense.curHp == zbCombatSystem.Instance.combatData2.defense.maxHp)
			{
				((VisualElement)m_lbCurrHpEnemy).style.color = StyleColor.op_Implicit(Color.white);
			}
			if ((int)zbCombatSystem.Instance.combatData2.defense.damageImmunity > 0)
			{
				zbUtil.SetLabelIfChanged(m_lbCurrHpEnemy, zbCombatSystem.Instance.combatData2.defense.curHp.ToString());
			}
			else
			{
				zbUtil.SetLabelIfChanged(m_lbCurrHpEnemy, zbCombatSystem.Instance.combatData2.defense.curHp.ToString());
			}
			if (((TextElement)m_btnDefEnemy).text != zbCombatSystem.Instance.combatData2.defense.shieldHp.ToString())
			{
				((TextElement)m_btnDefEnemy).text = zbCombatSystem.Instance.combatData2.defense.shieldHp.ToString();
			}
		}
	}

	public void OnSkillClick(ClickEvent evt, int skillIndex)
	{
		SkillDetail[] heroList = combatData.masterTeam.heroList.Select((zbHero s) => new SkillDetail
		{
			HeroTid = s.skillDetail.HeroTid,
			SkillType = s.skillDetail.SkillType,
			SkillId = s.skillDetail.SkillId
		}).ToArray();
		List<int> modList = combatData.masterTeam.modMgr.modList.Select((zbModBase r) => r.modRecord.id).ToList();
		zbUIMgr.Instance.ShowSkillFightModWnd(skillIndex, heroList, modList);
		zbCombatSystem.Instance.Pause();
	}

	public void OnSkillWall(ClickEvent evt, int skillIndex)
	{
		int num = 0;
		foreach (int wallSkill in combatData.masterTeam.battleTeamInfo.DefenseInfo.WallSkillList)
		{
			if (skillIndex == num)
			{
				zbWallSkillCsvRecord recordByKey = zbDataConfig.Instance.wallSkillCsvTable.GetRecordByKey(wallSkill);
				zbWallCsvRecord recordByKey2 = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(recordByKey.wallId);
				zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(recordByKey2.id, EPreviewType.Wall);
				break;
			}
			num++;
		}
		zbCombatSystem.Instance.Pause();
	}

	public void OnSkillClickEnemy(ClickEvent evt, int skillIndex)
	{
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			SkillDetail[] heroList = zbCombatSystem.Instance.combatData2.masterTeam.heroList.Select((zbHero s) => new SkillDetail
			{
				HeroTid = s.skillDetail.HeroTid,
				SkillType = s.skillDetail.SkillType,
				SkillId = s.skillDetail.SkillId
			}).ToArray();
			List<int> modList = zbCombatSystem.Instance.combatData2.masterTeam.modMgr.modList.Select((zbModBase r) => r.modRecord.id).ToList();
			zbUIMgr.Instance.ShowSkillFightModWnd(skillIndex, heroList, modList);
			zbCombatSystem.Instance.Pause();
		}
	}

	public void OnSkillClickSelf(ClickEvent evt, int skillIndex)
	{
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			SkillDetail[] heroList = combatData.masterTeam.heroList.Select((zbHero s) => new SkillDetail
			{
				HeroTid = s.skillDetail.HeroTid,
				SkillType = s.skillDetail.SkillType,
				SkillId = s.skillDetail.SkillId
			}).ToArray();
			List<int> modList = combatData.masterTeam.modMgr.modList.Select((zbModBase r) => r.modRecord.id).ToList();
			zbUIMgr.Instance.ShowSkillFightModWnd(skillIndex, heroList, modList);
			zbCombatSystem.Instance.Pause();
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		zbHero zbHero2 = PickHero(evt);
		if ((Object)(object)zbHero2 != (Object)null)
		{
			SetAlertRandgeVisible(zbHero2, vis: true);
		}
		PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_eleTopUI, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		SetAlertRandgeVisible(null, vis: false);
		PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_eleTopUI, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
	}

	public zbHero PickHero(PointerDownEvent evt)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.8f;
		float num2 = 1f;
		float num3 = 0.2f;
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		foreach (zbHero hero in combatData.masterTeam.heroList)
		{
			Vector3 position = ((Component)hero).transform.position;
			float num4 = position.y + hero.nodeHeroAvatar.transform.localPosition.y;
			float num5 = num * 0.5f;
			if (hero.nodeArmoredDefense.activeSelf)
			{
				num3 = 1f;
			}
			if (val.x >= position.x - num5 && val.x <= position.x + num5 && val.y >= num4 - num3 && val.y <= num4 + num2)
			{
				return hero;
			}
		}
		if (combatData.helperTeam != null)
		{
			foreach (zbHero hero2 in combatData.helperTeam.heroList)
			{
				Vector3 position2 = ((Component)hero2).transform.position;
				float num6 = position2.y + hero2.nodeHeroAvatar.transform.localPosition.y;
				float num7 = num * 0.5f;
				if (hero2.nodeArmoredDefense.activeSelf)
				{
					num3 = 1f;
				}
				if (val.x >= position2.x - num7 && val.x <= position2.x + num7 && val.y >= num6 - num3 && val.y <= num6 + num2)
				{
					return hero2;
				}
			}
		}
		return null;
	}

	public void SetAlertRandgeVisible(zbHero hero, bool vis)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (combatData == null || (Object)(object)combatData.alertRangeGizmo == (Object)null)
		{
			return;
		}
		if (vis)
		{
			if (!((Object)(object)hero == (Object)null))
			{
				((Component)combatData.alertRangeGizmo).transform.localPosition = new Vector3(0f, combatData.defense.GetAlertPositionY() - 7.5f, 0f);
				combatData.alertRangeGizmo.SetAlertHeight(hero.GetAlertRange());
				combatData.alertRangeGizmo.Show();
			}
		}
		else
		{
			combatData.alertRangeGizmo.Hide();
		}
	}

	private void UpdateGunAmmo()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		zbHeroQiang zbHeroQiang2 = (zbHeroQiang)combatData.masterTeam.GetHeroBySkillType(ESkillType.Qiang);
		if ((Object)(object)zbHeroQiang2 != (Object)null)
		{
			zbSafeInt currentAmmo = zbHeroQiang2.currentAmmo;
			string text = currentAmmo.ToString() + "/" + zbHeroQiang2.maxAmmo;
			zbUtil.SetLabelIfChanged(m_lbGunAmmo, text);
			zbUtil.SetLabelIfChanged(m_lbGunAmmoSelf, text);
			m_gunProgress.progress = 100f - zbHeroQiang2.progress;
			m_gunProgressSelf.progress = 100f - zbHeroQiang2.progress;
			if (m_LoadBg1.gameObject.activeSelf != zbHeroQiang2.progress < 100f)
			{
				m_LoadBg1.gameObject.SetActive(zbHeroQiang2.progress < 100f);
			}
			if (m_LoadBg1.gameObject.activeSelf)
			{
				m_LoadBg1.transform.position = new Vector3(((Component)zbHeroQiang2).transform.position.x, ((Component)zbHeroQiang2).transform.position.y + 1f, -2f);
				Vector2 size = m_Load1Renderer1.size;
				size.y = 0.4f * zbHeroQiang2.progress / 100f;
				m_Load1Renderer1.size = size;
				m_Load1.transform.localPosition = new Vector3(0f, (0f - (0.4f - size.y)) / 2f, -0.1f);
			}
			zbUtil.SetSpineVisibleWithFreeze(spineGunWuxian, zbHeroQiang2.limitlessTime > 0f);
			zbUtil.SetSpineVisibleWithFreeze(spineGunWuxianSelf, zbHeroQiang2.limitlessTime > 0f);
		}
		if (combatData.helperTeam != null)
		{
			zbHeroQiang zbHeroQiang3 = (zbHeroQiang)combatData.helperTeam.GetHeroBySkillType(ESkillType.Qiang);
			if ((Object)(object)zbHeroQiang3 != (Object)null)
			{
				if (m_LoadBg2.gameObject.activeSelf != zbHeroQiang3.progress < 100f)
				{
					m_LoadBg2.gameObject.SetActive(zbHeroQiang3.progress < 100f);
				}
				if (m_LoadBg2.gameObject.activeSelf)
				{
					m_LoadBg2.transform.position = new Vector3(((Component)zbHeroQiang3).transform.position.x, ((Component)zbHeroQiang3).transform.position.y + 1f, -2f);
					Vector2 size2 = m_Load1Renderer2.size;
					size2.y = 0.4f * zbHeroQiang3.progress / 100f;
					m_Load1Renderer2.size = size2;
					m_Load2.transform.localPosition = new Vector3(0f, (0f - (0.4f - size2.y)) / 2f, -0.1f);
				}
			}
		}
		UpdateEnemyGunAmmo();
	}

	private void UpdateEnemyGunAmmo()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		if (!zbCombatSystem.Instance.isPVPBattleType)
		{
			return;
		}
		zbHeroQiang zbHeroQiang2 = (zbHeroQiang)zbCombatSystem.Instance.combatData2.masterTeam.GetHeroBySkillType(ESkillType.Qiang);
		if ((Object)(object)zbHeroQiang2 != (Object)null)
		{
			Label lbGunAmmoEnemy = m_lbGunAmmoEnemy;
			zbSafeInt currentAmmo = zbHeroQiang2.currentAmmo;
			zbUtil.SetLabelIfChanged(lbGunAmmoEnemy, currentAmmo.ToString() + "/" + zbHeroQiang2.maxAmmo);
			m_gunProgressEnemy.progress = 100f - zbHeroQiang2.progress;
			if (m_LoadBg1Enemy.gameObject.activeSelf != zbHeroQiang2.progress < 100f)
			{
				m_LoadBg1Enemy.gameObject.SetActive(zbHeroQiang2.progress < 100f);
			}
			if (m_LoadBg1Enemy.gameObject.activeSelf)
			{
				m_LoadBg1Enemy.transform.position = new Vector3(((Component)zbHeroQiang2).transform.position.x, ((Component)zbHeroQiang2).transform.position.y + 1f, -2f);
				Vector2 size = m_Load1Renderer1Enemy.size;
				size.y = 0.4f * zbHeroQiang2.progress / 100f;
				m_Load1Renderer1Enemy.size = size;
				m_Load1Enemy.transform.localPosition = new Vector3(0f, (0f - (0.4f - size.y)) / 2f, -0.1f);
			}
			zbUtil.SetSpineVisibleWithFreeze(spineGunWuxianEnemy, zbHeroQiang2.limitlessTime > 0f);
		}
		if (zbCombatSystem.Instance.combatData2.helperTeam == null)
		{
			return;
		}
		zbHeroQiang zbHeroQiang3 = (zbHeroQiang)zbCombatSystem.Instance.combatData2.helperTeam.GetHeroBySkillType(ESkillType.Qiang);
		if ((Object)(object)zbHeroQiang3 != (Object)null)
		{
			if (m_LoadBg2Enemy.gameObject.activeSelf != zbHeroQiang3.progress < 100f)
			{
				m_LoadBg2Enemy.gameObject.SetActive(zbHeroQiang3.progress < 100f);
			}
			if (m_LoadBg2Enemy.gameObject.activeSelf)
			{
				m_LoadBg2Enemy.transform.position = new Vector3(((Component)zbHeroQiang3).transform.position.x, ((Component)zbHeroQiang3).transform.position.y + 1f, -2f);
				Vector2 size2 = m_Load1Renderer2.size;
				size2.y = 0.4f * zbHeroQiang3.progress / 100f;
				m_Load1Renderer2Enemy.size = size2;
				m_Load2Enemy.transform.localPosition = new Vector3(0f, (0f - (0.4f - size2.y)) / 2f, -0.1f);
			}
		}
	}

	private void UpdateSkill()
	{
		int num = 0;
		foreach (zbHero hero in combatData.masterTeam.heroList)
		{
			if (hero.skillDetail.SkillType == ESkillType.Qiang)
			{
				continue;
			}
			if (hero.needDurationFlag)
			{
				hero.needDurationProgress = Math.Max(hero.needDurationProgress - Time.deltaTime, 0f);
				if (hero.needDurationProgress > 0f)
				{
					hero.goingOnHowLong += Time.deltaTime;
				}
				if (m_topUiSkillVisible)
				{
					m_skillOutProgress[num].progress = 100f * Math.Clamp((hero.needDurationProgressMax + hero.needDurationProgressMaxTemp - hero.needDurationProgress) / (hero.needDurationProgressMax + hero.needDurationProgressMaxTemp), 0f, 1f);
				}
				m_skillOutProgressSelf[num].progress = 100f * Math.Clamp((hero.needDurationProgressMax + hero.needDurationProgressMaxTemp - hero.needDurationProgress) / (hero.needDurationProgressMax + hero.needDurationProgressMaxTemp), 0f, 1f);
			}
			float num2 = Math.Clamp(hero.nextFireTime / hero.GetAttrValueByType(EAttrSkillType.Cd), 0f, 1f);
			if (m_topUiSkillVisible)
			{
				m_skillInProgress[num].progress = 100f * num2;
			}
			m_skillInProgressSelf[num].progress = 100f * num2;
			num++;
		}
		zbCombatTeam masterTeam = zbCombatSystem.Instance.combatData.masterTeam;
		zbUtil.SetSpineVisibleWithFreeze(spineMechaProgress, masterTeam.mechaCallCount < masterTeam.mechaMaxCount);
		zbCombatTeam helperTeam = zbCombatSystem.Instance.combatData.helperTeam;
		if (helperTeam != null)
		{
			zbUtil.SetSpineVisibleWithFreeze(spineMechaProgress2, helperTeam.mechaCallCount < helperTeam.mechaMaxCount);
		}
		UpdateEnemySkill();
	}

	private void UpdateEnemySkill()
	{
		if (!zbCombatSystem.Instance.isPVPBattleType)
		{
			return;
		}
		int num = 0;
		foreach (zbHero hero in zbCombatSystem.Instance.combatData2.masterTeam.heroList)
		{
			if (hero.skillDetail.SkillType == ESkillType.Qiang)
			{
				continue;
			}
			if (hero.needDurationFlag)
			{
				hero.needDurationProgress = Math.Max(hero.needDurationProgress - Time.deltaTime, 0f);
				if (hero.needDurationProgress > 0f)
				{
					hero.goingOnHowLong += Time.deltaTime;
				}
				m_skillOutProgressEnemy[num].progress = 100f * Math.Clamp((hero.needDurationProgressMax + hero.needDurationProgressMaxTemp - hero.needDurationProgress) / (hero.needDurationProgressMax + hero.needDurationProgressMaxTemp), 0f, 1f);
			}
			float num2 = Math.Clamp(hero.nextFireTime / hero.GetAttrValueByType(EAttrSkillType.Cd), 0f, 1f);
			m_skillInProgressEnemy[num].progress = 100f * num2;
			num++;
		}
	}

	private void UpdateLevelName()
	{
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		switch (battleType)
		{
		case EBattleFuncType.DungeonEasyFight:
		case EBattleFuncType.DungeonRiotFight:
		case EBattleFuncType.DungeonBossFight:
			((TextElement)m_lbLevelName).text = zbUtil.Language(zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(enemyId).name);
			break;
		case EBattleFuncType.MainQuest:
			((TextElement)m_lbLevelName).text = zbResourceHelper.GetMainLevelName(enemyId);
			break;
		case EBattleFuncType.EliteQuest:
			((TextElement)m_lbLevelName).text = zbResourceHelper.GetEliteLevelName(enemyId);
			break;
		case EBattleFuncType.LegionBoss:
			((TextElement)m_lbLevelName).text = zbUtil.Language(zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecordByKey(enemyId).name);
			break;
		case EBattleFuncType.WorldBoss:
			((TextElement)m_lbLevelName).text = zbUtil.Language(zbDataConfig.Instance.worldBossCsvTable.GetRecordByKey(enemyId).levelName);
			break;
		case EBattleFuncType.WorldRescueDay:
		case EBattleFuncType.WorldRescueNight:
		case EBattleFuncType.WorldEventFightDay:
		case EBattleFuncType.WorldEventFightNight:
			((TextElement)m_lbLevelName).text = zbUtil.Language("lang.10441");
			break;
		case EBattleFuncType.HeroTrialQiang:
		case EBattleFuncType.HeroTrialPao:
		case EBattleFuncType.HeroTrialJiguang:
		case EBattleFuncType.HeroTrialBingdan:
		case EBattleFuncType.HeroTrialRanshaodan:
		case EBattleFuncType.HeroTrialHongzha:
		case EBattleFuncType.HeroTrialDianci:
		case EBattleFuncType.HeroTrialWurenji:
		case EBattleFuncType.HeroTrialMotuo:
		case EBattleFuncType.HeroTrialBingbao:
		case EBattleFuncType.HeroTrialShexian:
		case EBattleFuncType.HeroTrialDianqiu:
		case EBattleFuncType.HeroTrialLongjuan:
		case EBattleFuncType.HeroTrialFengren:
			((TextElement)m_lbLevelName).text = zbUtil.Language(zbDataConfig.Instance.heroTrialCsvTable.GetRecordByKey(enemyId).name);
			break;
		case EBattleFuncType.EndlessHorde:
			((TextElement)m_lbLevelName).text = zbUtil.Language("lang.10814");
			break;
		case EBattleFuncType.Legendary:
			((TextElement)m_lbLevelName).text = zbResourceHelper.GetLegendaryLevelName(enemyId);
			break;
		case EBattleFuncType.GuardVerify:
			((TextElement)m_lbLevelName).text = zbUtil.Language("lang.10875");
			break;
		case EBattleFuncType.Arena:
			((TextElement)m_lbLevelName).text = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey(45).name;
			break;
		case EBattleFuncType.CrossLadderMatch:
		case EBattleFuncType.CrossLadderUpgrade:
			((TextElement)m_lbLevelName).text = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey(53).name;
			break;
		case EBattleFuncType.MineWar:
		case EBattleFuncType.CrossMine:
		{
			zbMineNodeCsvRecord recordByKey5 = zbDataConfig.Instance.mineNodeCsvTable.GetRecordByKey(zbSelf.Instance.mineWarHandler.currPointId);
			((TextElement)m_lbLevelName).text = recordByKey5.name;
			break;
		}
		case EBattleFuncType.PeakArena:
			((TextElement)m_lbLevelName).text = zbUtil.Language("lang.11088");
			break;
		case EBattleFuncType.Truck:
		case EBattleFuncType.Train:
			((TextElement)m_lbLevelName).text = zbUtil.Language("lang.11102");
			break;
		case EBattleFuncType.WorldLair:
		case EBattleFuncType.WorldLairHelp:
		{
			zbWorldLairCsvRecord recordByKey4 = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(enemyId);
			((TextElement)m_lbLevelName).text = zbUtil.Language(recordByKey4.name);
			break;
		}
		case EBattleFuncType.BossSubject:
		{
			zbBossSubjectCsvRecord recordByKey3 = zbDataConfig.Instance.bossSubjectCsvTable.GetRecordByKey(enemyId);
			((TextElement)m_lbLevelName).text = zbUtil.Language(recordByKey3.name);
			break;
		}
		case EBattleFuncType.WallSubject:
		{
			zbWallSubjectCsvRecord recordByKey2 = zbDataConfig.Instance.wallSubjectCsvTable.GetRecordByKey(enemyId);
			((TextElement)m_lbLevelName).text = zbUtil.Language(recordByKey2.name);
			break;
		}
		case EBattleFuncType.MechaSubject:
		{
			zbMechaSubjectCsvRecord recordByKey = zbDataConfig.Instance.mechaSubjectCsvTable.GetRecordByKey(enemyId);
			((TextElement)m_lbLevelName).text = zbUtil.Language(recordByKey.name);
			break;
		}
		case (EBattleFuncType)6:
		case (EBattleFuncType)17:
		case (EBattleFuncType)18:
		case (EBattleFuncType)19:
		case (EBattleFuncType)20:
		case EBattleFuncType.ElementTrialElec:
		case EBattleFuncType.ElementTrialFire:
		case EBattleFuncType.ElementTrialIce:
		case EBattleFuncType.ElementTrialPhy:
		case EBattleFuncType.ElementTrialEnergy:
		case EBattleFuncType.ElementTrialWind:
		case EBattleFuncType.LegionWar:
		case EBattleFuncType.Facility:
		case EBattleFuncType.Season:
			break;
		}
	}

	public void ActionBossComing()
	{
		SkeletonAnimation component = bossCome.GetComponent<SkeletonAnimation>();
		bossCome.SetActive(true);
		component.AnimationState.SetAnimation(0, "animation", false);
		zbAudioMgr.Instance.PlayAudio("audio/combat/bosscoming.ogg");
	}

	public override void OnScreenSizeChnaged()
	{
		base.OnScreenSizeChnaged();
		zbCombatSystem.Instance.combatData.combatHud.m_hudPool.CheckHudRootWorldBoundPosition();
	}

	public void SetVisibleUI(bool flag)
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootHud", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootBubble", (string)null);
		if (flag)
		{
			val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			val2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbCombatSystem.Instance.isPVPBattleType)
			{
				m_camera1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_camera2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_pvpVS.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_pvpUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_eleTopUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_topUiSkillVisible = false;
			}
			else
			{
				m_camera1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_camera2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_pvpUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_eleTopUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_topUiSkillVisible = true;
			}
		}
		else
		{
			m_eleTopUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_topUiSkillVisible = false;
			val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			val2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_pvpUI.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public bool HaveMechaActiving()
	{
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			return false;
		}
		if ((Object)(object)zbCombatSystem.Instance.combatData.masterTeam.mecha != (Object)null && zbCombatSystem.Instance.combatData.masterTeam.mecha.mechaActiving)
		{
			return true;
		}
		if (zbCombatSystem.Instance.combatData.helperTeam != null && (Object)(object)zbCombatSystem.Instance.combatData.helperTeam.mecha != (Object)null && zbCombatSystem.Instance.combatData.helperTeam.mecha.mechaActiving)
		{
			return true;
		}
		return false;
	}

	public void OnMecha(ClickEvent evt, int index, zbCombatData combatData = null)
	{
		if (HaveMechaActiving())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10817"));
			return;
		}
		zbCombatTeam zbCombatTeam2 = null;
		if (index == 1)
		{
			zbCombatTeam2 = ((combatData == null) ? zbCombatSystem.Instance.combatData.masterTeam : combatData.masterTeam);
			if (zbCombatTeam2.mechaCallCount >= zbCombatTeam2.mechaMaxCount)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10845"));
				return;
			}
		}
		else
		{
			zbCombatTeam2 = ((combatData == null) ? zbCombatSystem.Instance.combatData.helperTeam : combatData.helperTeam);
			if (zbCombatTeam2.mechaCallCount >= zbCombatTeam2.mechaMaxCount)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10845"));
				return;
			}
		}
		zbCombatTeam obj = zbCombatTeam2;
		int mechaCallCount = obj.mechaCallCount + 1;
		obj.mechaCallCount = mechaCallCount;
		zbCombatTeam2.mecha.TriggerMecha();
	}

	public void PlayIconScaleAnim()
	{
		if (!isIconAnimationPlaying)
		{
			((MonoBehaviour)this).StartCoroutine(IconScaleCoroutine());
		}
	}

	private IEnumerator IconScaleCoroutine()
	{
		isIconAnimationPlaying = true;
		Vector3 originalScale = m_iconZombie.transform.scale;
		Vector3 targetScale = originalScale * 1.2f;
		float duration = 0.2f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float num = EaseInOutCubic(t);
			m_iconZombie.transform.scale = Vector3.Lerp(originalScale, targetScale, num);
			yield return null;
		}
		elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t2 = Mathf.Clamp01(elapsed / duration);
			float num2 = EaseInOutCubic(t2);
			m_iconZombie.transform.scale = Vector3.Lerp(targetScale, originalScale, num2);
			yield return null;
		}
		m_iconZombie.transform.scale = originalScale;
		isIconAnimationPlaying = false;
	}

	private float EaseInOutCubic(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
		}
		return 4f * t * t * t;
	}
}
