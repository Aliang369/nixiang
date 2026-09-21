using System;
using System.Collections.Generic;

[Serializable]
public class NewbieFestivalLocalDrawnSlotIndexes
{
	[SaveField(null, null)]
	public int DayIndex;

	[SaveField(null, null)]
	public List<int> SlotIndexes;
}
