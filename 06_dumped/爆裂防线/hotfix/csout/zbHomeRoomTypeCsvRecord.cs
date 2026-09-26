using System;

[Serializable]
public class zbHomeRoomTypeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public int subType { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public string door { get; set; }

	public string leftdoor { get; set; }

	public string rightdoor { get; set; }

	public string iconBroken { get; set; }

	public string spineBroken { get; set; }

	public string leftdoor_b { get; set; }

	public string rightdoo_b { get; set; }

	public string iconEdit { get; set; }

	public string iconType { get; set; }

	public string iconBuild { get; set; }

	public int destroy { get; set; }

	public int move { get; set; }

	public int repair { get; set; }

	public int maxNum { get; set; }

	public int length { get; set; }

	public int workerNum { get; set; }

	public int itemId { get; set; }

	public int peerageId { get; set; }

	public int[] compose { get; set; }

	public float[][] roomAttr { get; set; }

	public int sortIndex { get; set; }

	public int[] manSuit { get; set; }

	public int[] womanSuit { get; set; }

	public int[] robotSuit { get; set; }

	public string preShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHomeRoomTypeCsvRecord Read(zbBinReader r)
	{
		return new zbHomeRoomTypeCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			subType = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			door = r.ReadString(),
			leftdoor = r.ReadString(),
			rightdoor = r.ReadString(),
			iconBroken = r.ReadString(),
			spineBroken = r.ReadString(),
			leftdoor_b = r.ReadString(),
			rightdoo_b = r.ReadString(),
			iconEdit = r.ReadString(),
			iconType = r.ReadString(),
			iconBuild = r.ReadString(),
			destroy = r.ReadInt(),
			move = r.ReadInt(),
			repair = r.ReadInt(),
			maxNum = r.ReadInt(),
			length = r.ReadInt(),
			workerNum = r.ReadInt(),
			itemId = r.ReadInt(),
			peerageId = r.ReadInt(),
			compose = r.ReadIntArray(),
			roomAttr = r.ReadFloatArray2(),
			sortIndex = r.ReadInt(),
			manSuit = r.ReadIntArray(),
			womanSuit = r.ReadIntArray(),
			robotSuit = r.ReadIntArray(),
			preShow = r.ReadString()
		};
	}
}
