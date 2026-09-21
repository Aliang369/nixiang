using System.Collections.Generic;
using UnityEngine;

public class ClavisDef : CTScriptableObject, IAssetIcon
{
	public Sprite icon;

	[CTSOAssetName]
	[LocalizeText("钥匙名字")]
	public string clavisName;

	public Color nameTextColor;

	[LocalizeText("钥匙描述")]
	[TextArea]
	public string storyDescription;

	[LocalizeText("钥匙激活提示")]
	[TextArea]
	public string activatedHint;

	[LocalizeText("钥匙激活选项标题文本")]
	public string majorChoiceText;

	[LocalizeText("钥匙激活选项描述文本")]
	public string minorChoiceText;

	public bool activateAfterUnlock;

	public bool hiddenClavis;

	public int displaySortIndex;

	public GlobalTriggerObserver unlockObserver;

	public List<GlobalRewardSetup> activateRewards;

	public override string CTAssetIDPrepend => "CLA";

	public string NameWithColor => clavisName.ColorText(nameTextColor);

	public Sprite IconAsset => icon;

	public bool isUnlocked => Bootstrap.instance.CurrentApplicationState.clavisManager.IsUnlocked(this);

	public void InitObserver(ApplicationStateInfo applicationState)
	{
		if (unlockObserver == null || applicationState.clavisManager.IsUnlocked(this))
		{
			return;
		}
		unlockObserver.Init(applicationState, delegate
		{
			if (!applicationState.clavisManager.IsUnlocked(this))
			{
				applicationState.clavisManager.UnlockClavis(this);
			}
		});
	}

	public string GetMajorText(ApplicationStateInfo applicationState)
	{
		string text = majorChoiceText;
		if (!applicationState.clavisManager.IsUnlocked(this))
		{
			return text + ScriptableObjectSingleton<TextDataPreset>.instance.term_append_text_use_clavis_need_arg0.FormatWith(NameWithColor);
		}
		return text + ScriptableObjectSingleton<TextDataPreset>.instance.term_append_text_use_clavis_with_arg0.FormatWith(NameWithColor);
	}

	public string GetMinorText(ApplicationStateInfo applicationState)
	{
		return minorChoiceText;
	}
}
