using UnityEngine;

public abstract class PickBonusSelectionPanelBase : UIBase
{
	[SerializeField]
	public PickBonusSelectionInfoPanel itemInfoPanel;

	protected static void RemoveInvalidRuneBonuses(BattleBonus info)
	{
		if (info?.bonuses == null)
		{
			return;
		}
		for (int num = info.bonuses.Count - 1; num >= 0; num--)
		{
			if (info.bonuses[num] is RuneBonus runeBonus && (runeBonus.rune == null || !(runeBonus.rune.def != null)))
			{
				TraceLog.Error("[PickBonusSelectionPanel] Removed invalid RuneBonus before displaying bonus list.");
				info.bonuses.RemoveAt(num);
			}
		}
	}

	public abstract void OnSubPanelFinish(BattleBonusObject info);

	public abstract void Select(PickBonusSelector selector);
}
