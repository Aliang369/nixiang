using System.Linq;
using ContainerOfSoul.Display.Fortress;
using TMPro;
using UnityEngine;

public class UnlockTalentInfoModule_CharacterTalentUnlockPanel : UnlockTalentInfoDisplay.DisplayModule
{
	[SerializeField]
	private GameButton unlockButton;

	[SerializeField]
	private MatterCellGroup matterCellGroup;

	[SerializeField]
	private TextMeshProUGUI unlockButtonText;

	[SerializeField]
	private TextMeshProUGUI notUnlockAbleHintText;

	private CharacterTalentUnlockPanel characterTalentUnlockPanelParent;

	private TalentSlotCell talentCell;

	public RectTransform UnlockButtonRect
	{
		get
		{
			if (!(unlockButton != null))
			{
				return null;
			}
			return unlockButton.GetComponent<RectTransform>();
		}
	}

	public override void Init()
	{
		parent.InitUI(matterCellGroup);
		characterTalentUnlockPanelParent = GetComponentInParent<CharacterTalentUnlockPanel>(includeInactive: true);
		unlockButton.onClick.AddListener(_OnUnlockButtonClicked);
	}

	public override void Setup()
	{
		talentCell = characterTalentUnlockPanelParent?.GetSlotCell(base.talent);
	}

	public override void UpdateDisplay()
	{
		if (characterTalentUnlockPanelParent == null || talentCell == null)
		{
			unlockButton.gameObject.SetActive(value: false);
			matterCellGroup.Unset();
			notUnlockAbleHintText.gameObject.SetActive(value: false);
			return;
		}
		bool flag = UIDisplayModule.applicationState.matterManager.HasEnoughMatter(talentCell.UnlockCostMatters(characterTalentUnlockPanelParent.character.def));
		unlockButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock;
		if (talentCell.IsUnlocked(characterTalentUnlockPanelParent.character))
		{
			unlockButton.gameObject.SetActive(value: false);
		}
		else
		{
			unlockButton.gameObject.SetActive(value: true);
		}
		if (talentCell.IsUnlockAble(characterTalentUnlockPanelParent.character))
		{
			unlockButton.interactable = flag;
			if (flag)
			{
				notUnlockAbleHintText.gameObject.SetActive(value: false);
			}
			else
			{
				notUnlockAbleHintText.gameObject.SetActive(value: true);
				notUnlockAbleHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_matter_is_not_enough;
			}
		}
		else
		{
			unlockButton.interactable = false;
			if (talentCell.IsUnlocked(characterTalentUnlockPanelParent.character))
			{
				notUnlockAbleHintText.gameObject.SetActive(value: false);
				unlockButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlocked;
			}
			else
			{
				notUnlockAbleHintText.gameObject.SetActive(value: true);
				notUnlockAbleHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_neighbour_talent_not_unlock_yet;
			}
		}
		matterCellGroup.Setup((from m in talentCell.UnlockCostMatters(characterTalentUnlockPanelParent.character.def)
			where m.count != 0
			select m).ToList());
	}

	private void _OnUnlockButtonClicked()
	{
		if (!(characterTalentUnlockPanelParent == null))
		{
			talentCell.Unlock(UIDisplayModule.applicationState, characterTalentUnlockPanelParent.character);
		}
	}
}
