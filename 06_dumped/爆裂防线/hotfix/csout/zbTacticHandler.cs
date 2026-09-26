using System.Linq;
using Proto.Types;

public class zbTacticHandler
{
	private zbTacticCsvRecord[] tacticCsvRecord;

	public void Init()
	{
		tacticCsvRecord = zbDataConfig.Instance.tacticCsvTable.GetRecords().records.Values.ToArray();
	}

	public zbTacticCsvRecord[] GetAllTacticRecords()
	{
		return tacticCsvRecord;
	}

	public TacticDetail GetTacticDetail(int tacticId)
	{
		if (zbServerData.sGameData.TacticList != null)
		{
			foreach (TacticDetail tactic in zbServerData.sGameData.TacticList)
			{
				if (tactic.TacticTid == tacticId)
				{
					return tactic;
				}
			}
		}
		return null;
	}
}
