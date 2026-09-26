using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbServantItem>
	{
	}

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private VisualElement m_spriteSkill;

	private VisualElement m_spriteLock;

	private VisualElement m_spriteDeploy;

	private Label m_lbLevel;

	private Label m_lbName;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private zbServantWnd m_wndParent;

	private ServantDetail m_detail;

	private zbServantCsvRecord m_record;

	private VisualElement m_notify;

	private bool m_needTouch;

	public zbServantItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/servant/tb_servant.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbServantWnd wndParent, bool needTouch = true)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skill", (string)null);
		m_spriteLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_lock", (string)null);
		m_spriteDeploy = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_deploy", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_needTouch = needTouch;
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)this, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(ServantDetail detail)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		m_record = zbDataConfig.Instance.servantCsvTable.GetRecordByKey(m_detail.ServantTid);
		((TextElement)m_lbLevel).text = "Lv." + m_detail.Level;
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(m_detail.Quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		m_spriteLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((detail.Lock != EBoolState.True || detail.Deploy != EBoolState.False) ? 1 : 0));
		m_spriteDeploy.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(detail.Deploy != EBoolState.True));
		zbServantAvatarCsvRecord recordByKey = zbDataConfig.Instance.servantAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.servantIcon));
		}
		string skillTypeIconPathByType = zbResourceHelper.GetSkillTypeIconPathByType((ESkillType)m_record.skillType);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPathByType));
		RefreshSelect(m_detail.ServantId == m_wndParent.m_detail.ServantId);
		RefreshNotify();
	}

	private void RefreshNotify()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_detail.Deploy == EBoolState.True;
		bool flag2 = zbSelf.Instance.servantHandler.CanServantUpgrade(m_detail);
		bool flag3 = zbSelf.Instance.servantHandler.CanServantUpgradeAttr(m_detail);
		bool flag4 = zbSelf.Instance.servantHandler.CanServantUpgradeMainSkill(m_detail);
		bool flag5 = zbSelf.Instance.servantHandler.CanServantUpgradeSubSkill(m_detail);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || !(flag2 | flag3 | flag4 | flag5)) ? 1 : 0));
	}

	public void RefreshSelect(bool flag)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (m_needTouch)
		{
			m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	public void OnTouch()
	{
		if (m_needTouch)
		{
			m_wndParent.m_needScroll = false;
			m_wndParent.m_detail = m_detail;
			m_wndParent.RefreshCustom(m_detail, isFirstIn: false);
		}
	}
}
