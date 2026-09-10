using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

public class GameStatisticData : ScriptableObjectSingleton<GameStatisticData>
{
	public class ObjectTable
	{
		[OdinSerialize]
		public List<object> Rows = new List<object>();

		[OdinSerialize]
		public List<object> Columns = new List<object>();

		[OdinSerialize]
		[HideInInspector]
		private int[,] values;

		private int[,] ValuesMatrix
		{
			get
			{
				EnsureMatrixSize();
				return values;
			}
			set
			{
				values = value;
			}
		}

		public int Get(object rowKey, object colKey, int defaultValue = 0)
		{
			if (Rows == null || Columns == null || values == null)
			{
				return defaultValue;
			}
			int num = Rows.IndexOf(rowKey);
			int num2 = Columns.IndexOf(colKey);
			if (num < 0 || num2 < 0)
			{
				return defaultValue;
			}
			if (num >= values.GetLength(0) || num2 >= values.GetLength(1))
			{
				return defaultValue;
			}
			return values[num, num2];
		}

		public int Get<TRow, TCol>(TRow rowKey, TCol colKey, int defaultValue = 0)
		{
			return Get((object)rowKey, (object)colKey, defaultValue);
		}

		public bool TryGet(object rowKey, object colKey, out int value)
		{
			value = 0;
			if (Rows == null || Columns == null || values == null)
			{
				return false;
			}
			int num = Rows.IndexOf(rowKey);
			int num2 = Columns.IndexOf(colKey);
			if (num < 0 || num2 < 0)
			{
				return false;
			}
			if (num >= values.GetLength(0) || num2 >= values.GetLength(1))
			{
				return false;
			}
			value = values[num, num2];
			return true;
		}

		public IEnumerable<(object columnKey, int value)> GetRow(object rowKey)
		{
			int rowIndex = Rows?.IndexOf(rowKey) ?? (-1);
			if (rowIndex >= 0 && values != null)
			{
				int colCount = values.GetLength(1);
				for (int c = 0; c < colCount; c++)
				{
					object item = ((Columns != null && c < Columns.Count) ? Columns[c] : null);
					yield return (columnKey: item, value: values[rowIndex, c]);
				}
			}
		}

		public IEnumerable<(object rowKey, int value)> GetColumn(object colKey)
		{
			int colIndex = Columns?.IndexOf(colKey) ?? (-1);
			if (colIndex >= 0 && values != null)
			{
				int rowCount = values.GetLength(0);
				for (int r = 0; r < rowCount; r++)
				{
					object item = ((Rows != null && r < Rows.Count) ? Rows[r] : null);
					yield return (rowKey: item, value: values[r, colIndex]);
				}
			}
		}

		private void EnsureMatrixSize()
		{
			int num = Rows?.Count ?? 0;
			int num2 = Columns?.Count ?? 0;
			if (num == 0 || num2 == 0)
			{
				values = new int[0, 0];
			}
			else
			{
				if (values != null && values.GetLength(0) == num && values.GetLength(1) == num2)
				{
					return;
				}
				int[,] array = new int[num, num2];
				if (values != null)
				{
					int num3 = Mathf.Min(num, values.GetLength(0));
					int num4 = Mathf.Min(num2, values.GetLength(1));
					for (int i = 0; i < num3; i++)
					{
						for (int j = 0; j < num4; j++)
						{
							array[i, j] = values[i, j];
						}
					}
				}
				values = array;
			}
		}
	}

	public DifficultyLevel lowDifficultyThreshold;

	public DifficultyLevel additionalDifficultyThreshold;

	public DifficultyLevel topDifficulty;

	public float firstPhaseHealthPercent = 0.45f;

	public float secondPhaseHealthPercent = 0.55f;

	public AnimationCurve NormalThreatProgressCurve;

	public AnimationCurve EliteThreatProgressCurve;

	public AnimationCurve BossThreatProgressCurve;

	public AnimationCurve threatDifficultyCurve;

	public AnimationCurve threatDifficultyMulProgressCurve;

	public AnimationCurve NormalResistanceProgressCurve;

	public AnimationCurve EliteResistanceProgressCurve;

	public AnimationCurve BossResistanceProgressCurve;

	public AnimationCurve resistanceDifficultyCurve;

	public AnimationCurve resistanceDifficultyMulProgressCurve;

	public ObjectTable nightmareTable;

	public float GetExampleDPT(EnemyInitInfo initInfo)
	{
		return GetThreatValue(initInfo);
	}

	public int GetEnemyFirstPhaseHealthValue(EnemyInitInfo initInfo)
	{
		return (int)((float)GetEnemyTotalHealthValue(initInfo) * firstPhaseHealthPercent);
	}

	public int GetEnemySecondPhaseHealthValue(EnemyInitInfo initInfo)
	{
		return (int)((float)GetEnemyTotalHealthValue(initInfo) * secondPhaseHealthPercent);
	}

	public int GetEnemyTotalHealthValue(EnemyInitInfo initInfo)
	{
		return (int)(GetResistanceValue(initInfo) * initInfo.sourceDifficultySetup.difficultyInfo.healthPercent);
	}

	public float GetResistanceValue(EnemyInitInfo initInfo)
	{
		return GetResistanceValue(initInfo.encounterType, initInfo.enemyProgressLevel, initInfo.difficulty);
	}

	public float GetResistanceValue(EnemyEncounterType type, float level, DifficultyLevel difficulty)
	{
		float time = Mathf.Clamp((float)difficulty, 0f, 2.1474836E+09f);
		float num = 1f + resistanceDifficultyCurve.Evaluate(time) * resistanceDifficultyMulProgressCurve.Evaluate(level);
		return type switch
		{
			EnemyEncounterType.Normal => NormalResistanceProgressCurve.Evaluate(level), 
			EnemyEncounterType.Elite => EliteResistanceProgressCurve.Evaluate(level), 
			EnemyEncounterType.Boss => BossResistanceProgressCurve.Evaluate(level), 
			_ => NormalResistanceProgressCurve.Evaluate(level), 
		} * num;
	}

	public float GetThreatValue(EnemyInitInfo initInfo)
	{
		return GetThreatValue(initInfo.encounterType, initInfo.enemyProgressLevel, initInfo.difficulty);
	}

	public float GetEnemyThreatValuePerSlot(EnemyInitInfo initInfo)
	{
		int intentSlotCount = initInfo.sourceDifficultySetup.difficultyInfo.intentSlotCount;
		if (intentSlotCount == 0)
		{
			return 0f;
		}
		return GetThreatValue(initInfo) / (float)intentSlotCount;
	}

	public float GetThreatValue(EnemyEncounterType type, float progress, DifficultyLevel difficulty)
	{
		float time = Mathf.Clamp((float)difficulty, 0f, 2.1474836E+09f);
		float num = 1f + threatDifficultyCurve.Evaluate(time) * threatDifficultyMulProgressCurve.Evaluate(progress);
		return type switch
		{
			EnemyEncounterType.Normal => NormalThreatProgressCurve.Evaluate(progress), 
			EnemyEncounterType.Elite => EliteThreatProgressCurve.Evaluate(progress), 
			EnemyEncounterType.Boss => BossThreatProgressCurve.Evaluate(progress), 
			_ => NormalThreatProgressCurve.Evaluate(progress), 
		} * num;
	}

	public bool CheckItemWithNoSpellDamage(Item item)
	{
		if (item.GetDef()?.abilities == null)
		{
			return false;
		}
		foreach (Ability ability in item.GetDef().abilities)
		{
			foreach (DealDamage item2 in ability.GetDescendantNodeOfType<DealDamage>())
			{
				if (item2.targetBFT != GameEvent.EventBFTType.Owner)
				{
					return true;
				}
			}
		}
		return false;
	}
}
