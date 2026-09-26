using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonForceCityItem : zbViewBaseItem
{
	private zbSeasonForceCityWnd m_wndParent;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbLevel;

	private Label m_lbCount;

	private VisualElement m_spriteGood;

	private Button m_btnTouch;

	private SeasonGuardInfo m_data;

	private zbSeasonCityCsvRecord m_record;

	public zbSeasonForceCityItem(zbSeasonForceCityWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonForceCity.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_spriteGood = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "good", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(SeasonGuardInfo data)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_record = zbDataConfig.Instance.seasonCityCsvTable.GetRecordByKey(m_data.CityId);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.image));
		((TextElement)m_lbName).text = m_record.name;
		((TextElement)m_lbLevel).text = m_record.level.ToString();
		((TextElement)m_lbCount).text = m_data.GuardCount.ToString();
		m_spriteGood.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.Fallen != 0) ? 1 : 0));
	}

	public void OnTouch(ClickEvent evt)
	{
	}
}
