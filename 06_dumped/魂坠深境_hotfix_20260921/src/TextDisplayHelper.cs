using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public static class TextDisplayHelper
{
	private static readonly Regex colorTagMatchRegex = new Regex("<color=#([0-9a-fA-F]{8})>");

	private static readonly Regex richTextTagRegex = new Regex("<[^<>]*>");

	private static readonly Regex colorRichTextTagRegex = new Regex("<[^<>]*color[^<>]*>");

	private static readonly Regex newLineRegex = new Regex("\\r\\n|\\n\\r|\\n|\\r");

	public const string RefLinkObjectPrefix = "gen";

	public static string ViewText(this string text)
	{
		return text + ScriptableObjectSingleton<GameDataManager>.instance.viewTextIcon.ToTMPSpriteText().SizeTextPercent(75);
	}

	public static string GetRoughTimeSpanText(TimeSpan span)
	{
		if (span.Days >= 1)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_days.FormatWith(span.Days);
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_hours.FormatWith(span.Hours);
	}

	public static string GetTimeSpanText(TimeSpan span)
	{
		string text = "";
		if (span.Days >= 1)
		{
			text += ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_days.FormatWith(span.Days);
		}
		if (span.Hours >= 1)
		{
			text += ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_hours.FormatWith(span.Hours);
		}
		if (span.Days < 1)
		{
			text += ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_minutes.FormatWith(span.Minutes);
		}
		return text;
	}

	public static string GetDetailTimeSpanText(TimeSpan ts)
	{
		return ts.ToString("hh\\:mm\\:ss");
	}

	public static string GetAdaptiveCountdownText(TimeSpan span)
	{
		if (span <= TimeSpan.Zero)
		{
			return GetDetailTimeSpanText(TimeSpan.Zero);
		}
		if (span.TotalHours >= 24.0)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_days.FormatWith(Mathf.CeilToInt((float)span.TotalDays));
		}
		return GetDetailTimeSpanText(span);
	}

	public static string GetDateText(DateTime date)
	{
		return string.Concat("" + ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_day.FormatWith(date.Day), ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_hour.FormatWith(date.Hour));
	}

	public static string GetStanceChangeText(Stance.ChangeType type, bool isFull = false)
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		switch (type)
		{
		case Stance.ChangeType.Shift:
			if (isFull)
			{
				return instance.term_stance_shift_and_full;
			}
			return instance.term_stance_shift;
		case Stance.ChangeType.Stack:
			if (isFull)
			{
				return instance.term_stance_stack_and_full;
			}
			return instance.term_stance_stack;
		case Stance.ChangeType.Full:
			return instance.term_stance_full;
		case Stance.ChangeType.Break:
			return instance.term_stance_break;
		default:
			return string.Empty;
		}
	}

	public static string GetDetailTimeSpanText(long ticks)
	{
		return GetDetailTimeSpanText(new TimeSpan(ticks));
	}

	public static string FormatNameWithCount(string name, int count)
	{
		if (name.IsNullOrEmpty())
		{
			return string.Empty;
		}
		if (count <= 1)
		{
			return name;
		}
		return $"{name} <size=80%>x</size>{count}";
	}

	public static string NoBreak(this string text)
	{
		if (text.IsNullOrEmpty())
		{
			return text;
		}
		return "<nobr>" + text + "</nobr>";
	}

	public static string Strikethrough(this string text)
	{
		return "<s>" + text + "</s>";
	}

	public static string ColorText(this string text, Color color)
	{
		return "<color=#" + color.Hex() + ">" + text + "</color>";
	}

	public static string QualityColorText(this string text, QualityLevel qualityLevel)
	{
		return text.ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.qualityDisplayInfos[qualityLevel].qualityBaseColor);
	}

	public static string ConvertTextColorTag(this string text, Func<Color, Color> converter)
	{
		MatchCollection matchCollection = colorTagMatchRegex.Matches(text);
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (Match item in matchCollection)
		{
			int index = item.Index;
			int length = item.Length;
			Color arg = HexToColor(item.Groups[1].Value);
			stringBuilder.Append(text.Substring(num, index - num));
			stringBuilder.Append("<color=#" + converter(arg).Hex() + ">");
			num = index + length;
		}
		stringBuilder.Append(text.Substring(num));
		return stringBuilder.ToString();
	}

	public static string AlignText(this string text, TextAlignment align)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		return (int)align switch
		{
			0 => "<align=\"left\">" + text + "</align>", 
			1 => "<align=\"center\">" + text + "</align>", 
			2 => "<align=\"right\">" + text + "</align>", 
			_ => text, 
		};
	}

	public static string BoldText(this string text)
	{
		return "<b>" + text + "</b>";
	}

	public static string LinkReferenceObject(this string text, int objectIndex)
	{
		return text.LinkText(string.Format("{0}_{1}", "gen", objectIndex));
	}

	public static string LinkText(this string text, string locator)
	{
		return "<link=" + locator + "><u>" + text + "</u></link>";
	}

	public static string FlashAlphaText(this string text, int id)
	{
		return $"<link=fade{id}></link>{text}<link=endfade{id}></link>";
	}

	public static string NumToStringWithSign(int num)
	{
		return num.ToString("+0;-#");
	}

	public static string NumToStringWithSignAndZero(int num)
	{
		return num.ToString("+0;-#;0");
	}

	public static string NumToStringWithSign(float num)
	{
		return num.ToString("+0.##;-0.##;0");
	}

	public static string NumToPercentage(float num)
	{
		return (num * 100f).ToString("0.00", CultureInfo.CurrentCulture) + "%";
	}

	public static string NumToPercentageWithSmallPercentSign(float num, int valueSizePercent = 110, int percentSizePercent = 85)
	{
		string text = NumToPercentage(num);
		int num2 = text.LastIndexOf('%');
		if (num2 < 0)
		{
			return text.SizeTextPercent(valueSizePercent);
		}
		string text2 = text.Substring(0, num2);
		string text3 = text.Substring(num2);
		string percentDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.PercentDecimalSeparator;
		int num3 = (string.IsNullOrEmpty(percentDecimalSeparator) ? (-1) : text2.IndexOf(percentDecimalSeparator, StringComparison.Ordinal));
		if (num3 < 0)
		{
			num3 = text2.IndexOf(".", StringComparison.Ordinal);
		}
		if (num3 < 0)
		{
			return text2.SizeTextPercent(valueSizePercent) + text3.SizeTextPercent(percentSizePercent);
		}
		string text4 = text2.Substring(0, num3);
		string text5 = text2.Substring(num3) + text3;
		return text4.SizeTextPercent(valueSizePercent) + text5.SizeTextPercent(percentSizePercent);
	}

	public static string PosTextPercent(string text, int percent)
	{
		return "<pos=" + percent + "%>" + text + "</pos>";
	}

	public static string SizeText(this string text, int size)
	{
		return $"<size={size}>{text}</size>";
	}

	public static string SizeTextPercent(this string text, int percent)
	{
		return $"<size={percent}%>{text}</size>";
	}

	public static string ToNumSpriteString(int num, int offset = 0)
	{
		if (num == 0)
		{
			return $"<sprite={offset} tint=1>";
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (num > 0)
		{
			int num2 = num % 10;
			num /= 10;
			stringBuilder.Insert(0, $"<sprite={num2 + offset} tint=1>");
		}
		return stringBuilder.ToString();
	}

	public static string ToTMPSpriteText(this Sprite icon)
	{
		return "<sprite name=\"" + icon?.name + "\">";
	}

	public static string SoulText(CharacterClassType classType)
	{
		return ScriptableObjectSingleton<GameDataManager>.instance.GetCharacterClassSoulTextIcon(classType).ToTMPSpriteText();
	}

	public static string ToTMPSpriteText(this Sprite icon, Color color)
	{
		return "<sprite name=\"" + icon?.name + "\" color=#" + color.Hex() + ">";
	}

	public static string Hex(this Color color)
	{
		return ((Color32)color).Hex();
	}

	public static Color HexToColor(string str)
	{
		if (str.StartsWith("#"))
		{
			str = str.Substring(1);
		}
		return new Color32
		{
			r = byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber),
			g = byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber),
			b = byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber),
			a = byte.Parse(str.Substring(6, 2), NumberStyles.HexNumber)
		};
	}

	public static string Hex(this Color32 color)
	{
		return $"{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
	}

	public static string RemoveRichTextTag(this string s)
	{
		return richTextTagRegex.Replace(s, "");
	}

	public static IEnumerable<string> ExtractRichTextTags(this string s)
	{
		return from m in richTextTagRegex.Matches(s)
			select m.Value;
	}

	public static string RemoveColorRichTextTag(this string s)
	{
		return colorRichTextTagRegex.Replace(s, "");
	}

	public static string RemoveNewLine(this string s)
	{
		return newLineRegex.Replace(s, "");
	}

	public static string AppendIfNotEmpty(this string s, string append)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		return s + append;
	}

	public static string AppendNewLineIfNotEmpty(this string s, string newLineText)
	{
		if (string.IsNullOrEmpty(newLineText))
		{
			return s;
		}
		return s + newLineText + Environment.NewLine;
	}

	public static string GetProgressTextWithoutTag(this string str, float progress)
	{
		string text = str.RemoveRichTextTag();
		int num = ((progress >= 1f) ? text.Length : Mathf.RoundToInt(progress * (float)text.Length));
		int num2 = 0;
		int length = 0;
		int num3 = 0;
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == '<')
			{
				num2++;
			}
			if (num2 == 0)
			{
				length = i + 1;
				num3++;
				if (num3 >= num)
				{
					break;
				}
			}
			if (str[i] == '>')
			{
				num2--;
			}
		}
		return str.Substring(0, length);
	}

	public static IEnumerator ScrollTextCor(this TextMeshProUGUI tmp, string target, float speed, Func<bool> skipChecker = null)
	{
		float wordProgress = 0f;
		while (true)
		{
			wordProgress += Time.deltaTime * speed / (float)target.RemoveRichTextTag().Length;
			tmp.text = target.GetProgressTextWithoutTag(wordProgress);
			if (skipChecker != null && skipChecker())
			{
				wordProgress = 1f;
				tmp.text = target.GetProgressTextWithoutTag(wordProgress);
				break;
			}
			if (!(wordProgress >= 1f))
			{
				yield return new WaitForEndOfFrame();
				continue;
			}
			break;
		}
	}

	private static IEnumerator RollTextToNumberCor(this TextMeshProUGUI text, int baseNum, int deltaNum, float time)
	{
		float timer = 0f;
		while (!(timer > time))
		{
			timer += Time.deltaTime;
			int num = (int)Mathf.Lerp(baseNum, baseNum + deltaNum, timer / time);
			text.text = $"{num}";
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}
}
