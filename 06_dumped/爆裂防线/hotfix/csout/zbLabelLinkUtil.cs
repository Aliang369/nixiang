using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class zbLabelLinkUtil
{
	private static ELanguageType[] HideUnderLineLanguages = new ELanguageType[4]
	{
		ELanguageType.Ko,
		ELanguageType.Ja,
		ELanguageType.Zhcn,
		ELanguageType.Tr
	};

	public static string BuildResourceLink(EResourceType type, long id)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		string strName = zbResourceHelper.GetResorceIconPath(type, id).strName;
		string text = ColorUtility.ToHtmlStringRGB(zbResourceHelper.GetColor(zbResourceHelper.GetResourceQuality(type, id)));
		return "<color=#" + text + "><u>" + strName + "</u></color>";
	}

	public static void BuildModLink(string desc, Label label)
	{
		if (label == null || string.IsNullOrEmpty(desc))
		{
			return;
		}
		List<zbLangTermCsvRecord> list = zbDataConfig.Instance.langTermCsvTable.GetRecords().records.ToList();
		((TextElement)label).enableRichText = true;
		Dictionary<string, zbLangTermCsvRecord> dictionary = new Dictionary<string, zbLangTermCsvRecord>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, zbLangTermCsvRecord> idToRecordMap = new Dictionary<string, zbLangTermCsvRecord>();
		foreach (zbLangTermCsvRecord item in list)
		{
			string stringTable = zbLanguage.Instance.GetStringTable(item.name);
			if (!string.IsNullOrEmpty(stringTable) && !dictionary.ContainsKey(stringTable))
			{
				dictionary.Add(stringTable, item);
				if (!string.IsNullOrEmpty(item.desc))
				{
					idToRecordMap[item.name] = item;
				}
			}
		}
		ELanguageType languageType = zbLanguage.Instance.GetLanguageType();
		bool flag = Enumerable.Contains(HideUnderLineLanguages, languageType);
		List<LinkMatch> list2 = new List<LinkMatch>();
		foreach (KeyValuePair<string, zbLangTermCsvRecord> item2 in dictionary)
		{
			string key = item2.Key;
			int startIndex = 0;
			while (true)
			{
				int num = desc.IndexOf(key, startIndex, StringComparison.OrdinalIgnoreCase);
				if (num < 0)
				{
					break;
				}
				list2.Add(new LinkMatch
				{
					Index = num,
					Length = key.Length,
					Text = key,
					Record = item2.Value
				});
				startIndex = num + 1;
			}
		}
		list2 = (from x in list2
			orderby x.Length descending, x.Index
			select x).ToList();
		List<LinkMatch> list3 = new List<LinkMatch>();
		foreach (LinkMatch item3 in list2)
		{
			bool flag2 = false;
			foreach (LinkMatch item4 in list3)
			{
				if (item3.Index < item4.Index + item4.Length && item4.Index < item3.Index + item3.Length)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				list3.Add(item3);
			}
		}
		list3 = list3.OrderBy((LinkMatch x) => x.Index).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		int num2 = 0;
		foreach (LinkMatch item5 in list3)
		{
			stringBuilder.Append(desc, num2, item5.Index - num2);
			if (string.IsNullOrEmpty(item5.Record.desc))
			{
				stringBuilder.Append(item5.Text);
			}
			else
			{
				string name = item5.Record.name;
				if (flag)
				{
					stringBuilder.Append("<color=#64FF26><link=\"" + name + "\">" + item5.Text + "</link></color>");
				}
				else
				{
					stringBuilder.Append("<color=#64FF26><u><link=\"" + name + "\">" + item5.Text + "</link></u></color>");
				}
			}
			num2 = item5.Index + item5.Length;
		}
		stringBuilder.Append(desc, num2, desc.Length - num2);
		((TextElement)label).text = stringBuilder.ToString();
		EventCallback<PointerUpLinkTagEvent> val = delegate(PointerUpLinkTagEvent evt)
		{
			OnClickLink(evt, idToRecordMap);
		};
		if (((VisualElement)label).userData is EventCallback<PointerUpLinkTagEvent> val2)
		{
			((CallbackEventHandler)label).UnregisterCallback<PointerUpLinkTagEvent>(val2, (TrickleDown)0);
		}
		((VisualElement)label).userData = val;
		((CallbackEventHandler)label).RegisterCallback<PointerUpLinkTagEvent>(val, (TrickleDown)0);
	}

	private static void OnClickLink(PointerUpLinkTagEvent evt, Dictionary<string, zbLangTermCsvRecord> idToRecordMap)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (evt == null)
		{
			return;
		}
		string linkID = evt.linkID;
		if (!string.IsNullOrEmpty(linkID) && idToRecordMap.TryGetValue(linkID, out var value))
		{
			IEventHandler target = ((EventBase)evt).target;
			VisualElement val = (VisualElement)(object)((target is VisualElement) ? target : null);
			if (val != null)
			{
				Rect worldBound = val.worldBound;
				Vector2 position = ((Rect)(ref worldBound)).position;
				Vector2 pos = VisualElementExtensions.WorldToLocal(zbUIMgr.Instance.GetRootElement(), position);
				zbUIMgr.Instance.ShowTermDescTip(value, pos);
			}
		}
	}

	public static bool HasTerm(string desc)
	{
		if (string.IsNullOrEmpty(desc))
		{
			return false;
		}
		foreach (zbLangTermCsvRecord item in zbDataConfig.Instance.langTermCsvTable.GetRecords().records.Where((zbLangTermCsvRecord r) => !string.IsNullOrEmpty(r.desc)).ToList())
		{
			string stringTable = zbLanguage.Instance.GetStringTable(item.name);
			if (!string.IsNullOrEmpty(stringTable) && Regex.IsMatch(desc, Regex.Escape(stringTable), RegexOptions.IgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
