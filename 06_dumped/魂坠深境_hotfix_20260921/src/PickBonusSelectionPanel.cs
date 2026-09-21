using System;
using System.Collections;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickBonusSelectionPanel : PickBonusSelectionPanelBase, IShowAsync, IHideAsync
{
	[SerializeField]
	public ConfirmItemSelectionPanel itemSelectionPanel;

	[SerializeField]
	private ConfirmInfluenceSelectionPanel influenceSelectionPanel;

	[SerializeField]
	private ConfirmBossInfluenceSelectionPanel bossInfluenceSelectionPanel;

	[SerializeField]
	private PickBonusSelector selectorPrefab;

	[SerializeField]
	private RectTransform selectorParent;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	public LeaveButton leaveButton;

	[SerializeField]
	private Image background_Normal;

	[SerializeField]
	private Image background_Elite;

	[SerializeField]
	private Image background_Boss;

	[SerializeField]
	private RectTransform coinCrystalArea;

	[SerializeField]
	private CoinDisplay coinDisplay;

	[SerializeField]
	private CrystalDisplay crystalDisplay;

	[SerializeField]
	private GemCountDisplay gemCountDisplay;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	public ConvertablePivot coinImagePivot;

	[NonSerialized]
	public ConvertablePivot crystalImagePivot;

	[NonSerialized]
	public ConvertablePivot gemImagePivot;

	[NonSerialized]
	public bool endEncounterAfterBonus;

	[NonSerialized]
	public bool getAllItem;

	[NonSerialized]
	public BattleBonus info;

	[NonSerialized]
	private CellUIPrefabPool<PickBonusSelector, BattleBonusObject> pool;

	protected override void Init()
	{
		((Behaviour)(object)animator).enabled = false;
		selectorPrefab.gameObject.SetActive(value: false);
		GetPrefabPool(ref pool, selectorPrefab, selectorParent);
		pool.ConfigInfoPanel(itemInfoPanel);
		coinImagePivot = new ConvertablePivot(coinDisplay.image.transform);
		crystalImagePivot = new ConvertablePivot(crystalDisplay.image.transform);
		gemImagePivot = new ConvertablePivot(gemCountDisplay.image.transform);
		LeaveButton obj = leaveButton;
		obj.onLeave = (Action)Delegate.Combine(obj.onLeave, new Action(_OnLeaveButtonClicked));
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnHideAllInfo);
		InitUI(itemSelectionPanel);
		InitUI(influenceSelectionPanel);
		InitUI(bossInfluenceSelectionPanel);
		InitUI(itemInfoPanel);
		InitUI(leaveButton);
		InitUI(coinDisplay);
		InitUI(crystalDisplay);
		coinDisplay.GetModule<InfoModule>().infoPanel = itemInfoPanel;
		crystalDisplay.GetModule<InfoModule>().infoPanel = itemInfoPanel;
		UIBase.gameState.actionManager.actionQueueCleanEvent += SetDirty;
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnHideAllInfo);
		UIBase.gameState.actionManager.actionQueueCleanEvent -= SetDirty;
	}

	public void Setup(BattleBonus info)
	{
		this.info = info;
		PickBonusSelectionPanelBase.RemoveInvalidRuneBonuses(info);
		titleText.text = info.title;
		endEncounterAfterBonus = true;
		getAllItem = false;
		coinDisplay.Unset();
		crystalDisplay.Unset();
		gemCountDisplay.Unset();
		leaveButton.Show();
		foreach (BattleBonusObject bonuse in info.bonuses)
		{
			if (bonuse is CrystalBonus crystalBonus)
			{
				GetCrystalBonus(crystalBonus);
			}
			else if (bonuse is CoinBonus coinBonus)
			{
				GetCoinBonus(coinBonus);
			}
			else if (bonuse is GemBonus gemBonus)
			{
				GetGemBonus(gemBonus);
			}
			else
			{
				pool.Spawn(bonuse);
			}
		}
		itemSelectionPanel.gameObject.SetActive(value: false);
		influenceSelectionPanel.gameObject.SetActive(value: false);
		PathLocation.LocationType? locationType = UIBase.gameState.dungeonMapManager.currentLocation?.type;
		background_Normal.gameObject.SetActive(value: false);
		background_Elite.gameObject.SetActive(value: false);
		background_Boss.gameObject.SetActive(value: false);
		if (locationType == PathLocation.LocationType.EliteFight)
		{
			background_Elite.gameObject.SetActive(value: true);
		}
		else if (locationType == PathLocation.LocationType.Boss)
		{
			background_Boss.gameObject.SetActive(value: true);
		}
		else
		{
			background_Normal.gameObject.SetActive(value: true);
		}
		coinCrystalArea.gameObject.SetActive(UIBase.applicationState.guideManager.introFinished);
		void GetCoinBonus(CoinBonus coinBonus2)
		{
			coinDisplay.Setup(coinBonus2);
			UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				UIBase.gameState.actionManager.EnqueueAction(new WaitUntilAction(() => !((Behaviour)(object)animator).enabled));
				UIBase.gameState.actionManager.EnqueueAction(new PlayCollectVFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_collectCoin, coinImagePivot, UIBase.gameState.pivotManager.coinDisplayPivot, waitForFinish: false));
				UIBase.gameState.actionManager.EnqueueAction(new ChangeCoinAction(coinBonus2.count));
			}));
		}
		void GetCrystalBonus(CrystalBonus crystalBonus2)
		{
			crystalDisplay.Setup(crystalBonus2);
			UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				UIBase.gameState.actionManager.EnqueueAction(new WaitUntilAction(() => !((Behaviour)(object)animator).enabled));
				UIBase.gameState.actionManager.EnqueueAction(new PlayCollectVFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_collectCrystal, crystalImagePivot, UIBase.gameState.pivotManager.crystalDisplayPivot, waitForFinish: false));
				UIBase.gameState.actionManager.EnqueueAction(new ChangeCrystalAction(crystalBonus2.count));
			}));
		}
		void GetGemBonus(GemBonus gemBonus2)
		{
			gemCountDisplay.Setup(gemBonus2);
			UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				UIBase.gameState.actionManager.EnqueueAction(delegate
				{
					gemBonus2.TriggerBonus(UIBase.gameState, null);
				});
			}));
		}
	}

	public void Unset()
	{
		if (info != null)
		{
			info.onEnd?.Invoke();
			pool.DespawnAll();
			UIBase.gameState.UIManager.HideUI(UIBase.gameState.UIManager.pickSelectPanel);
			info = null;
		}
	}

	IEnumerator IShowAsync.ShowAnim()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.speed = base.uiTimeScale;
		animator.Play("Show");
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled || !base.gameObject.activeInHierarchy);
	}

	IEnumerator IHideAsync.HideAnim()
	{
		if (!getAllItem)
		{
			((Behaviour)(object)animator).enabled = true;
			animator.speed = base.uiTimeScale;
			animator.Play("Hide");
			yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled || !base.gameObject.activeInHierarchy);
		}
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		if (pool.activeCount == 0 && !getAllItem && UIBase.gameState.actionManager.isEmpty)
		{
			getAllItem = true;
			StartCoroutine(GetAllItemAnim());
		}
		IEnumerator GetAllItemAnim()
		{
			((Behaviour)(object)animator).enabled = true;
			animator.speed = base.uiTimeScale;
			animator.Play("GetAllItem");
			yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled || !base.gameObject.activeInHierarchy);
		}
	}

	public override void OnSubPanelFinish(BattleBonusObject info)
	{
		PickBonusSelector obj = pool.GetActiveElements().Find((PickBonusSelector s) => s.bonus == info);
		pool.Despawn(obj);
		itemInfoPanel.HideAllCommonInfo();
		SetDirty();
	}

	public override void Select(PickBonusSelector selector)
	{
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			selector.bonus.view = true;
			if (selector.bonus is SelectGetItemBonus bonus)
			{
				itemSelectionPanel.Setup(bonus);
			}
			else if (selector.bonus is AddInfluenceBonus addInfluenceBonus)
			{
				influenceSelectionPanel.Setup(addInfluenceBonus);
			}
			else if (selector.bonus is AddBossInfluenceBonus addBossInfluenceBonus)
			{
				bossInfluenceSelectionPanel.Setup(addBossInfluenceBonus);
			}
			else if (selector.bonus is RuneBonus runeBonus)
			{
				runeBonus.TriggerBonus(UIBase.gameState, delegate
				{
					selector.gameObject.SetActive(value: false);
					pool.Despawn(selector);
				});
			}
			else
			{
				selector.bonus.TriggerBonus(UIBase.gameState, delegate
				{
					selector.gameObject.SetActive(value: false);
					pool.Despawn(selector);
				});
			}
		}));
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(SetDirty));
	}

	private void FinishBonusSelection()
	{
		UIBase.gameState.gameEventManager.TriggerFinishBonus(info);
		Unset();
		if (endEncounterAfterBonus)
		{
			UIBase.gameState.actionManager.EnqueueAction(new EndCurrentEncounterAction(finish: true));
		}
	}

	private void _OnLeaveButtonClicked()
	{
		if (UIBase.gameState.showConfirm && info.bonuses.Any((BattleBonusObject b) => !b.view && (b is SelectGetItemBonus || b is AddInfluenceBonus || b is RuneBonus)))
		{
			UIBase.gameState.UIManager.leaveConfirmPanel.Setup(new LeaveConfirmPanel.Info
			{
				text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unview_bonus,
				onConfirm = delegate
				{
					FinishBonusSelection();
					SetDirty();
				},
				onCancel = delegate
				{
					leaveButton.Show();
				}
			});
		}
		else
		{
			FinishBonusSelection();
		}
	}

	private void _OnHideAllInfo()
	{
		itemInfoPanel.HideAllCommonInfo();
	}
}
