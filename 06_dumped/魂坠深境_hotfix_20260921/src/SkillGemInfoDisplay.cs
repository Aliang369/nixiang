using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillGemInfoDisplay : CellUI<SkillGem>
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private TextMeshProUGUI descriptionPrefab;

	[SerializeField]
	private RectTransform background;

	[SerializeField]
	private Image gem;

	[NonSerialized]
	private PrefabPool<TextMeshProUGUI> descriptionPool;

	[NonSerialized]
	private Dictionary<Vector2Int, Image> _hexImages = new Dictionary<Vector2Int, Image>();

	private SkillGem skillGem => base.setupData;

	protected override void Init()
	{
		descriptionPool = new PrefabPool<TextMeshProUGUI>(descriptionPrefab);
		gem.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(SkillGem _)
	{
		if (skillGem == null)
		{
			Unset();
			return;
		}
		base.gameObject.SetActive(value: true);
		nameText.text = skillGem.def.skillGemName;
		foreach (Image value in _hexImages.Values)
		{
			value.gameObject.SetActive(value: false);
		}
		gem.sprite = skillGem.def.GemSprite;
		int num = CalculateHexSize();
		HexagonHelper.GenerateHexShapeLayout(_hexImages, gem, background, skillGem.def.hexShape, num);
		CenterHexShape(skillGem.def.hexShape, num);
		descriptionPool.DespawnAll();
		foreach (var allWordDescription in skillGem.def.GetAllWordDescriptions(CellUI<SkillGem>.applicationState, null))
		{
			string item = allWordDescription.Item1;
			TextDataGenerator item2 = allWordDescription.Item2;
			TextMeshProUGUI textMeshProUGUI = descriptionPool.Spawn();
			textMeshProUGUI.text = item;
			new CommonTextLinkObjectDisplayer(CellUI<SkillGem>.applicationState, textMeshProUGUI, base.rectTransform).SetGenerator(item2);
		}
	}

	protected override void OnUnset()
	{
		descriptionPool?.DespawnAll();
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
		return Mathf.Max(1, Mathf.RoundToInt(num / 8f));
	}

	private void CenterHexShape(HexShape shape, float hexSize)
	{
		if (shape?.points == null || shape.points.Count == 0)
		{
			return;
		}
		Vector2 vector = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
		Vector2 vector2 = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		foreach (Vector2Int point in shape.points)
		{
			Vector2 hexPointPosition = HexagonHelper.GetHexPointPosition(point + shape.offset, hexSize);
			vector = Vector2.Min(vector, hexPointPosition);
			vector2 = Vector2.Max(vector2, hexPointPosition);
		}
		Vector2 vector3 = (vector + vector2) * 0.5f;
		foreach (Vector2Int point2 in shape.points)
		{
			Vector2Int vector2Int = point2 + shape.offset;
			if (_hexImages.TryGetValue(vector2Int, out var value))
			{
				Vector2 vector4 = HexagonHelper.GetHexPointPosition(vector2Int, hexSize) - vector3;
				value.transform.localPosition = new Vector3(vector4.x, vector4.y, value.transform.localPosition.z);
			}
		}
	}

	protected override void UpdateDisplay()
	{
		if (skillGem != null)
		{
			levelText.text = $"LV:{skillGem.def.level}";
		}
	}
}
