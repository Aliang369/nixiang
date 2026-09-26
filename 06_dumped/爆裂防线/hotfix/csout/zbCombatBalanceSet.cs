using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatBalanceSet
{
	private zbBaseWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnMusic;

	private Button m_btnSound;

	private Button m_btnDmgOn;

	private Button m_btnDmgOff;

	private Button[] m_btnsEffectCount = (Button[])(object)new Button[3];

	private Slider m_sliderMusic;

	private Slider m_sliderSound;

	private Slider m_slider_bubble;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnMusic = UQueryExtensions.Q<Button>(m_RootElement, "btn_music", (string)null);
		m_btnSound = UQueryExtensions.Q<Button>(m_RootElement, "btn_sound", (string)null);
		m_btnDmgOn = UQueryExtensions.Q<Button>(m_RootElement, "btn_dmgOn", (string)null);
		m_btnDmgOff = UQueryExtensions.Q<Button>(m_RootElement, "btn_dmgOff", (string)null);
		m_sliderMusic = UQueryExtensions.Q<Slider>(m_RootElement, "slider_Music", (string)null);
		m_sliderSound = UQueryExtensions.Q<Slider>(m_RootElement, "slider_Sound", (string)null);
		m_slider_bubble = UQueryExtensions.Q<Slider>(m_RootElement, "slider_bubble", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_btnsEffectCount[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_" + i, (string)null);
			int tempIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsEffectCount[i], (Action)delegate
			{
				OnToggleEffectCount(tempIndex);
			}, "", (TrickleDown)0);
		}
		zbUtil.SetSliderDraggerStyle("ui/image/button/btn_200x90.png", m_sliderMusic, m_sliderSound, m_slider_bubble);
		m_wndParent.m_uiEvent.RegisterValueChangedCallback<float>((BindableElement)(object)m_sliderMusic, OnMusicVolumeChange);
		m_wndParent.m_uiEvent.RegisterValueChangedCallback<float>((BindableElement)(object)m_sliderSound, OnSoundVolumeChange);
		m_wndParent.m_uiEvent.RegisterValueChangedCallback<float>((BindableElement)(object)m_slider_bubble, OnBubbleNumber);
		((BaseSlider<float>)(object)m_sliderMusic).lowValue = 0f;
		((BaseSlider<float>)(object)m_sliderMusic).highValue = 1f;
		((BaseSlider<float>)(object)m_sliderSound).lowValue = 0f;
		((BaseSlider<float>)(object)m_sliderSound).highValue = 1f;
		((BaseSlider<float>)(object)m_slider_bubble).lowValue = 0f;
		((BaseSlider<float>)(object)m_slider_bubble).highValue = 1f;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_sliderMusic, "unity-dragger", (string)null).transform.scale = new Vector3(0.8f, 0.8f, 1f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_sliderSound, "unity-dragger", (string)null).transform.scale = new Vector3(0.8f, 0.8f, 1f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_slider_bubble, "unity-dragger", (string)null).transform.scale = new Vector3(0.8f, 0.8f, 1f);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMusic, (Action)OnToggleMusic, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSound, (Action)OnToggleSound, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDmgOn, (Action)delegate
		{
			OnToggleDmgCount("On");
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDmgOff, (Action)delegate
		{
			OnToggleDmgCount("Off");
		}, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		((BaseField<float>)(object)m_sliderMusic).value = zbMusicMgr.Instance.GetMaxVolume();
		((BaseField<float>)(object)m_sliderSound).value = zbAudioMgr.Instance.GetMaxAudioVolume();
		((BaseField<float>)(object)m_slider_bubble).value = zbSelf.Instance.combatHandler.GetMaxBubbleNumber();
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnMusic, "icon_isCheck", (string)null)).visible = !zbMusicMgr.Instance.IsMute();
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnSound, "icon_isCheck", (string)null)).visible = !zbAudioMgr.Instance.IsMute();
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
		zbCombatSystem.Instance.combatData.SetMaxBubbleActive(volume);
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

	private void OnToggleDmgCount(string target)
	{
		bool flag = target == "On";
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDmgOn, "icon_isCheck", (string)null)).visible = flag;
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDmgOff, "icon_isCheck", (string)null)).visible = !flag;
	}

	private void OnToggleEffectCount(int index)
	{
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnsEffectCount[i], "icon_isCheck", (string)null)).visible = ((i == index) ? true : false);
		}
	}
}
