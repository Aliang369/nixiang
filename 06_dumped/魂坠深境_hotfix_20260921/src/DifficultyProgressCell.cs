using System;
using System.Collections;
using Coffee.UIExtensions;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyProgressCell : CellUI<DifficultyProgressCell.Data>
{
	public class Data
	{
		public int index;

		public bool isObtained;

		public DifficultyLevel? difficultyLevel;

		public Transform effectPivot;
	}

	[SerializeField]
	public VerticalGradientImage icon;

	[SerializeField]
	public Color32 iconObtainedTopColor;

	[SerializeField]
	public Color32 iconObtainedButtonColor;

	[SerializeField]
	public Color32 iconUnobtainedColor;

	[SerializeField]
	public Color32 textObtainedTopColor;

	[SerializeField]
	public Color32 textObtainedButtonColor;

	[SerializeField]
	public Color32 textUnobtainedColor;

	[SerializeField]
	public Image background;

	[SerializeField]
	public Color32 backgroundObtainedColor;

	[SerializeField]
	public Color32 backgroundUnobtainedColor;

	[SerializeField]
	private TextMeshProUGUI difficultyNameText;

	[SerializeField]
	private UIParticle effect;

	[SerializeField]
	private float upgradeAnimDuration = 0.35f;

	[NonSerialized]
	private PrefabPool<UIParticle> effectPool;

	private Sequence upgradeSequence;

	protected override void Init()
	{
		if ((UnityEngine.Object)(object)effect != null)
		{
			effectPool = new PrefabPool<UIParticle>(effect, base.transform);
		}
	}

	protected override void OnSetup(Data _)
	{
		icon.SetGradientColors(base.setupData.isObtained ? iconObtainedTopColor : iconUnobtainedColor, base.setupData.isObtained ? iconObtainedButtonColor : iconUnobtainedColor);
		background.color = (base.setupData.isObtained ? backgroundObtainedColor : backgroundUnobtainedColor);
		difficultyNameText.colorGradient = (base.setupData.isObtained ? new VertexGradient(textObtainedTopColor, textObtainedTopColor, textObtainedButtonColor, textObtainedButtonColor) : new VertexGradient(textUnobtainedColor));
		string text = "";
		Data data = base.setupData;
		if (data != null && data.difficultyLevel.HasValue)
		{
			DifficultyInfo difficultyInfo = CellUI<Data>.applicationState.difficultyManager.GetDifficultyInfo(base.setupData.difficultyLevel.Value);
			text = ((difficultyInfo != null) ? difficultyInfo.difficultyName : "");
		}
		difficultyNameText.text = text;
		difficultyNameText.gameObject.SetActive(!string.IsNullOrEmpty(text));
	}

	public IEnumerator UpgradeAnim()
	{
		upgradeSequence?.Kill();
		upgradeSequence = DOTween.Sequence();
		icon.SetGradientColors(iconUnobtainedColor, iconUnobtainedColor);
		background.color = backgroundUnobtainedColor;
		PlayEffect();
		upgradeSequence.Join(DOTween.To(() => icon.topColor, icon.SetTopColor, iconObtainedTopColor, upgradeAnimDuration));
		upgradeSequence.Join(DOTween.To(() => icon.bottomColor, icon.SetBottomColor, iconObtainedButtonColor, upgradeAnimDuration));
		upgradeSequence.Join(DOTweenModuleUI.DOColor(background, (Color)backgroundObtainedColor, upgradeAnimDuration));
		upgradeSequence.Play();
		yield return upgradeSequence.WaitForCompletion();
		base.setupData.isObtained = true;
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
			Vector3 position = icon.transform.position;
			effectPool.DespawnAll();
			effectPool.DestroyInActive();
			UIParticle obj = effectPool.Spawn(base.setupData.effectPivot);
			((Component)(object)obj).transform.position = position;
			((Component)(object)obj).gameObject.SetActive(value: true);
		}
	}
}
