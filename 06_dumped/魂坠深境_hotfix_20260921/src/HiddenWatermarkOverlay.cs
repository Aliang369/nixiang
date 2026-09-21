using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HiddenWatermarkOverlay : MonoBehaviour
{
	private const int SortingOrder = 32761;

	private const float RefreshInterval = 5f;

	private const string AdditiveShaderAddress = "Assets/Shader/UI/TMP_SDF_Additive.shader";

	[Header("隐写参数（实时生效，调好后固化）")]
	[SerializeField]
	[Range(0f, 12f)]
	private int blueDeltaCV = 8;

	[SerializeField]
	[Range(0f, 0.4f)]
	private float faceDilate = 0.15f;

	[SerializeField]
	private float fontSize = 18f;

	[SerializeField]
	private Vector2 margin = new Vector2(24f, 24f);

	private static readonly (Vector2 anchor, TextAlignmentOptions align)[] Corners = new(Vector2, TextAlignmentOptions)[4]
	{
		(new Vector2(0f, 1f), TextAlignmentOptions.TopLeft),
		(new Vector2(1f, 1f), TextAlignmentOptions.TopRight),
		(new Vector2(0f, 0f), TextAlignmentOptions.BottomLeft),
		(new Vector2(1f, 0f), TextAlignmentOptions.BottomRight)
	};

	private Canvas _canvas;

	private readonly List<(RectTransform rt, TextMeshProUGUI tmp, Vector2 sign)> _tiles = new List<(RectTransform, TextMeshProUGUI, Vector2)>();

	private IWatermarkContentProvider _provider;

	private Shader _additiveShader;

	private AsyncOperationHandle<Shader> _shaderHandle;

	private float _refreshTimer;

	public static HiddenWatermarkOverlay Create(Transform parent)
	{
		GameObject obj = new GameObject("HiddenWatermarkOverlay");
		obj.transform.SetParent(parent, worldPositionStays: false);
		HiddenWatermarkOverlay hiddenWatermarkOverlay = obj.AddComponent<HiddenWatermarkOverlay>();
		hiddenWatermarkOverlay.Build();
		return hiddenWatermarkOverlay;
	}

	private void Build()
	{
		_provider = new DefaultWatermarkContentProvider();
		_shaderHandle = Addressables.LoadAssetAsync<Shader>("Assets/Shader/UI/TMP_SDF_Additive.shader");
		_additiveShader = _shaderHandle.WaitForCompletion();
		_canvas = base.gameObject.AddComponent<Canvas>();
		_canvas.renderMode = (RenderMode)0;
		_canvas.overrideSorting = true;
		_canvas.sortingOrder = 32761;
		(Vector2, TextAlignmentOptions)[] corners = Corners;
		for (int i = 0; i < corners.Length; i++)
		{
			var (anchor, align) = corners[i];
			CreateCornerTile(anchor, align);
		}
		ApplyStyle();
		SetTextAll(_provider.GetText());
	}

	private void CreateCornerTile(Vector2 anchor, TextAlignmentOptions align)
	{
		GameObject obj = new GameObject("hidden");
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		RectTransform rectTransform = obj.AddComponent<RectTransform>();
		Vector2 vector = (rectTransform.pivot = anchor);
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		rectTransform.sizeDelta = new Vector2(600f, 80f);
		TextMeshProUGUI textMeshProUGUI = obj.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.font = TMP_Settings.defaultFontAsset;
		textMeshProUGUI.alignment = align;
		textMeshProUGUI.raycastTarget = false;
		textMeshProUGUI.enableWordWrapping = false;
		if (_additiveShader != null)
		{
			textMeshProUGUI.fontMaterial.shader = _additiveShader;
		}
		Vector2 item = new Vector2((anchor.x == 0f) ? 1f : (-1f), (anchor.y == 1f) ? (-1f) : 1f);
		_tiles.Add((rectTransform, textMeshProUGUI, item));
	}

	private void ApplyStyle()
	{
		float num = (float)blueDeltaCV / 255f;
		float b = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? Mathf.LinearToGammaSpace(num) : num);
		Color color = ((_additiveShader != null) ? new Color(0f, 0f, b, 1f) : new Color(0f, 0f, 1f, num));
		foreach (var tile in _tiles)
		{
			RectTransform item = tile.rt;
			TextMeshProUGUI item2 = tile.tmp;
			Vector2 item3 = tile.sign;
			item2.color = color;
			item2.fontSize = fontSize;
			if (_additiveShader != null)
			{
				item2.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
			}
			item.anchoredPosition = new Vector2(item3.x * margin.x, item3.y * margin.y);
		}
	}

	private void SetTextAll(string text)
	{
		foreach (var tile in _tiles)
		{
			tile.tmp.SetText(text);
		}
	}

	private void Update()
	{
		EnsureTopMost();
		ApplyStyle();
		_refreshTimer -= Time.unscaledDeltaTime;
		if (_refreshTimer <= 0f)
		{
			SetTextAll(_provider.GetText());
			_refreshTimer = 5f;
		}
	}

	private void EnsureTopMost()
	{
		if (_canvas.sortingOrder != 32761)
		{
			_canvas.sortingOrder = 32761;
		}
	}

	private void OnDestroy()
	{
		if (_shaderHandle.IsValid())
		{
			Addressables.Release(_shaderHandle);
		}
	}
}
