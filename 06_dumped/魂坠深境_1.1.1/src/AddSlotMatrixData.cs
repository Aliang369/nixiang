using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "打孔矩阵配置", menuName = "Singleton/AddSlotMatrixData")]
public class AddSlotMatrixData : ScriptableObject
{
	[Serializable]
	public class SlotProbabilityRow
	{
		public int currentSlot;

		public List<SlotProbability> targetSlotProbabilities = new List<SlotProbability>();
	}

	[Serializable]
	public class SlotProbability
	{
		public int targetSlot;

		public float probability;
	}

	public List<SlotProbabilityRow> slotMatrix = new List<SlotProbabilityRow>();

	private void ImportCSV()
	{
	}

	private void ExportCSV()
	{
	}
}
