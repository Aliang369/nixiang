using TMPro;

public static class TextEmergeEffectExtends
{
	public static TextEmergeEffect TextEmergeEffect(this TextMeshProUGUI text)
	{
		return text.GetComponent<TextEmergeEffect>();
	}
}
