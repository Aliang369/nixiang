using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillGemSlotDisplay : UIBase
{
	[SerializeField]
	private Image slot;

	private Dictionary<Vector2Int, Image> _hexImages = new Dictionary<Vector2Int, Image>();

	private Dictionary<Vector2Int, Image> _effectImages = new Dictionary<Vector2Int, Image>();

	private Dictionary<Vector2Int, Sequence> _effectSequences = new Dictionary<Vector2Int, Sequence>();

	private static readonly Color AddedSlotEffectColor = new Color(0.35f, 1f, 0.85f, 0.95f);

	private static readonly Color RemovedSlotEffectColor = new Color(1f, 0.45f, 0.2f, 0.95f);

	protected override void Init()
	{
		slot.gameObject.SetActive(value: false);
	}

	public void HideAllSlot()
	{
		foreach (Image value in _hexImages.Values)
		{
			value.gameObject.SetActive(value: false);
		}
		ClearSlotEffects();
	}

	public void Setup(HexShape hexShape, float hexCellSize)
	{
		HideAllSlot();
		HexagonHelper.GenerateHexShapeLayout(_hexImages, slot, base.rectTransform, hexShape, hexCellSize);
	}

	public void PlayAddedSlotEffects(IEnumerable<Vector2Int> points, float HexCellSize)
	{
		PlaySlotEffects(points, HexCellSize, isAdded: true);
	}

	public void PlayRemovedSlotEffects(IEnumerable<Vector2Int> points, float HexCellSize)
	{
		PlaySlotEffects(points, HexCellSize, isAdded: false);
	}

	public void ClearSlotEffects()
	{
		foreach (Sequence value in _effectSequences.Values)
		{
			value?.Kill();
		}
		_effectSequences.Clear();
		foreach (Image value2 in _effectImages.Values)
		{
			if (value2 != null)
			{
				value2.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void Dispose()
	{
		ClearSlotEffects();
	}

	private void PlaySlotEffects(IEnumerable<Vector2Int> points, float hexCellSize, bool isAdded)
	{
		if (points == null)
		{
			return;
		}
		foreach (Vector2Int point in points)
		{
			Image effectImage = GetOrCreateEffectImage(point, hexCellSize);
			effectImage.gameObject.SetActive(value: true);
			effectImage.transform.SetAsLastSibling();
			effectImage.color = (isAdded ? AddedSlotEffectColor : RemovedSlotEffectColor);
			if (_effectSequences.TryGetValue(point, out var value))
			{
				value?.Kill();
			}
			Vector3 localScale = (isAdded ? (Vector3.one * 0.35f) : Vector3.one);
			Vector3 endValue = (isAdded ? (Vector3.one * 1.18f) : (Vector3.one * 0.88f));
			Vector3 lingerScale = (isAdded ? (Vector3.one * 1.04f) : (Vector3.one * 0.72f));
			Sequence sequence = DOTween.Sequence();
			effectImage.rectTransform.localScale = localScale;
			Color color = effectImage.color;
			color.a = (isAdded ? 0f : 0.9f);
			effectImage.color = color;
			if (isAdded)
			{
				sequence.Append(DOTweenModuleUI.DOFade(effectImage, 0.95f, 0.14f));
				sequence.Join(effectImage.rectTransform.DOScale(endValue, 0.24f).SetEase(Ease.OutBack));
				sequence.Append(DOTweenModuleUI.DOFade(effectImage, 0.24f, 0.28f));
				sequence.Join(effectImage.rectTransform.DOScale(lingerScale, 0.22f).SetEase(Ease.OutQuad));
			}
			else
			{
				sequence.Append(DOTweenModuleUI.DOFade(effectImage, 0.95f, 0.08f));
				sequence.Join(effectImage.rectTransform.DOScale(endValue, 0.16f).SetEase(Ease.OutQuad));
				sequence.Append(DOTweenModuleUI.DOFade(effectImage, 0.2f, 0.26f));
				sequence.Join(effectImage.rectTransform.DOScale(lingerScale, 0.2f).SetEase(Ease.OutQuad));
			}
			sequence.OnComplete(delegate
			{
				if (effectImage != null)
				{
					effectImage.color = (isAdded ? new Color(AddedSlotEffectColor.r, AddedSlotEffectColor.g, AddedSlotEffectColor.b, 0.24f) : new Color(RemovedSlotEffectColor.r, RemovedSlotEffectColor.g, RemovedSlotEffectColor.b, 0.2f));
					effectImage.rectTransform.localScale = lingerScale;
				}
				_effectSequences.Remove(point);
			});
			sequence.Play();
			_effectSequences[point] = sequence;
		}
	}

	private Image GetOrCreateEffectImage(Vector2Int point, float hexCellSize)
	{
		if (_effectImages.TryGetValue(point, out var value) && value != null)
		{
			value.rectTransform.localPosition = HexagonHelper.GetHexPointPosition(point, hexCellSize);
			value.rectTransform.sizeDelta = Vector2.one * (hexCellSize * 2f);
			return value;
		}
		value = Object.Instantiate(slot, base.rectTransform);
		value.raycastTarget = false;
		value.transform.localPosition = HexagonHelper.GetHexPointPosition(point, hexCellSize);
		value.rectTransform.sizeDelta = Vector2.one * (hexCellSize * 2f);
		value.gameObject.SetActive(value: false);
		_effectImages[point] = value;
		return value;
	}
}
