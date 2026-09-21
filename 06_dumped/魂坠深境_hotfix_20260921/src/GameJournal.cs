using System.Collections.Generic;
using System.Text;

public class GameJournal
{
	[SaveField(null, null)]
	public List<GameJournalRecord> records = new List<GameJournalRecord>();

	public string GenerateJournalText(ApplicationStateInfo applicationState)
	{
		StringBuilder stringBuilder = new StringBuilder(10000);
		foreach (GameJournalRecord record in records)
		{
			if (record.IsVisibleToPlayer(applicationState))
			{
				stringBuilder.AppendLine(record.GenerateRecordLine(applicationState));
			}
		}
		return stringBuilder.ToString();
	}
}
