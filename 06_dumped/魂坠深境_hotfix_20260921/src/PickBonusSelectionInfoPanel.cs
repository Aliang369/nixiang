using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickBonusSelectionInfoPanel : CommonInfoPanelBase
{
	public interface IShowTextHint
	{
		string GetHint();
	}

	[SerializeField]
	private ItemInfoDisplayBase itemInfoDisplay;

	[SerializeField]
	public Image hintImage;

	[SerializeField]
	public TextMeshProUGUI hintText;

	[SerializeField]
	public SkillInfoDisplay skillInfoDisplay;

	protected override void UpdateInfo()
	{
		HideAllInfo();
		object obj = currentCommonInfoData?.target;
		if (obj != null)
		{
			if (obj is GetSingleItemBonus getSingleItemBonus)
			{
				SetupItemInfo(getSingleItemBonus.item);
			}
			else if (obj is Item item)
			{
				SetupItemInfo(item);
			}
			else if (obj is GetSkillBonus getSkillBonus)
			{
				skillInfoDisplay.Setup(getSkillBonus.skill);
			}
		}
	}

	private void HideAllInfo()
	{
		hintImage.gameObject.SetActive(value: false);
		hintText.text = null;
		itemInfoDisplay.showUncollectedStatus = false;
		itemInfoDisplay.Unset();
		skillInfoDisplay.Unset();
	}

	private void SetupItemInfo(Item item)
	{
		itemInfoDisplay.showUncollectedStatus = true;
		itemInfoDisplay.Setup(item);
	}
}
