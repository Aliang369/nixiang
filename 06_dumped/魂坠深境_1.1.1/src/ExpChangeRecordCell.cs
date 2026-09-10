using TMPro;

public class ExpChangeRecordCell : CellUI<GameRecord.ExpChangeRecord>
{
	public TextMeshProUGUI text;

	protected override void OnSetup(GameRecord.ExpChangeRecord data)
	{
		text.text = data.GetFullDescription(CellUI<GameRecord.ExpChangeRecord>.applicationState);
	}
}
