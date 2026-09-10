using UnityEngine;
using UnityEngine.UI.Extensions;

public class TalentSlotConnectLine : CellUI<(TalentTree.TalentSlotConnection, PlayerCharacterState)>
{
	public UILineRenderer lineRenderer;

	public Color lockedDarkenColor;

	public Color unlockConnectColor;

	public Color completeConnectColor;

	private (TalentTree.TalentSlotConnection, PlayerCharacterState) data;

	protected override void OnSetup((TalentTree.TalentSlotConnection, PlayerCharacterState) data)
	{
		this.data = data;
		SetDirty();
	}

	private void Update()
	{
		TalentTree.TalentSlotConnection item = data.Item1;
		base.transform.position = item.from.transform.position;
		lineRenderer.Points = new Vector2[2]
		{
			Vector2.zero,
			item.to.transform.localPosition - item.from.transform.localPosition
		};
		if (item.IsLocked(CellUI<(TalentTree.TalentSlotConnection, PlayerCharacterState)>.applicationState, data.Item2))
		{
			lineRenderer.color = lockedDarkenColor;
		}
		else if (item.IsComplete(CellUI<(TalentTree.TalentSlotConnection, PlayerCharacterState)>.applicationState, data.Item2))
		{
			lineRenderer.color = completeConnectColor;
		}
		else
		{
			lineRenderer.color = unlockConnectColor;
		}
	}
}
