using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.SkillOverview;

public class SkillPresetButtonCell : CellUI<ProfileSkillPreset>, IPointerClickHandler, IEventSystemHandler
{
	public TextMeshProUGUI unselectNameText;

	public TextMeshProUGUI selectNameText;

	public Image selectedImage;

	public Action<SkillPresetButtonCell> onclick;

	private SkillConfigPresetPanel parentPanel;

	public ProfileSkillPreset preset;

	public void OnPointerClick(PointerEventData eventData)
	{
		onclick?.Invoke(this);
	}

	protected override void Init()
	{
		base.Init();
		parentPanel = parent as SkillConfigPresetPanel;
	}

	protected override void OnSetup(ProfileSkillPreset t)
	{
		preset = t;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (preset != null)
		{
			unselectNameText.text = preset.name;
			selectNameText.text = preset.name;
			if (parentPanel.skillPreset == preset)
			{
				selectedImage.gameObject.SetActive(value: true);
			}
			else
			{
				selectedImage.gameObject.SetActive(value: false);
			}
		}
	}
}
