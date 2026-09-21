using System;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WillPowerProgressBarDisplay : UIBase, IFrameUpdate
{
	private const string NestActivityTitle = "巢穴活性";

	private static readonly int SineGlowColorId = Shader.PropertyToID("_SineGlowColor");

	private static readonly int SineGlowFadeId = Shader.PropertyToID("_SineGlowFade");

	private static readonly int SineGlowFrequencyId = Shader.PropertyToID("_SineGlowFrequency");

	[SerializeField]
	private TextMeshProUGUI titleLabel;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private List<Image> segments = new List<Image>();

	[SerializeField]
	private Color normalSegmentColor = new Color(0.96f, 0.83f, 0.47f, 1f);

	[SerializeField]
	private Color overLimitSegmentColor = new Color(0.9f, 0.22f, 0.18f, 1f);

	[SerializeField]
	private Image breathEffectImage;

	[SerializeField]
	private Material breathEffectMaterial;

	[SerializeField]
	private bool autoCreateBreathEffect = true;

	[SerializeField]
	private Vector2 breathEffectSizePadding = Vector2.zero;

	[SerializeField]
	private Color normalBreathColor = new Color(1f, 0.86f, 0.42f, 1f);

	[SerializeField]
	private Color overLimitBreathColor = new Color(1f, 0.16f, 0.08f, 1f);

	[SerializeField]
	private float normalBreathAlpha = 0.18f;

	[SerializeField]
	private Vector2 overLimitBreathAlphaRange = new Vector2(0.24f, 0.48f);

	[SerializeField]
	private float normalBreathFrequency = 1f;

	[SerializeField]
	private Vector2 overLimitBreathFrequencyRange = new Vector2(2.2f, 5f);

	[SerializeField]
	private float normalBreathFade = 0.35f;

	[SerializeField]
	private Vector2 overLimitBreathFadeRange = new Vector2(0.55f, 1f);

	private readonly List<Image> backgroundSegments = new List<Image>();

	private int templateSegmentCount;

	private float templateSegmentWidth;

	private Material breathEffectMaterialInstance;

	private int lastNestActivity = int.MinValue;

	private int lastNestActivityMaximum = int.MinValue;

	private Sequence nestActivityReduceSequence;

	private const float NestActivitySegmentAnimationDuration = 0.4f;

	private const float NestActivitySegmentAnimationInterval = 0.2f;

	private int displayedUsedWillPower = int.MinValue;

	private int displayedMaxWillPower = int.MinValue;

	private SpiderContractRuleDef SpiderRule => UIBase.gameState?.contractState?.selectedContract?.contractDef?.contractRule as SpiderContractRuleDef;

	private bool IsSpiderContract => SpiderRule != null;

	protected override void Init()
	{
		RefreshTitle();
		EnsureBreathEffect();
		if (UIBase.gameState?.gameEventManager != null)
		{
			UIBase.gameState.gameEventManager.onTriggerGameEvent += _OnGameEvent;
		}
		SetDirty();
	}

	protected override void Dispose()
	{
		nestActivityReduceSequence?.Kill();
		if (UIBase.gameState?.gameEventManager != null)
		{
			UIBase.gameState.gameEventManager.onTriggerGameEvent -= _OnGameEvent;
		}
		if (breathEffectMaterialInstance != null)
		{
			if (breathEffectImage != null && breathEffectImage.material == breathEffectMaterialInstance)
			{
				breathEffectImage.material = null;
			}
			UnityEngine.Object.Destroy(breathEffectMaterialInstance);
			breathEffectMaterialInstance = null;
		}
	}

	protected override void UpdateDisplay()
	{
		RefreshTitle();
		GetDisplayValue(out var value, out var maximum);
		UpdateValue(value, maximum);
	}

	private void UpdateValue(int usedWillPower, int maxWillPower)
	{
		maxWillPower = Mathf.Max(maxWillPower, 0);
		usedWillPower = Mathf.Max(usedWillPower, 0);
		displayedUsedWillPower = usedWillPower;
		displayedMaxWillPower = maxWillPower;
		if (label != null)
		{
			string text = ((usedWillPower > maxWillPower) ? usedWillPower.ToString().ColorText(overLimitSegmentColor) : usedWillPower.ToString());
			label.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(text, maxWillPower);
		}
		int num = Mathf.Min(usedWillPower, maxWillPower);
		int num2 = Mathf.Max(usedWillPower - maxWillPower, 0);
		int visibleCount = maxWillPower;
		EnsureSegmentCount(backgroundSegments, visibleCount);
		EnsureSegmentCount(segments, visibleCount);
		RefreshSegmentLayout(backgroundSegments, visibleCount);
		RefreshSegmentLayout(segments, visibleCount);
		for (int i = 0; i < segments.Count; i++)
		{
			Image image = segments[i];
			if (!(image == null))
			{
				bool flag = i < num;
				image.gameObject.SetActive(flag);
				if (flag)
				{
					ResetSegmentVisual(image);
					image.color = ((i < num2) ? overLimitSegmentColor : (IsSpiderContract ? overLimitSegmentColor : normalSegmentColor));
				}
			}
		}
		UpdateBreathEffect(usedWillPower, maxWillPower);
	}

	public void ConfigureTemporaryNestActivityFeedback()
	{
		if (label != null)
		{
			label.gameObject.SetActive(value: false);
		}
	}

	public void PlayNestActivityReduced(int oldActivity, int newActivity, Action onComplete = null)
	{
		if (!IsSpiderContract || oldActivity <= newActivity)
		{
			return;
		}
		GetDisplayValue(out var _, out var maximum);
		oldActivity = Mathf.Clamp(oldActivity, 0, maximum);
		newActivity = Mathf.Clamp(newActivity, 0, maximum);
		if (oldActivity <= newActivity)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		nestActivityReduceSequence?.Kill();
		UpdateValue(oldActivity, maximum);
		lastNestActivity = newActivity;
		lastNestActivityMaximum = maximum;
		nestActivityReduceSequence = DOTween.Sequence();
		for (int num = oldActivity - 1; num >= newActivity; num--)
		{
			Image segment = segments[num];
			if (!(segment == null))
			{
				Vector2 originalPosition = segment.rectTransform.anchoredPosition;
				Sequence t = DOTween.Sequence().Append(DOTweenModuleUI.DOAnchorPosY(segment.rectTransform, -24f, 0.4f, false).SetRelative()).Join(DOTweenModuleUI.DOFade(segment, 0f, 0.4f))
					.Join(segment.rectTransform.DOScale(1.12f, 0.120000005f))
					.AppendCallback(delegate
					{
						segment.gameObject.SetActive(value: false);
						segment.rectTransform.anchoredPosition = originalPosition;
						ResetSegmentVisual(segment);
					});
				nestActivityReduceSequence.Insert((float)(oldActivity - 1 - num) * 0.2f, t);
			}
		}
		nestActivityReduceSequence.OnComplete(delegate
		{
			UpdateValue(newActivity, maximum);
			nestActivityReduceSequence = null;
			onComplete?.Invoke();
		});
	}

	public void PlayNestActivityIncreased(int oldActivity, int newActivity, Action onComplete = null)
	{
		if (!IsSpiderContract || oldActivity >= newActivity)
		{
			return;
		}
		GetDisplayValue(out var _, out var maximum);
		oldActivity = Mathf.Clamp(oldActivity, 0, maximum);
		newActivity = Mathf.Clamp(newActivity, 0, maximum);
		if (oldActivity >= newActivity)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		nestActivityReduceSequence?.Kill();
		UpdateValue(oldActivity, maximum);
		lastNestActivity = newActivity;
		lastNestActivityMaximum = maximum;
		nestActivityReduceSequence = DOTween.Sequence();
		for (int i = oldActivity; i < newActivity; i++)
		{
			Image image = segments[i];
			if (!(image == null))
			{
				Vector2 anchoredPosition = image.rectTransform.anchoredPosition;
				image.gameObject.SetActive(value: true);
				image.rectTransform.anchoredPosition = anchoredPosition + Vector2.down * 24f;
				image.rectTransform.localScale = Vector3.one * 0.88f;
				Color color = image.color;
				color.a = 0f;
				image.color = color;
				Sequence t = DOTween.Sequence().Append(DOTweenModuleUI.DOAnchorPos(image.rectTransform, anchoredPosition, 0.4f, false)).Join(DOTweenModuleUI.DOFade(image, 1f, 0.4f))
					.Join(image.rectTransform.DOScale(1f, 0.4f));
				nestActivityReduceSequence.Insert((float)(i - oldActivity) * 0.2f, t);
			}
		}
		nestActivityReduceSequence.OnComplete(delegate
		{
			UpdateValue(newActivity, maximum);
			nestActivityReduceSequence = null;
			onComplete?.Invoke();
		});
	}

	private static void ResetSegmentVisual(Image segment)
	{
		segment.rectTransform.localScale = Vector3.one;
		Color color = segment.color;
		color.a = 1f;
		segment.color = color;
	}

	private void EnsureBreathEffect()
	{
		if (breathEffectImage == null && autoCreateBreathEffect)
		{
			breathEffectImage = CreateBreathEffectImage();
		}
		if (!(breathEffectImage == null))
		{
			breathEffectImage.raycastTarget = false;
			if (breathEffectMaterial != null && breathEffectMaterialInstance == null)
			{
				breathEffectMaterialInstance = new Material(breathEffectMaterial)
				{
					name = breathEffectMaterial.name + " (WillPower Instance)"
				};
				breathEffectImage.material = breathEffectMaterialInstance;
			}
		}
	}

	private Image CreateBreathEffectImage()
	{
		if (breathEffectMaterial == null)
		{
			return null;
		}
		Image image = base.transform.Find("Bg")?.GetComponent<Image>();
		if (image == null || image.sprite == null)
		{
			return null;
		}
		RectTransform rectTransform = image.rectTransform;
		GameObject obj = new GameObject("BreathGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
		{
			layer = base.gameObject.layer
		};
		RectTransform component = obj.GetComponent<RectTransform>();
		component.SetParent(base.transform, worldPositionStays: false);
		component.SetSiblingIndex(rectTransform.GetSiblingIndex() + 1);
		component.anchorMin = rectTransform.anchorMin;
		component.anchorMax = rectTransform.anchorMax;
		component.pivot = rectTransform.pivot;
		component.anchoredPosition = rectTransform.anchoredPosition;
		component.sizeDelta = rectTransform.sizeDelta + breathEffectSizePadding;
		Image component2 = obj.GetComponent<Image>();
		component2.sprite = image.sprite;
		component2.type = image.type;
		component2.preserveAspect = image.preserveAspect;
		component2.fillCenter = image.fillCenter;
		component2.pixelsPerUnitMultiplier = image.pixelsPerUnitMultiplier;
		component2.raycastTarget = false;
		return component2;
	}

	public void FrameUpdate()
	{
		if (IsSpiderContract)
		{
			GetDisplayValue(out var value, out var maximum);
			if (value != lastNestActivity || maximum != lastNestActivityMaximum)
			{
				lastNestActivity = value;
				lastNestActivityMaximum = maximum;
				SetDirty();
			}
		}
		else
		{
			int valueOrDefault = (UIBase.gameState?.playerState?.CurrentUsedWillPower).GetValueOrDefault();
			int valueOrDefault2 = (UIBase.gameState?.playerState?.MaxWillPower).GetValueOrDefault();
			if (valueOrDefault != displayedUsedWillPower || valueOrDefault2 != displayedMaxWillPower)
			{
				UpdateValue(valueOrDefault, valueOrDefault2);
			}
		}
		if (breathEffectImage != null && breathEffectImage.gameObject.activeInHierarchy)
		{
			GetDisplayValue(out var value2, out var maximum2);
			UpdateBreathEffect(value2, maximum2);
		}
	}

	private void RefreshTitle()
	{
		if (titleLabel != null)
		{
			titleLabel.text = (IsSpiderContract ? "巢穴活性" : ScriptableObjectSingleton<TextDataPreset>.instance.term_wisdom);
		}
	}

	private void GetDisplayValue(out int value, out int maximum)
	{
		if (IsSpiderContract)
		{
			value = SpiderRule.GetNestActivityCurrent(UIBase.gameState);
			maximum = SpiderRule.GetNestActivityMaximum(UIBase.gameState);
		}
		else
		{
			value = (UIBase.gameState?.playerState?.CurrentUsedWillPower).GetValueOrDefault();
			maximum = (UIBase.gameState?.playerState?.MaxWillPower).GetValueOrDefault();
		}
	}

	private void UpdateBreathEffect(int usedWillPower, int maxWillPower)
	{
		EnsureBreathEffect();
		if (!(breathEffectImage == null) && !(breathEffectMaterialInstance == null))
		{
			int num = PlayerGameStateInfo.CalculateMissingWillPowerLevel(usedWillPower, maxWillPower);
			float t = ((PlayerGameStateInfo.MaxMissingWillPowerLevel <= 0) ? 0f : Mathf.Clamp01((float)num / (float)PlayerGameStateInfo.MaxMissingWillPowerLevel));
			bool num2 = num > 0;
			Color color = (num2 ? overLimitBreathColor : normalBreathColor);
			float a = (num2 ? Mathf.Lerp(overLimitBreathAlphaRange.x, overLimitBreathAlphaRange.y, t) : normalBreathAlpha);
			float value = (num2 ? Mathf.Lerp(overLimitBreathFrequencyRange.x, overLimitBreathFrequencyRange.y, t) : normalBreathFrequency);
			float value2 = (num2 ? Mathf.Lerp(overLimitBreathFadeRange.x, overLimitBreathFadeRange.y, t) : normalBreathFade);
			breathEffectImage.color = new Color(color.r, color.g, color.b, a);
			breathEffectMaterialInstance.SetColor(SineGlowColorId, new Color(color.r * 1.5f, color.g * 1.5f, color.b * 1.5f, 0f));
			breathEffectMaterialInstance.SetFloat(SineGlowFrequencyId, value);
			breathEffectMaterialInstance.SetFloat(SineGlowFadeId, value2);
			breathEffectImage.gameObject.SetActive(maxWillPower > 0);
		}
	}

	private void EnsureSegmentCount(List<Image> targetSegments, int visibleCount)
	{
		CacheBackgroundSegments();
		CacheTemplateSegmentSize();
		if (targetSegments.Count != 0)
		{
			while (targetSegments.Count < visibleCount)
			{
				Image image = targetSegments[targetSegments.Count - 1];
				Image image2 = UnityEngine.Object.Instantiate(image, image.transform.parent);
				image2.name = image.name;
				targetSegments.Add(image2);
			}
		}
	}

	private void RefreshSegmentLayout(List<Image> targetSegments, int visibleCount)
	{
		if (targetSegments.Count == 0)
		{
			return;
		}
		float x = CalculateSegmentWidth(visibleCount);
		for (int i = 0; i < targetSegments.Count; i++)
		{
			Image image = targetSegments[i];
			if (!(image == null))
			{
				image.gameObject.SetActive(i < visibleCount);
				image.rectTransform.sizeDelta = new Vector2(x, image.rectTransform.sizeDelta.y);
			}
		}
	}

	private float CalculateSegmentWidth(int visibleCount)
	{
		if (visibleCount <= 0 || templateSegmentCount <= 0 || templateSegmentWidth <= 0f)
		{
			return templateSegmentWidth;
		}
		float num = templateSegmentWidth * (float)templateSegmentCount;
		return Mathf.Min(templateSegmentWidth, num / (float)visibleCount);
	}

	private void CacheBackgroundSegments()
	{
		if (backgroundSegments.Count > 0)
		{
			return;
		}
		Transform transform = base.transform.Find("ProgressBg");
		if (transform == null)
		{
			return;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			Image component = transform.GetChild(i).GetComponent<Image>();
			if (component != null)
			{
				backgroundSegments.Add(component);
			}
		}
	}

	private void CacheTemplateSegmentSize()
	{
		if (templateSegmentCount <= 0 || !(templateSegmentWidth > 0f))
		{
			templateSegmentCount = Mathf.Max(segments.Count, backgroundSegments.Count);
			if (segments.Count > 0 && segments[0] != null)
			{
				templateSegmentWidth = segments[0].rectTransform.sizeDelta.x;
			}
			else if (backgroundSegments.Count > 0 && backgroundSegments[0] != null)
			{
				templateSegmentWidth = backgroundSegments[0].rectTransform.sizeDelta.x;
			}
		}
	}

	private void _OnGameEvent(GameEvent gameEvent)
	{
		GameEvent.Type type = gameEvent.type;
		if (type == GameEvent.Type.AddInfluence || type == GameEvent.Type.RemoveInfluence || type == GameEvent.Type.ChangeWillPower)
		{
			SetDirty();
		}
	}
}
