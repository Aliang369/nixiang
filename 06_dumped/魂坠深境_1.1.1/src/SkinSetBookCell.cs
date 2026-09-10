using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSetBookCell : CellUI<ItemSkinSetDef>, IDisplayModulesController
{
	private const float GatherTextSpacing = 12f;

	private const float GatherTextHorizontalPadding = 28f;

	public Image icon;

	public Image background;

	public TextMeshProUGUI nameText;

	public TextMeshProUGUI descriptionText;

	public TextMeshProUGUI gatherTitleText;

	public TextMeshProUGUI gatherSourceText;

	public TextMeshProUGUI collectProgressText;

	public RectTransform collectProgressBar;

	[NonSerialized]
	private float progressBarFullWidth;

	[NonSerialized]
	private GameObject newRedDot;

	protected override void Init()
	{
		progressBarFullWidth = collectProgressBar.sizeDelta.x;
		newRedDot = base.transform.Find("BtnTip/NewRedDot")?.gameObject ?? base.transform.Find("NewRedDot")?.gameObject;
		InputModule module = GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase inputModuleBase)
		{
			if (inputModuleBase.pointerClicked)
			{
				CellUI<ItemSkinSetDef>.applicationState.itemSkinManager.ClearUnviewedChangedItemSkinsInSet(base.setupData);
				SetDirty();
				GetComponentInParent<SkinSetBookPanel>()?.SetDirty();
				CellUI<ItemSkinSetDef>.fortressState?.uiManager?.mainUIPanel?.SetDirty();
				SkinSetDisplay.ShowSkinSetPanel(base.setupData, CellUI<ItemSkinSetDef>.fortressState);
			}
		});
	}

	protected override void OnSetup(ItemSkinSetDef _)
	{
		icon.sprite = base.setupData.icon;
		background.sprite = base.setupData.background;
		nameText.text = base.setupData.skinSetName;
		descriptionText.text = base.setupData.description;
		RefreshGatherText();
		List<ItemSkinDef> indexSubSkins = base.setupData.GetIndexSubSkins(CellUI<ItemSkinSetDef>.applicationState.assetCatalog);
		int count = indexSubSkins.Count;
		int num = indexSubSkins.Count((ItemSkinDef s) => CellUI<ItemSkinSetDef>.applicationState.itemSkinManager.IsItemSkinUnlocked(s));
		collectProgressText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(num, count);
		float num2 = ((count > 0) ? ((float)num / (float)count) : 0f);
		collectProgressBar.sizeDelta = new Vector2(progressBarFullWidth * num2, collectProgressBar.sizeDelta.y);
		if (newRedDot != null)
		{
			newRedDot.SetActive(CellUI<ItemSkinSetDef>.applicationState.itemSkinManager.HasUnviewedChangedItemSkinInSet(base.setupData));
		}
	}

	private void RefreshGatherText()
	{
		if (!(base.setupData == null))
		{
			if (gatherTitleText != null)
			{
				gatherTitleText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_get_source_title;
			}
			if (gatherSourceText != null)
			{
				gatherSourceText.text = base.setupData.GetSourceText();
			}
			CenterGatherTexts();
		}
	}

	private void CenterGatherTexts()
	{
		if (!(gatherTitleText == null) && !(gatherSourceText == null))
		{
			RectTransform rectTransform = (RectTransform)gatherTitleText.transform;
			RectTransform rectTransform2 = (RectTransform)gatherSourceText.transform;
			RectTransform rectTransform3 = (RectTransform)base.transform;
			gatherTitleText.ForceMeshUpdate();
			gatherSourceText.ForceMeshUpdate();
			float num = Mathf.Max(0f, rectTransform3.rect.width - 28f);
			float num2 = Mathf.Ceil(gatherTitleText.preferredWidth);
			float num3 = Mathf.Ceil(gatherSourceText.preferredWidth);
			float num4 = ((num3 > 0f) ? 12f : 0f);
			float num5 = num2 + num4 + num3;
			if (num5 > num)
			{
				num3 = Mathf.Max(0f, num - num2 - num4);
				num5 = num2 + num4 + num3;
			}
			Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0.5f, rectTransform.anchorMin.y));
			rectTransform.anchorMin = anchorMin;
			anchorMin = (rectTransform2.anchorMax = new Vector2(0.5f, rectTransform2.anchorMin.y));
			rectTransform2.anchorMin = anchorMin;
			rectTransform.pivot = new Vector2(0f, rectTransform.pivot.y);
			rectTransform2.pivot = new Vector2(0f, rectTransform2.pivot.y);
			float num6 = (0f - num5) * 0.5f;
			rectTransform.anchoredPosition = new Vector2(num6, rectTransform.anchoredPosition.y);
			rectTransform2.anchoredPosition = new Vector2(num6 + num2 + num4, rectTransform2.anchoredPosition.y);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num2);
			rectTransform2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num3);
		}
	}
}
