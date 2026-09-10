using System;
using System.Collections;
using Coffee.UIExtensions;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonProgressCell : CellUI<SeasonProgressCell.Data>
{
	public class Data
	{
		public int index;

		public bool completed;

		public bool current;

		public int currentExp;

		public int maxExp;

		public bool animate;

		public bool fromCompleted;

		public bool toCompleted;

		public bool toCurrent;

		public int fromExp;

		public int toExp;

		public Transform effectPivot;

		public Action<Vector3> onProgressPointChanged;
	}

	[SerializeField]
	private VerticalGradientImage image;

	[SerializeField]
	private Color completedColorTop = Color.white;

	[SerializeField]
	private Color completedColorBottom = Color.white;

	[SerializeField]
	private Color uncompletedColor = Color.gray;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Color completedBackgroundColor = Color.white;

	[SerializeField]
	private Color uncompletedBackgroundColor = Color.gray;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private UIParticle effect;

	[SerializeField]
	private float upgradeAnimDuration = 0.45f;

	[NonSerialized]
	private PrefabPool<UIParticle> effectPool;

	private Sequence upgradeSequence;

	private int displayExp;

	protected override void Init()
	{
		if ((UnityEngine.Object)(object)effect != null)
		{
			effectPool = new PrefabPool<UIParticle>(effect, base.transform);
		}
	}

	protected override void OnSetup(Data setupData)
	{
		bool completed = setupData.completed;
		image.SetGradientColors(completed ? completedColorTop : uncompletedColor, completed ? completedColorBottom : uncompletedColor);
		background.color = (completed ? completedBackgroundColor : uncompletedBackgroundColor);
		if (setupData.completed)
		{
			slider.value = 1f;
		}
		else if (!setupData.current || setupData.maxExp <= 0)
		{
			slider.value = 0f;
		}
		else
		{
			slider.value = Mathf.Clamp01((float)setupData.currentExp / (float)setupData.maxExp);
		}
		progressText.gameObject.SetActive(setupData.current);
		if (setupData.current)
		{
			progressText.text = $"{setupData.currentExp}/{setupData.maxExp}";
		}
	}

	public IEnumerator UpgradeAnim(bool playEffect)
	{
		if (base.setupData.animate)
		{
			upgradeSequence?.Kill();
			upgradeSequence = DOTween.Sequence();
			displayExp = base.setupData.fromExp;
			progressText.gameObject.SetActive(value: true);
			progressText.text = $"{displayExp}/{base.setupData.maxExp}";
			slider.value = ((base.setupData.maxExp <= 0) ? 0f : Mathf.Clamp01((float)displayExp / (float)base.setupData.maxExp));
			base.setupData.onProgressPointChanged?.Invoke(GetProgressPointWorldPosition());
			image.SetGradientColors(base.setupData.fromCompleted ? completedColorTop : uncompletedColor, base.setupData.fromCompleted ? completedColorBottom : uncompletedColor);
			background.color = (base.setupData.fromCompleted ? completedBackgroundColor : uncompletedBackgroundColor);
			if (playEffect)
			{
				PlayEffect();
			}
			upgradeSequence.Join(DOTween.To(() => image.topColor, image.SetTopColor, completedColorTop, upgradeAnimDuration));
			upgradeSequence.Join(DOTween.To(() => image.bottomColor, image.SetBottomColor, completedColorBottom, upgradeAnimDuration));
			upgradeSequence.Join(DOTweenModuleUI.DOColor(background, completedBackgroundColor, upgradeAnimDuration));
			upgradeSequence.Join(DOTween.To(() => displayExp, delegate(int value)
			{
				displayExp = value;
				slider.value = ((base.setupData.maxExp <= 0) ? 0f : Mathf.Clamp01((float)displayExp / (float)base.setupData.maxExp));
				progressText.text = $"{displayExp}/{base.setupData.maxExp}";
				base.setupData.onProgressPointChanged?.Invoke(GetProgressPointWorldPosition());
			}, base.setupData.toExp, upgradeAnimDuration));
			upgradeSequence.Play();
			yield return upgradeSequence.WaitForCompletion();
			base.setupData.completed = base.setupData.toCompleted;
			base.setupData.current = base.setupData.toCurrent;
			base.setupData.currentExp = base.setupData.toExp;
			OnSetup(base.setupData);
		}
	}

	protected override void OnUnset()
	{
		upgradeSequence?.Kill();
		upgradeSequence = null;
		effectPool?.DespawnAll();
		effectPool?.DestroyInActive();
	}

	private void PlayEffect()
	{
		if (!((UnityEngine.Object)(object)effect == null) && effectPool != null)
		{
			Vector3 position = image.transform.position;
			effectPool.DespawnAll();
			effectPool.DestroyInActive();
			UIParticle obj = effectPool.Spawn(base.setupData.effectPivot);
			((Component)(object)obj).transform.position = position;
			((Component)(object)obj).gameObject.SetActive(value: true);
		}
	}

	private Vector3 GetProgressPointWorldPosition()
	{
		if (slider == null)
		{
			return base.rectTransform.TransformPoint(base.rectTransform.rect.center);
		}
		RectTransform component = slider.GetComponent<RectTransform>();
		if (component == null)
		{
			return base.rectTransform.TransformPoint(base.rectTransform.rect.center);
		}
		float normalizedValue = slider.normalizedValue;
		Rect rect = component.rect;
		Vector2 center = rect.center;
		switch (slider.direction)
		{
		case Slider.Direction.RightToLeft:
			center.x = Mathf.Lerp(rect.xMax, rect.xMin, normalizedValue);
			break;
		case Slider.Direction.BottomToTop:
			center.y = Mathf.Lerp(rect.yMin, rect.yMax, normalizedValue);
			break;
		case Slider.Direction.TopToBottom:
			center.y = Mathf.Lerp(rect.yMax, rect.yMin, normalizedValue);
			break;
		default:
			center.x = Mathf.Lerp(rect.xMin, rect.xMax, normalizedValue);
			break;
		}
		return component.TransformPoint(center);
	}
}
