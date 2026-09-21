using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Common.UI;
using TMPro;
using UnityEngine;

public class DailyQuestPanel : UIBase, IShowAsync, IHideAsync
{
	private static readonly Regex LegacyResetTimeRegex = new Regex("(?<!\\d)4(?::00)?(?:\\s*[ap]\\.?\\s*m\\.?)?(?!\\d)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	public GameButton backButton;

	public MatterCell refreshMatter;

	public DailyQuestCell dailyQuestCellPrefab;

	public RectTransform dailyQuestCellParent;

	[SerializeField]
	private TextMeshProUGUI dailyReplaceHint;

	private CellUIPrefabPool<DailyQuestCell, RoutineQuest> cellPool;

	protected override void Init()
	{
		InitUI(refreshMatter);
		GetPrefabPool(ref cellPool, dailyQuestCellPrefab, dailyQuestCellParent);
		backButton.onClick.AddListener(((UIBase)this).HideUI);
		refreshMatter.Setup(UIBase.applicationState.matterManager.GetMatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.refreshDailyQuestMatter.matterDef));
	}

	protected override void UpdateDisplay()
	{
		IEnumerable<RoutineQuest> source = UIBase.applicationState.routineQuestManager.currentDailyQuests.Where((RoutineQuest q) => q != null && !q.AllLayerFinished);
		cellPool.Sync(source.ToList());
		UpdateDailyResetHint();
	}

	private void UpdateDailyResetHint()
	{
		if (!(dailyReplaceHint == null))
		{
			string input = dailyReplaceHint.text;
			TMPLocalize component = dailyReplaceHint.GetComponent<TMPLocalize>();
			string text = ((component == null) ? null : LocalizationManager.GetLocalizeTerm(component.id))?.GetText(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
			if (!string.IsNullOrEmpty(text))
			{
				input = text;
			}
			DateTime localResetTime = ScriptableObjectSingleton<CalendarDataManager>.instance.NextDailyResetTime.ToLocalTime();
			dailyReplaceHint.text = LegacyResetTimeRegex.Replace(input, (Match match) => (!match.Value.Contains(":") && localResetTime.Minute == 0) ? localResetTime.Hour.ToString(CultureInfo.InvariantCulture) : localResetTime.ToString("H:mm", CultureInfo.InvariantCulture), 1);
		}
	}

	void IShowAsync.OnShowStart()
	{
		SetActiveAndDirtyWithFadeAnim(active: true);
	}

	void IHideAsync.OnHideStart()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
	}
}
