using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StanceDisplay : UIBase
{
	public class StanceViewState
	{
		public bool showHint;

		public bool isPreview;

		public bool isUncertain;

		public string nameText;

		public Color nameColor;

		public string valueText;

		public Color valueColor;
	}

	public GameObject stanceHint;

	public GameButton stanceButton;

	public TextMeshProUGUI stanceNameText;

	public TextMeshProUGUI stanceValueText;

	public TextMeshProUGUI stanceTriggerText;

	public CanvasGroup stanceHintCanvasGroup;

	public Stance cachedStance;

	private Stance previewStance;

	private StanceViewState stanceViewState;

	private Effect cachedSpellNode;

	[Tooltip("How many flashes should occur for each trigger effect.")]
	[SerializeField]
	private int triggerFlashCount = 2;

	[Tooltip("Seconds for a single fade in / fade out step.")]
	[SerializeField]
	private float triggerFlashInterval = 0.15f;

	private readonly Queue<Stance.ChangeType> _effectQueue = new Queue<Stance.ChangeType>();

	private bool _isPlayingEffect;

	protected override void Init()
	{
		stanceButton.onClick.AddListener(_OnStanceButtonClicked);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeChangeStance, _OnBeforeChangeStance);
		stanceViewState = new StanceViewState
		{
			showHint = false,
			isPreview = false,
			isUncertain = false,
			nameText = string.Empty,
			nameColor = Color.white,
			valueText = string.Empty,
			valueColor = Color.white
		};
	}

	protected override void Dispose()
	{
		cachedStance = null;
		stanceButton.onClick.RemoveListener(_OnStanceButtonClicked);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeChangeStance, _OnBeforeChangeStance);
	}

	private void _OnBeforeChangeStance(GameEvent.Type type, GameEvent eve)
	{
		eve.GetEveObj<Stance>(GameEvent.EveObjType.Origin);
		Stance eveObj = eve.GetEveObj<Stance>(GameEvent.EveObjType.Destination);
		eve.GetEveObj<Stance>(GameEvent.EveObjType.Target);
		ApplyStance(eveObj);
		string stringValue = eve.stringValue;
		if (!Enum.TryParse<Stance.ChangeType>(stringValue, out var _))
		{
			Debug.LogError($"无效的架势类型: {stringValue} 来自 {eve.SourceItem}");
		}
	}

	private void Update()
	{
		UpdatePreviewStance();
		UpdatePreviewStanceFade();
	}

	private void UpdatePreviewStanceFade()
	{
		if (stanceViewState.isPreview)
		{
			stanceHintCanvasGroup.alpha = 0.5f;
		}
		else
		{
			stanceHintCanvasGroup.alpha = 1f;
		}
	}

	private void UpdatePreviewStance()
	{
		Effect effect = UIBase.gameState.UIManager.playerItemArea.controlInfo.spellItem?.def.defaultSpell?.spellNode;
		if (cachedSpellNode != effect)
		{
			cachedSpellNode = effect;
			CastSpellNode(effect);
		}
		void CastSpellNode(Effect spellNode)
		{
			if (spellNode == null)
			{
				stanceViewState.isPreview = false;
			}
			else
			{
				List<ChangeStance> descendantNodeOfType = spellNode.GetDescendantNodeOfType<ChangeStance>(includeSelf: true);
				if (descendantNodeOfType.Count == 0)
				{
					stanceViewState.isPreview = false;
				}
				else if (descendantNodeOfType.Count > 1)
				{
					stanceViewState.isUncertain = true;
				}
				else
				{
					ChangeStance changeStance = null;
					using (List<ChangeStance>.Enumerator enumerator = descendantNodeOfType.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							changeStance = enumerator.Current;
						}
					}
					foreach (IfElseCastNode item in spellNode.GetDescendantNodeOfType<IfElseCastNode>())
					{
						if (item.CheckDescendantContains(changeStance))
						{
							stanceViewState.isUncertain = true;
							return;
						}
					}
					GameEvent eve = UIBase.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo.GenerateSpellCastEvent(GameEvent.TriggerType.InBattleDisplay);
					NodeDisplayInfo nodeDisplayInfo = changeStance.GetNodeDisplayInfo(eve);
					if (nodeDisplayInfo.refStance == null)
					{
						ApplyNullStance();
					}
					else
					{
						bool showHint = stanceViewState.showHint;
						if (nodeDisplayInfo.refStance.valid)
						{
							stanceViewState.isUncertain = false;
							stanceViewState.isPreview = true;
							if (!showHint)
							{
								stanceViewState.showHint = true;
							}
							previewStance = Stance.Cast(cachedStance, nodeDisplayInfo.refStance.value, out var _, preview: true);
							ApplyPreview();
						}
						else
						{
							if (!showHint)
							{
								stanceViewState.showHint = false;
							}
							stanceViewState.isUncertain = true;
						}
					}
				}
			}
		}
	}

	protected override void UpdateDisplay()
	{
		if (cachedStance != null)
		{
			ApplyStance(cachedStance, setDirty: false);
		}
		else
		{
			stanceViewState.showHint = false;
			ApplyNullStance(setDirty: false);
		}
		if (stanceViewState.isPreview)
		{
			stanceViewState.showHint = true;
			ApplyPreview(setDirty: false);
			if (stanceViewState.isUncertain)
			{
				ApplyUncertainPreview(setDirty: false);
				stanceTriggerText.gameObject.SetActive(value: false);
			}
		}
		if (stanceViewState.showHint)
		{
			stanceHint.SetActive(value: true);
			stanceNameText.text = stanceViewState.nameText;
			stanceNameText.color = stanceViewState.nameColor;
			stanceValueText.text = stanceViewState.valueText;
			stanceValueText.color = stanceViewState.valueColor;
		}
		else
		{
			stanceHint.SetActive(value: false);
		}
	}

	private void _OnStanceButtonClicked()
	{
		CommonInfoData data = new CommonInfoData(UIBase.gameState.stanceManager.currentStance, CommonInfoData.ControlType.Click, stanceHint.GetComponent<RectTransform>(), false);
		UIBase.gameState.UIManager.universalCommonInfoPanel.AppendInfo(data);
	}

	private void _OnGameEvent(GameEvent.Type type, GameEvent eve)
	{
		cachedStance = UIBase.gameState.stanceManager.currentStance;
		SetDirty();
	}

	public void ApplyPreview(bool setDirty = true)
	{
		stanceViewState.isPreview = true;
		string text;
		if (previewStance == null)
		{
			ApplyNullStance();
			text = TextDisplayHelper.GetStanceChangeText(Stance.ChangeType.Break);
		}
		else
		{
			stanceViewState.nameText = previewStance.def.stanceShortName;
			stanceViewState.nameColor = previewStance.def.stanceColor;
			stanceViewState.valueText = previewStance.value + " / " + UIBase.gameState.stanceManager.stanceMaxValue;
			stanceViewState.valueColor = previewStance.def.stanceColor;
			text = _GetTriggerText(previewStance);
		}
		if (text != null)
		{
			stanceTriggerText.text = text;
			stanceTriggerText.gameObject.SetActive(value: true);
		}
		else
		{
			stanceTriggerText.gameObject.SetActive(value: false);
		}
		if (setDirty)
		{
			SetDirty();
		}
	}

	public void ApplyStance(Stance stance, bool setDirty = true)
	{
		stanceTriggerText.gameObject.SetActive(value: false);
		cachedStance = stance;
		if (stance == null || stance.def == null || stance.value <= 0)
		{
			stanceViewState.showHint = false;
			return;
		}
		stanceViewState.showHint = true;
		stanceViewState.nameText = stance.def.stanceShortName;
		stanceViewState.nameColor = stance.def.stanceColor;
		stanceViewState.valueText = stance.value + " / " + UIBase.gameState.stanceManager.stanceMaxValue;
		stanceViewState.valueColor = stance.def.stanceColor;
		if (setDirty)
		{
			SetDirty();
		}
	}

	public void ApplyUncertainPreview(bool setDirty = true)
	{
		stanceViewState.isPreview = true;
		stanceViewState.isUncertain = true;
		stanceViewState.nameText = "???";
		stanceViewState.nameColor = Color.white;
		stanceViewState.valueText = "?";
		stanceViewState.valueColor = Color.white;
		if (setDirty)
		{
			SetDirty();
		}
	}

	public void ApplyNullStance(bool setDirty = true)
	{
		stanceViewState.nameText = string.Empty;
		stanceViewState.nameColor = Color.white;
		stanceViewState.valueText = string.Empty;
		stanceViewState.valueColor = Color.white;
		if (setDirty)
		{
			SetDirty();
		}
	}

	public void TriggerAbilityAnim(Stance stance, Ability.AbilityAnimPriority priority, Stance.ChangeType stanceChangeType = Stance.ChangeType.None)
	{
		if (stance != null && !(stance.def == null))
		{
			EnqueueDisplayEffect(stanceChangeType, Coroutine);
		}
		IEnumerator Coroutine()
		{
			_isPlayingEffect = true;
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Spell);
			if (priority >= Ability.AbilityAnimPriority.Weak)
			{
				UIBase.applicationState.runningGameState.StartCoroutine(() => PlayIconEffect(1));
			}
			if (priority >= Ability.AbilityAnimPriority.Medium)
			{
				CommonInfoData data = new CommonInfoData(UIBase.gameState.stanceManager.currentStance, CommonInfoData.ControlType.Click, stanceHint.GetComponent<RectTransform>(), false);
				UIBase.gameState.UIManager.universalCommonInfoPanel.AppendInfo(data);
				yield return new WaitForSeconds(0.6f);
				UIBase.gameState.UIManager.universalCommonInfoPanel.HideAll();
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Spell);
			_isPlayingEffect = false;
			yield return new WaitUntil(() => UIBase.gameState.actionManager.isEmpty);
			_TryPlayNextEffect(Coroutine);
		}
	}

	private IEnumerator PlayIconEffect(int times)
	{
		Image component = stanceHint.GetComponent<Image>();
		stanceHint.GetComponentInChildren<Image>();
		List<Image> images = new List<Image>();
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < times; i++)
		{
			Image image = UnityEngine.Object.Instantiate(component, base.transform);
			image.gameObject.SetActive(value: true);
			images.Add(image);
			RectTransform rectTransform = image.rectTransform;
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image.rectTransform.DOScale(new Vector3(2f, 2f, 1f), 1f));
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOAnchorPosY(rectTransform, anchoredPosition.y + 100f, 1f, false).SetEase(Ease.OutQuad));
		}
		sequence.Play();
		SetDirty();
		yield return sequence.WaitForKill();
		foreach (Image item in images)
		{
			if ((bool)item)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	public void EnqueueDisplayEffect(Stance.ChangeType type, Func<IEnumerator> playEffectFunc)
	{
		_effectQueue.Enqueue(type);
		_TryPlayNextEffect(playEffectFunc);
	}

	private void _TryPlayNextEffect(Func<IEnumerator> playEffectFunc)
	{
		if (!_isPlayingEffect && _effectQueue.Count != 0)
		{
			_effectQueue.Dequeue();
			StartCoroutine(playEffectFunc());
		}
	}

	private IEnumerator CoPlayEffect(Stance stance)
	{
		_isPlayingEffect = true;
		string text = _GetTriggerText(stance);
		stanceTriggerText.text = text;
		stanceTriggerText.gameObject.SetActive(value: true);
		TMP_Text tempText = UnityEngine.Object.Instantiate(stanceTriggerText, stanceTriggerText.transform.parent);
		tempText.transform.position += Vector3.right * 50f;
		tempText.text = text;
		tempText.gameObject.SetActive(value: true);
		Color originalColor = stance.def.stanceColor;
		originalColor.a = 1f;
		tempText.color = originalColor;
		for (int i = 0; i < triggerFlashCount; i++)
		{
			yield return tempText.DOFade(0f, triggerFlashInterval).WaitForCompletion();
			yield return tempText.DOFade(1f, triggerFlashInterval).WaitForCompletion();
		}
		tempText.text = string.Empty;
		tempText.color = originalColor;
		SetDirty();
		_isPlayingEffect = false;
	}

	private void ShowTriggerText(string text)
	{
		stanceTriggerText.text = text;
		stanceTriggerText.gameObject.SetActive(value: true);
	}

	private string _GetTriggerText(Stance displayStance)
	{
		string result = null;
		bool num = displayStance.def == UIBase.gameState.stanceManager.currentStance?.def;
		bool flag = !num && UIBase.gameState.stanceManager.currentStance != null;
		bool flag2 = num && displayStance.value > UIBase.gameState.stanceManager.currentStance?.value;
		bool num2 = displayStance.value <= 0;
		bool flag3 = displayStance.value >= UIBase.gameState.stanceManager.stanceMaxValue;
		if (num2)
		{
			result = TextDisplayHelper.GetStanceChangeText(Stance.ChangeType.Break);
		}
		else if (flag)
		{
			result = TextDisplayHelper.GetStanceChangeText(Stance.ChangeType.Shift, flag3);
		}
		else if (flag2)
		{
			result = TextDisplayHelper.GetStanceChangeText(Stance.ChangeType.Stack, flag3);
		}
		else if (flag3)
		{
			result = TextDisplayHelper.GetStanceChangeText(Stance.ChangeType.Full);
		}
		return result;
	}
}
