using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyTrialPanel : CellUI<IdolState>
{
	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private CellUIPrefabPoolConfig<DifficultyProgressCell, DifficultyProgressCell.Data> pool;

	private Coroutine scrollToLastObtainedCellCoroutine;

	private IdolState idolState => base.setupData;

	private List<DifficultyLevel> rewardableDifficultyLevels => ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.GetRewardableDifficultyLevels(CellUI<IdolState>.applicationState.assetCatalog);

	private int maxProgress => rewardableDifficultyLevels.Count;

	private int currentProgress => rewardableDifficultyLevels.Count((DifficultyLevel d) => d <= idolState.currentDifficultyLevel);

	protected override void Init()
	{
		Init(pool);
	}

	protected override void UpdateDisplay()
	{
		UpdateProgressDisplay();
	}

	protected override void OnSetup(IdolState setupData)
	{
		base.gameObject.SetActive(value: true);
		SetDirty();
	}

	private void UpdateProgressDisplay()
	{
		if (pool?.pool != null)
		{
			pool.DespawnAll();
			List<DifficultyLevel> list = rewardableDifficultyLevels;
			for (int i = 0; i < list.Count; i++)
			{
				pool.Spawn(new DifficultyProgressCell.Data
				{
					index = i,
					isObtained = (list[i] <= idolState.currentDifficultyLevel),
					difficultyLevel = list[i]
				});
			}
			ScheduleScrollToLastObtainedCell();
		}
		progressText.text = $"{TextDisplayHelper.SoulText(idolState.GetCharacterClassDef().mClass)} {CurrentSoulCount()}/{MaxSoulCount()}";
	}

	private void ScheduleScrollToLastObtainedCell()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (scrollToLastObtainedCellCoroutine != null)
			{
				StopCoroutine(scrollToLastObtainedCellCoroutine);
			}
			scrollToLastObtainedCellCoroutine = StartCoroutine(ScrollToLastObtainedCell());
		}
	}

	private IEnumerator ScrollToLastObtainedCell()
	{
		yield return null;
		scrollToLastObtainedCellCoroutine = null;
		if (scrollRect == null || scrollRect.content == null || pool?.pool == null || pool.pool.activeCount <= 0)
		{
			yield break;
		}
		int num = currentProgress - 1;
		if (num >= 0)
		{
			Canvas.ForceUpdateCanvases();
			num = Mathf.Clamp(num, 0, pool.pool.activeCount - 1);
			DifficultyProgressCell activeElement = pool.pool.GetActiveElement(num);
			if (!(activeElement == null))
			{
				ScrollToCellCenter(activeElement.GetComponent<RectTransform>());
			}
		}
	}

	private void ScrollToCellCenter(RectTransform target)
	{
		RectTransform content = scrollRect.content;
		RectTransform rectTransform = ((scrollRect.viewport != null) ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>());
		if (!(target == null) && !(content == null) && !(rectTransform == null))
		{
			Vector3 position = target.TransformPoint(target.rect.center);
			Vector3 vector = content.InverseTransformPoint(position);
			if (scrollRect.horizontal)
			{
				float num = content.rect.width - rectTransform.rect.width;
				scrollRect.horizontalNormalizedPosition = ((num <= 0f) ? 0f : Mathf.Clamp01((vector.x - rectTransform.rect.width * 0.5f) / num));
			}
			if (scrollRect.vertical)
			{
				float num2 = content.rect.height - rectTransform.rect.height;
				scrollRect.verticalNormalizedPosition = ((num2 <= 0f) ? 1f : Mathf.Clamp01(1f - (vector.y + rectTransform.rect.height * 0.5f) / num2));
			}
		}
	}

	private string GetDescriptionText()
	{
		DifficultyLevel? difficultyLevel = NextRewardableDifficulty();
		if (!difficultyLevel.HasValue)
		{
			return "";
		}
		DifficultyInfo difficultyInfo = CellUI<IdolState>.applicationState.difficultyManager.GetDifficultyInfo(difficultyLevel.Value);
		string text = ((difficultyInfo != null) ? difficultyInfo.difficultyName : "");
		string text2 = "<color=#613311><b>「" + text + "」</b></color>";
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_breakthrough_difficulty_arg0_get_soul_arg1.FormatWith(text2, TextDisplayHelper.SoulText(idolState.characterClassDef.mClass));
	}

	private int CurrentSoulCount()
	{
		return ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.GetDifficultySoulRewardCount(idolState.currentDifficultyLevel, CellUI<IdolState>.applicationState.assetCatalog);
	}

	private int MaxSoulCount()
	{
		return ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.GetRewardableDifficultyLevels(CellUI<IdolState>.applicationState.assetCatalog).Count;
	}

	private DifficultyLevel? NextRewardableDifficulty()
	{
		List<DifficultyLevel> list = (from d in ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.GetRewardableDifficultyLevels(CellUI<IdolState>.applicationState.assetCatalog)
			orderby d
			select d).ToList();
		if (list.Count <= 0)
		{
			return null;
		}
		DifficultyLevel difficultyLevel = list[0];
		if (idolState == null || idolState.currentDifficultyLevel < difficultyLevel)
		{
			return difficultyLevel;
		}
		foreach (DifficultyLevel item in list)
		{
			if (item > idolState.currentDifficultyLevel)
			{
				return item;
			}
		}
		return null;
	}
}
