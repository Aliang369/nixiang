using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DraftMapProgressAxisDisplay : UIBase
{
	private const float NodeHeight = 84f;

	private const float NodeSpacing = 6.25f;

	[SerializeField]
	private RectTransform nodeParent;

	[SerializeField]
	private DraftMapProgressNodeDisplay nodePrefab;

	[SerializeField]
	private Sprite connectorSprite;

	[SerializeField]
	private Sprite nodeSprite;

	[SerializeField]
	private Vector2 nodeSize = new Vector2(60f, 60f);

	[SerializeField]
	private float connectorWidth = 14f;

	[SerializeField]
	private float axisX = 52f;

	[NonSerialized]
	private readonly List<DraftMapProgressNodeDisplay> nodeDisplays = new List<DraftMapProgressNodeDisplay>();

	protected override void Init()
	{
		CreateRuntimeLayoutIfNeeded();
		if (nodePrefab != null)
		{
			nodePrefab.gameObject.SetActive(value: false);
		}
	}

	public void Setup()
	{
		GameStateInfo gameStateInfo = UIBase.gameState;
		bool flag = gameStateInfo == null || !gameStateInfo.IsIntroGame;
		base.gameObject.SetActive(flag);
		if (flag)
		{
			RebuildNodes(BuildNodeData());
			SetDirty();
		}
	}

	protected override void UpdateDisplay()
	{
		RebuildNodes(BuildNodeData());
	}

	private List<DraftMapProgressNodeDisplay.NodeData> BuildNodeData()
	{
		List<DraftMapProgressNodeDisplay.NodeData> list = new List<DraftMapProgressNodeDisplay.NodeData>();
		List<DraftMapRef> list2 = UIBase.gameState?.dungeonMapManager?.visitedDraftMapRefs?.Where((DraftMapRef mapRef) => mapRef != null).ToList() ?? new List<DraftMapRef>();
		int num = list2.Count - 1;
		int targetStage = GetTargetStage();
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			DraftMapInfoData assetPreData = list2[num2].GetAssetPreData(UIBase.applicationState.assetCatalog);
			int stageNumber = num2 + 1;
			list.Add(new DraftMapProgressNodeDisplay.NodeData
			{
				displayName = (assetPreData?.mapDisplayName ?? ScriptableObjectSingleton<TextDataPreset>.instance.term_map_unknown_area),
				state = ((num2 == num) ? DraftMapProgressNodeDisplay.State.Current : DraftMapProgressNodeDisplay.State.Finished),
				showConnector = true,
				stageNumber = stageNumber
			});
		}
		int num3 = Mathf.Max(0, targetStage - list2.Count);
		for (int num4 = 0; num4 < num3; num4++)
		{
			int stageNumber2 = list2.Count + num4 + 1;
			list.Add(new DraftMapProgressNodeDisplay.NodeData
			{
				displayName = ScriptableObjectSingleton<TextDataPreset>.instance.term_map_unknown_area,
				state = DraftMapProgressNodeDisplay.State.Unknown,
				showConnector = true,
				stageNumber = stageNumber2
			});
		}
		if (list.Count > 0)
		{
			list[list.Count - 1].showConnector = false;
		}
		return list;
	}

	private int GetTargetStage()
	{
		GameStateInfo gameStateInfo = UIBase.gameState;
		if (gameStateInfo != null && gameStateInfo.IsIntroGame)
		{
			return Mathf.Max(1, UIBase.gameState.dungeonMapManager?.visitedDraftMapRefs?.Count ?? 1);
		}
		FixedContractRouteDef fixedContractRouteDef = UIBase.gameState?.newGameConfigData?.ResolvedFixedContractRoute;
		if (UIBase.gameState?.contractState?.selectedContract?.contractDef?.contractRule is SpiderContractRuleDef && (object)fixedContractRouteDef != null && fixedContractRouteDef.FinalStage > 0)
		{
			return fixedContractRouteDef.FinalStage;
		}
		if (UIBase.applicationState.guideManager.IsAfterIntroProtect)
		{
			return 2;
		}
		DungeonGenerationManager instance = ScriptableObjectSingleton<DungeonGenerationManager>.instance;
		if (instance == null)
		{
			return (UIBase.gameState?.dungeonMapManager?.visitedDraftMapRefs?.Count).GetValueOrDefault();
		}
		if (!UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableLongGameStage))
		{
			return instance.initFinishGameStage;
		}
		if (UIBase.gameState == null || UIBase.gameState.difficultyState.contractDifficulty < DifficultyLevel.Nightmare || !FinalEndingKeyUtility.HasPlayerRepairedFinalEndingKey(UIBase.gameState))
		{
			return instance.baseFinishGameStage;
		}
		return instance.finalFinishGameStage;
	}

	private void RebuildNodes(List<DraftMapProgressNodeDisplay.NodeData> data)
	{
		EnsureNodeDisplayCount(data.Count);
		for (int i = 0; i < nodeDisplays.Count; i++)
		{
			bool flag = i < data.Count;
			nodeDisplays[i].gameObject.SetActive(flag);
			if (flag)
			{
				nodeDisplays[i].Setup(data[i]);
			}
		}
		RefreshNodeConnectors(data.Count);
	}

	private void RefreshNodeConnectors(int activeNodeCount)
	{
		float num = nodeParent.GetComponent<VerticalLayoutGroup>()?.spacing ?? 6.25f;
		for (int i = 0; i < activeNodeCount - 1; i++)
		{
			RectTransform rectTransform = nodeDisplays[i].transform as RectTransform;
			RectTransform rectTransform2 = nodeDisplays[i + 1].transform as RectTransform;
			if (!(rectTransform == null) && !(rectTransform2 == null))
			{
				float connectorHeight = rectTransform.sizeDelta.y * 0.5f + num + rectTransform2.sizeDelta.y * 0.5f;
				nodeDisplays[i].SetConnectorHeight(connectorHeight);
			}
		}
	}

	private void EnsureNodeDisplayCount(int count)
	{
		while (nodeDisplays.Count < count)
		{
			DraftMapProgressNodeDisplay item = CreateNodeDisplay();
			nodeDisplays.Add(item);
		}
	}

	private DraftMapProgressNodeDisplay CreateNodeDisplay()
	{
		DraftMapProgressNodeDisplay draftMapProgressNodeDisplay;
		if (nodePrefab != null)
		{
			draftMapProgressNodeDisplay = UnityEngine.Object.Instantiate(nodePrefab, nodeParent);
			draftMapProgressNodeDisplay.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject obj = new GameObject("DraftMapProgressNode", typeof(RectTransform));
			RectTransform obj2 = obj.transform as RectTransform;
			obj2.SetParent(nodeParent, worldPositionStays: false);
			obj2.sizeDelta = new Vector2(320f, 84f);
			draftMapProgressNodeDisplay = obj.AddComponent<DraftMapProgressNodeDisplay>();
		}
		InitUI(draftMapProgressNodeDisplay);
		if (nodePrefab == null)
		{
			draftMapProgressNodeDisplay.ApplyStyle(connectorSprite, nodeSprite, nodeSize, connectorWidth, 90.25f, axisX);
		}
		return draftMapProgressNodeDisplay;
	}

	private void CreateRuntimeLayoutIfNeeded()
	{
		if (!(nodeParent != null))
		{
			RectTransform obj = base.transform as RectTransform;
			obj.anchorMin = new Vector2(1f, 0.5f);
			obj.anchorMax = new Vector2(1f, 0.5f);
			obj.pivot = new Vector2(1f, 0.5f);
			obj.sizeDelta = new Vector2(360f, 720f);
			Image orAddComponent = base.gameObject.GetOrAddComponent<Image>();
			orAddComponent.raycastTarget = false;
			orAddComponent.color = new Color(0f, 0f, 0f, 0f);
			GameObject obj2 = new GameObject("AxisBackplate", typeof(RectTransform));
			RectTransform obj3 = obj2.transform as RectTransform;
			obj3.SetParent(base.transform, worldPositionStays: false);
			obj3.anchorMin = new Vector2(1f, 0f);
			obj3.anchorMax = new Vector2(1f, 1f);
			obj3.pivot = new Vector2(1f, 0.5f);
			obj3.anchoredPosition = new Vector2(-8f, 0f);
			obj3.sizeDelta = new Vector2(184f, -8f);
			Image image = obj2.AddComponent<Image>();
			image.raycastTarget = false;
			image.color = new Color(0.035f, 0.027f, 0.018f, 0.34f);
			GameObject gameObject = new GameObject("Nodes", typeof(RectTransform));
			nodeParent = gameObject.transform as RectTransform;
			nodeParent.SetParent(base.transform, worldPositionStays: false);
			nodeParent.anchorMin = new Vector2(0f, 0.5f);
			nodeParent.anchorMax = new Vector2(1f, 0.5f);
			nodeParent.pivot = new Vector2(0.5f, 0.5f);
			nodeParent.anchoredPosition = new Vector2(0f, 68f);
			nodeParent.sizeDelta = new Vector2(-24f, 0f);
			VerticalLayoutGroup verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
			verticalLayoutGroup.childAlignment = (TextAnchor)3;
			verticalLayoutGroup.spacing = 6.25f;
			verticalLayoutGroup.childControlWidth = false;
			verticalLayoutGroup.childControlHeight = false;
			verticalLayoutGroup.childForceExpandWidth = false;
			verticalLayoutGroup.childForceExpandHeight = false;
		}
	}
}
