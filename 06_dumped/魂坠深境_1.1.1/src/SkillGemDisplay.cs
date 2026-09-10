using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillGemDisplay : CellUI<SkillGem>, IDisplayModulesController
{
	public abstract class DisplayModule : UIDisplayModule
	{
		protected SkillGemDisplay display => parent as SkillGemDisplay;
	}

	[SerializeField]
	private RectTransform background;

	[SerializeField]
	private TextMeshProUGUI numText;

	[SerializeField]
	private Image gem;

	private int _hexSize;

	public Dictionary<Vector2Int, Image> _hexImages = new Dictionary<Vector2Int, Image>();

	protected override void Init()
	{
		gem.gameObject.SetActive(value: false);
		_hexSize = CalculateHexSize();
	}

	protected override void OnSetup(SkillGem skillGem)
	{
		base.gameObject.SetActive(value: true);
		if (TryGetComponent<GemMountingDraggerModule>(out var component))
		{
			component.Setup(skillGem);
		}
		foreach (Image value in _hexImages.Values)
		{
			value.gameObject.SetActive(value: false);
		}
		_hexSize = CalculateHexSize();
		gem.sprite = skillGem.def.GemSprite;
		HexagonHelper.GenerateHexShapeLayout(_hexImages, gem, background, skillGem.def.hexShape, _hexSize);
		GetModule<InputModule>().enabled = true;
	}

	protected override void OnUnset()
	{
		foreach (Image value in _hexImages.Values)
		{
			value.gameObject.SetActive(value: false);
		}
		gem.gameObject.SetActive(value: false);
		numText.text = "";
		GetModule<InputModule>().enabled = false;
		base.gameObject.SetActive(value: false);
	}

	private int CalculateHexSize()
	{
		float num = ((background != null) ? background.rect.width : 0f);
		if (num <= 0f && base.rectTransform != null)
		{
			num = base.rectTransform.rect.width;
		}
		if (num <= 0f && background != null)
		{
			num = Mathf.Max(background.sizeDelta.x, background.sizeDelta.y);
		}
		if (num <= 0f && base.rectTransform != null)
		{
			num = Mathf.Max(base.rectTransform.sizeDelta.x, base.rectTransform.sizeDelta.y);
		}
		if (num <= 0f)
		{
			num = 170f;
		}
		return Mathf.Max(1, Mathf.RoundToInt(num / 10f));
	}

	public void CenterHexShape()
	{
		HexShape hexShape = base.setupData?.def?.hexShape;
		if (hexShape?.points == null || hexShape.points.Count == 0)
		{
			return;
		}
		Vector2 vector = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
		Vector2 vector2 = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		foreach (Vector2Int point in hexShape.points)
		{
			Vector2 hexPointPosition = HexagonHelper.GetHexPointPosition(point + hexShape.offset, _hexSize);
			vector = Vector2.Min(vector, hexPointPosition);
			vector2 = Vector2.Max(vector2, hexPointPosition);
		}
		Vector2 vector3 = (vector + vector2) * 0.5f;
		foreach (Vector2Int point2 in hexShape.points)
		{
			Vector2Int vector2Int = point2 + hexShape.offset;
			if (_hexImages.TryGetValue(vector2Int, out var value))
			{
				Vector2 vector4 = HexagonHelper.GetHexPointPosition(vector2Int, _hexSize) - vector3;
				value.transform.localPosition = new Vector3(vector4.x, vector4.y, value.transform.localPosition.z);
			}
		}
	}

	protected override void UpdateDisplay()
	{
		numText.text = base.setupData.amount.ToString();
	}
}
