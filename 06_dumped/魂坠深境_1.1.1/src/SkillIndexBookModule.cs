using ContainerOfSoul.Display.Fortress.IndexDisplay;
using UnityEngine;
using UnityEngine.UI;

public class SkillIndexBookModule : SkillDisplay.DisplayModule
{
	public GameObject indexNotDiscoverDarken;

	public Image notDiscoverDarkenSkillIcon;

	public IndexBookContentMask indexBookContentMask;

	public override void Init()
	{
		base.display.InitUI(indexBookContentMask);
	}

	public override void Setup()
	{
		indexBookContentMask.Setup(GetIndexEntry());
	}

	public override void UpdateDisplay()
	{
		SkillQualityIndexEntry indexEntry = GetIndexEntry();
		if (UIDisplayModule.applicationState.discoverManager.IsDiscovered(indexEntry))
		{
			indexNotDiscoverDarken.gameObject.SetActive(value: false);
			return;
		}
		indexNotDiscoverDarken.gameObject.SetActive(value: true);
		notDiscoverDarkenSkillIcon.sprite = base.display.setupData.skillDef.Icon;
	}

	private SkillQualityIndexEntry GetIndexEntry()
	{
		return UIDisplayModule.applicationState.assetCatalog.GetSkillQualityIndexEntry(base.display.setupData.skillDef, base.display.setupData.QualityLevel);
	}
}
