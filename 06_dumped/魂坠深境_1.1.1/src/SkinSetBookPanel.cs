using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkinSetBookPanel : UIBase
{
	private const int PageSize = 2;

	[SerializeField]
	private SkinSetBookCell leftCell;

	[SerializeField]
	private SkinSetBookCell rightCell;

	[SerializeField]
	private GameButton prevButton;

	[SerializeField]
	private GameButton nextButton;

	[SerializeField]
	private TextMeshProUGUI pageText;

	[SerializeField]
	private GameObject comingSoonPage;

	[SerializeField]
	private TextMeshProUGUI comingSoonText;

	[NonSerialized]
	private int currentPage;

	[NonSerialized]
	private List<ItemSkinSetDef> sortedDefs = new List<ItemSkinSetDef>();

	[NonSerialized]
	private GameObject prevButtonRedDot;

	[NonSerialized]
	private GameObject nextButtonRedDot;

	[NonSerialized]
	private bool autoJumpedToUnviewedPage;

	private int PageCount => Mathf.CeilToInt((float)sortedDefs.Count / 2f);

	protected override void Init()
	{
		InitUI(leftCell);
		InitUI(rightCell);
		prevButtonRedDot = prevButton.transform.Find("NewRedDot")?.gameObject;
		nextButtonRedDot = nextButton.transform.Find("NewRedDot")?.gameObject;
		prevButton.onClick.AddListener(_OnPrevButtonClicked);
		nextButton.onClick.AddListener(_OnNextButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(SetDirty);
		SetDirty();
	}

	protected override void Dispose()
	{
		prevButton.onClick.RemoveListener(_OnPrevButtonClicked);
		nextButton.onClick.RemoveListener(_OnNextButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(SetDirty);
	}

	public void JumpToFirstUnviewedPageOnNextDisplay()
	{
		autoJumpedToUnviewedPage = false;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		sortedDefs = UIBase.fortressState.applicationState.assetCatalog.skinSetCatalog.ValidAssets;
		sortedDefs.Sort((ItemSkinSetDef a, ItemSkinSetDef b) => a.SOindex.CompareTo(b.SOindex));
		int pageCount = PageCount;
		if (!autoJumpedToUnviewedPage)
		{
			autoJumpedToUnviewedPage = true;
			int firstUnviewedPageIndex = GetFirstUnviewedPageIndex();
			if (firstUnviewedPageIndex >= 0)
			{
				currentPage = firstUnviewedPageIndex;
			}
		}
		currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, pageCount - 1));
		SetupCell(leftCell, currentPage * 2);
		SetupCell(rightCell, currentPage * 2 + 1);
		pageText.text = $"{currentPage + 1}/{Mathf.Max(1, pageCount)}";
		prevButton.gameObject.SetActive(currentPage > 0);
		nextButton.gameObject.SetActive(currentPage < pageCount - 1);
		if (prevButtonRedDot != null)
		{
			prevButtonRedDot.SetActive(HasUnviewedChangedSkinBeforePage(currentPage));
		}
		if (nextButtonRedDot != null)
		{
			nextButtonRedDot.SetActive(HasUnviewedChangedSkinAfterPage(currentPage));
		}
		comingSoonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_coming_soon;
		comingSoonPage.SetActive(value: false);
	}

	private int GetFirstUnviewedPageIndex()
	{
		for (int i = 0; i < sortedDefs.Count; i++)
		{
			if (UIBase.applicationState.itemSkinManager.HasUnviewedChangedItemSkinInSet(sortedDefs[i]))
			{
				return i / 2;
			}
		}
		return -1;
	}

	private bool HasUnviewedChangedSkinBeforePage(int page)
	{
		int num = Mathf.Min(page * 2, sortedDefs.Count);
		for (int i = 0; i < num; i++)
		{
			if (UIBase.applicationState.itemSkinManager.HasUnviewedChangedItemSkinInSet(sortedDefs[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasUnviewedChangedSkinAfterPage(int page)
	{
		for (int i = Mathf.Min((page + 1) * 2, sortedDefs.Count); i < sortedDefs.Count; i++)
		{
			if (UIBase.applicationState.itemSkinManager.HasUnviewedChangedItemSkinInSet(sortedDefs[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void SetupCell(SkinSetBookCell cell, int index)
	{
		if (index >= 0 && index < sortedDefs.Count)
		{
			cell.gameObject.SetActive(value: true);
			cell.Setup(sortedDefs[index]);
		}
		else
		{
			cell.Unset();
			cell.gameObject.SetActive(value: false);
		}
	}

	private void _OnPrevButtonClicked()
	{
		if (currentPage > 0)
		{
			currentPage--;
			SetDirty();
		}
	}

	private void _OnNextButtonClicked()
	{
		if (currentPage < PageCount - 1)
		{
			currentPage++;
			SetDirty();
		}
	}
}
