using System;
using TMPro;

public class CarnivalCheckInPanel : FestivalCheckInPanel
{
	public TextMeshProUGUI carnivalRemainTimeText;

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		string text = (UIBase.applicationState.dailyCheckManager.weekCheckState?.festivalDef)?.limitedTimeActivity?.FullID;
		if (!(carnivalRemainTimeText == null))
		{
			if (!string.IsNullOrEmpty(text) && UIBase.applicationState.limitedTimeActivityManager.TryGetRuntimeConfig(text, out var config))
			{
				TimeSpan span = config.EndTimeUtc - DateTime.UtcNow;
				carnivalRemainTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_remain_time_arg0.FormatWith(TextDisplayHelper.GetTimeSpanText(span));
			}
			else
			{
				carnivalRemainTimeText.text = string.Empty;
			}
		}
	}
}
