using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GemMountedDisplay : CellUI<SkillGem>, IDisplayModulesController
{
	[SerializeField]
	private bool haveOutline;

	[SerializeField]
	private float alphaThreshold = 0.1f;

	[SerializeField]
	private RectTransform outlineParent;

	[SerializeField]
	private RectTransform gemParent;

	[SerializeField]
	private Image outline;

	[SerializeField]
	private Image gem;

	[SerializeField]
	private GameButton rightRotateButton;

	[SerializeField]
	private GameButton unequipButton;

	[SerializeField]
	private Color unPinColor;

	[SerializeField]
	private Color validColor;

	[SerializeField]
	private Color invalidColor;

	public SkillGem SkillGem;

	private Dictionary<Vector2Int, Image> _hexImages = new Dictionary<Vector2Int, Image>();

	public Dictionary<Vector2Int, Image> _hexOutlineImages = new Dictionary<Vector2Int, Image>();

	private GemMountingPanel gemMountingPanel => GetComponentInParent<GemMountingPanel>();

	private bool isDragging => gemMountingPanel.isDragging;

	protected override void Init()
	{
		gem.gameObject.SetActive(value: false);
		outline.gameObject.SetActive(value: false);
		rightRotateButton.onClick.AddListener(RotateRight);
		if (unequipButton != null)
		{
			TextMeshProUGUI componentInChildren = unequipButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unequip;
			}
			unequipButton.onClick.AddListener(Unequip);
		}
		SetOperationButtonsVisible(visible: false);
	}

	protected override void OnSetup(SkillGem setupData)
	{
		SkillGem = setupData;
		gem.sprite = setupData.def.GemSprite;
		UpdateDisplay(HexagonHelper.RotateHexShape(setupData.def.hexShape, setupData.rotateIndex));
		SetSelected(b: false);
	}

	public void UpdateDisplay(HexShape shape)
	{
		foreach (Image value2 in _hexImages.Values)
		{
			value2.gameObject.SetActive(value: false);
		}
		foreach (Image value3 in _hexOutlineImages.Values)
		{
			value3.gameObject.SetActive(value: false);
		}
		if (haveOutline)
		{
			HexagonHelper.GenerateHexShapeLayout(_hexOutlineImages, outline, outlineParent, shape, HexagonHelper.HexCellSize);
		}
		HexagonHelper.GenerateHexShapeLayout(_hexImages, gem, gemParent, shape, HexagonHelper.HexCellSize);
		foreach (KeyValuePair<Vector2Int, Image> hexOutlineImage in _hexOutlineImages)
		{
			hexOutlineImage.Deconstruct(out var _, out var value);
			value.gameObject.SetActive(value: false);
		}
		if (TryGetComponent<GemMountingDraggerModule>(out var component))
		{
			component.Setup(base.setupData);
		}
	}

	private void Update()
	{
		if (gemMountingPanel == null)
		{
			return;
		}
		foreach (var (vector2Int2, image2) in _hexOutlineImages)
		{
			if (image2.gameObject.activeInHierarchy)
			{
				if (!gemMountingPanel.IsPositionValid(this, SkillGem.tempOffset + vector2Int2))
				{
					image2.color = invalidColor;
				}
				else if (isDragging)
				{
					image2.color = unPinColor;
				}
				else
				{
					image2.color = validColor;
				}
			}
		}
	}

	private void RotateRight()
	{
		gemMountingPanel?.RotateMountedGem(this);
	}

	private void Unequip()
	{
		gemMountingPanel?.UnmountMountedGem(this);
	}

	public void SetSelected(bool b)
	{
		foreach (Vector2Int item in _hexImages.Keys.Where((Vector2Int key) => _hexImages[key].gameObject.activeInHierarchy))
		{
			if (_hexOutlineImages.TryGetValue(item, out var value))
			{
				value.gameObject.SetActive(b);
			}
		}
		SetOperationButtonsVisible(b);
	}

	private void SetOperationButtonsVisible(bool visible)
	{
		if (rightRotateButton != null)
		{
			rightRotateButton.gameObject.SetActive(visible);
		}
		if (unequipButton != null)
		{
			unequipButton.gameObject.SetActive(value: false);
		}
	}

	public List<Vector2Int> GetVisibleGemPoints()
	{
		return (from pair in _hexImages
			where pair.Value != null && pair.Value.gameObject.activeInHierarchy
			select pair.Key).ToList();
	}

	public List<Vector2Int> GetVisibleOutlinePoints()
	{
		return (from pair in _hexOutlineImages
			where pair.Value != null && pair.Value.gameObject.activeInHierarchy
			select pair.Key).ToList();
	}
}
