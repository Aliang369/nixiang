public class ConditionCheckResult
{
	public string Description;

	public bool IsMet;

	public zbItemData NeedItem;

	public bool IsMaxLevel;

	public bool ShowInUI;

	public ConditionCheckResult(string description, bool isMet, zbItemData needItem = null, bool isMaxLevel = false, bool showInUI = true)
	{
		Description = description;
		IsMet = isMet;
		NeedItem = needItem;
		IsMaxLevel = isMaxLevel;
		ShowInUI = showInUI;
	}
}
