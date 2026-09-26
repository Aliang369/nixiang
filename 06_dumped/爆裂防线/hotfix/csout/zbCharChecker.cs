using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

public class zbCharChecker
{
	private static readonly Regex SqlInjectionRegex = new Regex("\\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXEC|UNION|OR|AND|XP_)\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static readonly Regex SpecialCharsRegex = new Regex("(--|;|/\\*|\\*/|'|\")", RegexOptions.Compiled);

	private static readonly Regex HtmlTagRegex = new Regex("<.*?>", RegexOptions.Compiled);

	private static readonly HashSet<string> SensitiveWords = new HashSet<string>();

	public static void InsertNewBadwords()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		foreach (JToken item in (JArray)zbDataConfig.Instance.badwordsJsonTable.GetRecord()["banned_words"])
		{
			SensitiveWords.Add(((object)item).ToString());
		}
	}

	public static bool IsSafeChatMessage(string input, int maxInputLength = 200)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return true;
		}
		if (input.Length > maxInputLength)
		{
			return false;
		}
		if (SqlInjectionRegex.IsMatch(input))
		{
			return false;
		}
		if (ContainsSensitiveWords(input))
		{
			return false;
		}
		return true;
	}

	public static string SanitizeChatMessage(string input, int maxInputLength = 200)
	{
		if (string.IsNullOrEmpty(input))
		{
			return string.Empty;
		}
		if (input.Length > maxInputLength)
		{
			input = input.Substring(0, maxInputLength);
		}
		input = SqlInjectionRegex.Replace(input, "");
		input = SpecialCharsRegex.Replace(input, "");
		input = HtmlTagRegex.Replace(input, "");
		return input.Trim();
	}

	private static bool ContainsSensitiveWords(string input)
	{
		string text = input.Replace(" ", "");
		foreach (string sensitiveWord in SensitiveWords)
		{
			if (text.Contains(sensitiveWord))
			{
				return true;
			}
		}
		return false;
	}

	public static string CensorSensitiveWords(string input)
	{
		foreach (string sensitiveWord in SensitiveWords)
		{
			input = input.Replace(sensitiveWord, "***");
		}
		return input;
	}
}
