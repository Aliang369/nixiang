using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatterModule_GlobalItemBoxInfoDisplay : GlobalItemBoxInfoDisplay.DisplayModule
{
	[SerializeField]
	private Button openButton;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private RectTransform descriptionScrollRoot;

	[SerializeField]
	private RectTransform descriptionViewport;

	[SerializeField]
	private LayoutElement descriptionScrollLayout;

	[SerializeField]
	private ScrollRect descriptionScrollRect;

	private const float PopupHardMaxHeight = 1200f;

	private const float EdgeRatio = 0.03f;

	private const float MinEdge = 24f;

	private const float MaxEdge = 40f;

	private const float DescriptionMinHeight = 179f;

	private const float OverflowEpsilon = 4f;

	private bool descriptionScrollInitialized;

	public override void Init()
	{
		openButton.onClick.AddListener(delegate
		{
			if (base.setup is MatterInfo matterInfo)
			{
				if (matterInfo.matterDef.lootSetup is FixedLootSetup)
				{
					UIDisplayModule.fortressState.uiManager.storehousePanel.matterPanel.matterHintPanel.HideAllCommonInfo();
					UIDisplayModule.fortressState.uiManager.randomGlobalRewardBundleConfirmPanel.Setup(matterInfo);
				}
				else if (matterInfo.matterDef.lootSetup is PickLootSetup)
				{
					UIDisplayModule.fortressState.uiManager.storehousePanel.matterPanel.matterHintPanel.HideAllCommonInfo();
					UIDisplayModule.fortressState.uiManager.globalRewardBundleExchangePanel.Setup(matterInfo);
				}
				else if (matterInfo.matterDef.matterType == MatterDef.MatterType.Obsession || matterInfo.matterDef.matterType == MatterDef.MatterType.ObsessionDebris)
				{
					UIDisplayModule.fortressState.uiManager.storehousePanel.matterPanel.matterHintPanel.HideAllCommonInfo();
					UIDisplayModule.fortressState.uiManager.characterInfoBasePanel.ShowUI(matterInfo.GetAttachedPlayerCharacterState(UIDisplayModule.applicationState));
				}
			}
		});
		InitializeDescriptionScroll();
	}

	public override void Setup()
	{
		openButton.transform.parent.gameObject.SetActive(value: false);
		if (base.setup is MatterInfo matterInfo)
		{
			LootSetup lootSetup = matterInfo.matterDef.lootSetup;
			if (lootSetup is FixedLootSetup || lootSetup is PickLootSetup)
			{
				openButton.transform.parent.gameObject.SetActive(value: true);
				text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_open_loot;
			}
			if ((matterInfo.matterDef.matterType == MatterDef.MatterType.Obsession || matterInfo.matterDef.matterType == MatterDef.MatterType.ObsessionDebris) && !matterInfo.GetAttachedPlayerCharacterState(UIDisplayModule.applicationState).locked)
			{
				openButton.transform.parent.gameObject.SetActive(value: true);
				text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_upgrade_obsession_level;
			}
		}
		ApplyDescriptionLayout();
	}

	public override void Unset()
	{
		ResetDescriptionScrollPosition();
	}

	private void InitializeDescriptionScroll()
	{
		if (description == null || descriptionScrollRoot == null || descriptionViewport == null || descriptionScrollLayout == null || descriptionScrollRect == null)
		{
			Debug.LogError("MatterPanel description scroll references are incomplete.", this);
			return;
		}
		if (description.rectTransform.parent != descriptionViewport)
		{
			int siblingIndex = description.rectTransform.GetSiblingIndex();
			description.rectTransform.SetParent(descriptionViewport, worldPositionStays: false);
			descriptionScrollRoot.SetSiblingIndex(siblingIndex);
		}
		ResetDescriptionRectTransform();
		descriptionScrollRect.content = description.rectTransform;
		descriptionScrollRect.viewport = descriptionViewport;
		descriptionScrollRoot.gameObject.SetActive(value: true);
		descriptionScrollInitialized = true;
	}

	private void ApplyDescriptionLayout()
	{
		if (!descriptionScrollInitialized)
		{
			return;
		}
		RectTransform rectTransform = base.display.GetComponentInParent<UniversalCommonInfoCell>(includeInactive: true)?.rectTransform;
		VerticalLayoutGroup component = base.display.GetComponent<VerticalLayoutGroup>();
		if (rectTransform == null || component == null)
		{
			Debug.LogError("MatterPanel description scroll cannot resolve its available area or root layout.", this);
			return;
		}
		ResetDescriptionRectTransform();
		descriptionScrollLayout.minHeight = 179f;
		descriptionScrollLayout.preferredHeight = 179f;
		descriptionScrollLayout.flexibleHeight = 0f;
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.display.rectTransform);
		Canvas.ForceUpdateCanvases();
		float height = rectTransform.rect.height;
		float width = descriptionViewport.rect.width;
		if (height <= 0f || width <= 0f)
		{
			Debug.LogError($"MatterPanel description scroll received an invalid layout size: areaHeight={height}, viewportWidth={width}.", this);
			return;
		}
		float num = description.GetPreferredValues(description.text, width, float.PositiveInfinity).y;
		if (float.IsNaN(num) || float.IsInfinity(num))
		{
			num = 179f;
		}
		num = Mathf.Max(0f, num);
		description.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
		description.ForceMeshUpdate();
		float num2 = Mathf.Clamp(height * 0.03f, 24f, 40f);
		float num3 = Mathf.Max(0f, Mathf.Min(1200f, height - num2 * 2f));
		float num4 = CalculateFixedHeight(component);
		float num5 = Mathf.Max(0f, num3 - num4);
		float num6 = Mathf.Min(179f, num5);
		float num7 = Mathf.Clamp(num, num6, num5);
		descriptionScrollLayout.minHeight = num6;
		descriptionScrollLayout.preferredHeight = num7;
		descriptionScrollRect.vertical = num > num7 + 4f;
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.display.rectTransform);
		Canvas.ForceUpdateCanvases();
		ResetDescriptionScrollPosition();
	}

	private float CalculateFixedHeight(VerticalLayoutGroup rootLayout)
	{
		float num = rootLayout.padding.top + rootLayout.padding.bottom;
		int num2 = 0;
		for (int i = 0; i < base.display.transform.childCount; i++)
		{
			if (!(base.display.transform.GetChild(i) is RectTransform rectTransform) || !rectTransform.gameObject.activeInHierarchy)
			{
				continue;
			}
			LayoutElement component = rectTransform.GetComponent<LayoutElement>();
			if (!(component != null) || !component.ignoreLayout)
			{
				num2++;
				if (rectTransform != descriptionScrollRoot)
				{
					num += LayoutUtility.GetPreferredHeight(rectTransform);
				}
			}
		}
		return num + rootLayout.spacing * (float)Mathf.Max(0, num2 - 1);
	}

	private void ResetDescriptionRectTransform()
	{
		RectTransform rectTransform = description.rectTransform;
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.pivot = new Vector2(0.5f, 1f);
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.sizeDelta = new Vector2(0f, rectTransform.sizeDelta.y);
	}

	private void ResetDescriptionScrollPosition()
	{
		if (descriptionScrollInitialized)
		{
			descriptionScrollRect.StopMovement();
			descriptionScrollRect.verticalNormalizedPosition = 1f;
		}
	}
}
