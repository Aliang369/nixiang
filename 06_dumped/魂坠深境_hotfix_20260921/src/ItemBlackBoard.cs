using System;
using System.Collections.Generic;
using OdinEx;

[Serializable]
public class ItemBlackBoard
{
	public enum ValueType
	{
		None = 0,
		Attack = 1001,
		Defence = 1002,
		Heal = 1003,
		AddCounter = 1004,
		AddBuff = 1005,
		Special = 1006,
		Times = 1007,
		EnchantValue = 1008,
		CoolDown = 1009,
		EnergyCounter = 1010,
		ItemCount = 1011,
		Index = 1012,
		Cost = 1013,
		Health = 1014,
		SubItemCounter = 1015,
		InitCounter = 1016,
		InitMaxCounter = 1017,
		EnchantValue2 = 1018,
		MoveSlot = 1019,
		Dissolve = 1020,
		CustomInitCooldown = 100001,
		CustomRespawnTime = 100002,
		BattleStartCast = 100003,
		InitSpawn = 100006,
		InitIntentIndex = 100007,
		StayInTable = 100008,
		InitWastePile = 100010,
		DisplayUnitIndex = 100011,
		GenerateCount = 100012,
		SpawnOnPhase2 = 100013,
		SpawnInExIntent = 100014,
		RemoveInExIntent = 100015
	}

	[Serializable]
	public class ValueInfo
	{
		[TableColumnLabel("标记")]
		[SaveField(null, null)]
		public ValueType type;

		[TableColumnLabel("值")]
		[SaveField(null, null)]
		public int value;

		public ValueInfo(ValueType type, int value)
		{
			this.type = type;
			this.value = value;
		}

		private ValueInfo()
		{
		}
	}

	[SaveField(null, null)]
	public List<ValueInfo> values = new List<ValueInfo>();

	public int? CustomRespawnTime => GetValue(ValueType.CustomRespawnTime);

	public int? CustomInitCooldown => GetValue(ValueType.CustomInitCooldown);

	public bool BattleStartCast => GetValue(ValueType.BattleStartCast).HasValue;

	public bool InitSpawn => GetValue(ValueType.InitSpawn).HasValue;

	public bool StayInTable => GetValue(ValueType.StayInTable).HasValue;

	public bool SpawnInExIntent => GetValue(ValueType.SpawnInExIntent).HasValue;

	public bool RemoveInExIntent => GetValue(ValueType.RemoveInExIntent).HasValue;

	public bool InitWastePile => GetValue(ValueType.InitWastePile).HasValue;

	public int ItemGenerateCount => GetValue(ValueType.GenerateCount) ?? 1;

	public bool SpawnOnPhase2 => GetValue(ValueType.SpawnOnPhase2).HasValue;

	public void SetValue(ValueType type, int value)
	{
		int num = values.FindIndex((ValueInfo t) => t.type == type);
		if (num == -1)
		{
			ValueInfo item = new ValueInfo(type, value);
			values.Add(item);
		}
		else
		{
			ValueInfo valueInfo = values[num];
			valueInfo.value = value;
			values[num] = valueInfo;
		}
	}

	public int? GetValue(ValueType type)
	{
		if (values == null)
		{
			return null;
		}
		int num = values.FindIndex((ValueInfo t) => t.type == type);
		if (num == -1)
		{
			return null;
		}
		return values[num].value;
	}
}
