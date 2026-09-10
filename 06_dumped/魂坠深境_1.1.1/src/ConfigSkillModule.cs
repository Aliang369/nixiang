using System;
using System.Linq;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigSkillModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private Image selectedGlow;

	[SerializeField]
	private Image currentSelectedGlow;

	[SerializeField]
	private TextMeshProUGUI selectedSkillIndex;

	[SerializeField]
	private TextMeshProUGUI currentSelectedSkillIndex;

	[SerializeField]
	private RectTransform skillNonInspectedHint;

	[SerializeField]
	private bool showNonInspectedHint;

	[SerializeField]
	private GameObject unselectableMask;

	[NonSerialized]
	private SkillConfigPresetPanel configPanelParent;

	[NonSerialized]
	private ProfileSkillPresetSlotCell profileSkillPresetSlotCellParent;

	private PlayerCharacterState characterState => configPanelParent?.character ?? UIDisplayModule.applicationState.newGameConfigManager.currentNewGameConfig.GetCharacterState(UIDisplayModule.applicationState);

	private ProfileSkillPreset skillPreset => characterState.skillPreset;

	public PresetSkillSlot configSlot => skillPreset?.configSkillSlots.FirstOrDefault((PresetSkillSlot s) => s.skill == base.profileSkill);

	public override void Init()
	{
		InputModule module = parent.GetModule<InputModule>();
		module.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(module.onPointerClick, (Action<InputModuleBase>)delegate
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (base.profileSkill != null && !base.profileSkill.inspected)
			{
				base.profileSkill.inspected = true;
			}
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
		});
		configPanelParent = parent.GetUIInParents<SkillConfigPresetPanel>();
	}

	public override void UpdateDisplay()
	{
		if (base.profileSkill != null)
		{
			skillNonInspectedHint.gameObject.SetActive(showNonInspectedHint && base.profileSkill != null && !base.profileSkill.inspected);
			if (configSlot != null)
			{
				currentSelectedGlow.gameObject.SetActive(value: true);
				selectedSkillIndex.gameObject.SetActive(value: false);
				currentSelectedSkillIndex.gameObject.SetActive(value: true);
				currentSelectedSkillIndex.text = configSlot.displayIndex.ToString();
			}
			else if (base.profileSkill != null && configSlot != null)
			{
				currentSelectedGlow.gameObject.SetActive(value: false);
				selectedSkillIndex.gameObject.SetActive(value: true);
				currentSelectedSkillIndex.gameObject.SetActive(value: false);
				selectedSkillIndex.text = configSlot.displayIndex.ToString();
			}
			else
			{
				currentSelectedGlow.gameObject.SetActive(value: false);
				currentSelectedSkillIndex.gameObject.SetActive(value: false);
				selectedSkillIndex.gameObject.SetActive(value: false);
			}
		}
	}
}
