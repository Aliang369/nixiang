using System;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPreviewWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private zbPlayerAvatar m_spineAvatar;

	private SkeletonAnimation m_spineGun;

	private SkeletonAnimation m_spineHero;

	private SkeletonAnimation m_spineMecha;

	public SpriteRenderer m_spriteYingzi;

	private SkeletonAnimation m_spineFoothold;

	private Label m_lbName;

	private VisualElement m_nodeStar;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbAttr;

	private Label m_lbTip;

	private VisualElement m_nodeSkill;

	private VisualElement m_mechaSkillsWrapper;

	private VisualElement m_spriteSkillFrame;

	private VisualElement m_spriteSkill;

	private Label m_lbDesc;

	private Button m_btnTouch;

	private Button m_btnClose;

	private Button m_btnSuitPreview;

	private Button m_btnList;

	private VisualElement[] m_mechaSkills = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_btnsMechaSkills = (Button[])(object)new Button[5];

	private Button m_btnMask;

	private Label m_lbMechaSkill;

	private EPreviewType m_type;

	private int m_id;

	private int m_star;

	private string m_attrValue;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = m_GameObjectSceneRoot.GetComponentInChildren<zbSpineScene>();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_spineGun = ((Component)m_GameObjectSceneRoot.transform.Find("gunAnimation")).GetComponent<SkeletonAnimation>();
		m_spineHero = ((Component)m_GameObjectSceneRoot.transform.Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		m_spineMecha = ((Component)m_GameObjectSceneRoot.transform.Find("mecha")).GetComponent<SkeletonAnimation>();
		m_spriteYingzi = ((Component)m_GameObjectSceneRoot.transform.Find("yingziCom")).GetComponent<SpriteRenderer>();
		m_spineFoothold = ((Component)m_GameObjectSceneRoot.transform.Find("foothold")).GetComponent<SkeletonAnimation>();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>(m_RootElement, "stars", (string)null);
		m_mechaSkillsWrapper = UQueryExtensions.Q<VisualElement>(m_RootElement, "mechaSkills", (string)null);
		m_btnMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMask, (Action)OnMask, "", (TrickleDown)0);
		m_lbMechaSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_mechaSkill", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_mechaSkills[i] = UQueryExtensions.Q<VisualElement>(m_mechaSkillsWrapper, "skill" + i, (string)null);
			m_btnsMechaSkills[i] = UQueryExtensions.Q<Button>(m_mechaSkillsWrapper, "btn_skill" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsMechaSkills[i], (Action)delegate
			{
				OnMechaSkills(tempIndex);
			}, "", (TrickleDown)0);
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeStar, "star" + i, (string)null);
		}
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_nodeSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill", (string)null);
		m_spriteSkillFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnSuitPreview = UQueryExtensions.Q<Button>(m_RootElement, "btn_suitPreview", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_RootElement, "btn_list", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSuitPreview, (Action)OnSuitPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnList, "", (TrickleDown)0);
	}

	public void RefreshCustom(EPreviewType type, int id, int star = 1, string attrValue = "")
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		m_type = type;
		m_id = id;
		m_star = star;
		m_attrValue = attrValue;
		zbUIEffect.SetStars(m_spriteStars, star);
		((Component)m_spineAvatar).gameObject.SetActive(type == EPreviewType.Suit);
		((Component)m_spineGun).gameObject.SetActive(type == EPreviewType.Gun);
		((Component)m_spineHero).gameObject.SetActive(type == EPreviewType.Hero);
		((Component)m_spineMecha).gameObject.SetActive(type == EPreviewType.Mecha);
		((Component)m_spriteYingzi).gameObject.SetActive(type == EPreviewType.Hero || type == EPreviewType.Mecha);
		((Component)m_spineFoothold).gameObject.SetActive(type == EPreviewType.Foothold);
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(type == EPreviewType.Hero));
		((VisualElement)m_lbAttr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(type == EPreviewType.Hero));
		((VisualElement)m_lbTip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(type == EPreviewType.Hero));
		((VisualElement)m_btnList).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((type != EPreviewType.Suit && type != EPreviewType.Wall) ? 1 : 0));
		m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(type == EPreviewType.Mecha));
		m_mechaSkillsWrapper.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(type != EPreviewType.Mecha));
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (type == EPreviewType.Gun)
		{
			m_spineScene.SetSceneBg("ui/image/suitGun/gun_bg.jpg");
			((Component)m_spineScene.skeletonAnimation).gameObject.SetActive(true);
			m_spineScene.SetSkeletonDataAsset("animation/gun/gun_scene/qiangxie_bg_SkeletonData.asset");
		}
		else
		{
			m_spineScene.SetSceneBg("ui/image/suitGun/suit_bg.jpg");
			((Component)m_spineScene.skeletonAnimation).gameObject.SetActive(true);
			m_spineScene.SetSkeletonDataAsset("animation/suit/suit_scene/shizhuang_bg_SkeletonData.asset");
		}
		m_spriteSkillFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/suitGun/skill_empty.png"));
		m_spriteSkill.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(0.8f, 0.8f)));
		switch (type)
		{
		case EPreviewType.Suit:
			RefreshSuit(id);
			break;
		case EPreviewType.Gun:
			RefreshGun(id);
			break;
		case EPreviewType.Hero:
			RefreshHero(id);
			break;
		case EPreviewType.Mecha:
			RefreshMecha(id);
			break;
		case EPreviewType.Foothold:
			RefreshFoothold(id);
			break;
		}
		AutoFontSize();
	}

	private void AutoFontSize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		float num = 24f;
		((VisualElement)m_lbDesc).style.fontSize = StyleLength.op_Implicit(num);
		if (((TextElement)m_lbDesc).MeasureTextSize(((TextElement)m_lbDesc).text, 547f, (MeasureMode)0, 0f, (MeasureMode)0).y > 70f)
		{
			num = 18f;
		}
		((VisualElement)m_lbDesc).style.fontSize = StyleLength.op_Implicit(num);
		zbUtil.AutoFontSize((TextElement)(object)m_lbTip, 20f, 16f, 720f);
	}

	public void RefreshSuit(int id)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord record) => record.propId == id);
		if (zbPlayerSuitCsvRecord2 != null)
		{
			m_spineAvatar.RefreshCustom(zbPlayerSuitCsvRecord2.avatarId, zbSelf.Instance.GetGunId());
			((TextElement)m_lbName).text = zbPlayerSuitCsvRecord2.name;
			if (zbPlayerSuitCsvRecord2.grade == 2)
			{
				m_spriteSkillFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/suitGun/skill_empty1.png"));
				m_spriteSkill.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(1f, 1f)));
				zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_spineAvatar);
			}
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(zbPlayerSuitCsvRecord2.skillIcon));
			((TextElement)m_lbDesc).text = zbSelf.Instance.suitGunHandler.GetSkillDesc(zbPlayerSuitCsvRecord2.id, m_star);
			zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(zbPlayerSuitCsvRecord2.id, m_star);
			((TextElement)m_lbAttr).text = string.Format(zbUtil.Language("lang.10582"), suitStarUpgrade.atk);
		}
	}

	public void RefreshGun(int id)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerGunCsvRecord zbPlayerGunCsvRecord2 = zbDataConfig.Instance.playerGunCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerGunCsvRecord record) => record.propId == id);
		if (zbPlayerGunCsvRecord2 != null)
		{
			zbUtil.RefreshSpine(m_spineGun, zbPlayerGunCsvRecord2.frontBone + "_SkeletonData.asset", "animation", loop: true);
			((TextElement)m_lbName).text = zbPlayerGunCsvRecord2.name;
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(zbPlayerGunCsvRecord2.skillIcon));
			if (zbPlayerGunCsvRecord2.mod2 > 0)
			{
				string text = zbUtil.Language("lang.11155") + ": " + zbUtil.Language(zbPlayerGunCsvRecord2.desc2);
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.11154") + ": " + zbPlayerGunCsvRecord2.desc + "\n<color=#FF7173>" + text + "</color>";
			}
			else
			{
				((TextElement)m_lbDesc).text = zbPlayerGunCsvRecord2.desc;
			}
			zbPlayerGunUpgradeCsvRecord gunStarUpgrade = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(zbPlayerGunCsvRecord2.id, m_star);
			((TextElement)m_lbAttr).text = string.Format(zbUtil.Language("lang.10581"), gunStarUpgrade.dmg * 100f);
		}
	}

	public void RefreshHero(int id)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			((TextElement)m_lbName).text = recordByKey.name;
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				zbUtil.RefreshSpine(m_spineHero, recordByKey2.frontBone + "_SkeletonData.asset", "idle", loop: true);
			}
			zbSkillCsvRecord recordByKey3 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(recordByKey.skillId);
			if (recordByKey3 != null)
			{
				m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey3.icon));
				((TextElement)m_lbDesc).text = recordByKey3.desc;
			}
		}
	}

	public void RefreshMecha(int id)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		zbMechaCsvRecord zbMechaCsvRecord2 = zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values.FirstOrDefault((zbMechaCsvRecord record) => record.propId == id);
		if (zbMechaCsvRecord2 != null)
		{
			((TextElement)m_lbName).text = zbMechaCsvRecord2.name;
			zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(zbMechaCsvRecord2.avatarId);
			if (recordByKey != null)
			{
				zbUtil.RefreshSpine(m_spineMecha, recordByKey.fightBone + "_SkeletonData.asset", "idle", loop: true);
			}
			for (int num = 0; num < 5; num++)
			{
				zbMechaSkillCsvRecord recordByKey2 = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(zbMechaCsvRecord2.skillList[num]);
				m_mechaSkills[num].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
			}
			if (m_attrValue != "")
			{
				((TextElement)m_lbAttr).text = zbUtil.Language("lang.10798") + "+" + m_attrValue;
				return;
			}
			zbMechaStarUpgradeCsvRecord recordByKey3 = zbDataConfig.Instance.mechaStarUpgradeCsvTable.GetRecordByKey(m_star);
			((TextElement)m_lbAttr).text = zbUtil.Language("lang.10798") + "+" + recordByKey3.attack;
		}
	}

	public void RefreshFoothold(int id)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		zbFootholdCsvRecord zbFootholdCsvRecord2 = zbDataConfig.Instance.footholdCsvTable.GetRecords().records.Values.FirstOrDefault((zbFootholdCsvRecord record) => record.propId == id);
		if (zbFootholdCsvRecord2 != null)
		{
			((TextElement)m_lbName).text = zbFootholdCsvRecord2.name;
			zbUtil.RefreshSpine(m_spineFoothold, zbFootholdCsvRecord2.mainScene + "_SkeletonData.asset", "loop", loop: true);
			if (zbFootholdCsvRecord2.icon == "")
			{
				m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(zbFootholdCsvRecord2.icon));
				((TextElement)m_lbDesc).text = zbSelf.Instance.guardHandler.GetFootholdEffectDesc(zbFootholdCsvRecord2);
			}
			zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(zbFootholdCsvRecord2.id, m_star);
			((TextElement)m_lbAttr).text = zbUtil.Language("lang.10862") + "+" + $"{footholdStarUpgradeRecord.dmg:0.##%}";
		}
	}

	private void OnTouch()
	{
		if (m_type == EPreviewType.Suit)
		{
			if (!(m_spineAvatar.GetAnimationName(0) == "show"))
			{
				m_spineAvatar.SetAnimation(0, "show", loop: false);
				m_spineAvatar.AddAnimation(0, "idle", loop: true);
			}
		}
		else if (m_type == EPreviewType.Hero)
		{
			m_spineHero.AnimationState.SetAnimation(0, "show", false);
			m_spineHero.AnimationState.AddAnimation(0, "idle", true, 0f);
		}
		else if (m_type == EPreviewType.Mecha)
		{
			m_spineMecha.AnimationState.SetAnimation(0, "idle", true);
		}
	}

	private void OnMechaSkills(int index)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values.FirstOrDefault((zbMechaCsvRecord record) => record.propId == m_id);
		string text = ((index == 0) ? GetMechaPowerSkillDescByIndex(index) : (zbUtil.Language("lang.11477") + GetMechaBaseSkillDescByIndex(index) + "\n" + string.Format(zbUtil.Language("lang.11478"), index + 1) + GetMechaPowerSkillDescByIndex(index)));
		((TextElement)m_lbMechaSkill).text = text;
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private string GetMechaBaseSkillDescByIndex(int index)
	{
		zbMechaCsvRecord zbMechaCsvRecord2 = zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values.FirstOrDefault((zbMechaCsvRecord record) => record.propId == m_id);
		int num = 10011;
		switch (index)
		{
		case 0:
			num = zbMechaCsvRecord2.skillList[0];
			break;
		case 1:
			num = zbMechaCsvRecord2.skillList[1];
			break;
		case 2:
			num = zbMechaCsvRecord2.skillList[2];
			break;
		case 3:
			num = zbMechaCsvRecord2.skillList[3];
			break;
		case 4:
			num = zbMechaCsvRecord2.skillList[4];
			break;
		}
		zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(num);
		return zbSelf.Instance.mechaHandler.GetMechaSkillDesc(num);
	}

	private string GetMechaPowerSkillDescByIndex(int index)
	{
		zbMechaCsvRecord zbMechaCsvRecord2 = zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values.FirstOrDefault((zbMechaCsvRecord record) => record.propId == m_id);
		int num = 10011;
		switch (index)
		{
		case 0:
			num = zbMechaCsvRecord2.skillList[0];
			break;
		case 1:
			num = zbMechaCsvRecord2.skill2;
			break;
		case 2:
			num = zbMechaCsvRecord2.skill3;
			break;
		case 3:
			num = zbMechaCsvRecord2.skill4;
			break;
		case 4:
			num = zbMechaCsvRecord2.skill5;
			break;
		}
		zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(num);
		return zbSelf.Instance.mechaHandler.GetMechaSkillDesc(num);
	}

	private void OnMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnSuitPreview()
	{
		if (m_type == EPreviewType.Suit)
		{
			zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord record) => record.propId == m_id);
			zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(zbPlayerSuitCsvRecord2.id, m_type);
		}
	}

	private void OnList()
	{
		if (m_type == EPreviewType.Suit)
		{
			zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord record) => record.propId == m_id);
			zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(zbPlayerSuitCsvRecord2.id, m_type);
		}
		if (m_type == EPreviewType.Wall)
		{
			zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_id, EPreviewType.Wall);
		}
		if (m_type == EPreviewType.Mecha)
		{
			zbMechaCsvRecord zbMechaCsvRecord2 = zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values.FirstOrDefault((zbMechaCsvRecord record) => record.propId == m_id);
			zbUIMgr.Instance.ShowMechaSkillUpgradePreview(zbMechaCsvRecord2.avatarId);
		}
	}

	private void OnClose()
	{
		OnMask();
		Hide();
	}
}
