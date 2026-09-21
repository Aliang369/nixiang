using System;
using UnityEngine;

[Serializable]
public class DialogueText
{
	public float afterWaitTimeRatio = 1f;

	public float prewarmTime;

	public float readSpeedRatio = 1f;

	[TextArea(1, 5)]
	[LocalizeText("对话文本")]
	public string text;

	public float AfterWaitTime => afterWaitTimeRatio * ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueBaseWaitTime;

	public float ReadSpeed => 9999f;

	public float SimuReadTime
	{
		get
		{
			if (!(ReadSpeed > 0f))
			{
				return 0f;
			}
			return (float)(text?.RemoveRichTextTag().Length ?? 0) / ReadSpeed;
		}
	}
}
