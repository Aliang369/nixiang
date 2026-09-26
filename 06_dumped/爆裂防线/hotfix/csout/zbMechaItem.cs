using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteMecha;

	private Label m_lbLevel;

	private VisualElement[] m_spriteStars;

	private VisualElement m_spriteUsed;

	private VisualElement m_spriteSelect;

	private VisualElement m_spriteMask;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbMechaWnd m_wndParent;

	private zbMechaCsvRecord m_record;

	private MechaDetail m_data;

	public zbMechaItem(zbMechaWnd wndParent)
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		m_spriteStars = (VisualElement[])(object)new VisualElement[5];
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mecha/tb_mecha.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteMecha = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "star" + i, (string)null);
		}
		m_spriteUsed = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_used", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_spriteMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_mask", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-3f, -2f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbMechaCsvRecord record)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_data = zbSelf.Instance.mechaHandler.GetMechaDetail(m_record.id);
		if (m_data != null)
		{
			((TextElement)m_lbLevel).text = "Lv." + m_data.Level;
			zbUIEffect.SetStars(m_spriteStars, m_data.Star);
		}
		else
		{
			zbUIEffect.SetStars(m_spriteStars, 0);
		}
		m_spriteUsed.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetMechaId() != m_record.id) ? 1 : 0));
		m_spriteMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data != null));
		((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data == null));
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)m_record.quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			m_spriteMecha.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasMechaNotify(m_record.id)));
		RefreshSelect(m_record.id == m_wndParent.m_selectTid);
	}

	public void RefreshSelect(bool flag)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void OnTouch()
	{
		m_wndParent.m_needScroll = false;
		m_wndParent.RefreshCustom(m_record, isFirstIn: false);
	}
}
