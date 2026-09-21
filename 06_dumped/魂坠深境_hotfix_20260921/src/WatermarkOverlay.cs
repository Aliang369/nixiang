using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WatermarkOverlay : MonoBehaviour
{
	private const int SortingOrder = 32760;

	private const float TextRefreshInterval = 5f;

	[Header("平铺密度（改后重建）")]
	[SerializeField]
	private float tileSpacingX = 360f;

	[SerializeField]
	private float tileSpacingY = 160f;

	[SerializeField]
	private float coverageScale = 1.15f;

	[SerializeField]
	private int maxPerAxis = 40;

	[Header("外观（改后重建）")]
	[SerializeField]
	private float fontSize = 30f;

	[SerializeField]
	private Vector2 tileSize = new Vector2(360f, 48f);

	[SerializeField]
	private string warningText = "创作者专用测试版本";

	[Header("斜向 / 透明度 / 漂移（实时生效）")]
	[SerializeField]
	private float textAngle = 30f;

	[SerializeField]
	private float minAlpha = 0.01f;

	[SerializeField]
	private float maxAlpha = 0.02f;

	[SerializeField]
	private float alphaSpeed = 0.9f;

	[SerializeField]
	private float driftAmplitude = 35f;

	[SerializeField]
	private float driftSpeed = 0.3f;

	private Canvas _canvas;

	private RectTransform _grid;

	private CanvasGroup _gridGroup;

	private readonly List<TextMeshProUGUI> _pidTiles = new List<TextMeshProUGUI>();

	private IWatermarkContentProvider _provider;

	private float _refreshTimer;

	private int _builtW;

	private int _builtH;

	private bool _layoutDirty;

	public static WatermarkOverlay Create(Transform parent)
	{
		GameObject obj = new GameObject("WatermarkOverlay");
		obj.transform.SetParent(parent, worldPositionStays: false);
		WatermarkOverlay watermarkOverlay = obj.AddComponent<WatermarkOverlay>();
		watermarkOverlay.Build();
		return watermarkOverlay;
	}

	private void Build()
	{
		_provider = new DefaultWatermarkContentProvider();
		_canvas = base.gameObject.AddComponent<Canvas>();
		_canvas.renderMode = (RenderMode)0;
		_canvas.overrideSorting = true;
		_canvas.sortingOrder = 32760;
		GameObject gameObject = new GameObject("grid");
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		_grid = gameObject.AddComponent<RectTransform>();
		RectTransform grid = _grid;
		RectTransform grid2 = _grid;
		Vector2 vector = (_grid.pivot = new Vector2(0.5f, 0.5f));
		Vector2 anchorMin = (grid2.anchorMax = vector);
		grid.anchorMin = anchorMin;
		_grid.localRotation = Quaternion.Euler(0f, 0f, textAngle);
		_gridGroup = gameObject.AddComponent<CanvasGroup>();
		_gridGroup.blocksRaycasts = false;
		_gridGroup.interactable = false;
		RebuildGrid();
	}

	private void RebuildGrid()
	{
		for (int num = _grid.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(_grid.GetChild(num).gameObject);
		}
		_pidTiles.Clear();
		float num2 = Screen.width;
		float num3 = Screen.height;
		float num4 = Mathf.Sqrt(num2 * num2 + num3 * num3) * coverageScale;
		int num5 = Mathf.Min(Mathf.CeilToInt(num4 / Mathf.Max(tileSpacingX, 1f)), maxPerAxis);
		int num6 = Mathf.Min(Mathf.CeilToInt(num4 / Mathf.Max(tileSpacingY, 1f)), maxPerAxis);
		float num7 = (float)(-num5) * 0.5f * tileSpacingX;
		float num8 = (float)(-num6) * 0.5f * tileSpacingY;
		string text = _provider.GetText();
		for (int i = 0; i <= num5; i++)
		{
			for (int j = 0; j <= num6; j++)
			{
				bool flag = ((i + j) & 1) == 0;
				TextMeshProUGUI textMeshProUGUI = CreateTile(flag ? warningText : text);
				textMeshProUGUI.rectTransform.anchoredPosition = new Vector2(num7 + (float)i * tileSpacingX, num8 + (float)j * tileSpacingY);
				if (!flag)
				{
					_pidTiles.Add(textMeshProUGUI);
				}
			}
		}
		_builtW = Screen.width;
		_builtH = Screen.height;
	}

	private TextMeshProUGUI CreateTile(string text)
	{
		GameObject obj = new GameObject("tile");
		obj.transform.SetParent(_grid, worldPositionStays: false);
		RectTransform rectTransform = obj.AddComponent<RectTransform>();
		Vector2 vector = (rectTransform.pivot = new Vector2(0.5f, 0.5f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		rectTransform.sizeDelta = tileSize;
		TextMeshProUGUI textMeshProUGUI = obj.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.font = TMP_Settings.defaultFontAsset;
		textMeshProUGUI.fontSize = fontSize;
		textMeshProUGUI.color = Color.white;
		textMeshProUGUI.alignment = TextAlignmentOptions.Center;
		textMeshProUGUI.raycastTarget = false;
		textMeshProUGUI.enableWordWrapping = false;
		textMeshProUGUI.SetText(text);
		return textMeshProUGUI;
	}

	private void Update()
	{
		EnsureTopMost();
		if (_layoutDirty)
		{
			_layoutDirty = false;
			RebuildGrid();
		}
		if (Screen.width != _builtW || Screen.height != _builtH)
		{
			RebuildGrid();
		}
		_refreshTimer -= Time.unscaledDeltaTime;
		if (_refreshTimer <= 0f)
		{
			string text = _provider.GetText();
			foreach (TextMeshProUGUI pidTile in _pidTiles)
			{
				pidTile.SetText(text);
			}
			_refreshTimer = 5f;
		}
		float unscaledTime = Time.unscaledTime;
		_grid.localRotation = Quaternion.Euler(0f, 0f, textAngle);
		float t = (Mathf.Sin(unscaledTime * alphaSpeed) + 1f) * 0.5f;
		_gridGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
		_grid.anchoredPosition = new Vector2(Mathf.Sin(unscaledTime * driftSpeed) * driftAmplitude, Mathf.Cos(unscaledTime * driftSpeed * 0.85f) * driftAmplitude);
	}

	private void OnValidate()
	{
		if (_grid != null)
		{
			_layoutDirty = true;
		}
	}

	private void EnsureTopMost()
	{
		if (_canvas.sortingOrder != 32760)
		{
			_canvas.sortingOrder = 32760;
		}
		if (base.transform.GetSiblingIndex() != base.transform.parent.childCount - 1)
		{
			base.transform.SetAsLastSibling();
		}
	}
}
