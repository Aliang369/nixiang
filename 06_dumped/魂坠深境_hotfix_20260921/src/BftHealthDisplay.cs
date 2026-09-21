using System;
using Common.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public class BftHealthDisplay : UIBase
{
	public enum BarAnimType
	{
		Immediate,
		Speed,
		Time
	}

	[Serializable]
	public struct BarAnimConfig
	{
		public BarAnimType type;

		public float speed;

		public float time;
	}

	[SerializeField]
	public TextMeshProUGUI healthText;

	[SerializeField]
	public TextMeshProUGUI tempHealthText;

	[SerializeField]
	public RectTransform tempHealthTextArea;

	[SerializeField]
	public RectTransform maxHealthBar;

	[SerializeField]
	public RectTransform healthBar;

	[SerializeField]
	public RectTransform healHealthBar;

	[SerializeField]
	public RectTransform hitHealthBar;

	[SerializeField]
	public RectTransform tempMaxHealthBar;

	[SerializeField]
	public RectTransform tempHealthBar;

	[SerializeField]
	public RectTransform tempHealHealthBar;

	[SerializeField]
	public RectTransform tempHitHealthBar;

	[SerializeField]
	public BarAnimConfig healthBarConfig;

	[SerializeField]
	public BarAnimConfig delayHealthBarConfig;

	[SerializeField]
	public float delay;

	[NonSerialized]
	private float currentHealthTarget;

	[NonSerialized]
	private float currentTempHealthTarget;

	[NonSerialized]
	private Sequence mainHealthBarSequence;

	[NonSerialized]
	private Sequence tempHealthBarSequence;

	protected virtual BattleFieldTransform bft => UIBase.gameState.playerState.bft;

	protected override void Init()
	{
		healthBar.SetWidth(maxHealthBar.GetWidth());
		hitHealthBar.SetWidth(maxHealthBar.GetWidth());
		AddListener(UIEvent.OnPanelShow, _OnPanelShow);
		AddListener(UIEvent.OnBattleStart, _OnBattleStart);
		currentHealthTarget = -1f;
		currentTempHealthTarget = -1f;
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BftHealthChange, _OnHealthChange);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BftMaxHealthChange, _OnHealthChange);
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BftHealthChange, _OnHealthChange);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BftMaxHealthChange, _OnHealthChange);
	}

	protected override void UpdateDisplay()
	{
		int num = bft.health;
		int num2 = bft.maxHealth;
		healthText.text = $"{num.Clamp(0, num)}/{num2}";
		int num3 = bft.tempHealth;
		tempHealthText.text = ((num3 > 0) ? $"+{num3}" : "");
		tempHealthTextArea.gameObject.SetActive(num3 > 0);
		UpdateHealthBar(num, num2, num3);
	}

	private void UpdateHealthBar(int currentHealth, int maxHealth, int tempHealth)
	{
		float num = ((float)currentHealth / (float)maxHealth).Clamp(0f, 1f);
		if (Math.Abs(currentHealthTarget - num) > 0.001f)
		{
			currentHealthTarget = num;
			mainHealthBarSequence?.Kill();
			mainHealthBarSequence = GenerateHealthBarTween(num, maxHealthBar, healthBar, hitHealthBar, healHealthBar);
		}
		float num2 = ((float)tempHealth / (float)maxHealth).Clamp(0f, 1f);
		if (Math.Abs(currentTempHealthTarget - num2) > 0.001f)
		{
			currentTempHealthTarget = num2;
			tempHealthBarSequence?.Kill();
			tempHealthBarSequence = GenerateHealthBarTween(num2, tempMaxHealthBar, tempHealthBar, tempHitHealthBar, tempHealHealthBar);
		}
	}

	private Sequence GenerateHealthBarTween(float target, RectTransform maxHealthBar, RectTransform healthBar, RectTransform hitHealthBar, RectTransform healHealthBar)
	{
		float width = maxHealthBar.GetWidth();
		float width2 = healthBar.GetWidth();
		float width3 = hitHealthBar.GetWidth();
		float width4 = healHealthBar.GetWidth();
		float num = (width2 / width).Clamp(0f, 1f);
		float num2 = (width3 / width).Clamp(0f, 1f);
		float num3 = (width4 / width).Clamp(0f, 1f);
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		bool flag = target < num;
		if (!flag && num3 < num)
		{
			num3 = num;
		}
		healthBar.SetWidth(maxHealthBar.GetWidth() * num);
		hitHealthBar.SetWidth(maxHealthBar.GetWidth() * num2);
		healHealthBar.SetWidth(maxHealthBar.GetWidth() * num3);
		float time = GetTime(healthBarConfig, flag ? num : num3, target);
		float time2 = GetTime(delayHealthBarConfig, flag ? num2 : num, target);
		if (num > target)
		{
			sequence.Insert(0f, BarTween(healthBar, num, target, time));
		}
		else
		{
			sequence.Insert(delay, BarTween(healthBar, num, target, time2));
		}
		if (num3 < num + 0.01f || num3 < target)
		{
			sequence.Insert(0f, BarTween(healHealthBar, num3, target, time));
		}
		else
		{
			sequence.Insert(delay, BarTween(healHealthBar, num3, target, time2));
		}
		sequence.Insert(delay, BarTween(hitHealthBar, num2, target, time2));
		sequence.Play();
		return sequence;
	}

	private TweenerCore<float, float, FloatOptions> BarTween(RectTransform healthBar, float current, float target, float time)
	{
		return DOTween.To(() => current, delegate(float x)
		{
			current = x;
			healthBar.SetWidth(maxHealthBar.GetWidth() * current);
		}, target, time);
	}

	private float GetTime(BarAnimConfig config, float current, float target)
	{
		return config.type switch
		{
			BarAnimType.Immediate => 0f, 
			BarAnimType.Speed => Mathf.Abs(current - target) / config.speed, 
			BarAnimType.Time => config.time, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected virtual void ResetUI()
	{
		if (bft != null)
		{
			UpdateDisplay();
			mainHealthBarSequence?.Complete(withCallbacks: true);
			tempHealthBarSequence?.Complete(withCallbacks: true);
		}
	}

	private void _OnHealthChange(GameEvent.Type type, GameEvent ge)
	{
		SetDirty();
	}

	private void _OnPanelShow()
	{
		if (bft != null)
		{
			ResetUI();
		}
	}

	protected virtual void _OnBattleStart()
	{
	}
}
