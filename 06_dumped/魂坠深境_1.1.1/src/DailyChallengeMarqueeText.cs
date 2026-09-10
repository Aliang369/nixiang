using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class DailyChallengeMarqueeText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	[Min(1f)]
	private float speed = 36f;

	[SerializeField]
	[Min(0f)]
	private float endHoldSeconds = 1.2f;

	private float maxOffset;

	private float offset;

	private float direction = 1f;

	private float holdRemaining;

	private bool layoutDirty = true;

	public TextMeshProUGUI Label => label;

	public bool IsOverflowing => maxOffset > 0.5f;

	public void SetText(string value)
	{
		label.text = value ?? string.Empty;
		layoutDirty = true;
		ResetMotion();
	}

	private void OnEnable()
	{
		layoutDirty = true;
		ResetMotion();
	}

	private void OnRectTransformDimensionsChange()
	{
		layoutDirty = true;
		ResetMotion();
	}

	private void LateUpdate()
	{
		if (layoutDirty)
		{
			RecalculateOverflow();
		}
		if (!IsOverflowing)
		{
			return;
		}
		if (holdRemaining > 0f)
		{
			holdRemaining -= Time.unscaledDeltaTime;
			return;
		}
		offset += direction * speed * Time.unscaledDeltaTime;
		if (offset >= maxOffset)
		{
			offset = maxOffset;
			direction = -1f;
			holdRemaining = endHoldSeconds;
		}
		else if (offset <= 0f)
		{
			offset = 0f;
			direction = 1f;
			holdRemaining = endHoldSeconds;
		}
		ApplyOffset();
	}

	private void RecalculateOverflow()
	{
		layoutDirty = false;
		RectTransform rectTransform = base.transform as RectTransform;
		if (rectTransform == null || rectTransform.rect.width <= 0f)
		{
			layoutDirty = true;
			return;
		}
		label.enableWordWrapping = false;
		label.overflowMode = TextOverflowModes.Overflow;
		label.ForceMeshUpdate();
		float num = Mathf.Max(rectTransform.rect.width, Mathf.Ceil(label.preferredWidth));
		content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
		maxOffset = Mathf.Max(0f, num - rectTransform.rect.width);
		offset = Mathf.Clamp(offset, 0f, maxOffset);
		ApplyOffset();
	}

	private void ResetMotion()
	{
		offset = 0f;
		direction = 1f;
		holdRemaining = endHoldSeconds;
		ApplyOffset();
	}

	private void ApplyOffset()
	{
		if (!(content == null))
		{
			Vector2 anchoredPosition = content.anchoredPosition;
			anchoredPosition.x = 0f - offset;
			content.anchoredPosition = anchoredPosition;
		}
	}
}
