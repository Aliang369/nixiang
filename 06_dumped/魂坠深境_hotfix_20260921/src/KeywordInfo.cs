public class KeywordInfo
{
	public KeywordDef keywordDef;

	public string subText;

	public int value;

	public GameEvent displayGameEvent;

	public KeywordInfo(KeywordDef keywordDef, GameEvent displayGameEvent)
	{
		this.keywordDef = keywordDef;
		this.displayGameEvent = displayGameEvent;
	}
}
