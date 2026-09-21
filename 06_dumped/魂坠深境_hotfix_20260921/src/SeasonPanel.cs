using System.Collections;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class SeasonPanel : CellUI<Season>, IShowAsync
{
	[SerializeField]
	private LevelLineDisplay seasonLevelLineDisplay;

	[SerializeField]
	private Image lockMark;

	private Season currentSeason => base.setupData;

	protected override void Init()
	{
		InitUI(seasonLevelLineDisplay);
	}

	protected override void OnSetup(Season setupData)
	{
		seasonLevelLineDisplay.Setup(currentSeason.seasonLeveLine);
	}

	protected override void UpdateDisplay()
	{
		lockMark.gameObject.SetActive(!currentSeason.seasonPassUnlocked);
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		base.gameObject.SetActive(value: true);
		SetDirty();
		seasonLevelLineDisplay.SetScrollCenter();
		yield return null;
		seasonLevelLineDisplay.UpdateNextBigPreviewCell();
		SetDirty();
	}
}
