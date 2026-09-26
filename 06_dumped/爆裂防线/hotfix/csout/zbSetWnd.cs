using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSetWnd : zbBaseWnd
{
	private Slider m_sliderMusic;

	private Slider m_sliderSound;

	private Slider m_slider_bubble;

	private Toggle m_Toggle_consumption;

	private Button m_btnMusic;

	private Button m_btnSound;

	private Button m_btnCall;

	private Button m_btnClose;

	private VisualElement m_pnlSet;

	private VisualElement m_pnlNotify;

	private Button m_btnSet;

	private Button m_btnNotify;

	private Sprite m_selectedIcon;

	private zbSetNotifyPnl m_setNotifyPnl;

	protected override void OnEnable()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_selectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		bool flag = false;
		val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_pnlSet = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel0", (string)null);
		m_btnSet = UQueryExtensions.Q<Button>(val, "tab0", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSet, (Action)OnTabSet, "", (TrickleDown)0);
		m_pnlNotify = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel1", (string)null);
		m_btnNotify = UQueryExtensions.Q<Button>(val, "tab1", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNotify, (Action)OnTabNotify, "", (TrickleDown)0);
		m_setNotifyPnl = new zbSetNotifyPnl(m_pnlNotify, this);
		m_btnMusic = UQueryExtensions.Q<Button>(m_pnlSet, "btn_music", (string)null);
		m_btnSound = UQueryExtensions.Q<Button>(m_pnlSet, "btn_sound", (string)null);
		m_sliderMusic = UQueryExtensions.Q<Slider>(m_pnlSet, "slider_music", (string)null);
		m_sliderSound = UQueryExtensions.Q<Slider>(m_pnlSet, "slider_Sound", (string)null);
		m_slider_bubble = UQueryExtensions.Q<Slider>(m_pnlSet, "slider_bubble", (string)null);
		m_Toggle_consumption = UQueryExtensions.Q<Toggle>(m_pnlSet, "Toggle_consumption", (string)null);
		zbUtil.SetSliderDraggerStyle("ui/image/button/btn_200x90.png", m_sliderMusic, m_sliderSound, m_slider_bubble);
		m_btnCall = UQueryExtensions.Q<Button>(m_RootElement, "btn_call", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		((VisualElement)m_btnCall).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
		m_uiEvent.RegisterValueChangedCallback<float>((BindableElement)(object)m_sliderMusic, OnMusicVolumeChange);
		m_uiEvent.RegisterValueChangedCallback<float>((BindableElement)(object)m_sliderSound, OnSoundVolumeChange);
		m_uiEvent.RegisterValueChangedCallback<float>((BindableElement)(object)m_slider_bubble, OnBubbleNumber);
		m_uiEvent.RegisterValueChangedCallback<bool>((BindableElement)(object)m_Toggle_consumption, OnToggleConsumption);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMusic, (Action)OnToggleMusic, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSound, (Action)OnToggleSound, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action<ClickEvent>)OnCall, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		((BaseSlider<float>)(object)m_sliderMusic).lowValue = 0f;
		((BaseSlider<float>)(object)m_sliderMusic).highValue = 1f;
		((BaseSlider<float>)(object)m_sliderSound).lowValue = 0f;
		((BaseSlider<float>)(object)m_sliderSound).highValue = 1f;
		((BaseSlider<float>)(object)m_slider_bubble).lowValue = 0f;
		((BaseSlider<float>)(object)m_slider_bubble).highValue = 1f;
		((BaseField<float>)(object)m_sliderMusic).value = zbMusicMgr.Instance.GetMaxVolume();
		((BaseField<float>)(object)m_sliderSound).value = zbAudioMgr.Instance.GetMaxAudioVolume();
		((BaseField<float>)(object)m_slider_bubble).value = zbSelf.Instance.combatHandler.GetMaxBubbleNumber();
		((BaseField<bool>)(object)m_Toggle_consumption).value = zbSetSave.Instance.GetConsumption() == 1;
		m_gameEevent.RegisterEvent(EGameEvent.Event_Push_Set_Ack, OnPushSetAck);
		OnTabSet();
	}

	private void DoSelectTab(int idx)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		m_pnlSet.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((idx != 0) ? 1 : 0));
		m_pnlNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(idx != 1));
		StyleBackground val = default(StyleBackground);
		((StyleBackground)(ref val))._002Ector(m_selectedIcon);
		StyleBackground val2 = default(StyleBackground);
		((StyleBackground)(ref val2)).keyword = (StyleKeyword)3;
		StyleBackground val3 = val2;
		((VisualElement)m_btnSet).style.backgroundImage = ((idx == 0) ? val : val3);
		((VisualElement)m_btnNotify).style.backgroundImage = ((idx == 1) ? val : val3);
	}

	private void OnTabSet()
	{
		DoSelectTab(0);
	}

	private void OnTabNotify()
	{
		DoSelectTab(1);
		m_setNotifyPnl.Refresh();
	}

	private void OnPushSetAck(object ackData)
	{
		m_setNotifyPnl.OnPushSetAck();
	}

	public void OnMusicVolumeChange(float volume)
	{
		zbMusicMgr.Instance.SetMaxVolume(volume);
	}

	public void OnSoundVolumeChange(float volume)
	{
		zbAudioMgr.Instance.SetMaxAudiovVolume(volume);
	}

	private void OnBubbleNumber(float volume)
	{
		zbSelf.Instance.combatHandler.SetMaxBubbleNumber(volume);
	}

	private void OnToggleConsumption(bool evt)
	{
		zbSetSave.Instance.SetConsumption(evt ? 1 : 0);
	}

	public void OnCall(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSupportContact();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public override void Refresh()
	{
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnMusic, "icon_isCheck", (string)null)).visible = !zbMusicMgr.Instance.IsMute();
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnSound, "icon_isCheck", (string)null)).visible = !zbAudioMgr.Instance.IsMute();
	}

	private void OnToggleMusic()
	{
		Label obj = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnMusic, "icon_isCheck", (string)null);
		bool visible = ((VisualElement)obj).visible;
		zbMusicMgr.Instance.SetMute(visible ? true : false);
		((VisualElement)obj).visible = !visible;
	}

	private void OnToggleSound()
	{
		Label obj = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnSound, "icon_isCheck", (string)null);
		bool visible = ((VisualElement)obj).visible;
		zbAudioMgr.Instance.SetMute(visible ? true : false);
		((VisualElement)obj).visible = !visible;
	}
}
