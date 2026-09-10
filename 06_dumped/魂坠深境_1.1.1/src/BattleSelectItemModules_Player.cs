using UnityEngine;

public class BattleSelectItemModules_Player : BattleSelectItemModules
{
	protected override Vector2 GetIdlePosition()
	{
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
		float num = playerItemAnimConfig.itemOffset + playerItemAnimConfig.itemIdleSpace + 0.001f;
		float width = (base.slotDisplay.transform.parent as RectTransform).GetWidth();
		int num2 = (int)(width / num);
		int num3 = (panel as BattleTableItemSelectPanel).playerItemDisplayPool.IndexOf(base.slotDisplay);
		int activeCount = (panel as BattleTableItemSelectPanel).playerItemDisplayPool.activeCount;
		if (activeCount <= num2)
		{
			return new Vector2(((float)num3 - (float)activeCount / 2f + 0.5f) * num, 0f);
		}
		float num4 = width / (float)activeCount;
		return new Vector2(((float)num3 - (float)activeCount / 2f + 0.5f) * num4, 0f);
	}
}
