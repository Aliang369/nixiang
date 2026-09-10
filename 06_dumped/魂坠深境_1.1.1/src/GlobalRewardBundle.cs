using System.Collections.Generic;
using UnityEngine;

public class GlobalRewardBundle : CTScriptableObject
{
	[CTSOAssetName]
	[LocalizeText("礼包")]
	public string title;

	[TextArea(1, 6)]
	[LocalizeText("礼包")]
	public string description;

	[TextArea(1, 6)]
	[LocalizeText("礼包")]
	public string subDescription;

	public Sprite icon;

	public QualityLevel displayQuality = QualityLevel.Epic;

	public List<GlobalRewardSetup> rewards;

	public override string CTAssetIDPrepend => "GLOBALREWARDBUNDLE";

	public string GetAllBundleBrief
	{
		get
		{
			string text = "";
			foreach (GlobalRewardSetup reward in rewards)
			{
				text = text + reward.GetRewardBrief() + "\n";
			}
			return text;
		}
	}
}
