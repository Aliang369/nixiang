using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPreviewWallWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private zbWallAvatar wallAvatar;

	private Label m_lbName;

	private VisualElement m_nodeStar;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbAttr;

	private Label m_lbTip;

	private VisualElement m_nodeSkill;

	private VisualElement m_spriteSkillFrame;

	private VisualElement m_spriteSkill;

	private Label m_lbDesc;

	private Button m_btnTouch;

	private Button m_btnList;

	private Button m_btnPreview;

	private int m_id;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = m_GameObjectSceneRoot.GetComponentInChildren<zbSpineScene>();
		wallAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("wallAvatar")).GetComponent<zbWallAvatar>();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>(m_RootElement, "stars", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeStar, "star" + i, (string)null);
		}
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_nodeSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill", (string)null);
		m_spriteSkillFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_RootElement, "btn_list", (string)null);
		m_btnPreview = UQueryExtensions.Q<Button>(m_RootElement, "btn_preview", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnList, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPreview, (Action)OnList, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void RefreshCustom(int id, int star = 1)
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		zbUIEffect.SetStars(m_spriteStars, star);
		zbWallCsvRecord wallRecord = zbDataConfig.Instance.wallCsvTable.GetRecords().records.Values.FirstOrDefault((zbWallCsvRecord record) => record.propId == id);
		if (wallRecord != null)
		{
			m_id = wallRecord.id;
			if (wallRecord.backGround != "")
			{
				m_spineScene.skeletonObject.SetActive(true);
				m_spineScene.SetSceneBg(wallRecord.backGround);
				m_spineScene.SetSkeletonDataAsset(wallRecord.backEffect + "_SkeletonData.asset", defaultZ: false);
				m_spineScene.SetAnimation("animation", isLoop: true);
				((VisualElement)m_lbName).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/wall/wall_titleBlue.png"));
			}
			else
			{
				m_spineScene.skeletonObject.SetActive(false);
				m_spineScene.SetSceneBg("ui/image/wall/wall_bg.jpg");
				((VisualElement)m_lbName).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/wall/wall_title.png"));
			}
			((TextElement)m_lbName).text = wallRecord.name;
			wallAvatar.RefreshCustom(wallRecord.id);
			if (wallRecord.skillIcon == "")
			{
				m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(wallRecord.skillIcon));
				((TextElement)m_lbDesc).text = zbSelf.Instance.wallHandler.GetSkillDesc(wallRecord.id, star);
			}
			zbWallStarUpgradeCsvRecord zbWallStarUpgradeCsvRecord2 = zbDataConfig.Instance.wallStarUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbWallStarUpgradeCsvRecord r) => r.id == wallRecord.starUpgradeId && r.star == star);
			if (zbWallStarUpgradeCsvRecord2 != null)
			{
				((TextElement)m_lbAttr).text = zbUtil.Language("lang.10839") + $"<color=#FFE600>+{zbWallStarUpgradeCsvRecord2.damageImmunity}</color> ";
			}
		}
	}

	private void OnList()
	{
		zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_id, EPreviewType.Wall);
	}

	private void OnTouch()
	{
		Hide();
	}
}
