using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class RewardUncollectedTag : MonoBehaviour
{
	[Serializable]
	private struct LayoutSpec
	{
		public RewardUncollectedTagLayout layout;

		public Vector2 anchoredPosition;

		public Vector2 sizeDelta;

		public float fontSizeMax;
	}

	[Header("References")]
	[SerializeField]
	private RectTransform tagRoot;

	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private TextMeshProUGUI tagText;

	[Header("Config")]
	[SerializeField]
	private RewardUncollectedTagLayout layout;

	[SerializeField]
	private List<LayoutSpec> layoutSpecs = new List<LayoutSpec>();

	public static RewardUncollectedTag AttachTo(Component parent, RewardUncollectedTagLayout layout = RewardUncollectedTagLayout.Card)
	{
		if (parent == null)
		{
			return null;
		}
		RewardUncollectedTag rewardUncollectedTag = FindAttachedTag(parent);
		if (rewardUncollectedTag == null)
		{
			rewardUncollectedTag = InstantiateConfiguredPrefab(parent);
		}
		if (rewardUncollectedTag == null)
		{
			return null;
		}
		rewardUncollectedTag.SetLayout(layout);
		rewardUncollectedTag.Hide();
		return rewardUncollectedTag;
	}

	public static RewardUncollectedTag AttachToItemInfoDisplay(ItemInfoDisplayBase itemInfoDisplay)
	{
		if (itemInfoDisplay == null)
		{
			return null;
		}
		MainItemInfoDisplayBase componentInChildren = itemInfoDisplay.GetComponentInChildren<MainItemInfoDisplayBase>(includeInactive: true);
		return AttachTo((componentInChildren != null) ? ((Component)componentInChildren) : ((Component)itemInfoDisplay), RewardUncollectedTagLayout.LargeCard);
	}

	public void Setup(ICTAsset content, ApplicationStateInfo applicationState)
	{
		CacheReferences();
		ApplyLayout();
		bool flag = ShouldShow(content, applicationState);
		base.gameObject.SetActive(flag);
		if (flag && tagText != null)
		{
			tagText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_index_uncollected;
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Awake()
	{
		CacheReferences();
	}

	private void CacheReferences()
	{
		if (tagRoot == null)
		{
			tagRoot = base.transform as RectTransform;
		}
		if (layoutElement == null)
		{
			layoutElement = GetComponent<LayoutElement>();
		}
		if (backgroundImage == null)
		{
			backgroundImage = GetComponent<Image>();
		}
		if (tagText == null)
		{
			tagText = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		}
	}

	private void SetLayout(RewardUncollectedTagLayout targetLayout)
	{
		layout = targetLayout;
		CacheReferences();
		ApplyLayout();
	}

	private void ApplyLayout()
	{
		CacheReferences();
		if (!(tagRoot == null))
		{
			LayoutSpec layoutSpec = GetLayoutSpec(layout);
			tagRoot.anchorMin = new Vector2(0.5f, 0f);
			tagRoot.anchorMax = new Vector2(0.5f, 0f);
			tagRoot.pivot = new Vector2(0.5f, 0.5f);
			tagRoot.anchoredPosition = layoutSpec.anchoredPosition;
			tagRoot.sizeDelta = layoutSpec.sizeDelta;
			tagRoot.SetAsLastSibling();
			if (layoutElement != null)
			{
				layoutElement.ignoreLayout = true;
			}
			ApplyTextLayout(layoutSpec);
		}
	}

	private void ApplyTextLayout(LayoutSpec layoutSpec)
	{
		if (!(tagText == null))
		{
			tagText.fontSize = layoutSpec.fontSizeMax;
			tagText.fontSizeMax = layoutSpec.fontSizeMax;
		}
	}

	private static bool ShouldShow(ICTAsset content, ApplicationStateInfo applicationState)
	{
		if (content != null && applicationState?.discoverManager != null && DiscoverManager.IsContentInConfiguredIndexPage(content, applicationState.assetCatalog))
		{
			return !applicationState.discoverManager.IsDiscovered(content);
		}
		return false;
	}

	private static RewardUncollectedTag FindAttachedTag(Component parent)
	{
		if (parent.TryGetComponent<RewardUncollectedTag>(out var component))
		{
			return component;
		}
		foreach (Transform item in parent.transform)
		{
			if (item.TryGetComponent<RewardUncollectedTag>(out component))
			{
				return component;
			}
		}
		return null;
	}

	private static RewardUncollectedTag InstantiateConfiguredPrefab(Component parent)
	{
		RewardUncollectedTag rewardUncollectedTag = ScriptableObjectSingleton<GameDataManager>.instance?.rewardUncollectedTagPrefab;
		if (rewardUncollectedTag == null)
		{
			Debug.LogError("GameDataManager.rewardUncollectedTagPrefab is not assigned.", parent);
			return null;
		}
		return UnityEngine.Object.Instantiate(rewardUncollectedTag, parent.transform, worldPositionStays: false);
	}

	private bool TryGetLayoutSpec(RewardUncollectedTagLayout targetLayout, out LayoutSpec layoutSpec)
	{
		for (int i = 0; i < layoutSpecs.Count; i++)
		{
			if (layoutSpecs[i].layout == targetLayout)
			{
				layoutSpec = layoutSpecs[i];
				return true;
			}
		}
		layoutSpec = default(LayoutSpec);
		return false;
	}

	private LayoutSpec GetLayoutSpec(RewardUncollectedTagLayout targetLayout)
	{
		if (TryGetLayoutSpec(targetLayout, out var layoutSpec))
		{
			return layoutSpec;
		}
		if (targetLayout != RewardUncollectedTagLayout.Card && TryGetLayoutSpec(RewardUncollectedTagLayout.Card, out layoutSpec))
		{
			return layoutSpec;
		}
		return new LayoutSpec
		{
			layout = RewardUncollectedTagLayout.Card,
			anchoredPosition = new Vector2(0f, -82f),
			sizeDelta = new Vector2(148f, 48f),
			fontSizeMax = 32f
		};
	}
}
