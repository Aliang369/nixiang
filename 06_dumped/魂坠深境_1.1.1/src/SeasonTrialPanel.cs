using System.Collections;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonTrialPanel : CellUI<IdolState>
{
	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private GameButton toGameButton;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private CellUIPrefabPoolConfig<SeasonProgressCell, SeasonProgressCell.Data> pool;

	private Coroutine scrollToCurrentCellCoroutine;

	private IdolState idolState => base.setupData;

	private int maxProgress => ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.seasonExperienceLimit;

	private int maxSoulOffering => ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.experienceToSoulSacrificeRatio;

	private int currentExp => base.setupData.seasonExperience;

	private int currentProgress => base.setupData.seasonSoul;

	protected override void Init()
	{
		Init(pool);
		toGameButton?.onClick.AddListener(OnToGameButtonClick);
	}

	protected override void OnSetup(IdolState setupData)
	{
		base.gameObject.SetActive(value: true);
		UpdateProgressCells();
		UpdateTextDisplay();
		SetDirty();
	}

	private void UpdateProgressCells()
	{
		if (pool?.pool != null)
		{
			pool.DespawnAll();
			for (int i = 0; i < maxProgress; i++)
			{
				pool.Spawn(new SeasonProgressCell.Data
				{
					index = i,
					completed = (i < currentProgress),
					current = (i == currentProgress && currentProgress < maxProgress),
					currentExp = currentExp,
					maxExp = maxSoulOffering
				});
			}
			ScheduleScrollToCurrentCell();
		}
	}

	private void ScheduleScrollToCurrentCell()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (scrollToCurrentCellCoroutine != null)
			{
				StopCoroutine(scrollToCurrentCellCoroutine);
			}
			scrollToCurrentCellCoroutine = StartCoroutine(ScrollToCurrentCell());
		}
	}

	private IEnumerator ScrollToCurrentCell()
	{
		yield return null;
		scrollToCurrentCellCoroutine = null;
		if (!(scrollRect == null) && !(scrollRect.content == null) && pool?.pool != null && pool.pool.activeCount > 0)
		{
			Canvas.ForceUpdateCanvases();
			int index = Mathf.Clamp(currentProgress, 0, pool.pool.activeCount - 1);
			SeasonProgressCell activeElement = pool.pool.GetActiveElement(index);
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

	private void UpdateTextDisplay()
	{
		string arg = TextDisplayHelper.SoulText(idolState.idolDef.attachClass);
		progressText.text = $"{arg} {currentProgress}/{maxProgress}";
	}

	private void OnToGameButtonClick()
	{
		CellUI<IdolState>.fortressState.uiManager.idolBasePanel.HideUI();
		CellUI<IdolState>.fortressState.uiManager.baseGameUIPanel.ShowUI();
	}
}
