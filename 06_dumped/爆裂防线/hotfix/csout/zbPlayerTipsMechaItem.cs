using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerTipsMechaItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteMecha;

	private Label m_lbLevel;

	private Button m_btnTouch;

	private VisualElement m_nodeStar;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private zbPlayerTipsDetailWnd m_wndParent;

	private zbMechaCsvRecord m_record;

	private MechaDetail m_data;

	public zbPlayerTipsMechaItem(zbPlayerTipsDetailWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_tipsMecha.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteMecha = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_bottom", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "iconstar" + i, (string)null);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(MechaDetail data)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_record = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(m_data.MechaId);
		if (m_data != null)
		{
			((TextElement)m_lbLevel).text = "Lv." + m_data.Level;
		}
		((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data == null));
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)m_record.quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			m_spriteMecha.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
		zbUIEffect.SetStars(m_spriteStars, m_data.Star);
	}

	public void OnTouch()
	{
		if (m_record.propId == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11338"));
		}
		else
		{
			zbUIMgr.Instance.ShowPreview(EPreviewType.Mecha, m_record.propId, m_data.Star, m_data.Attack.ToString());
		}
	}
}
