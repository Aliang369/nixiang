using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameLootRewardCell : CellUI<GameLootRewardDisplayData>, IPointerClickHandler, IEventSystemHandler
{
	public TextMeshProUGUI countText;

	public Image iconImage;

	private new GameLootRewardsDisplayArea parent;

	protected override void Init()
	{
		parent = GetUIInParents<GameLootRewardsDisplayArea>();
	}

	protected override void UpdateDisplay()
	{
		countText.text = base.setupData.count.ToString();
		iconImage.sprite = base.setupData.icon;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		parent.ShowInfo(base.setupData.description);
	}
}
