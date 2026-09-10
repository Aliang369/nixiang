using System;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldEventItemChoiceDisplay : CellUI<WorldEventChoice>
{
	[SerializeField]
	private float tweenTime;

	[SerializeField]
	private GameButton button;

	[SerializeField]
	private RectTransform displayObject;

	[SerializeField]
	private TextMeshProUGUI eventNameText;

	[SerializeField]
	private TextMeshProUGUI eventDescriptionText;

	[SerializeField]
	private RectTransform referenceParent;

	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private InfluenceDisplay influenceDisplayPrefab;

	[SerializeField]
	private EnchantDisplay enchantDisplayCommonPrefab;

	[SerializeField]
	private Image displayImage;

	[SerializeField]
	private RectTransform confirmHint;

	[NonSerialized]
	private Vector3 displayObjectInitLocalPos;

	[NonSerialized]
	private CellUIPrefabPool<EnchantDisplay, Enchant> enchantDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	[NonSerialized]
	private UncollectedStatusCommonInfoPanel uncollectedStatusInfoPanel;

	[NonSerialized]
	public WorldEventChoice itemChoice;

	[NonSerialized]
	private bool selected;

	[NonSerialized]
	private Sequence sequence;

	private static float selectedScale = 1.1f;

	private static float unselectedScale = 1f;

	public bool interactable
	{
		get
		{
			if (CellUI<WorldEventChoice>.gameState.actionManager.isEmpty)
			{
				if (itemChoice.executeAbleChecker != null)
				{
					return itemChoice.executeAbleChecker();
				}
				return true;
			}
			return false;
		}
	}

	private WorldEventPanel panel => CellUI<WorldEventChoice>.gameState.UIManager.currentEventPanel;

	protected override void Init()
	{
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, referenceParent);
		influenceDisplayPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplayPrefab, referenceParent);
		enchantDisplayPool = GetPrefabPool<EnchantDisplay, Enchant>(enchantDisplayCommonPrefab, referenceParent);
		uncollectedStatusInfoPanel = new UncollectedStatusCommonInfoPanel(() => CellUI<WorldEventChoice>.gameState.UIManager.universalCommonInfoPanel);
		itemDisplayPool.ConfigInfoPanel(uncollectedStatusInfoPanel);
		button.onClick.AddListener(_OnPointerClick);
		if (PlatformHelper.GetPlatform() == PlatformHelper.Type.PC)
		{
			GameButton gameButton = button;
			gameButton.onHighlight = (Action)Delegate.Combine(gameButton.onHighlight, (Action)delegate
			{
				panel.SelectChoice(this);
			});
			GameButton gameButton2 = button;
			gameButton2.onUnhighlight = (Action)Delegate.Combine(gameButton2.onUnhighlight, (Action)delegate
			{
				panel.UnselectChoices();
			});
		}
	}

	protected override void OnSetup(WorldEventChoice dailyContract)
	{
		itemChoice = dailyContract;
		itemDisplayPrefab.gameObject.SetActive(value: false);
		influenceDisplayPrefab.gameObject.SetActive(value: false);
		enchantDisplayCommonPrefab.gameObject.SetActive(value: false);
		displayImage.gameObject.SetActive(value: false);
		confirmHint.gameObject.SetActive(value: false);
		displayObject.localPosition = displayObjectInitLocalPos;
		displayObject.localScale = Vector3.one * unselectedScale;
		SetChoiceText();
		if (dailyContract.displayItems != null)
		{
			foreach (Item displayItem in dailyContract.displayItems)
			{
				itemDisplayPool.Spawn(displayItem);
			}
		}
		if (dailyContract.displayInfluences != null)
		{
			foreach (Influence displayInfluence in dailyContract.displayInfluences)
			{
				ConfigureInfluenceInfoPanel(influenceDisplayPool.Spawn(displayInfluence));
			}
		}
		if (dailyContract.displayEnchants != null)
		{
			foreach (Enchant displayEnchant in dailyContract.displayEnchants)
			{
				enchantDisplayPool.Spawn(displayEnchant);
			}
		}
		if (dailyContract.displayImage != null)
		{
			displayImage.sprite = dailyContract.displayImage;
			displayImage.gameObject.SetActive(value: true);
		}
		selected = false;
	}

	protected override void OnUnset()
	{
		itemDisplayPool.DespawnAll();
		influenceDisplayPool.DespawnAll();
		enchantDisplayPool.DespawnAll();
		displayImage.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		bool flag = panel.selected == this;
		if (selected != flag)
		{
			selected = flag;
			if (selected)
			{
				Select();
			}
			else
			{
				UnSelect();
			}
		}
		SetChoiceText();
		button.interactable = itemChoice.executeAbleChecker?.Invoke() ?? true;
	}

	private void Select()
	{
		PlaySelectTween();
	}

	private void UnSelect()
	{
		PlayUnSelectTween();
	}

	private void Confirm()
	{
		panel.worldEvent.lastSelectChoiceIndex = panel.worldEvent.currentDisplayInfo.choices.IndexOf(itemChoice);
		panel.worldEvent.lastSelectedChoice = itemChoice;
		panel.selected = null;
		itemChoice.executeAction?.Invoke();
		panel.AfterChoiceConfirm(itemChoice);
		if (!itemChoice.stayAfterChoose)
		{
			panel.HideDescription();
		}
		CellUI<WorldEventChoice>.gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			if (!(panel == null))
			{
				panel.SetDirty();
			}
		}));
		panel.SetDirty();
	}

	private void PlaySelectTween()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		confirmHint.gameObject.SetActive(value: true);
		sequence.Append(displayObject.DOLocalMoveX(displayObject.localPosition.x + 50f, tweenTime));
		sequence.Join(displayObject.DOScale(selectedScale, tweenTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
	}

	private void PlayUnSelectTween()
	{
		confirmHint.gameObject.SetActive(value: false);
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.Append(displayObject.DOLocalMove(displayObjectInitLocalPos, tweenTime));
		sequence.Join(displayObject.DOScale(unselectedScale, tweenTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
	}

	private void SetChoiceText()
	{
		eventNameText.text = itemChoice.buttonText;
		eventDescriptionText.text = itemChoice.descriptionText;
		Button componentInChildren = GetComponentInChildren<Button>();
		if (componentInChildren != null && componentInChildren.GetComponentInChildren<Text>() != null)
		{
			componentInChildren.GetComponentInChildren<Text>().text = itemChoice.buttonText;
		}
	}

	private void ConfigureInfluenceInfoPanel(InfluenceDisplay display)
	{
		if (!(display?.image == null))
		{
			display.image.raycastTarget = true;
			WorldEventInfluenceInfoTrigger worldEventInfluenceInfoTrigger = display.image.GetComponent<WorldEventInfluenceInfoTrigger>();
			if (worldEventInfluenceInfoTrigger == null)
			{
				worldEventInfluenceInfoTrigger = display.image.gameObject.AddComponent<WorldEventInfluenceInfoTrigger>();
			}
			worldEventInfluenceInfoTrigger.Setup(display.influence, uncollectedStatusInfoPanel, display.GetComponent<RectTransform>());
		}
	}

	private void _OnPointerClick()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (interactable)
		{
			if (selected)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_event_choice_confirm);
				Confirm();
			}
			else
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_event_choice_select);
				panel.SelectChoice(this);
			}
		}
	}
}
