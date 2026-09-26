using System.Collections.Generic;
using UnityEngine;

public class zbCombatFPS
{
	private float _accumTime;

	private int _frameCount;

	private int _lowFpsHitCount;

	private int _highFpsHitCount;

	private const int STABLE_HIT_COUNT = 2;

	public float checkInterval = 1f;

	public int lowFpsThreshold = 40;

	public int highFpsThreshold = 50;

	public int spriteReplaceThreshold = 20;

	public int spineExitThreshold = 30;

	public int veryLowFpsTheshold = 10;

	private bool _isLowPerformanceMode;

	private bool _isVeryLowPeformanceMode;

	private bool _isSpriteReplaceMode;

	private bool _isSpriteExitMode;

	private int spriteReplaceBatch = 5;

	private bool _openSpriteMode = true;

	private float _spriteStepTimer;

	public static readonly int MANUAL_UPDATE_FRAME = 3;

	public void Update()
	{
		_accumTime += Time.unscaledDeltaTime;
		_frameCount++;
		if (IsOpenSpriteMode())
		{
			_spriteStepTimer += Time.unscaledDeltaTime;
			if (_spriteStepTimer >= 0.1f)
			{
				_spriteStepTimer = 0f;
				if (_isSpriteReplaceMode)
				{
					ReplaceMonstersSpriteStep(_isVeryLowPeformanceMode ? (spriteReplaceBatch * 3) : spriteReplaceBatch);
				}
				else if (_isSpriteExitMode)
				{
					ExitedMonstersSpriteStep(spriteReplaceBatch);
				}
			}
		}
		if (_accumTime < checkInterval)
		{
			return;
		}
		float num = (float)_frameCount / _accumTime;
		if (!_isLowPerformanceMode)
		{
			if (num < (float)lowFpsThreshold)
			{
				_lowFpsHitCount++;
				if (_lowFpsHitCount >= 2)
				{
					EnterLowPerformanceMode(num);
					_lowFpsHitCount = 0;
				}
			}
			else
			{
				_lowFpsHitCount = 0;
			}
		}
		else if (num > (float)highFpsThreshold)
		{
			_highFpsHitCount++;
			if (_highFpsHitCount >= 2)
			{
				ExitLowPerformanceMode(num);
				_highFpsHitCount = 0;
			}
		}
		else
		{
			_highFpsHitCount = 0;
		}
		if (IsOpenSpriteMode())
		{
			if (_isLowPerformanceMode && !_isSpriteReplaceMode && num < (float)spriteReplaceThreshold)
			{
				EnterSpriteReplaceSpineMode(num);
			}
			else if (_isSpriteReplaceMode && num > (float)spineExitThreshold)
			{
				ExitSpriteReplaceSpineMode();
			}
			if (_isSpriteReplaceMode && num < (float)veryLowFpsTheshold)
			{
				_isVeryLowPeformanceMode = true;
			}
			else
			{
				_isVeryLowPeformanceMode = false;
			}
		}
		_accumTime = 0f;
		_frameCount = 0;
	}

	private void EnterLowPerformanceMode(float avgFps)
	{
		_isLowPerformanceMode = true;
		zbPerfLog.Log($"[combatFPS] 进入低性能档 avgFps={avgFps:0.#}（骨架全场 {MANUAL_UPDATE_FRAME} 帧步进）");
		SetMonstersManualUpdate(manual: true);
	}

	private void ExitLowPerformanceMode(float avgFps)
	{
		_isLowPerformanceMode = false;
		zbPerfLog.Log($"[combatFPS] 退出低性能档 avgFps={avgFps:0.#}");
		SetMonstersManualUpdate(manual: false);
	}

	private static void SetMonstersManualUpdate(bool manual)
	{
		SetMonstersManualUpdate(zbCombatSystem.Instance.combatData, manual);
		SetMonstersManualUpdate(zbCombatSystem.Instance.combatData2, manual);
	}

	private static void SetMonstersManualUpdate(zbCombatData combatData, bool manual)
	{
		if (combatData == null)
		{
			return;
		}
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			zbMonster value = monster.Value;
			if (!((Object)(object)value == (Object)null) && !value.isUsingSprite)
			{
				value.SetSkeletonManualUpdate(manual);
			}
		}
	}

	private void EnterSpriteReplaceSpineMode(float avgFps)
	{
		_isSpriteReplaceMode = true;
		_isSpriteExitMode = false;
		zbPerfLog.Log($"[combatFPS] 进入静态图替换档 avgFps={avgFps:0.#}（怪物骨架逐步换为静态图）");
	}

	private void ExitSpriteReplaceSpineMode()
	{
		_isSpriteReplaceMode = false;
		_isSpriteExitMode = true;
	}

	private void ReplaceMonstersSpriteStep(int count)
	{
		ReplaceMonstersSpriteStep(zbCombatSystem.Instance.combatData, count);
		ReplaceMonstersSpriteStep(zbCombatSystem.Instance.combatData2, count);
	}

	private void ReplaceMonstersSpriteStep(zbCombatData combatData, int count)
	{
		if (combatData == null)
		{
			return;
		}
		Dictionary<int, zbMonster> monsterList = combatData.monsters.monsterList;
		if (monsterList.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<int, zbMonster> item in monsterList)
		{
			zbMonster value = item.Value;
			if (!((Object)(object)value == (Object)null))
			{
				if (!value.isUsingSprite && !value.IsBoss())
				{
					value.UseSpriteReplaceSpine(bUseSprite: true);
					num++;
				}
				if (num >= count)
				{
					break;
				}
			}
		}
	}

	private void ExitedMonstersSpriteStep(int count)
	{
		if ((0u | (ExitedMonstersSpriteStep(zbCombatSystem.Instance.combatData, count) ? 1u : 0u) | (ExitedMonstersSpriteStep(zbCombatSystem.Instance.combatData2, count) ? 1u : 0u)) == 0)
		{
			_isSpriteExitMode = false;
		}
	}

	private bool ExitedMonstersSpriteStep(zbCombatData combatData, int count)
	{
		if (combatData == null)
		{
			return false;
		}
		Dictionary<int, zbMonster> monsterList = combatData.monsters.monsterList;
		if (monsterList.Count == 0)
		{
			return false;
		}
		int num = 0;
		bool result = false;
		foreach (KeyValuePair<int, zbMonster> item in monsterList)
		{
			zbMonster value = item.Value;
			if (!((Object)(object)value == (Object)null) && value.isUsingSprite)
			{
				if (num < count)
				{
					value.UseSpriteReplaceSpine(bUseSprite: false);
					zbSpineGpuAnimRuntime.ReconvertAfterSpriteRestore(value);
					num++;
				}
				else
				{
					result = true;
				}
			}
		}
		return result;
	}

	public bool IsLowPerformanceMode()
	{
		return _isLowPerformanceMode;
	}

	public bool IsSpriteReplaceMode()
	{
		return _isSpriteReplaceMode;
	}

	public bool IsVeryLowPerfomanceMode()
	{
		return _isVeryLowPeformanceMode;
	}

	private bool IsOpenSpriteMode()
	{
		return _openSpriteMode;
	}
}
