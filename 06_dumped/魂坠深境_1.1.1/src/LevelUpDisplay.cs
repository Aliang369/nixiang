using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpDisplay : CellUI<LevelUpDisplay.LevelUpData>
{
	public class LevelUpData
	{
		public string title;

		public int remainExp;

		public int currentLevel;

		public int nextLevel;

		public int fromExp;

		public int maxExp;

		public int toExp;

		public List<IGlobalItemDisplaySetup> rewards;

		public override string ToString()
		{
			return $"{title} {remainExp} [{currentLevel}->{nextLevel}] {fromExp}->{toExp}/{maxExp} {rewards?.Count}";
		}
	}

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private TextMeshProUGUI nextLevelText;

	[SerializeField]
	private TextMeshProUGUI remainExpText;

	[SerializeField]
	private TextMeshProUGUI expText;

	[SerializeField]
	private TextMeshProUGUI upgradeHint;

	[SerializeField]
	private Slider expSlider;

	[SerializeField]
	private GlobalItemDisplay prefab;

	[SerializeField]
	private float levelUpAnimationDuration = 1f;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	[NonSerialized]
	private Sequence levelUpSequence;

	[NonSerialized]
	private int displayExp;

	private LevelUpData currentData => base.setupData;

	protected override void Init()
	{
		GetPrefabPool(ref pool, prefab);
	}

	protected override void OnSetup(LevelUpData setupData)
	{
		base.gameObject.SetActive(value: true);
		displayExp = currentData.fromExp;
		levelText.text = $"{currentData.currentLevel + 1}";
		nextLevelText.text = $"{currentData.nextLevel + 1}";
		expText.text = $"{displayExp}/{currentData.maxExp}";
		expSlider.value = (float)displayExp / (float)currentData.maxExp;
		title.text = currentData.title;
		upgradeHint.gameObject.SetActive(value: false);
		pool.DespawnAll();
		if (currentData.rewards != null)
		{
			List<IGlobalItemDisplaySetup> list = currentData.rewards.Where((IGlobalItemDisplaySetup r) => !r.isHidden).ToList();
			if (!list.IsNullOrEmpty())
			{
				upgradeHint.gameObject.SetActive(value: true);
				upgradeHint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_level_upgrade_hint.FormatWith(currentData.nextLevel + 1);
				pool.Sync(list);
			}
		}
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
		levelUpSequence?.Kill();
		levelUpSequence = null;
		pool.DespawnAll();
	}

	private IEnumerator PlayLevelUpAnimation()
	{
		levelUpSequence?.Kill();
		levelUpSequence = DOTween.Sequence();
		expSlider.value = (float)currentData.fromExp / (float)currentData.maxExp;
		remainExpText.text = $"{currentData.remainExp}";
		levelUpSequence.Append(DOTween.To(() => displayExp, delegate(float value)
		{
			displayExp = (int)value;
			expSlider.value = (float)displayExp / (float)currentData.maxExp;
			expText.text = $"{displayExp}/{currentData.maxExp}";
			remainExpText.text = $"{currentData.remainExp - displayExp + currentData.fromExp}";
		}, currentData.toExp, levelUpAnimationDuration));
		levelUpSequence.Play();
		yield return levelUpSequence.WaitForCompletion();
	}

	public IEnumerator DisplayCor(LevelUpData data)
	{
		TraceLog.Debug($"[LevelUpDisplay] DisplayCor({data})");
		Setup(data);
		yield return PlayLevelUpAnimation();
	}

	public IEnumerator DisplayCor(LevelLine.ChangeExpResult result, string title = null)
	{
		int currentRemainExp = result.deltaExp;
		int currentLevel = result.initLeveLIndex;
		Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_credit);
		for (int i = 0; i < result.levelUps.Count; i++)
		{
			LevelLine.ChangeExpResult.LevelRewardInfo levelRewardInfo = result.levelUps[i];
			LevelLineDef.LineLevel lineLevel = result.levelLineDef.levels[currentLevel];
			List<IGlobalItemDisplaySetup> list = new List<IGlobalItemDisplaySetup>();
			if (levelRewardInfo.baseReward != null)
			{
				list.Add(levelRewardInfo.baseReward);
			}
			if (levelRewardInfo.appendReward != null)
			{
				list.Add(levelRewardInfo.appendReward);
			}
			LevelUpData levelUpData = new LevelUpData
			{
				remainExp = currentRemainExp,
				currentLevel = levelRewardInfo.levelIndex,
				nextLevel = levelRewardInfo.levelIndex + 1,
				title = title
			};
			levelUpData.fromExp = ((i == 0) ? result.initExp : 0);
			levelUpData.toExp = lineLevel.expCost;
			levelUpData.maxExp = lineLevel.expCost;
			levelUpData.rewards = list;
			currentRemainExp -= levelUpData.toExp - levelUpData.fromExp;
			yield return DisplayCor(levelUpData);
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_target);
			currentLevel++;
		}
		if (currentLevel < result.levelLineDef.levels.Count - 1 && currentRemainExp > 0)
		{
			LevelLineDef.LineLevel lineLevel2 = result.levelLineDef.levels[currentLevel];
			List<IGlobalItemDisplaySetup> list2 = new List<IGlobalItemDisplaySetup>();
			if (lineLevel2.reward != null)
			{
				list2.Add(lineLevel2.reward);
			}
			if (lineLevel2.appendReward != null)
			{
				list2.Add(lineLevel2.appendReward);
			}
			LevelUpData levelUpData2 = new LevelUpData
			{
				remainExp = currentRemainExp,
				currentLevel = currentLevel,
				nextLevel = currentLevel + 1,
				title = title
			};
			levelUpData2.fromExp = ((result.initLeveLIndex == currentLevel) ? result.initExp : 0);
			levelUpData2.toExp = levelUpData2.fromExp + currentRemainExp;
			levelUpData2.maxExp = lineLevel2.expCost;
			levelUpData2.rewards = list2;
			yield return DisplayCor(levelUpData2);
		}
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_credit);
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(result.GetAllRewards());
		globalRewardPackage.title = title + " " + ScriptableObjectSingleton<TextDataPreset>.instance.term_level_upgrades;
		globalRewardPackage.hint = "";
		if (globalRewardPackage.anyUnhiddenReward)
		{
			CellUI<LevelUpData>.gameState.UIManager.globalRewardInfoDisplay.Setup(globalRewardPackage);
			yield return new WaitUntil(() => !CellUI<LevelUpData>.gameState.UIManager.globalRewardInfoDisplay.gameObject.activeInHierarchy);
		}
		Unset();
	}
}
