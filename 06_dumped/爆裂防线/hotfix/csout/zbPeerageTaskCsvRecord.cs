using System;

[Serializable]
public class zbPeerageTaskCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int peerageId { get; set; }

	public int type { get; set; }

	public int target { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPeerageTaskCsvRecord Read(zbBinReader r)
	{
		return new zbPeerageTaskCsvRecord
		{
			id = r.ReadInt(),
			peerageId = r.ReadInt(),
			type = r.ReadInt(),
			target = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
