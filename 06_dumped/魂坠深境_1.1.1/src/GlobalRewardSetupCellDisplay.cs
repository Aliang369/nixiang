using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalRewardSetupCellDisplay : CellUI<GlobalRewardSetup>, IDisplayModulesController
{
	[SerializeField]
	private SkillDisplay skillCell;

	[SerializeField]
	private MatterCell matterCell;

	[SerializeField]
	private ItemDisplayBase itemDisplay;

	[SerializeField]
	private Image characterImage;

	[SerializeField]
	private Image textRewardHintImage;

	[SerializeField]
	private TextMeshProUGUI textRewardHintText;

	private GlobalRewardSetup reward => base.setupData;

	protected override void Init()
	{
		InitUI(matterCell);
		InitUI(skillCell);
		InitUI(itemDisplay);
		FortressUIManager uIInParents = GetUIInParents<FortressUIManager>();
		if ((bool)uIInParents)
		{
			this.SetInfoPanel(uIInParents.universalCommonInfoPanel);
		}
		GameUIManager uIInParents2 = GetUIInParents<GameUIManager>();
		if ((bool)uIInParents2)
		{
			this.SetInfoPanel(uIInParents2.universalCommonInfoPanel);
		}
	}

	protected override void OnSetup(GlobalRewardSetup t)
	{
		skillCell.SetActiveAndDirty(active: false);
		matterCell.SetActiveAndDirty(active: false);
		itemDisplay.SetActiveAndDirty(active: false);
		characterImage.gameObject.SetActive(value: false);
		textRewardHintImage.gameObject.SetActive(value: false);
		textRewardHintText.gameObject.SetActive(value: false);
		if (reward is SkillReward skillReward)
		{
			skillCell.SetActiveAndDirty(active: true);
			skillCell.Setup(skillReward.GetGameSkill());
		}
		else if (reward is MatterReward matterReward)
		{
			matterCell.SetActiveAndDirty(active: true);
			MatterInfo matterInfo = new MatterInfo(matterReward.matterDef, matterReward.amount);
			matterCell.Setup(matterInfo);
		}
		else if (reward is UnlockItemSkin unlockItemSkin)
		{
			itemDisplay.SetActiveAndDirty(active: true);
			Item item = unlockItemSkin.skin.GenerateSkinedItem();
			itemDisplay.Setup(item);
		}
		else if (reward is UnlockCharacter unlockCharacter)
		{
			characterImage.gameObject.SetActive(value: true);
			textRewardHintText.gameObject.SetActive(value: true);
			characterImage.sprite = unlockCharacter.character.fullBody;
			textRewardHintText.text = unlockCharacter.character.characterName;
		}
		else
		{
			textRewardHintImage.gameObject.SetActive(value: true);
			textRewardHintText.gameObject.SetActive(value: true);
			textRewardHintText.text = reward.GlobalItemCellName;
			textRewardHintImage.sprite = reward.GlobalItemCellIcon;
		}
	}
}
