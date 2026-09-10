using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RitualDisplay : UIBase, IPointerClickHandler, IEventSystemHandler
{
	public RectTransform RitualBaseArea;

	public TextMeshProUGUI ritualLimitText;

	public RitualPrayCell prayCellPrefab;

	public RectTransform prayCellParent;

	public RectTransform prayDetailArea;

	public Image prayDetailHintIcon;

	public Color openDetailColor;

	public Color closeDetailColor;

	private CellUIPrefabPool<RitualPrayCell, Pray> prayCellPool;

	private bool ritualEventTriggered;

	public void OnPointerClick(PointerEventData eventData)
	{
		prayDetailArea.gameObject.SetActive(!prayDetailArea.gameObject.activeSelf);
		SetDirty();
	}

	protected override void Init()
	{
		base.Init();
		prayCellPool = GetPrefabPool<RitualPrayCell, Pray>(prayCellPrefab, prayCellParent);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, _OnGameEvent);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterRitualTrigger, _OnGameEvent);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterAddPrayToRitual, _OnGameEvent);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterRemovePrayFormRitual, _OnGameEvent);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, _OnGameEvent);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterRitualTrigger, _OnGameEvent);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterAddPrayToRitual, _OnGameEvent);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterRemovePrayFormRitual, _OnGameEvent);
	}

	protected override void UpdateDisplay()
	{
		if (ritualEventTriggered || UIBase.gameState.playerState.CharacterClassState.classDef.mClass == CharacterClassType.Inquisitor)
		{
			RitualBaseArea.gameObject.SetActive(value: true);
			ritualLimitText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(UIBase.gameState.ritualManager.currentPrays.Count, UIBase.gameState.ritualManager.prayLimit);
			prayCellPool.DespawnAll();
			foreach (Pray currentPray in UIBase.gameState.ritualManager.currentPrays)
			{
				prayCellPool.Spawn(currentPray);
			}
			prayDetailHintIcon.color = (prayDetailArea.gameObject.activeSelf ? openDetailColor : closeDetailColor);
		}
		else
		{
			RitualBaseArea.gameObject.SetActive(value: false);
		}
	}

	private void _OnGameEvent(GameEvent.Type type, GameEvent eve)
	{
		if (type == GameEvent.Type.AfterRitualTrigger || type == GameEvent.Type.AfterAddPrayToRitual || type == GameEvent.Type.AfterRemovePrayFormRitual)
		{
			ritualEventTriggered = true;
		}
		SetDirty();
	}

	private void _OnEmptyAreaClicked()
	{
		prayDetailArea.gameObject.SetActive(value: false);
	}
}
