using System;
using Common.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public class ConstellationDetailPanel : CellUI<Constellation>, IDisplayModulesController, IFrameUpdate
{
	public class DisplayModule : UIDisplayModule
	{
		protected ConstellationDetailPanel constellationDetailPanel => parent as ConstellationDetailPanel;

		protected Constellation constellation => constellationDetailPanel.setupData;
	}

	[SerializeField]
	public ConstellationRewardDetailPanel constellationRewardDetailPanel;

	[SerializeField]
	private RectTransform constellationPivot;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI finishProgresstext;

	[SerializeField]
	private TextMeshProUGUI constellationEndTimeText;

	[SerializeField]
	private TextMeshProUGUI constellationRemainTimeText;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Transform allFinishRewardDisplayArea;

	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private RectTransform rewardArea;

	[NonSerialized]
	public Constellation constellationInstance;

	[NonSerialized]
	public ChallengeStar focusStar;

	[NonSerialized]
	public CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> rewardDisplayPool;

	private float nextTimeRefresh;

	private Constellation prefab => base.setupData;

	protected override void Init()
	{
		InitUI(constellationRewardDetailPanel);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		CellUI<Constellation>.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		GetPrefabPool(ref rewardDisplayPool, globalItemDisplay);
	}

	protected override void Dispose()
	{
		CellUI<Constellation>.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		text.text = constellationInstance.constellationName;
		description.text = constellationInstance.constellationDescription;
		finishProgresstext.text = constellationInstance.GetNumText();
		allFinishRewardDisplayArea.gameObject.SetActive(constellationInstance.reward != null);
		rewardArea.gameObject.SetActive(value: false);
		if (!(constellationInstance?.reward?.rewards).IsNullOrEmpty())
		{
			rewardArea.gameObject.SetActive(value: true);
			rewardDisplayPool.Sync(constellationInstance.reward.rewards.ConvertAll((Converter<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)));
		}
		UpdateTimeDisplay(DateTime.UtcNow);
	}

	public void FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy && !(Time.unscaledTime < nextTimeRefresh))
		{
			nextTimeRefresh = Time.unscaledTime + 1f;
			UpdateTimeDisplay(DateTime.UtcNow);
		}
	}

	private void UpdateTimeDisplay(DateTime utcNow)
	{
		UnityDateTime? unityDateTime = constellationInstance?.GetExpireDateTime();
		if (!unityDateTime.HasValue)
		{
			constellationEndTimeText.text = "";
			constellationRemainTimeText.text = "";
			return;
		}
		DateTime utcDateTime = unityDateTime.Value.UtcDateTime;
		constellationEndTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_contract_end_date_time_arg0.FormatWith(utcDateTime.ToLocalTime().ToString("yyyy/MM/dd"));
		constellationRemainTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_contract_remain_time_arg0_to_expire.FormatWith(TextDisplayHelper.GetAdaptiveCountdownText(utcDateTime - utcNow));
	}

	protected override void OnSetup(Constellation setupData)
	{
		base.gameObject.SetActive(value: true);
		if (constellationInstance != null)
		{
			DisposeUI(constellationInstance);
			UnityEngine.Object.Destroy(constellationInstance.gameObject);
		}
		focusStar = null;
		constellationInstance = UnityEngine.Object.Instantiate(prefab, constellationPivot);
		InitUI(constellationInstance);
		constellationRewardDetailPanel.gameObject.SetActive(value: false);
		UpdateDisplay();
		SetDirty();
	}

	protected override void OnUnset()
	{
		focusStar = null;
	}

	public void Focus(ChallengeStar star)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (focusStar != null && focusStar != star)
		{
			UnFocus();
		}
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_stargaze_star_select);
		focusStar = star;
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = star.starImage.transform.DOScale(ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.focusStarScale, ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.focusDuration);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(SetDirty));
		SetDirty();
	}

	public void UnFocus()
	{
		if (!(focusStar == null))
		{
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = focusStar.starImage.transform.DOScale(Vector3.one, ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.focusDuration);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(SetDirty));
			focusStar = null;
			SetDirty();
		}
	}

	private void _OnBackButtonClicked()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
		manager.SetDirty();
	}

	private void _OnEmptyAreaClicked()
	{
		UnFocus();
	}
}
