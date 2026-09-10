using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using UnityEngine;

public class ElectronicSingerEvent : WorldEvent
{
	public abstract class SingerEventOption : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		[LocalizeText("事件选项名称")]
		public string choiceName;

		[LocalizeText("事件选项描述")]
		public string choiceDescription;

		public CheckNode choosable;

		public CheckNode presenceCheck;

		public bool showTextReference = true;

		public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public abstract void Execute(ElectronicSingerEvent owner, Action onExecute);

		public virtual List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode> { presenceCheck, choosable };
		}
	}

	public class SingerEffectOption : SingerEventOption
	{
		public Effect effect;

		public override void Execute(ElectronicSingerEvent owner, Action onExecute)
		{
			effect?.Cast(owner.tempGameEvent);
			onExecute?.Invoke();
		}

		public override List<IEffectNode> GetChildren()
		{
			List<IEffectNode> children = base.GetChildren();
			if (effect != null)
			{
				children.Add(effect);
			}
			return children;
		}
	}

	public class SingerBattleOption : SingerEventOption
	{
		[LocalizeText("事件描述")]
		public string description = "The enemy prepares to attack!";

		[LocalizeText("按钮文本")]
		public string enterBattleButtonText = "Fight";

		[LocalizeText("按钮文本")]
		public string enterBattleDescription = "Start the battle immediately.";

		public EnterBattleEffect battleEffect = new EnterBattleEffect();

		public override void Execute(ElectronicSingerEvent owner, Action onExecute)
		{
			owner.currentDisplayInfo.ClearChoice();
			owner.currentDisplayInfo.description = description;
			WorldEventChoice worldEventChoice = new WorldEventChoice();
			worldEventChoice.buttonText = enterBattleButtonText;
			worldEventChoice.descriptionText = enterBattleDescription;
			if (battleEffect != null && battleEffect.bonusItems != null)
			{
				foreach (ItemDefinition bonusItem in battleEffect.bonusItems)
				{
					if (bonusItem != null)
					{
						worldEventChoice.AddDisplayItem(new Item(bonusItem));
					}
				}
			}
			worldEventChoice.executeAction = delegate
			{
				battleEffect?.Cast(owner.tempGameEvent);
			};
			owner.currentDisplayInfo.AddChoice(worldEventChoice);
			owner.SetPanelDirty();
		}

		public override List<IEffectNode> GetChildren()
		{
			List<IEffectNode> children = base.GetChildren();
			children.Add(battleEffect);
			return children;
		}
	}

	public class SingerBattleStage : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		[LocalizeText("事件描述")]
		public string description = "The enemy prepares to attack!";

		[LocalizeText("按钮文本")]
		public string enterBattleButtonText = "Fight";

		[LocalizeText("按钮文本")]
		public string enterBattleDescription = "Start the battle immediately.";

		public EnterBattleEffect battleEffect = new EnterBattleEffect();

		public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public void Execute(ElectronicSingerEvent owner)
		{
			owner.currentDisplayInfo.ClearChoice();
			owner.currentDisplayInfo.description = description;
			WorldEventChoice worldEventChoice = new WorldEventChoice();
			worldEventChoice.buttonText = enterBattleButtonText;
			worldEventChoice.descriptionText = enterBattleDescription;
			if (battleEffect != null && battleEffect.bonusItems != null)
			{
				foreach (ItemDefinition bonusItem in battleEffect.bonusItems)
				{
					if (bonusItem != null)
					{
						worldEventChoice.AddDisplayItem(new Item(bonusItem));
					}
				}
			}
			worldEventChoice.executeAction = delegate
			{
				battleEffect?.Cast(owner.tempGameEvent);
			};
			owner.currentDisplayInfo.AddChoice(worldEventChoice);
			owner.SetPanelDirty();
		}

		public List<IEffectNode> GetChildren()
		{
			List<IEffectNode> list = new List<IEffectNode>();
			if (battleEffect != null)
			{
				list.Add(battleEffect);
			}
			return list;
		}
	}

	public class LyricData
	{
		[LocalizeText("歌词")]
		public string lyric;
	}

	public class SingerStage
	{
		[LocalizeText("事件描述")]
		public string startStageDescription = "The electronic singer begins to sing:";

		[LocalizeText("事件描述")]
		public string roundStageDescription = "The singer looks at you, waiting for the next line.";

		[LocalizeText("事件描述")]
		public string firstWrongStageDescription = "You seem to have forgotten the lyrics...";

		[LocalizeText("事件描述")]
		public string secondWrongStageDescription = "The audience looks displeased...";

		[LocalizeText("事件描述")]
		public string rewardStageDescription = "The performance was a success!";

		public List<LyricData> lyricPool1 = new List<LyricData>();

		public List<LyricData> lyricPool2 = new List<LyricData>();

		public List<LyricData> lyricPool3 = new List<LyricData>();

		[OdinSerialize]
		public List<SingerEventOption> costPool = new List<SingerEventOption>();

		[OdinSerialize]
		public List<SingerEventOption> startOptions = new List<SingerEventOption>();

		[OdinSerialize]
		public List<SingerEventOption> refuseOptions = new List<SingerEventOption>();

		[LocalizeText("选项名称")]
		public List<string> recoveryOptionNames = new List<string>();

		[LocalizeText("选项名称")]
		public string fallbackRecoveryOptionName = "其实是……";

		[LocalizeText("选项名称")]
		public string bluffOptionName = "";

		public List<SingerEventOption> apologyOptions = new List<SingerEventOption>();

		public List<SingerEventOption> battleOptions = new List<SingerEventOption>();

		[OdinSerialize]
		public SingerBattleStage thirdRoundFailBattleStage = new SingerBattleStage();

		public List<SingerEventOption> finalRewards = new List<SingerEventOption>();

		[LocalizeText("事件选项后描述")]
		public List<string> finalRewardChosenDescriptions = new List<string>();

		[NonSerialized]
		[HideInInspector]
		private int currentRoundIndex;

		[NonSerialized]
		[HideInInspector]
		private List<LyricData> correctLyrics = new List<LyricData>();

		[NonSerialized]
		[HideInInspector]
		private List<List<LyricData>> roundOptions = new List<List<LyricData>>();

		[NonSerialized]
		[HideInInspector]
		private List<LyricData> currentUnselectedOptions = new List<LyricData>();

		public void Init(ElectronicSingerEvent owner)
		{
			currentRoundIndex = 0;
			if (correctLyrics == null)
			{
				correctLyrics = new List<LyricData>();
			}
			else
			{
				correctLyrics.Clear();
			}
			if (roundOptions == null)
			{
				roundOptions = new List<List<LyricData>>();
			}
			else
			{
				roundOptions.Clear();
			}
			if (currentUnselectedOptions == null)
			{
				currentUnselectedOptions = new List<LyricData>();
			}
			else
			{
				currentUnselectedOptions.Clear();
			}
			SetupRoundData(lyricPool1);
			SetupRoundData(lyricPool2);
			SetupRoundData(lyricPool3);
			ShowStartStage(owner);
		}

		private void SetupRoundData(List<LyricData> pool)
		{
			if (pool == null || pool.Count < 3)
			{
				Debug.LogError("[ElectronicSingerEvent] Lyric pool needs at least 3 items!");
				return;
			}
			List<LyricData> list = pool.OrderBy((LyricData x) => UnityEngine.Random.value).Take(3).ToList();
			LyricData item = list[0];
			correctLyrics.Add(item);
			roundOptions.Add(list.OrderBy((LyricData x) => UnityEngine.Random.value).ToList());
		}

		public void ShowStartStage(ElectronicSingerEvent owner)
		{
			string text = string.Join(", ", correctLyrics.Select((LyricData x) => x.lyric));
			owner.currentDisplayInfo.description = startStageDescription + "\n\"" + text + "\"";
			owner.currentDisplayInfo.ClearChoice();
			if (startOptions != null && startOptions.Count > 0)
			{
				AddEventOption(owner, startOptions[0], delegate
				{
					ShowRound(owner, 0);
				});
			}
			if (refuseOptions != null && refuseOptions.Count > 0)
			{
				SingerEventOption opt = refuseOptions[0];
				AddEventOption(owner, opt, delegate
				{
					owner.FinishedWorldEvent();
				});
			}
			owner.SetPanelDirty();
		}

		private void ShowRound(ElectronicSingerEvent owner, int roundIndex)
		{
			if (roundIndex >= 3)
			{
				ShowRewardStage(owner);
				return;
			}
			currentRoundIndex = roundIndex;
			owner.currentDisplayInfo.description = roundStageDescription;
			owner.currentDisplayInfo.ClearChoice();
			List<LyricData> options = roundOptions[roundIndex];
			foreach (LyricData opt in options)
			{
				AddEventOption(owner, null, delegate
				{
					if (opt == correctLyrics[roundIndex])
					{
						ShowRound(owner, roundIndex + 1);
					}
					else
					{
						currentUnselectedOptions = options.Where((LyricData x) => x != opt).ToList();
						ShowFirstWrongStage(owner);
					}
				}, opt.lyric);
			}
			owner.SetPanelDirty();
		}

		private void ShowFirstWrongStage(ElectronicSingerEvent owner)
		{
			owner.currentDisplayInfo.description = firstWrongStageDescription;
			owner.currentDisplayInfo.ClearChoice();
			for (int i = 0; i < currentUnselectedOptions.Count; i++)
			{
				LyricData lyricData = currentUnselectedOptions[i];
				string overrideDescription = ((recoveryOptionNames != null && i < recoveryOptionNames.Count) ? recoveryOptionNames[i] : fallbackRecoveryOptionName);
				AddEventOption(owner, null, delegate
				{
					if (lyricData == correctLyrics[currentRoundIndex])
					{
						ShowRound(owner, currentRoundIndex + 1);
					}
					else if (currentRoundIndex == 2)
					{
						if (thirdRoundFailBattleStage != null)
						{
							thirdRoundFailBattleStage.Execute(owner);
						}
						else if (battleOptions != null && battleOptions.Count > 0)
						{
							battleOptions[0].Execute(owner, null);
						}
						else
						{
							owner.FinishedWorldEvent();
						}
					}
					else
					{
						ShowSecondWrongStage(owner);
					}
				}, lyricData.lyric, overrideDescription);
			}
			if (currentRoundIndex != 2 && costPool != null && costPool.Count > 0)
			{
				SingerEventOption opt = costPool[UnityEngine.Random.Range(0, costPool.Count)];
				AddEventOption(owner, opt, delegate
				{
					ShowRound(owner, currentRoundIndex + 1);
				}, bluffOptionName);
			}
			owner.SetPanelDirty();
		}

		private void ShowSecondWrongStage(ElectronicSingerEvent owner)
		{
			owner.currentDisplayInfo.description = secondWrongStageDescription;
			owner.currentDisplayInfo.ClearChoice();
			if (apologyOptions != null && apologyOptions.Count > 0)
			{
				foreach (SingerEventOption apologyOption in apologyOptions)
				{
					AddEventOption(owner, apologyOption, delegate
					{
						ShowRound(owner, currentRoundIndex + 1);
					});
				}
			}
			if (battleOptions != null && battleOptions.Count > 0)
			{
				foreach (SingerEventOption battleOption in battleOptions)
				{
					SingerBattleOption singerBattleOpt = battleOption as SingerBattleOption;
					if (singerBattleOpt != null)
					{
						AddEventOption(owner, singerBattleOpt, delegate
						{
							singerBattleOpt.battleEffect?.Cast(owner.tempGameEvent);
						}, null, null, executeOption: false);
					}
					else
					{
						AddEventOption(owner, battleOption, null);
					}
				}
			}
			owner.SetPanelDirty();
		}

		private void ShowRewardStage(ElectronicSingerEvent owner)
		{
			owner.currentDisplayInfo.description = rewardStageDescription;
			owner.currentDisplayInfo.ClearChoice();
			if (finalRewards != null)
			{
				for (int i = 0; i < finalRewards.Count; i++)
				{
					SingerEventOption opt = finalRewards[i];
					string chosenDescription = ((finalRewardChosenDescriptions != null && i < finalRewardChosenDescriptions.Count) ? finalRewardChosenDescriptions[i] : null);
					AddEventOption(owner, opt, delegate
					{
						if (!string.IsNullOrEmpty(chosenDescription))
						{
							owner.EnqueueAction(new ActionAction(delegate
							{
								owner.RefreshAfterChoose(chosenDescription);
							}));
						}
						else
						{
							owner.FinishedWorldEvent();
						}
					});
				}
			}
			owner.SetPanelDirty();
		}

		private void AddEventOption(ElectronicSingerEvent owner, SingerEventOption opt, Action onExecute, string overrideName = null, string overrideDescription = null, bool executeOption = true)
		{
			string text = overrideName ?? opt?.choiceName;
			if (string.IsNullOrEmpty(text) || (opt != null && opt.presenceCheck != null && opt.presenceCheck.Check(owner.tempGameEvent) == false))
			{
				return;
			}
			WorldEventChoice worldEventChoice = new WorldEventChoice();
			worldEventChoice.buttonText = text;
			string rawDescription = overrideDescription ?? opt?.choiceDescription ?? "";
			worldEventChoice.descriptionText = TextDataPreset.FormatEffectObjectDescription(owner, rawDescription, owner.tempGameEvent, out var generator);
			worldEventChoice.executeAbleChecker = () => opt == null || opt.choosable == null || opt.choosable.Check(owner.tempGameEvent) == true;
			if (opt != null && opt.showTextReference && generator != null)
			{
				foreach (Item item in generator.refLinkObjects.OfType<Item>())
				{
					worldEventChoice.AddDisplayItem(item);
				}
				foreach (GameSkill item2 in generator.refLinkObjects.OfType<GameSkill>())
				{
					worldEventChoice.AddDisplaySkill(item2);
				}
				foreach (Rune item3 in generator.refLinkObjects.OfType<Rune>())
				{
					worldEventChoice.AddDisplayRune(item3);
				}
				foreach (Enchant item4 in generator.refLinkObjects.OfType<Enchant>())
				{
					worldEventChoice.AddDisplayEnchant(item4);
				}
				foreach (Influence item5 in generator.refLinkObjects.OfType<Influence>())
				{
					worldEventChoice.AddDisplayInfluence(item5);
				}
			}
			if (opt is SingerBattleOption { battleEffect: not null } singerBattleOption && singerBattleOption.battleEffect.bonusItems != null)
			{
				foreach (ItemDefinition bonusItem in singerBattleOption.battleEffect.bonusItems)
				{
					if (bonusItem != null)
					{
						worldEventChoice.AddDisplayItem(new Item(bonusItem));
					}
				}
			}
			worldEventChoice.executeAction = delegate
			{
				if ((opt != null) & executeOption)
				{
					opt.Execute(owner, onExecute);
				}
				else
				{
					onExecute?.Invoke();
				}
			};
			owner.currentDisplayInfo.AddChoice(worldEventChoice);
		}

		public List<IEffectNode> GetChildren()
		{
			List<IEffectNode> list = new List<IEffectNode>();
			if (costPool != null)
			{
				foreach (SingerEventOption item in costPool)
				{
					if (item != null)
					{
						list.Add(item);
					}
				}
			}
			if (startOptions != null)
			{
				foreach (SingerEventOption startOption in startOptions)
				{
					if (startOption != null)
					{
						list.Add(startOption);
					}
				}
			}
			if (refuseOptions != null)
			{
				foreach (SingerEventOption refuseOption in refuseOptions)
				{
					if (refuseOption != null)
					{
						list.Add(refuseOption);
					}
				}
			}
			if (apologyOptions != null)
			{
				foreach (SingerEventOption apologyOption in apologyOptions)
				{
					if (apologyOption != null)
					{
						list.Add(apologyOption);
					}
				}
			}
			if (battleOptions != null)
			{
				foreach (SingerEventOption battleOption in battleOptions)
				{
					if (battleOption != null)
					{
						list.Add(battleOption);
					}
				}
			}
			if (thirdRoundFailBattleStage != null)
			{
				list.Add(thirdRoundFailBattleStage);
			}
			if (finalRewards != null)
			{
				foreach (SingerEventOption finalReward in finalRewards)
				{
					if (finalReward != null)
					{
						list.Add(finalReward);
					}
				}
			}
			return list;
		}
	}

	[OdinSerialize]
	public SingerStage singerStage = new SingerStage();

	[NonSerialized]
	[HideInInspector]
	public GameEvent tempGameEvent;

	protected override void Init()
	{
		base.Init();
		tempGameEvent = new GameEvent(GameEvent.Type.WorldEventChoose, GameEvent.TriggerType.Real);
		tempGameEvent.gameState = gameState;
		tempGameEvent.SetCaster(this);
		tempGameEvent.TargetWorldEvent = this;
		singerStage.Init(this);
	}

	public override List<IEffectNode> GetChildren()
	{
		return singerStage.GetChildren();
	}
}
