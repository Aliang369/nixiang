using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.MatterCells;

public class CounterModule : MatterCell.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI numText;

	private Material defaultCounterMaterial;

	public override void Init()
	{
		if (!(numText == null))
		{
			defaultCounterMaterial = numText.fontSharedMaterial;
		}
	}

	public override void UpdateDisplay()
	{
		numText.text = base.matterInfo.count.ToString();
	}

	public void SetCounterMaterial(Material material)
	{
		if (!(numText == null))
		{
			numText.fontSharedMaterial = ((material != null) ? material : defaultCounterMaterial);
			numText.UpdateMeshPadding();
		}
	}
}
