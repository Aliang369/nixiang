using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MultyLevelUpDisplay : CellUI<GameRecord>
{
	[SerializeField]
	private GameObject difficultyTrialRoot;

	[SerializeField]
	private GameObject seasonTrialRoot;

	[SerializeField]
	private ScrollRect difficultyScrollRect;

	[SerializeField]
	private ScrollRect seasonScrollRect;

	[SerializeField]
	private CellUIPrefabPoolConfig<DifficultyProgressCell, DifficultyProgressCell.Data> difficultyPool;

	[SerializeField]
	private CellUIPrefabPoolConfig<SeasonProgressCell, SeasonProgressCell.Data> seasonPool;

	[SerializeField]
	private float difficultyScrollAnimDuration = 0.35f;

	private readonly List<DifficultyProgressCell> difficultyCells = new List<DifficultyProgressCell>();

	private readonly List<SeasonProgressCell> seasonCells = new List<SeasonProgressCell>();

	private Sequence difficultyScrollSequence;

	protected override void Init()
	{
		Init(difficultyPool);
		Init(seasonPool);
		base.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(GameRecord setupData)
	{
		base.gameObject.SetActive(value: true);
		BuildDifficultyCells();
		BuildSeasonCells();
	}

	protected override void OnUnset()
	{
		difficultyPool?.DespawnAll();
		seasonPool?.DespawnAll();
		difficultyCells.Clear();
		seasonCells.Clear();
		difficultyScrollSequence?.Kill();
		difficultyScrollSequence = null;
		base.gameObject.SetActive(value: false);
	}

	public IEnumerator DisplayCor(GameRecord record)
	{
		if (!HasAnyProgress(record))
		{
			yield break;
		}
		Setup(record);
		yield return null;
		foreach (DifficultyProgressCell cell in difficultyCells)
		{
			yield return SmoothScrollToCell(difficultyScrollRect, cell.GetComponent<RectTransform>(), difficultyScrollAnimDuration);
			yield return cell.UpgradeAnim();
			yield return new WaitForSeconds(0.5f);
		}
		foreach (SeasonProgressCell seasonCell in seasonCells)
		{
			yield return seasonCell.UpgradeAnim(seasonCell.setupData.toCompleted && !seasonCell.setupData.fromCompleted);
		}
		yield return new WaitForSeconds(0.5f);
		Unset();
	}

	public bool HasAnyProgress(GameRecord record)
	{
		if (!HasDifficultyProgress(record))
		{
			return HasSeasonProgress(record);
		}
		return true;
	}

	private bool HasDifficultyInfo(GameRecord record)
	{
		return record?.difficultyTrialProgressInfo != null;
	}

	private bool HasDifficultyProgress(GameRecord record)
	{
		GameRecord.DifficultyTrialProgressInfo difficultyTrialProgressInfo = record?.difficultyTrialProgressInfo;
		if (difficultyTrialProgressInfo != null)
		{
			return difficultyTrialProgressInfo.newDifficultyLevel > difficultyTrialProgressInfo.oldDifficultyLevel;
		}
		return false;
	}

	private bool HasSeasonInfo(GameRecord record)
	{
		return record?.soulExpInfo != null;
	}

	private bool HasSeasonProgress(GameRecord record)
	{
		IdolManager.SoulExpInfo soulExpInfo = record?.soulExpInfo;
		if (soulExpInfo != null)
		{
			if (soulExpInfo.old == soulExpInfo.newExp)
			{
				return soulExpInfo.oldSeasonSoul != soulExpInfo.newSeasonSoul;
			}
			return true;
		}
		return false;
	}

	private void BuildDifficultyCells()
	{
		difficultyPool.DespawnAll();
		difficultyCells.Clear();
		GameRecord.DifficultyTrialProgressInfo difficultyTrialProgressInfo = base.setupData?.difficultyTrialProgressInfo;
		bool flag = HasDifficultyInfo(base.setupData);
		difficultyTrialRoot?.SetActive(flag);
		if (!flag)
		{
			return;
		}
		List<DifficultyLevel> rewardableDifficultyLevels = ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.GetRewardableDifficultyLevels(CellUI<GameRecord>.applicationState.assetCatalog);
		for (int i = 0; i < rewardableDifficultyLevels.Count; i++)
		{
			DifficultyLevel difficultyLevel = rewardableDifficultyLevels[i];
			DifficultyProgressCell item = difficultyPool.Spawn(new DifficultyProgressCell.Data
			{
				index = i,
				isObtained = (difficultyLevel <= difficultyTrialProgressInfo.oldDifficultyLevel),
				difficultyLevel = difficultyLevel,
				effectPivot = base.transform
			});
			if (difficultyLevel > difficultyTrialProgressInfo.oldDifficultyLevel && difficultyLevel <= difficultyTrialProgressInfo.newDifficultyLevel)
			{
				difficultyCells.Add(item);
			}
		}
	}

	private void BuildSeasonCells()
	{
		seasonPool.DespawnAll();
		seasonCells.Clear();
		IdolManager.SoulExpInfo soulExpInfo = base.setupData?.soulExpInfo;
		bool flag = HasSeasonInfo(base.setupData);
		seasonTrialRoot?.SetActive(flag);
		if (!flag)
		{
			return;
		}
		int seasonExperienceLimit = ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.seasonExperienceLimit;
		int experienceToSoulSacrificeRatio = ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.experienceToSoulSacrificeRatio;
		int num = Mathf.Clamp(soulExpInfo.oldSeasonSoul, 0, seasonExperienceLimit);
		int num2 = Mathf.Clamp(soulExpInfo.newSeasonSoul, 0, seasonExperienceLimit);
		for (int i = 0; i < seasonExperienceLimit; i++)
		{
			SeasonProgressCell.Data data = new SeasonProgressCell.Data
			{
				index = i,
				completed = (i < num),
				current = (i == num && num < seasonExperienceLimit),
				currentExp = ((i == num) ? soulExpInfo.old : 0),
				maxExp = experienceToSoulSacrificeRatio,
				animate = (i >= num && i <= num2 && i < seasonExperienceLimit),
				fromCompleted = (i < num),
				toCompleted = (i < num2),
				toCurrent = (i == num2 && num2 < seasonExperienceLimit),
				fromExp = ((i == num) ? soulExpInfo.old : 0),
				toExp = ResolveSeasonCellToExp(i, num2, soulExpInfo.newExp, experienceToSoulSacrificeRatio),
				effectPivot = base.transform,
				onProgressPointChanged = delegate(Vector3 point)
				{
					ScrollToWorldPoint(seasonScrollRect, point);
				}
			};
			SeasonProgressCell item = seasonPool.Spawn(data);
			if (data.animate)
			{
				seasonCells.Add(item);
			}
		}
	}

	private int ResolveSeasonCellToExp(int index, int newSeasonSoul, int newExp, int maxExp)
	{
		if (index < newSeasonSoul)
		{
			return maxExp;
		}
		if (index == newSeasonSoul)
		{
			return newExp;
		}
		return 0;
	}

	private void ScrollToCell(ScrollRect scrollRect, RectTransform target)
	{
		if (!(scrollRect == null) && !(scrollRect.content == null) && !(target == null))
		{
			Canvas.ForceUpdateCanvases();
			if (!(((scrollRect.viewport != null) ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>()) == null))
			{
				Vector3 worldPoint = target.TransformPoint(target.rect.center);
				ScrollToWorldPoint(scrollRect, worldPoint);
			}
		}
	}

	private IEnumerator SmoothScrollToCell(ScrollRect scrollRect, RectTransform target, float duration)
	{
		if (!TryGetCellCenterNormalizedPosition(scrollRect, target, out var normalizedPosition))
		{
			yield break;
		}
		difficultyScrollSequence?.Kill();
		difficultyScrollSequence = DOTween.Sequence();
		if (scrollRect.horizontal)
		{
			difficultyScrollSequence.Join(DOTween.To(() => scrollRect.horizontalNormalizedPosition, delegate(float value)
			{
				scrollRect.horizontalNormalizedPosition = value;
			}, normalizedPosition.x, duration));
		}
		if (scrollRect.vertical)
		{
			difficultyScrollSequence.Join(DOTween.To(() => scrollRect.verticalNormalizedPosition, delegate(float value)
			{
				scrollRect.verticalNormalizedPosition = value;
			}, normalizedPosition.y, duration));
		}
		difficultyScrollSequence.Play();
		yield return difficultyScrollSequence.WaitForCompletion();
		difficultyScrollSequence = null;
	}

	private bool TryGetCellCenterNormalizedPosition(ScrollRect scrollRect, RectTransform target, out Vector2 normalizedPosition)
	{
		normalizedPosition = Vector2.zero;
		if (scrollRect == null || scrollRect.content == null || target == null)
		{
			return false;
		}
		Canvas.ForceUpdateCanvases();
		RectTransform rectTransform = ((scrollRect.viewport != null) ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>());
		if (rectTransform == null)
		{
			return false;
		}
		Vector3 position = target.TransformPoint(target.rect.center);
		Vector3 vector = scrollRect.content.InverseTransformPoint(position);
		normalizedPosition.x = scrollRect.horizontalNormalizedPosition;
		normalizedPosition.y = scrollRect.verticalNormalizedPosition;
		if (scrollRect.horizontal)
		{
			float num = scrollRect.content.rect.width - rectTransform.rect.width;
			normalizedPosition.x = ((num <= 0f) ? scrollRect.horizontalNormalizedPosition : Mathf.Clamp01((vector.x - rectTransform.rect.width * 0.5f) / num));
		}
		if (scrollRect.vertical)
		{
			float num2 = scrollRect.content.rect.height - rectTransform.rect.height;
			normalizedPosition.y = ((num2 <= 0f) ? scrollRect.verticalNormalizedPosition : Mathf.Clamp01(1f - (vector.y + rectTransform.rect.height * 0.5f) / num2));
		}
		return true;
	}

	private void ScrollToWorldPoint(ScrollRect scrollRect, Vector3 worldPoint)
	{
		if (scrollRect == null || scrollRect.content == null)
		{
			return;
		}
		Canvas.ForceUpdateCanvases();
		RectTransform rectTransform = ((scrollRect.viewport != null) ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>());
		if (!(rectTransform == null))
		{
			Vector3 vector = rectTransform.InverseTransformPoint(worldPoint);
			Vector2 center = rectTransform.rect.center;
			if (scrollRect.horizontal)
			{
				float num = scrollRect.content.rect.width - rectTransform.rect.width;
				scrollRect.horizontalNormalizedPosition = ((num <= 0f) ? scrollRect.horizontalNormalizedPosition : Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + (vector.x - center.x) / num));
			}
			if (scrollRect.vertical)
			{
				float num2 = scrollRect.content.rect.height - rectTransform.rect.height;
				scrollRect.verticalNormalizedPosition = ((num2 <= 0f) ? scrollRect.verticalNormalizedPosition : Mathf.Clamp01(scrollRect.verticalNormalizedPosition - (vector.y - center.y) / num2));
			}
		}
	}
}
