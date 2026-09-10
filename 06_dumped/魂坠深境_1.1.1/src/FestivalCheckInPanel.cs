using System.Collections;
using Common.UI;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FestivalCheckInPanel : UIBase, IStackPanel
{
	public FestivalCheckInCell festivalCheckInCellPrefab;

	public TextMeshProUGUI festivalTimeText;

	public GameButton closeButton;

	public ScrollRect scrollRect;

	public Image characterImage;

	[SerializeField]
	private RectTransform characterSpineRoot;

	public SkeletonGraphic characterSpinePrefab;

	private CellUIPrefabPool<FestivalCheckInCell, FestivalCheckInDayState> dailyCheckCellPool;

	private SkeletonGraphic characterSpine;

	private string builtFestivalId;

	protected override void Init()
	{
		closeButton.onClick.AddListener(_OnCloseButtonClicked);
		dailyCheckCellPool = GetPrefabPool<FestivalCheckInCell, FestivalCheckInDayState>(festivalCheckInCellPrefab, festivalCheckInCellPrefab.transform.parent);
		RebuildCellsIfNeeded();
		SetupCharacterSpine();
	}

	protected void RebuildCellsIfNeeded()
	{
		WeekCheckInState weekCheckState = UIBase.applicationState.dailyCheckManager.weekCheckState;
		FestivalDef festivalDef = weekCheckState?.festivalDef;
		string text = festivalDef?.FullID;
		if (!(builtFestivalId == text))
		{
			dailyCheckCellPool.DespawnAll();
			if (weekCheckState != null && festivalDef != null)
			{
				dailyCheckCellPool.Spawn(weekCheckState.GenerateAllCheckInDaysInfo());
			}
			builtFestivalId = text;
		}
	}

	protected override void UpdateDisplay()
	{
		RebuildCellsIfNeeded();
		FestivalDef festivalDef = UIBase.applicationState.dailyCheckManager.weekCheckState?.festivalDef;
		if (festivalDef != null)
		{
			festivalTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_festival_active_time_range_arg0_to_arg1.FormatWith(festivalDef.startDate.LocalDateTime.ToLongDateString(), festivalDef.endDate.LocalDateTime.ToLongDateString());
		}
		SetupCharacterSpine();
	}

	private void SetupCharacterSpine()
	{
		if (!((Object)(object)characterSpine != null) && !((Object)(object)characterSpinePrefab == null) && !(characterImage == null) && !(characterSpineRoot == null))
		{
			characterImage.gameObject.SetActive(value: false);
			characterSpine = Object.Instantiate<SkeletonGraphic>(characterSpinePrefab, (Transform)characterSpineRoot);
			((Graphic)(object)characterSpine).raycastTarget = false;
			RectTransform obj = characterSpine.RectTransform<SkeletonGraphic>();
			obj.anchorMin = Vector2.zero;
			obj.anchorMax = Vector2.one;
			obj.anchoredPosition = Vector2.zero;
			obj.sizeDelta = Vector2.zero;
			obj.localScale = Vector3.one;
		}
	}

	public IEnumerator AutoGive(FestivalCheckInResult festivalCheckInResult)
	{
		RebuildCellsIfNeeded();
		FestivalCheckInCell festivalCheckInCell = dailyCheckCellPool.GetActiveElements().Find((FestivalCheckInCell c) => c.info.index == festivalCheckInResult.CurrentCheckInIndex);
		if (!(festivalCheckInCell == null))
		{
			SetActiveAndDirty(active: true);
			Canvas.ForceUpdateCanvases();
			scrollRect.horizontalNormalizedPosition = Mathf.Clamp01((festivalCheckInCell.RectTransform().anchoredPosition.x - scrollRect.RectTransform().rect.width * 0.5f) / scrollRect.RectTransform().rect.width);
			SetDirty();
			yield return festivalCheckInCell.PlayGiveAnim(festivalCheckInResult);
			yield return new WaitUntil(() => !UIBase.applicationState.fortressState.uiManager.gachaResultDisplay.gameObject.activeInHierarchy && !UIBase.applicationState.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy);
			yield return new WaitUntil(() => !base.gameObject.activeInHierarchy);
			UIBase.fortressState.uiManager.mainUIPanel.SetDirty();
		}
	}

	private void _OnCloseButtonClicked()
	{
		UIBase.applicationState.dailyCheckManager.TryAdvanceFestivalChain();
		UIBase.fortressState.uiManager.mainUIPanel.SetDirty();
		SetActiveAndDirtyWithFadeAnim(active: false);
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
	}
}
