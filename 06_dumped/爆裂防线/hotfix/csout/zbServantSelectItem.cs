using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantSelectItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbServantSelectItem>
	{
	}

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private VisualElement m_spriteSkill;

	private Label m_lbLevel;

	private Label m_lbName;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private zbServantSelectWnd m_wndParent;

	private ServantDetail m_detail;

	private zbServantCsvRecord m_record;

	public zbServantSelectItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/servant/tb_servantSelect.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbServantSelectWnd wndParent)
	{
		m_wndParent = wndParent;
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skill", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(ServantDetail detail)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		m_record = zbDataConfig.Instance.servantCsvTable.GetRecordByKey(m_detail.ServantTid);
		((TextElement)m_lbLevel).text = "Lv." + m_detail.Level;
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(m_detail.Quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		zbServantAvatarCsvRecord recordByKey = zbDataConfig.Instance.servantAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.servantIcon));
		}
		string skillTypeIconPathByType = zbResourceHelper.GetSkillTypeIconPathByType((ESkillType)m_record.skillType);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPathByType));
		RefreshSelect(m_detail.ServantId == m_wndParent.m_selectedServantId);
	}

	public void RefreshSelect(bool flag)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void OnTouch()
	{
		m_wndParent.m_selectedServantId = m_detail.ServantId;
		m_wndParent.RefreshCustom();
	}
}
