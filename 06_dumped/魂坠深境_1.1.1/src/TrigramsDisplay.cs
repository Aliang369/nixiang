using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TrigramsDisplay : UIBase
{
	private const int DetailedPowerDisplayThreshold = 10;

	[SerializeField]
	public TrigramsAbilityDisplay trigramsAbilityDisplay;

	[SerializeField]
	public TrigramsStateDisplay trigramsStateDisplay;

	[SerializeField]
	public TrigramsLineDisplay trigramsLineDisplay;

	[SerializeField]
	private PowerDisplay powerDisplayPrefab;

	[SerializeField]
	private RectTransform powerDisplayParent;

	[SerializeField]
	private TextMeshProUGUI yinText;

	[SerializeField]
	private TextMeshProUGUI yangText;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	public GameButton hintButton;

	[SerializeField]
	private RectTransform hintButtonAnimPivot;

	[NonSerialized]
	private CellUIPrefabPool<PowerDisplay, PowerInfo> powerDisplayPool;

	[NonSerialized]
	private bool isPlayingAnim;

	[NonSerialized]
	private bool showingDetailedPower;

	[NonSerialized]
	private List<PowerDisplay> yangPowerDisplays;

	[NonSerialized]
	private List<PowerDisplay> yinPowerDisplays;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		InitUI(trigramsAbilityDisplay);
		InitUI(trigramsStateDisplay);
		InitUI(trigramsLineDisplay);
		powerDisplayPrefab.gameObject.SetActive(value: false);
		powerDisplayPool = GetPrefabPool<PowerDisplay, PowerInfo>(powerDisplayPrefab, powerDisplayParent);
		yangPowerDisplays = new List<PowerDisplay>();
		yinPowerDisplays = new List<PowerDisplay>();
		isPlayingAnim = false;
		showingDetailedPower = false;
		hintButton.onClick.AddListener(_OnHintButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, _OnUpdateEvents);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, _OnUpdateEvents);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.PlayerTurnEnd, _OnUpdateEvents);
		canvasGroup.alpha = 0f;
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, _OnUpdateEvents);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, _OnUpdateEvents);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.PlayerTurnEnd, _OnUpdateEvents);
	}

	protected override void UpdateDisplay()
	{
		if (isPlayingAnim)
		{
			SetDirty();
			return;
		}
		yinText.text = $"{UIBase.gameState.trigramsManager.yinPower}";
		yangText.text = $"{UIBase.gameState.trigramsManager.yangPower}";
	}

	public void Setup()
	{
	}

	public int GetDisplayIndex(PowerDisplay display)
	{
		if (yinPowerDisplays.Contains(display))
		{
			return UIBase.gameState.trigramsManager.maxPower - yinPowerDisplays.IndexOf(display) - 1;
		}
		if (yangPowerDisplays.Contains(display))
		{
			return yangPowerDisplays.IndexOf(display);
		}
		return -1;
	}

	private void _OnUpdateEvents(GameEvent.Type _1, GameEvent eve)
	{
		SetDirty();
	}

	private void _OnEmptyClicked()
	{
		SetDirty();
	}

	private void _OnHintButtonClicked()
	{
		UIBase.gameState.actionManager.EnqueueAction(new ShowGuideHintSetAction(UIBase.applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_XiaKe));
	}

	private void OnDrawGizmos()
	{
		GizmosHelper.DrawString($"{UIBase.gameState?.trigramsManager.yangPower}-{UIBase.gameState?.trigramsManager.yinPower}-{UIBase.gameState?.trigramsManager.turnMaxYangPower}-{UIBase.gameState?.trigramsManager.turnMaxYinPower}", base.gameObject.transform.position);
	}

	public bool HasAnim()
	{
		if (isPlayingAnim)
		{
			return true;
		}
		for (int i = 0; i < yangPowerDisplays.Count; i++)
		{
			if (yangPowerDisplays[i].hasAnim)
			{
				return true;
			}
		}
		for (int j = 0; j < yinPowerDisplays.Count; j++)
		{
			if (yinPowerDisplays[j].hasAnim)
			{
				return true;
			}
		}
		return false;
	}

	public void OnBattleStartInit()
	{
		SetDirty();
		UIBase.gameState.StartCoroutine(Anim_OnEnterLocationInit);
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.5f));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
	}

	private IEnumerator Anim_OnEnterLocationInit()
	{
		RebuildPowerDisplays();
		trigramsLineDisplay.UpdateImmediately();
		yield break;
	}

	public void OnBattleEnd()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.5f));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
	}

	public void OnChangYinYangPower(int yinAmount, int yangAmount)
	{
		SetDirty();
		if (SyncDetailedPowerDisplayMode())
		{
			return;
		}
		if (yangAmount != 0)
		{
			UIBase.gameState.StartCoroutine(() => Anim_OnChangYinYangPower(yangAmount, TrigramsManager.YinYangType.Yang));
		}
		if (yinAmount != 0)
		{
			UIBase.gameState.StartCoroutine(() => Anim_OnChangYinYangPower(yinAmount, TrigramsManager.YinYangType.Yin));
		}
	}

	public void OnConvertAllPowerToTurn()
	{
		SetDirty();
		RebuildPowerDisplays();
	}

	private IEnumerator Anim_OnChangYinYangPower(int amount, TrigramsManager.YinYangType type)
	{
		if (!showingDetailedPower)
		{
			yield break;
		}
		if (amount > 0)
		{
			List<PowerDisplay> list = new List<PowerDisplay>();
			for (int i = 0; i < amount; i++)
			{
				PowerDisplay item = powerDisplayPool.Spawn(new PowerInfo());
				switch (type)
				{
				case TrigramsManager.YinYangType.Yin:
					yinPowerDisplays.Insert(0, item);
					break;
				case TrigramsManager.YinYangType.Yang:
					yangPowerDisplays.Insert(0, item);
					break;
				default:
					throw new ArgumentOutOfRangeException("type", type, null);
				}
				list.Add(item);
			}
			List<WrapperEnumerator> list2 = new List<WrapperEnumerator>();
			foreach (PowerDisplay display in list)
			{
				list2.Add(UIBase.gameState.StartCoroutine(() => display.PlayTween_Add(type)));
			}
			yield return WrapperEnumerator.WaitAll(list2);
		}
		if (amount >= 0)
		{
			yield break;
		}
		List<PowerDisplay> displays = new List<PowerDisplay>();
		for (int num = 0; num < -amount; num++)
		{
			PowerDisplay powerDisplay = type switch
			{
				TrigramsManager.YinYangType.Yin => RemoveMostRight(), 
				TrigramsManager.YinYangType.Yang => RemoveMostLeft(), 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
			if (powerDisplay != null)
			{
				displays.Add(powerDisplay);
			}
		}
		List<WrapperEnumerator> list3 = new List<WrapperEnumerator>();
		foreach (PowerDisplay display2 in displays)
		{
			list3.Add(UIBase.gameState.StartCoroutine(() => display2.PlayTween_Remove(type)));
		}
		yield return WrapperEnumerator.WaitAll(list3);
		foreach (PowerDisplay item2 in displays)
		{
			powerDisplayPool.Despawn(item2);
		}
	}

	public void OnConvertYinYang(int amount, TrigramsManager.YinYangType type)
	{
		SetDirty();
		UIBase.gameState.StartCoroutine(() => Anim_OnConvertYinYang(amount, type));
	}

	private IEnumerator Anim_OnConvertYinYang(int amount, TrigramsManager.YinYangType type)
	{
		if (SyncDetailedPowerDisplayMode() || !showingDetailedPower)
		{
			yield break;
		}
		List<PowerDisplay> list;
		List<PowerDisplay> list2;
		switch (type)
		{
		case TrigramsManager.YinYangType.Yin:
			list = yinPowerDisplays;
			list2 = yangPowerDisplays;
			break;
		case TrigramsManager.YinYangType.Yang:
			list = yangPowerDisplays;
			list2 = yinPowerDisplays;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
		List<WrapperEnumerator> list3 = new List<WrapperEnumerator>();
		for (int i = 0; i < amount; i++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			List<PowerDisplay> list4 = list;
			PowerDisplay display = list4[list4.Count - 1];
			list.Remove(display);
			list2.Add(display);
			list3.Add(UIBase.gameState.StartCoroutine(() => display.PlayTween_Convert(type)));
		}
		yield return WrapperEnumerator.WaitAll(list3);
	}

	private bool SyncDetailedPowerDisplayMode()
	{
		bool flag = UIBase.gameState.trigramsManager.maxPower <= 10;
		if (showingDetailedPower == flag)
		{
			return false;
		}
		RebuildPowerDisplays();
		return true;
	}

	private void RebuildPowerDisplays()
	{
		powerDisplayPool.DespawnAll();
		yangPowerDisplays.Clear();
		yinPowerDisplays.Clear();
		showingDetailedPower = UIBase.gameState.trigramsManager.maxPower <= 10;
		if (!showingDetailedPower)
		{
			return;
		}
		for (int i = 0; i < (int)UIBase.gameState.trigramsManager.yangPower; i++)
		{
			PowerDisplay item = powerDisplayPool.Spawn(new PowerInfo());
			yangPowerDisplays.Add(item);
		}
		for (int j = 0; j < (int)UIBase.gameState.trigramsManager.yinPower; j++)
		{
			PowerDisplay item2 = powerDisplayPool.Spawn(new PowerInfo());
			yinPowerDisplays.Add(item2);
		}
		foreach (PowerDisplay yangPowerDisplay in yangPowerDisplays)
		{
			yangPowerDisplay.UpdateDisplayImmediately(TrigramsManager.YinYangType.Yang);
		}
		foreach (PowerDisplay yinPowerDisplay in yinPowerDisplays)
		{
			yinPowerDisplay.UpdateDisplayImmediately(TrigramsManager.YinYangType.Yin);
		}
	}

	private PowerDisplay RemoveMostLeft()
	{
		if (yangPowerDisplays.Count > 0)
		{
			PowerDisplay powerDisplay = yangPowerDisplays[0];
			yangPowerDisplays.Remove(powerDisplay);
			return powerDisplay;
		}
		if (yinPowerDisplays.Count > 0)
		{
			PowerDisplay powerDisplay2 = yinPowerDisplays[yinPowerDisplays.Count - 1];
			yinPowerDisplays.Remove(powerDisplay2);
			return powerDisplay2;
		}
		return null;
	}

	private PowerDisplay RemoveMostRight()
	{
		if (yinPowerDisplays.Count > 0)
		{
			PowerDisplay powerDisplay = yinPowerDisplays[0];
			yinPowerDisplays.Remove(powerDisplay);
			return powerDisplay;
		}
		if (yangPowerDisplays.Count > 0)
		{
			PowerDisplay powerDisplay2 = yangPowerDisplays[yangPowerDisplays.Count - 1];
			yangPowerDisplays.Remove(powerDisplay2);
			return powerDisplay2;
		}
		return null;
	}

	public void PlayHintAnim()
	{
		hintButton.gameObject.SetActive(value: true);
		Sequence obj = DOTween.Sequence();
		Vector3 position = hintButton.transform.position;
		hintButton.transform.position = hintButtonAnimPivot.position;
		obj.Append(hintButton.transform.DOMove(position, 2f));
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			hintButton.transform.position = position;
		});
		obj.Play();
	}
}
