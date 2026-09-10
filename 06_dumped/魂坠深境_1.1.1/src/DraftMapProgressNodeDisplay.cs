using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DraftMapProgressNodeDisplay : CellUI<DraftMapProgressNodeDisplay.NodeData>
{
	public class NodeData
	{
		public string displayName;

		public State state;

		public bool showConnector;

		public int stageNumber;
	}

	public enum State
	{
		Finished,
		Current,
		Unknown
	}

	[SerializeField]
	private Image connectorImage;

	[SerializeField]
	private Image nodeImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI stateText;

	[SerializeField]
	private TextMeshProUGUI stageNumberText;

	[SerializeField]
	private Sprite connectorSprite;

	[SerializeField]
	private Sprite nodeSprite;

	[SerializeField]
	private Vector2 nodeSize = new Vector2(60f, 60f);

	[SerializeField]
	private Vector2 currentNodeSize = new Vector2(62f, 62f);

	[SerializeField]
	private Vector2 finishedNodeSize = new Vector2(54f, 54f);

	[SerializeField]
	private Vector2 unknownNodeSize = new Vector2(44f, 44f);

	[SerializeField]
	private Vector2 defaultCellSize = new Vector2(320f, 84f);

	[SerializeField]
	private Vector2 currentCellSize = new Vector2(340f, 104f);

	[SerializeField]
	private Vector2 finishedCellSize = new Vector2(320f, 84f);

	[SerializeField]
	private Vector2 unknownCellSize = new Vector2(320f, 84f);

	[SerializeField]
	private float connectorWidth = 14f;

	[SerializeField]
	private float connectorHeight = 89f;

	[SerializeField]
	private float axisX = 52f;

	protected override void Init()
	{
		CreateRuntimeLayoutIfNeeded();
		RefreshStyleLayout();
	}

	protected override void OnSetup(NodeData setupData)
	{
		ApplyStateLayout(setupData.state);
		connectorImage.gameObject.SetActive(setupData.showConnector);
	}

	public void SetConnectorHeight(float height)
	{
		connectorHeight = height;
		if (!(connectorImage == null))
		{
			(connectorImage.transform as RectTransform).sizeDelta = new Vector2(connectorHeight, connectorWidth);
		}
	}

	public void ApplyStyle(Sprite connector, Sprite node, Vector2 markerSize, float lineWidth, float lineHeight, float lineX)
	{
		connectorSprite = connector;
		nodeSprite = node;
		nodeSize = markerSize;
		connectorWidth = lineWidth;
		connectorHeight = lineHeight;
		axisX = lineX;
		RefreshStyleLayout();
	}

	protected override void UpdateDisplay()
	{
		NodeData nodeData = base.setupData;
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		object obj = nodeData.state switch
		{
			State.Finished => dungeonMapDisplayConfig.finished, 
			State.Current => dungeonMapDisplayConfig.selectable, 
			_ => dungeonMapDisplayConfig.cannotReach, 
		};
		Color baseColor = ((DungeonMapDisplay.Config.StateConfig)obj).iconColor;
		Color baseColor2 = ((DungeonMapDisplay.Config.StateConfig)obj).nameTextColor;
		nodeImage.sprite = nodeSprite;
		nodeImage.color = GetNodeColor(nodeData.state, baseColor);
		nodeImage.preserveAspect = true;
		nameText.text = nodeData.displayName;
		nameText.color = GetTextColor(nodeData.state, baseColor2);
		stateText.text = GetStateText(nodeData.state);
		stateText.color = GetStateColor(nodeData.state, baseColor2);
		stageNumberText.text = nodeData.stageNumber.ToString();
		stageNumberText.fontSize = GetStageNumberFontSize(nodeData.state);
		stageNumberText.color = GetStageNumberColor(nodeData.state);
		if (connectorImage != null)
		{
			connectorImage.sprite = connectorSprite;
			connectorImage.type = Image.Type.Simple;
			connectorImage.gameObject.SetActive(nodeData.showConnector);
			connectorImage.color = GetConnectorColor(nodeData.state, baseColor);
		}
	}

	private static string GetStateText(State state)
	{
		return state switch
		{
			State.Finished => ScriptableObjectSingleton<TextDataPreset>.instance.term_map_state_explored, 
			State.Current => ScriptableObjectSingleton<TextDataPreset>.instance.term_map_state_current, 
			_ => ScriptableObjectSingleton<TextDataPreset>.instance.term_map_state_unexplored, 
		};
	}

	private void CreateRuntimeLayoutIfNeeded()
	{
		if (connectorImage == null)
		{
			GameObject gameObject = new GameObject("Connector", typeof(RectTransform));
			RectTransform obj = gameObject.transform as RectTransform;
			obj.SetParent(base.transform, worldPositionStays: false);
			obj.SetAsFirstSibling();
			obj.anchorMin = new Vector2(0f, 0.5f);
			obj.anchorMax = new Vector2(0f, 0.5f);
			obj.pivot = new Vector2(0f, 0.5f);
			connectorImage = gameObject.AddComponent<Image>();
			connectorImage.raycastTarget = false;
		}
		ApplyCellSize(defaultCellSize);
		if (nodeImage == null)
		{
			GameObject gameObject2 = new GameObject("NodeMarker", typeof(RectTransform));
			RectTransform obj2 = gameObject2.transform as RectTransform;
			obj2.SetParent(base.transform, worldPositionStays: false);
			obj2.anchorMin = new Vector2(0f, 0.5f);
			obj2.anchorMax = new Vector2(0f, 0.5f);
			obj2.pivot = new Vector2(0.5f, 0.5f);
			nodeImage = gameObject2.AddComponent<Image>();
			nodeImage.raycastTarget = false;
		}
		if ((object)nameText == null)
		{
			nameText = CreateText("MapName", new Vector2(98f, 12f), new Vector2(220f, 30f), 22, TextAlignmentOptions.MidlineLeft);
		}
		if ((object)stateText == null)
		{
			stateText = CreateText("State", new Vector2(98f, -17f), new Vector2(220f, 22f), 14, TextAlignmentOptions.MidlineLeft);
		}
		if ((object)stageNumberText == null)
		{
			stageNumberText = CreateStageNumberText();
		}
	}

	private void RefreshStyleLayout()
	{
		if (!(nodeImage == null))
		{
			_ = base.transform;
			ApplyCellSize(defaultCellSize);
			RectTransform obj = nodeImage.transform as RectTransform;
			obj.anchoredPosition = new Vector2(axisX, 0f);
			obj.sizeDelta = nodeSize;
			if (stageNumberText != null)
			{
				RectTransform obj2 = stageNumberText.transform as RectTransform;
				obj2.anchorMin = Vector2.zero;
				obj2.anchorMax = Vector2.one;
				obj2.offsetMin = new Vector2(7f, 7f);
				obj2.offsetMax = new Vector2(-7f, -7f);
			}
			if (connectorImage != null)
			{
				RectTransform obj3 = connectorImage.transform as RectTransform;
				obj3.anchoredPosition = new Vector2(axisX, 0f);
				obj3.localRotation = Quaternion.Euler(0f, 0f, -90f);
				obj3.sizeDelta = new Vector2(connectorHeight, connectorWidth);
			}
			if (nameText != null)
			{
				RectTransform obj4 = nameText.transform as RectTransform;
				obj4.anchoredPosition = new Vector2(axisX + nodeSize.x * 0.76f, 12f);
				obj4.sizeDelta = new Vector2(220f, 30f);
			}
			if (stateText != null)
			{
				RectTransform obj5 = stateText.transform as RectTransform;
				obj5.anchoredPosition = new Vector2(axisX + nodeSize.x * 0.76f, -17f);
				obj5.sizeDelta = new Vector2(220f, 22f);
			}
		}
	}

	private void ApplyStateLayout(State state)
	{
		ApplyCellSize(state switch
		{
			State.Current => currentCellSize, 
			State.Finished => finishedCellSize, 
			_ => unknownCellSize, 
		});
		RectTransform rectTransform = nodeImage.transform as RectTransform;
		rectTransform.sizeDelta = state switch
		{
			State.Current => currentNodeSize, 
			State.Finished => finishedNodeSize, 
			_ => unknownNodeSize, 
		};
		if (connectorImage != null)
		{
			(connectorImage.transform as RectTransform).sizeDelta = new Vector2(connectorHeight, connectorWidth);
		}
		if (base.transform.parent is RectTransform rect)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rect);
		}
	}

	private void ApplyCellSize(Vector2 cellSize)
	{
		RectTransform obj = base.transform as RectTransform;
		float x = ((cellSize.x > 0f) ? Mathf.Clamp01(axisX / cellSize.x) : 0.5f);
		obj.pivot = new Vector2(x, 0.5f);
		obj.sizeDelta = cellSize;
	}

	private static Color GetNodeColor(State state, Color baseColor)
	{
		return state switch
		{
			State.Current => new Color(baseColor.r, baseColor.g, baseColor.b, 1f), 
			State.Finished => new Color(baseColor.r, baseColor.g, baseColor.b, 0.78f), 
			_ => new Color(baseColor.r, baseColor.g, baseColor.b, 0.68f), 
		};
	}

	private static Color GetConnectorColor(State state, Color baseColor)
	{
		return state switch
		{
			State.Current => new Color(baseColor.r, baseColor.g, baseColor.b, 0.95f), 
			State.Finished => new Color(baseColor.r, baseColor.g, baseColor.b, 0.72f), 
			_ => new Color(baseColor.r, baseColor.g, baseColor.b, 0.62f), 
		};
	}

	private static Color GetTextColor(State state, Color baseColor)
	{
		if (state != State.Unknown)
		{
			return new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
		}
		return new Color(baseColor.r, baseColor.g, baseColor.b, 0.82f);
	}

	private static Color GetStateColor(State state, Color baseColor)
	{
		float a = ((state == State.Current) ? 0.95f : 0.68f);
		return new Color(baseColor.r, baseColor.g, baseColor.b, a);
	}

	private static float GetStageNumberFontSize(State state)
	{
		return state switch
		{
			State.Current => 28f, 
			State.Finished => 24f, 
			_ => 21f, 
		};
	}

	private static Color GetStageNumberColor(State state)
	{
		return state switch
		{
			State.Current => new Color(1f, 0.91f, 0.63f, 1f), 
			State.Finished => new Color(0.82f, 0.78f, 0.68f, 0.9f), 
			_ => new Color(0.58f, 0.56f, 0.51f, 0.82f), 
		};
	}

	private TextMeshProUGUI CreateStageNumberText()
	{
		GameObject obj = new GameObject("StageNumber", typeof(RectTransform));
		(obj.transform as RectTransform).SetParent(nodeImage.transform, worldPositionStays: false);
		TextMeshProUGUI textMeshProUGUI = obj.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.raycastTarget = false;
		textMeshProUGUI.fontSize = 24f;
		textMeshProUGUI.fontStyle = FontStyles.Bold;
		textMeshProUGUI.alignment = TextAlignmentOptions.Center;
		textMeshProUGUI.enableWordWrapping = false;
		textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
		return textMeshProUGUI;
	}

	private TextMeshProUGUI CreateText(string name, Vector2 position, Vector2 size, int fontSize, TextAlignmentOptions alignment)
	{
		GameObject obj = new GameObject(name, typeof(RectTransform));
		RectTransform obj2 = obj.transform as RectTransform;
		obj2.SetParent(base.transform, worldPositionStays: false);
		obj2.anchorMin = new Vector2(0f, 0.5f);
		obj2.anchorMax = new Vector2(0f, 0.5f);
		obj2.pivot = new Vector2(0f, 0.5f);
		obj2.anchoredPosition = position;
		obj2.sizeDelta = size;
		TextMeshProUGUI textMeshProUGUI = obj.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.raycastTarget = false;
		textMeshProUGUI.fontSize = fontSize;
		textMeshProUGUI.alignment = alignment;
		textMeshProUGUI.overflowMode = TextOverflowModes.Ellipsis;
		textMeshProUGUI.enableWordWrapping = false;
		return textMeshProUGUI;
	}
}
