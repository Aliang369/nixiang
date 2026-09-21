using UnityEngine;
using UnityEngine.UI;

public class IdolLevelCellModule : GlobalItemDisplay.DisplayModule
{
	[SerializeField]
	private Image unlockMask;

	private IdolLevelCell cell => GetComponentInParent<IdolLevelCell>();

	public override void UpdateDisplay()
	{
		bool active = cell.level <= cell.idolState.currentLevel;
		unlockMask.gameObject.SetActive(active);
	}
}
