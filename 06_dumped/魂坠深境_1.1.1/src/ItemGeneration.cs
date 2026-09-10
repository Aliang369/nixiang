using System;
using System.Collections.Generic;
using OdinEx;

[Serializable]
public class ItemGeneration
{
	[Serializable]
	public class ItemBlackBoardValueSetup
	{
		public enum ValueType
		{
			FixValue,
			ResistanceP,
			ThreatP,
			ThreatSlotP,
			ResistanceSlotP
		}

		[TableColumnLabel("黑板标记")]
		[SaveField(null, null)]
		public ItemBlackBoard.ValueType type;

		[TableColumnLabel("值类型")]
		[SaveField(null, null)]
		public ValueType valueType;

		[TableColumnLabel("值")]
		[SaveField(null, null)]
		public float value;
	}

	public ItemDefinition item;

	[TableColumnLabel("数据预设黑板")]
	[SaveField(null, null)]
	public List<ItemBlackBoardValueSetup> blackBoardValueSetups;

	public ItemGeneration()
	{
		item = null;
	}

	public ItemBlackBoard GenerateBlackBoard(EnemyInitInfo initInfo)
	{
		ItemBlackBoard itemBlackBoard = new ItemBlackBoard();
		foreach (ItemBlackBoardValueSetup blackBoardValueSetup in blackBoardValueSetups)
		{
			switch (blackBoardValueSetup.valueType)
			{
			case ItemBlackBoardValueSetup.ValueType.FixValue:
				itemBlackBoard.SetValue(blackBoardValueSetup.type, (int)blackBoardValueSetup.value);
				break;
			case ItemBlackBoardValueSetup.ValueType.ResistanceP:
			{
				float resistanceValue = ScriptableObjectSingleton<GameStatisticData>.instance.GetResistanceValue(initInfo);
				itemBlackBoard.SetValue(blackBoardValueSetup.type, (int)(resistanceValue * blackBoardValueSetup.value));
				break;
			}
			case ItemBlackBoardValueSetup.ValueType.ResistanceSlotP:
			{
				int valueOrDefault = (initInfo.sourceDifficultySetup?.difficultyInfo?.intentSlotCount).GetValueOrDefault();
				if (valueOrDefault != 0)
				{
					float num = ScriptableObjectSingleton<GameStatisticData>.instance.GetResistanceValue(initInfo) / (float)valueOrDefault;
					itemBlackBoard.SetValue(blackBoardValueSetup.type, (int)(num * blackBoardValueSetup.value));
				}
				break;
			}
			case ItemBlackBoardValueSetup.ValueType.ThreatP:
			{
				float threatValue = ScriptableObjectSingleton<GameStatisticData>.instance.GetThreatValue(initInfo);
				itemBlackBoard.SetValue(blackBoardValueSetup.type, (int)(threatValue * blackBoardValueSetup.value));
				break;
			}
			case ItemBlackBoardValueSetup.ValueType.ThreatSlotP:
			{
				float enemyThreatValuePerSlot = ScriptableObjectSingleton<GameStatisticData>.instance.GetEnemyThreatValuePerSlot(initInfo);
				itemBlackBoard.SetValue(blackBoardValueSetup.type, (int)(enemyThreatValuePerSlot * blackBoardValueSetup.value));
				break;
			}
			}
		}
		return itemBlackBoard;
	}

	public Item GenerateItem(EnemyInitInfo initInfo)
	{
		if (item == null)
		{
			return null;
		}
		ItemBlackBoard itemBlackBoard = GenerateBlackBoard(initInfo);
		return new Item(item, itemBlackBoard);
	}
}
