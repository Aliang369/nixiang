using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.SkillOverview;

public class ProfileSkillPresetSlotCell : CellUI<ProfileSkillPresetSlotCell.Info>, IDisplayModulesController
{
	public class Info
	{
		public PresetSkillSlot slot;

		public PlayerCharacterState character;

		public Action<ProfileSkillPresetSlotCell> onClick;
	}

	[SerializeField]
	public SkillDisplay skillCell;

	[SerializeField]
	public TextMeshProUGUI slotIndexText;

	[SerializeField]
	public RectTransform unlockButtonLabel;

	[SerializeField]
	public Image presetSlotWordHint;

	[SerializeField]
	public GameObject selectedOutline;

	[SerializeField]
	public GameObject configSlotBase;

	[SerializeField]
	public GameObject innateSlotBase;

	[NonSerialized]
	private SelectSkillActivatePanel selectSkillActivatePanelParent;

	private Info info => base.setupData;

	public PlayerCharacterState character => info.character;

	public PresetSkillSlot skillSlot => info.slot;

	private ProfileSkill skill => skillSlot?.skill;

	private GameSkill displaySkill
	{
		get
		{
			if (!(skillSlot is GameSkillSlot gameSkillSlot))
			{
				return skill?.GetGameSkill();
			}
			return gameSkillSlot.gameSkill;
		}
	}

	public bool isInner => skillSlot?.characterInnateSkillSlot ?? false;

	private SkillConfigPresetPanel skillConfigParent => CellUI<Info>.fortressState?.uiManager?.skillConfigPanel.skillConfigPresetPanel;

	protected override void Init()
	{
		InitUI(skillCell);
		selectSkillActivatePanelParent = GetUIInParents<SelectSkillActivatePanel>();
		InputModule module = GetModule<InputModule>();
		module.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(module.onPointerClick, (Action<InputModuleBase>)delegate
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			info?.onClick?.Invoke(this);
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
		});
	}

	protected override void OnSetup(Info data)
	{
		base.rectTransform.localScale = Vector3.one;
		skillCell.transform.localPosition = Vector3.zero;
		skillCell.transform.localScale = Vector3.one;
		if (displaySkill != null)
		{
			skillCell.Setup(displaySkill);
		}
		else
		{
			skillCell.Unset();
		}
		SkillWordDef skillWordDef = skillSlot?.slotWord;
		presetSlotWordHint.gameObject.SetActive(skillWordDef != null);
		SetDirty();
	}

	protected override void OnUnset()
	{
		base.rectTransform.localScale = Vector3.one;
		skillCell.transform.localScale = Vector3.one;
		skillCell.Unset();
		if (selectedOutline != null)
		{
			selectedOutline.SetActive(value: false);
		}
	}

	protected override void UpdateDisplay()
	{
		if (character == null)
		{
			return;
		}
		if (selectedOutline != null)
		{
			selectedOutline.SetActive((skillConfigParent != null && skillConfigParent.selectedCell == this) || (selectSkillActivatePanelParent != null && selectSkillActivatePanelParent.selectingPresetSlot == skillSlot));
		}
		if (isInner)
		{
			configSlotBase.gameObject.SetActive(value: false);
			innateSlotBase.gameObject.SetActive(value: true);
			unlockButtonLabel.gameObject.SetActive(value: false);
			slotIndexText.gameObject.SetActive(value: false);
			skillCell.gameObject.SetActive(skill != null);
			return;
		}
		configSlotBase.gameObject.SetActive(value: true);
		innateSlotBase.gameObject.SetActive(value: false);
		if (skillSlot.IsUnLocked(character.GenerateCharacterConfigInit(CellUI<Info>.applicationState)))
		{
			unlockButtonLabel.gameObject.SetActive(value: false);
			if (skill == null)
			{
				skillCell.gameObject.SetActive(value: false);
				slotIndexText.gameObject.SetActive(value: true);
				slotIndexText.text = skillSlot.displayIndex.ToString();
			}
			else
			{
				slotIndexText.gameObject.SetActive(value: false);
				skillCell.gameObject.SetActive(value: true);
			}
		}
		else
		{
			unlockButtonLabel.gameObject.SetActive(value: true);
			slotIndexText.gameObject.SetActive(value: false);
			skillCell.gameObject.SetActive(value: false);
		}
	}
}
