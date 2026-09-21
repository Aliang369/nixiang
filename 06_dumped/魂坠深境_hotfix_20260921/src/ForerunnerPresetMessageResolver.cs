using System.Collections.Generic;
using UnityEngine;

public static class ForerunnerPresetMessageResolver
{
	public static bool TryGet(string presetMessageId, out TextDataPreset.ForerunnerPresetMessage message)
	{
		message = null;
		if (presetMessageId.IsNullOrEmpty())
		{
			return false;
		}
		List<TextDataPreset.ForerunnerPresetMessage> list = ScriptableObjectSingleton<TextDataPreset>.instance?.forerunnerPresetMessages;
		if (list == null)
		{
			return false;
		}
		foreach (TextDataPreset.ForerunnerPresetMessage item in list)
		{
			if (item != null && item.id == presetMessageId)
			{
				message = item;
				return true;
			}
		}
		return false;
	}

	public static List<TextDataPreset.ForerunnerPresetMessage> GetEnabledMessages()
	{
		List<TextDataPreset.ForerunnerPresetMessage> list = new List<TextDataPreset.ForerunnerPresetMessage>();
		List<TextDataPreset.ForerunnerPresetMessage> list2 = ScriptableObjectSingleton<TextDataPreset>.instance?.forerunnerPresetMessages;
		if (list2 == null)
		{
			return list;
		}
		foreach (TextDataPreset.ForerunnerPresetMessage item in list2)
		{
			if (item != null && item.enabled && !item.id.IsNullOrEmpty() && !item.text.IsNullOrEmpty())
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static bool IsEnabledForUpload(string presetMessageId)
	{
		if (TryGet(presetMessageId, out var message))
		{
			return message.enabled;
		}
		return false;
	}

	public static TextDataPreset.ForerunnerPresetMessage PickRandomEnabled()
	{
		List<TextDataPreset.ForerunnerPresetMessage> enabledMessages = GetEnabledMessages();
		if (enabledMessages.Count != 0)
		{
			return enabledMessages[Random.Range(0, enabledMessages.Count)];
		}
		return null;
	}

	public static string ResolveText(ForerunnerLegacy legacy)
	{
		if (legacy == null)
		{
			return string.Empty;
		}
		if (TryGet(legacy.presetMessageId, out var message) && !message.text.IsNullOrEmpty())
		{
			return message.text;
		}
		if (!legacy.note.IsNullOrEmpty())
		{
			return legacy.note;
		}
		if (!legacy.presetMessageId.IsNullOrEmpty())
		{
			Debug.LogError("Unknown forerunner preset message ID: " + legacy.presetMessageId);
		}
		return legacy.note ?? string.Empty;
	}
}
