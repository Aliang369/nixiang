using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class GameSkillPresetSlotCell : CellUI<GameSkillSlot>, IDisplayModulesController
{
	public SkillDisplay skillCell;

	public TextMeshProUGUI slotIndexText;

	public Image presetSlotWordHint;

	public GameObject configSlotBase;

	public GameObject innateSlotBase;

	public GameObject lockLabel;

	public GameObject outline;

	private SelectSkillActivatePanel selectSkillParent;

	public GameSkill skill => base.setupData.gameSkill;

	protected override void Init()
	{
		InitUI(skillCell);
		selectSkillParent = GetUIInParents<SelectSkillActivatePanel>();
	}

	protected override void OnSetup(GameSkillSlot data)
	{
		skillCell.Setup(skill);
		SetDirty();
	}

	protected override void OnUnset()
	{
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData == null)
		{
			return;
		}
		if (base.setupData.characterInnateSkillSlot || base.setupData.IsUnLocked(CellUI<GameSkillSlot>.gameState.playerState.characterGameConfig))
		{
			lockLabel.gameObject.SetActive(value: false);
			if (skill == null)
			{
				slotIndexText.gameObject.SetActive(value: true);
				slotIndexText.text = base.setupData.displayIndex.ToString();
			}
			else
			{
				slotIndexText.gameObject.SetActive(value: false);
			}
		}
		else
		{
			lockLabel.gameObject.SetActive(value: true);
			slotIndexText.gameObject.SetActive(value: false);
		}
		skillCell.SetActiveAndDirty(skill != null);
		if (selectSkillParent?.selectingPresetSlot == base.setupData && skill != null)
		{
			outline.gameObject.SetActive(value: true);
		}
		else
		{
			outline.gameObject.SetActive(value: false);
		}
		if (base.setupData.characterInnateSkillSlot)
		{
			configSlotBase.gameObject.SetActive(value: false);
			innateSlotBase.gameObject.SetActive(value: true);
		}
		else
		{
			configSlotBase.gameObject.SetActive(value: true);
			innateSlotBase.gameObject.SetActive(value: false);
		}
		presetSlotWordHint.gameObject.SetActive(base.setupData.withWordSlot);
	}
}
