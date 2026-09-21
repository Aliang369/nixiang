using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineLevelCell : CellUI<(LevelLine line, LevelLineDef.LineLevel level)>
{
	public class Config
	{
		public Color completeColor;

		public Color normalColor;
	}

	public Slider currentLevelExpSlider;

	public Image currentLevelIndexImage;

	public Image currentLevelIndexImage2;

	public TextMeshProUGUI currentLevelIndexText;

	public TextMeshProUGUI currentLevelIndexText2;

	public TextMeshProUGUI sumExpText;

	public LineLevelGlobalRewardDisplay majorRewardDisplay;

	public LineLevelGlobalRewardDisplay appendRewardDisplay;

	public bool manualUpdateDisplay;

	public LevelLine line;

	public LevelLineDef.LineLevel level;

	public Image expImage;

	public TextMeshProUGUI expText;

	public Image maxExpImage;

	public TextMeshProUGUI maxExpText;

	public bool hideMaxExpText;

	public static Config config => ScriptableObjectSingleton<VFXDataPreset>.instance.lineLevelCellConfig;

	public Image indexImage
	{
		get
		{
			if (!WithAnyBiyReward)
			{
				return currentLevelIndexImage;
			}
			return currentLevelIndexImage2;
		}
	}

	public TextMeshProUGUI indexText
	{
		get
		{
			if (level.baseRewardType != LevelLineDef.LineLevel.LevelRewardType.Big && level.appendRewardType != LevelLineDef.LineLevel.LevelRewardType.Big)
			{
				return currentLevelIndexText;
			}
			return currentLevelIndexText2;
		}
	}

	private bool WithAnyBiyReward
	{
		get
		{
			if (level.baseRewardType != LevelLineDef.LineLevel.LevelRewardType.Big)
			{
				return level.appendRewardType == LevelLineDef.LineLevel.LevelRewardType.Big;
			}
			return true;
		}
	}

	protected override void Init()
	{
		InitUI(majorRewardDisplay);
		InitUI(appendRewardDisplay);
	}

	protected override void OnSetup((LevelLine line, LevelLineDef.LineLevel level) t)
	{
		(line, level) = t;
		currentLevelIndexImage.gameObject.SetActive(!WithAnyBiyReward);
		currentLevelIndexImage2.gameObject.SetActive(WithAnyBiyReward);
		expImage.gameObject.SetActive(value: false);
		maxExpImage.gameObject.SetActive(value: false);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (!manualUpdateDisplay)
		{
			UpdateLineLevelDisplay();
		}
	}

	public void UpdateLineLevelDisplay()
	{
		expImage.gameObject.SetActive(value: false);
		maxExpImage.gameObject.SetActive(value: false);
		int lineLevelIndex = line.GetLineLevelIndex(level);
		if (lineLevelIndex > line.currentLevelIndex)
		{
			currentLevelExpSlider.value = 0f;
			indexImage.color = config.normalColor;
			indexText.color = config.normalColor;
		}
		else if (lineLevelIndex < line.currentLevelIndex)
		{
			currentLevelExpSlider.value = 1f;
			indexImage.color = config.completeColor;
			indexText.color = config.completeColor;
		}
		else
		{
			expImage.gameObject.SetActive(value: true);
			if (!hideMaxExpText)
			{
				maxExpImage.gameObject.SetActive(value: true);
			}
			expText.text = $"{line.currentExp}";
			maxExpText.text = $"{line.CurrentLevelExpCost}";
			currentLevelExpSlider.value = (float)line.currentExp / (float)line.CurrentLevelExpCost;
			indexImage.color = config.normalColor;
			indexText.color = config.normalColor;
		}
		indexText.text = (lineLevelIndex + 1).ToString();
		sumExpText.text = level.GetSumExpToThisLevel(line.levelLineDef).ToString();
		if (level.reward != null)
		{
			majorRewardDisplay.SetActiveAndDirty(active: true);
			majorRewardDisplay.Setup((level.reward, line, level));
		}
		else
		{
			majorRewardDisplay.SetActiveAndDirty(active: false);
		}
		if (level.appendReward != null)
		{
			appendRewardDisplay.SetActiveAndDirty(active: true);
			appendRewardDisplay.Setup((level.appendReward, line, level));
		}
		else
		{
			appendRewardDisplay.SetActiveAndDirty(active: false);
		}
	}
}
