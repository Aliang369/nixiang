using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralToolkit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChallengeStar : CellUI<ContractState>, IPointerClickHandler, IEventSystemHandler
{
	public Sprite challengeStartContractSprite;

	public Sprite dailyShuffleContractSprite;

	public Sprite StoryContractSprite;

	public Sprite challengeStartContractGlowSprite;

	public Sprite StoryContractGlowSprite;

	public Image starImage;

	public Image glowImage;

	public Image unlockImage;

	public Image finishImage;

	public TextMeshProUGUI text;

	public List<ChallengeStar> connectStars;

	public UnityDateTime startDate = new UnityDateTime(DateTime.UtcNow);

	public UnityDateTime endDate = new UnityDateTime(DateTime.UtcNow);

	[FormerlySerializedAs("contractDef")]
	public ContractDef presetContractDef;

	[NonSerialized]
	private Constellation constellation;

	[NonSerialized]
	private ConstellationDetailPanel constellationDetailPanel;

	public ContractDef ContractDef => base.setupData?.contractDef;

	public bool Unlocked
	{
		get
		{
			if (constellation == null)
			{
				return false;
			}
			if (constellationType == Constellation.ConstellationType.SeasonChallenge && (DateTime.UtcNow < startDate.UtcDateTime || DateTime.UtcNow > endDate.UtcDateTime))
			{
				return false;
			}
			if (constellation.startStar == null)
			{
				return true;
			}
			ChallengeStar[] source = (from s in constellation.starLineConnections
				where s.to == this
				select s.@from).ToArray();
			if (this == constellation.startStar)
			{
				return true;
			}
			return source.All((ChallengeStar s) => s.Finished);
		}
	}

	private bool Finished
	{
		get
		{
			if (base.setupData != null)
			{
				return base.setupData.finishedThisCycle;
			}
			return false;
		}
	}

	private bool Cleaned
	{
		get
		{
			if (Finished)
			{
				return base.setupData.finishedMaxDifficultyThisCycle >= ScriptableObjectSingleton<GameStatisticData>.instance.topDifficulty;
			}
			return false;
		}
	}

	private bool IsFocusStar => constellationDetailPanel.focusStar == this;

	private Constellation.ConstellationType? constellationType => constellation?.constellationType;

	protected override void Init()
	{
		constellation = GetUIInParents<Constellation>();
		constellationDetailPanel = GetUIInParents<ConstellationDetailPanel>();
		if (presetContractDef != null)
		{
			_setupData = CellUI<ContractState>.applicationState.contractManager.GetContractState(presetContractDef);
			text.text = base.setupData.contractDef.Name;
			SetDirty();
		}
	}

	protected override void OnSetup(ContractState contractState)
	{
		text.text = contractState.contractDef.Name;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (!Unlocked)
		{
			starImage.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.lockedStarColor;
		}
		else if (!Finished)
		{
			starImage.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.unlockedStarColor;
		}
		else if (Cleaned)
		{
			starImage.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.clearedStarColor;
		}
		else
		{
			starImage.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.finishedStarColor;
		}
		unlockImage.gameObject.SetActive(!Unlocked);
		finishImage.gameObject.SetActive(Finished);
		glowImage.gameObject.SetActive(base.setupData.contractDef.contractType == ContractDef.ContractType.HardChallenge || base.setupData.contractDef.contractType == ContractDef.ContractType.Story);
		if (IsFocusStar)
		{
			starImage.transform.localScale = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.focusStarScale * Vector3.one;
		}
		else
		{
			starImage.transform.localScale = Vector3.one;
		}
		switch (base.setupData.contractDef.contractType)
		{
		case ContractDef.ContractType.BaseGame:
		case ContractDef.ContractType.Challenge:
		case ContractDef.ContractType.HardChallenge:
			starImage.sprite = challengeStartContractSprite;
			glowImage.sprite = challengeStartContractGlowSprite;
			glowImage.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.hardChallengeStarGlowColor;
			glowImage.color = glowImage.color.WithA(starImage.color.maxColorComponent);
			break;
		case ContractDef.ContractType.WeekShuffle:
			starImage.sprite = dailyShuffleContractSprite;
			glowImage.sprite = challengeStartContractGlowSprite;
			break;
		case ContractDef.ContractType.Story:
			starImage.sprite = StoryContractSprite;
			glowImage.sprite = StoryContractGlowSprite;
			glowImage.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.storyStarGlowColor;
			glowImage.color = glowImage.color.WithA(starImage.color.maxColorComponent);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public ChallengeStar GetPrevious()
	{
		if (constellation?.startStar == null)
		{
			return null;
		}
		return constellation.starLineConnections.Find((Constellation.StarLineConnection s) => s.to == this)?.from;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		constellationDetailPanel.Focus(this);
	}
}
