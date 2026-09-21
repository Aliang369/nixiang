using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.GameConfig;

public class InitItemCell : CellUI<ItemDefinition>
{
	[SerializeField]
	private Image itemImage;

	protected override void OnSetup(ItemDefinition t)
	{
		itemImage.sprite = t.Icon;
	}
}
