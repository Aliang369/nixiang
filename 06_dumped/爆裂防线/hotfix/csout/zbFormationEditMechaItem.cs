using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationEditMechaItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteMecha;

	private Label m_lbName;

	private Label m_lbLevel;

	private VisualElement m_spriteMask;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbFormationWnd m_wndParent;

	private zbMechaCsvRecord m_record;

	private MechaDetail m_data;

	public zbFormationEditMechaItem(zbFormationWnd wndParent)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/formation/tb_formationEditMecha.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteMecha = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_spriteMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_mask", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-3f, 0f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbMechaCsvRecord record)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_data = zbSelf.Instance.mechaHandler.GetMechaDetail(m_record.id);
		if (m_data != null)
		{
			((TextElement)m_lbLevel).text = "Lv." + m_data.Level;
		}
		m_spriteMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data != null));
		((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data == null));
		((TextElement)m_lbName).text = m_record.name;
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)m_record.quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			m_spriteMecha.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasMechaNotify(m_record.id)));
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetMechaId() != m_record.id) ? 1 : 0));
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_data == null && !zbSelf.Instance.mechaHandler.HasMechaActiveNotify(m_record.id))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, 0L);
		}
		else
		{
			zbUIMgr.Instance.ShowMechaDetail(m_record);
		}
	}
}
