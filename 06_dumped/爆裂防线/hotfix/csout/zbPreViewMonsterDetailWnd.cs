using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPreViewMonsterDetailWnd : zbBaseWnd
{
	private VisualElement m_root;

	private Button m_btnTouchTest;

	private Button m_btnClose;

	private VisualElement m_spriteBg1;

	private VisualElement m_spriteBg2;

	private SpineVisualElement m_spineMonster;

	private Label m_lbName;

	private Label m_lbDesc;

	private Label m_lbHandleBookDesc;

	private Label m_lbNoSkill;

	private VisualElement m_nodeType;

	private Button m_btnType;

	private VisualElement m_tipType;

	private Label m_lbType;

	private IVisualElementScheduledItem m_hideSchedule;

	private Button[] m_btnSkillTypes = (Button[])(object)new Button[6];

	private zbMonsterCsvRecord m_record;

	private List<int> m_skillTypeList = new List<int>();

	private float m_bgHeight = 319f;

	private float m_scrollSpeed = 30f;

	private float m_bg1Top;

	private float m_bg2Top;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_root = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "close", (string)null);
		m_spriteBg1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg1", (string)null);
		m_spriteBg2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg2", (string)null);
		m_spineMonster = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_monster", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbHandleBookDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_handleBookDesc", (string)null);
		m_lbNoSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_noSkill", (string)null);
		m_nodeType = UQueryExtensions.Q<VisualElement>(m_RootElement, "attrType", (string)null);
		m_btnType = UQueryExtensions.Q<Button>(m_nodeType, "btn_type", (string)null);
		m_tipType = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip_type", (string)null);
		m_lbType = UQueryExtensions.Q<Label>(m_tipType, "lb_tip", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnType, (Action)OnType, "", (TrickleDown)0);
		for (int i = 0; i < 6; i++)
		{
			m_btnSkillTypes[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_skillType" + i, (string)null);
			int currentIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkillTypes[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillTypes(evt, currentIndex);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(zbMonsterCsvRecord record, bool visUpgrade = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		m_tipType.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bg1Top = 0f;
		m_bg2Top = m_bgHeight;
		m_spriteBg1.style.top = StyleLength.op_Implicit(m_bg1Top);
		m_spriteBg2.style.top = StyleLength.op_Implicit(m_bg2Top);
		m_record = record;
		((TextElement)m_lbName).text = m_record.name;
		if (m_record.elementType == 0)
		{
			m_nodeType.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_nodeType.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnType).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/monster/monster_type{m_record.elementType}.png"));
			((TextElement)m_lbType).text = zbSelf.Instance.handbookHandler.GetElementTypeString(m_record.elementType);
		}
		bool num = zbSelf.Instance.handbookHandler.CheckMonsterIsActive(m_record.id);
		zbHandbookCsvRecord recordByKey = zbDataConfig.Instance.handbookCsvTable.GetRecordByKey(m_record.handbookId);
		((TextElement)m_lbHandleBookDesc).text = zbUtil.Language(recordByKey.desc2);
		if (!num)
		{
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10595");
		}
		else
		{
			zbLabelLinkUtil.BuildModLink(recordByKey.desc, m_lbDesc);
			((TextElement)m_lbDesc).text = ((TextElement)m_lbDesc).text.Replace("<color=#64FF26>", "<color=#3CB317>");
		}
		zbMonsterAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		zbUtil.RefreshSpineUI(m_spineMonster, recordByKey2.fightBone + "_SkeletonData.asset", "move", loop: true);
		if (m_record.type == 3)
		{
			((VisualElement)m_spineMonster).transform.scale = new Vector3(0.5f * record.bodyScale, 0.5f * record.bodyScale, 0.5f * record.bodyScale);
		}
		else
		{
			((VisualElement)m_spineMonster).transform.scale = new Vector3(1f * record.bodyScale, 1f * record.bodyScale, 1f * record.bodyScale);
		}
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_btnSkillTypes[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num2 = 0;
		m_skillTypeList.Clear();
		int[] adviceType = m_record.adviceType;
		foreach (int num3 in adviceType)
		{
			zbSkillTypeCsvRecord recordByKey3 = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(num3);
			if (recordByKey3 != null)
			{
				m_skillTypeList.Add(num3);
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey3.icon1);
				((VisualElement)m_btnSkillTypes[num2]).style.backgroundImage = StyleBackground.op_Implicit(val);
				((VisualElement)m_btnSkillTypes[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				num2++;
			}
		}
		((VisualElement)m_lbNoSkill).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num2 != 0) ? 1 : 0));
	}

	private void OnType()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_tipType.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_hideSchedule != null)
		{
			m_hideSchedule.Pause();
			m_hideSchedule = null;
		}
		m_hideSchedule = m_RootElement.schedule.Execute((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_tipType.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}).StartingIn(5000L);
	}

	public void OnSkillTypes(ClickEvent evt, int index)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (m_skillTypeList[index] > 0)
		{
			Vector2 val = VisualElementExtensions.WorldToLocal(m_root, Vector2.op_Implicit(((PointerEventBase<ClickEvent>)(object)evt).position));
			val.x = Mathf.Clamp(val.x - 160f, 20f, 340f);
			val.y = ((val.y < 840f) ? (val.y + 50f) : (val.y - 280f));
			zbUIMgr.Instance.ShowTipSkillType(m_skillTypeList[index], val);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		m_bg1Top -= m_scrollSpeed * Time.deltaTime;
		m_bg2Top -= m_scrollSpeed * Time.deltaTime;
		if (m_bg1Top <= 0f - m_bgHeight)
		{
			m_bg1Top = m_bg2Top + m_bgHeight;
		}
		if (m_bg2Top <= 0f - m_bgHeight)
		{
			m_bg2Top = m_bg1Top + m_bgHeight;
		}
		m_spriteBg1.style.top = StyleLength.op_Implicit(m_bg1Top);
		m_spriteBg2.style.top = StyleLength.op_Implicit(m_bg2Top);
	}
}
