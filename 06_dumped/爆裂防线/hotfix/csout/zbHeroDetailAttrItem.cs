using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroDetailAttrItem : zbViewBaseItem
{
	private WarRoomAttrDetail m_attrDetail;

	private readonly zbHeroDetailWnd m_wndParent;

	private readonly Label m_lbkey;

	private readonly Label m_lbValue;

	private readonly Button m_btnTouch;

	private ESkillType m_skillType;

	public zbHeroDetailAttrItem(zbHeroDetailWnd wndParent, VisualTreeAsset tAsset)
	{
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_lbkey = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_key", (string)null);
		m_lbValue = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_value", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		if (m_wndParent?.m_uiEvent != null && m_btnTouch != null)
		{
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnPointerDown, "", (TrickleDown)0);
		}
	}

	public void Refresh(WarRoomAttrDetail attrDetail, ESkillType _eSkillType, int heroTid)
	{
		m_attrDetail = attrDetail;
		m_skillType = _eSkillType;
		zbSkillAttrCsvRecord recordByKey = zbDataConfig.Instance.skillAttrCsvTable.GetRecordByKey((int)m_attrDetail.Type);
		((TextElement)m_lbkey).text = recordByKey.name ?? zbUtil.Language("lang.10119");
		string text = ((m_attrDetail.Type == EAttrSkillType.Damage) ? ((int)m_attrDetail.Value).ToString() : zbAttrHelper.FormatAttrValue(m_attrDetail.Value, recordByKey.showPercent > 0));
		if (zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType(m_skillType) != null)
		{
			HeroAwakenDetail heroAwakenDetail = zbSelf.Instance.heroHandler.GetHeroAwakenDetail(m_skillType);
			if (heroAwakenDetail != null && heroAwakenDetail.AttrDetail.Type == m_attrDetail.Type && heroAwakenDetail.AttrDetail.Value > 0f)
			{
				text += $"(<color=#2DF421>+{heroAwakenDetail.AttrDetail.Value}</color>)";
			}
		}
		((TextElement)m_lbValue).text = text;
	}

	private void OnPointerDown(ClickEvent evt)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (((EventBase)evt).target is VisualElement)
		{
			float x = ((TextElement)m_lbValue).MeasureTextSize(((TextElement)m_lbValue).text, 0f, (MeasureMode)0, 0f, (MeasureMode)0).x;
			Rect worldBound = ((VisualElement)m_lbValue).worldBound;
			float num = ((Rect)(ref worldBound)).xMax - x;
			worldBound = ((VisualElement)m_lbValue).worldBound;
			Vector2 pos = default(Vector2);
			((Vector2)(ref pos))._002Ector(num, ((Rect)(ref worldBound)).center.y);
			zbUIMgr.Instance.ShowTipAttr(m_skillType, m_attrDetail, pos);
		}
	}
}
