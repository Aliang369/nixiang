using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class LevelLineRewardAnimPanel : UIBase
{
	private const float progressSpeed = 1.35f;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI remainExpText;

	public LevelLineDisplay levelLineDisplay;

	public RectTransform levelLineContentRect;

	public RectTransform focusCenterPivot;

	public GameButton clickToCloseButton;

	public CanvasGroup panelCanvasGroup;

	private GameOverRewardPanel gameOverRewardPanel;

	private Vector3 focusCenterPivotTolevelLineContentRectDelta;

	private int remainExp;

	protected override void Init()
	{
		InitUI(levelLineDisplay);
		clickToCloseButton.onClick.AddListener(_OnCloseButtonClicked);
		gameOverRewardPanel = GetUIInParents<GameOverRewardPanel>();
		focusCenterPivotTolevelLineContentRectDelta = levelLineContentRect.position - focusCenterPivot.position;
	}

	private void SetRemainExpAndText(int exp)
	{
		remainExp = exp;
		remainExpText.text = remainExp.ToString();
	}

	private IEnumerator PlayDumpExpAnim(LevelLine beforeLine, LevelLine.ChangeExpResult result)
	{
		int num = (beforeLine.IsMaxLevel ? beforeLine.currentExp : 0);
		int dumExpCost = beforeLine.levelLineDef.expDump.expCost;
		float initProgress = (float)num / (float)dumExpCost;
		float currentProgress = initProgress;
		foreach (GlobalRewardInfo dumpReward in result.dumpRewards)
		{
			MoveContentRectCenterToDumpReward();
			int initRemainExp = remainExp;
			int targetRemainExp = remainExp - (dumExpCost - num);
			levelLineDisplay.dumpRewardDisplay.Setup(dumpReward);
			do
			{
				SetRemainExpAndText((int)Mathf.Lerp(initRemainExp, targetRemainExp, (currentProgress - initProgress) / (1f - initProgress)));
				currentProgress += 1.35f * Time.deltaTime;
				levelLineDisplay.expDumpText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith((int)(currentProgress * (float)dumExpCost), dumExpCost);
				levelLineDisplay.expDumpProgressIcon.fillAmount = currentProgress;
				yield return new WaitForEndOfFrame();
			}
			while (!(currentProgress >= 1f));
			num = 0;
			currentProgress = 0f;
			SetRemainExpAndText(targetRemainExp);
			levelLineDisplay.expDumpProgressIcon.fillAmount = currentProgress;
		}
		if (result.dumpRewards.Count > 0 || beforeLine.IsMaxLevel)
		{
			int targetRemainExp = remainExp;
			int initRemainExp = 0;
			do
			{
				SetRemainExpAndText((int)Mathf.Lerp(targetRemainExp, initRemainExp, (currentProgress - initProgress) / (1f - initProgress)));
				currentProgress += 1.35f * Time.deltaTime;
				levelLineDisplay.expDumpProgressIcon.fillAmount = currentProgress;
				yield return new WaitForEndOfFrame();
			}
			while (!(currentProgress >= (float)result.finalExp / (float)dumExpCost));
			SetRemainExpAndText(initRemainExp);
		}
	}

	private LineLevelCell GetLevelCell(LevelLineDef.LineLevel level)
	{
		return levelLineDisplay.lineLevelCellPool.GetActiveElements().First((LineLevelCell c) => c.level == level);
	}

	public void MoveContentRectCenterToExpHandler(LineLevelCell cell)
	{
		MoveContentRectTo(cell.currentLevelExpSlider.handleRect);
	}

	public void MoveContentRectCenterToDumpReward()
	{
		MoveContentRectTo(levelLineDisplay.expDumpProgressIcon.rectTransform);
	}

	private void MoveContentRectTo(RectTransform target)
	{
		Vector3 vector = target.position - levelLineContentRect.position + focusCenterPivotTolevelLineContentRectDelta;
		vector.x = ((vector.x > 0f) ? vector.x : 0f);
		levelLineContentRect.anchoredPosition = new Vector3(0f - vector.x, levelLineContentRect.anchoredPosition.y);
	}

	private void _OnCloseButtonClicked()
	{
		clickToCloseButton.gameObject.SetActive(value: false);
	}
}
