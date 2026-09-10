using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelLineDisplay : UIBase, IShowAsync
{
	public ScrollRect scrollRect;

	public LineLevelCell lineLevelCellPrefab;

	public RectTransform lineLevelCellParent;

	public Slider currentLevelExpSlider;

	public GlobalItemDisplay dumpRewardDisplay;

	public GameObject expDumpArea;

	[FormerlySerializedAs("expDumpIcon")]
	public Image expDumpProgressIcon;

	public TextMeshProUGUI expDumpText;

	[NonSerialized]
	public CellUIPrefabPool<LineLevelCell, (LevelLine line, LevelLineDef.LineLevel level)> lineLevelCellPool;

	public LevelLine line;

	public LineLevelCell nextBigRewardPreviewCell;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	protected override void Init()
	{
		InitUI(dumpRewardDisplay);
		if (nextBigRewardPreviewCell != null)
		{
			InitUI(nextBigRewardPreviewCell);
		}
		lineLevelCellPool = GetPrefabPool<LineLevelCell, (LevelLine, LevelLineDef.LineLevel)>(lineLevelCellPrefab, lineLevelCellParent);
		scrollRect.onValueChanged.AddListener(_OnScrollValueChanged);
	}

	public void Setup(LevelLine line)
	{
		this.line = line;
		lineLevelCellPool.UpdateSequenceDisplay(line.levelLineDef.levels.Select((LevelLineDef.LineLevel level) => (line: line, level: level)).ToList(), layoutGroup);
		SetDirty();
		if (nextBigRewardPreviewCell != null && manager.gameObject.activeInHierarchy)
		{
			manager.StartCoroutine(DelayUpdateNextBigPreviewCell());
		}
		IEnumerator DelayUpdateNextBigPreviewCell()
		{
			yield return null;
			UpdateNextBigPreviewCell();
		}
	}

	protected override void UpdateDisplay()
	{
		if (line.next != null)
		{
			dumpRewardDisplay.gameObject.SetActive(value: true);
			dumpRewardDisplay.Setup(line.next);
		}
		else
		{
			dumpRewardDisplay.gameObject.SetActive(value: false);
		}
		if (line.levelLineDef.expDump != null && line.levelLineDef.levelLineAutoGiveReward)
		{
			expDumpArea.SetActive(value: true);
			if (line.IsMaxLevel)
			{
				expDumpProgressIcon.fillAmount = (float)line.currentExp / (float)line.levelLineDef.expDump.expCost;
				currentLevelExpSlider.value = expDumpProgressIcon.fillAmount;
				expDumpText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(line.currentExp, line.levelLineDef.expDump.expCost);
			}
			else
			{
				currentLevelExpSlider.value = 0f;
				expDumpProgressIcon.fillAmount = 0f;
				expDumpText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(0, line.levelLineDef.expDump.expCost);
			}
		}
		else
		{
			expDumpArea.SetActive(value: false);
		}
		expDumpArea.transform.SetSiblingIndex(0);
		if (nextBigRewardPreviewCell != null)
		{
			UpdateNextBigPreviewCell();
		}
	}

	private void _OnScrollValueChanged(Vector2 pos)
	{
		if (nextBigRewardPreviewCell != null)
		{
			UpdateNextBigPreviewCell();
		}
	}

	public void SetScrollCenter()
	{
		LineLevelCell lineLevelCell = lineLevelCellPool.GetActiveElements().Find((LineLevelCell l) => l.level == line.CurrentLevel);
		if (lineLevelCell == null || scrollRect == null)
		{
			return;
		}
		RectTransform component = lineLevelCell.GetComponent<RectTransform>();
		RectTransform content = scrollRect.content;
		Vector3 position = component.TransformPoint(component.rect.center);
		Vector3 vector = content.InverseTransformPoint(position);
		float width = content.rect.width;
		float width2 = scrollRect.viewport.rect.width;
		float num = vector.x - component.rect.width * 0.5f;
		float num2 = width - (vector.x + component.rect.width * 0.5f);
		float horizontalNormalizedPosition = 0f;
		if (scrollRect.horizontal)
		{
			float num3 = Mathf.Max(1f, width - width2);
			float num4 = Mathf.Clamp((vector.x - width2 * 0.5f) / num3, 0f, 1f);
			if (num < 0f)
			{
				num4 = 0f;
			}
			else if (num2 < 0f)
			{
				num4 = 1f;
			}
			horizontalNormalizedPosition = num4;
		}
		scrollRect.horizontalNormalizedPosition = horizontalNormalizedPosition;
	}

	public void UpdateNextBigPreviewCell()
	{
		IEnumerable<LineLevelCell> source = from l in lineLevelCellPool.GetActiveElements()
			where RectTransformUtility.RectangleContainsScreenPoint(base.rectTransform, (Vector2)l.transform.position)
			select l;
		int maxInViewLevelIndex = source.Select((LineLevelCell c) => line.GetLineLevelIndex(c.level)).DefaultIfEmpty(0).Max();
		LevelLineDef.LineLevel lineLevel = line.levelLineDef.levels.FirstOrDefault((LevelLineDef.LineLevel l) => l.baseRewardType == LevelLineDef.LineLevel.LevelRewardType.Big && line.GetLineLevelIndex(l) > maxInViewLevelIndex && !line.baseRewardGotLevels.Contains(line.GetLineLevelIndex(l)));
		if (lineLevel == null)
		{
			nextBigRewardPreviewCell.SetActiveAndDirty(active: false);
			return;
		}
		nextBigRewardPreviewCell.Setup((line, lineLevel));
		nextBigRewardPreviewCell.SetActiveAndDirty(active: true);
	}
}
