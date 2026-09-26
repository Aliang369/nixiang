using System;

[Serializable]
public class zbFunctionUnlockCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int unlockType { get; set; }

	public int unlockVal { get; set; }

	public int serverHours { get; set; }

	public int showTime { get; set; }

	public int[][] awardList { get; set; }

	public string linkUrl { get; set; }

	public string bannerPath { get; set; }

	public string subjectPath { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFunctionUnlockCsvRecord Read(zbBinReader r)
	{
		return new zbFunctionUnlockCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			unlockType = r.ReadInt(),
			unlockVal = r.ReadInt(),
			serverHours = r.ReadInt(),
			showTime = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			linkUrl = r.ReadString(),
			bannerPath = r.ReadString(),
			subjectPath = r.ReadString()
		};
	}
}
