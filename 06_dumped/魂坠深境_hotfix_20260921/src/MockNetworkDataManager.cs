using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MockNetworkDataManager", menuName = "Singleton/MockNetworkDataManager")]
public class MockNetworkDataManager : ScriptableObjectSingleton<MockNetworkDataManager>
{
	public class MockNetworkRankingData
	{
		public ContractDef contract;

		public List<RankingManager.RankingData> rankingRecords;

		public RankingManager.RankingData localBestRecord;

		public List<RankingManager.RankingData> GetRandkingListData(int pageIndex)
		{
			return rankingRecords?.Where((RankingManager.RankingData d) => d.rankIndex > pageIndex * 20 && d.rankIndex < (pageIndex + 1) * 20 + 1).ToList();
		}

		public RankingManager.RankingData GetLocalBestRecord()
		{
			return localBestRecord;
		}

		private void AddData()
		{
			rankingRecords.Add(new RankingManager.RankingData
			{
				rankIndex = rankingRecords.Count + 1,
				recordPreviewInfo = rankingRecords[0].recordPreviewInfo
			});
		}
	}

	public class MockShareForerunnerDataSetup
	{
		public ItemDefinition item;

		public int stage;

		public string note;

		public string signature;

		public DraftMapRef leaveDraftMapRef;

		public int upvoteCount;

		public int downvoteCount;

		public ForerunnerLegacy ToData()
		{
			ForerunnerLegacy forerunnerLegacy = new ForerunnerLegacy();
			forerunnerLegacy.guid = Guid.NewGuid().ToString();
			forerunnerLegacy.SetItem(new Item(item));
			forerunnerLegacy.stage = stage;
			forerunnerLegacy.note = note;
			forerunnerLegacy.signature = signature;
			forerunnerLegacy.sourceDraftMap = leaveDraftMapRef;
			forerunnerLegacy.upvoteCount = upvoteCount;
			forerunnerLegacy.downvoteCount = downvoteCount;
			forerunnerLegacy.shareLegacy = true;
			return forerunnerLegacy;
		}
	}

	public class MockNetworkMailData
	{
		public string title;

		public string content;

		public string sender;

		public UnityDateTime sendTime;

		public List<GlobalRewardSetup> rewards;

		public MailData ToData(ApplicationStateInfo applicationState)
		{
			return new MailData
			{
				id = Guid.NewGuid(),
				title = title,
				content = content,
				sender = sender,
				SendTime = sendTime.UtcDateTime,
				attachRewards = rewards.Select((GlobalRewardSetup x) => x.GenerateReward(applicationState, GlobalRewardSourceType.Other, "")).ToList()
			};
		}
	}

	public class MockPVPDefenderData
	{
		public class MockBattleRecord
		{
			public int damageToChallenger;

			public FinishBattleEncounterAction.FinishType battleFinishType;

			public string challengePlayerNickName;

			public PVPDefenderBattleRecord ToData(PVPDefender defender, ApplicationStateInfo applicationState)
			{
				return new PVPDefenderBattleRecord
				{
					defenderPresetGuid = defender.guid,
					gameRecordId = null,
					dealtDamageToChallenger = damageToChallenger,
					challengerFinishType = battleFinishType,
					challengePlayerId = null,
					challengePlayerNickName = challengePlayerNickName,
					defenderPlayerId = Bootstrap.instance.CurrentAccountID,
					defenderPlayerNickName = applicationState.playerInfoManager.DataName,
					battleDateTime = new UnityDateTime(DateTime.UtcNow)
				};
			}
		}

		public string defenderName;

		public EnemyAssetRef enemyAvatarRef;

		public DeckPreset deckPreset;

		public List<MockBattleRecord> battleRecords;

		public PVPDefender ToData(ApplicationStateInfo applicationState)
		{
			PVPDefender data = new PVPDefender();
			data.guid = defenderName;
			data.sourcePlayerId = "888888888";
			data.sourcePlayerName = "测试账号名字";
			data.defenderName = defenderName;
			data.enemyAvatarRef = enemyAvatarRef;
			data.deckPreset = deckPreset;
			data.battleRecords = (from r in battleRecords.OrEmptyIfNull()
				select r.ToData(data, applicationState)).ToList();
			return data;
		}

		public void FromDate(PVPDefender data)
		{
			defenderName = data.defenderName;
			enemyAvatarRef = data.enemyAvatarRef;
			deckPreset = data.deckPreset;
		}
	}

	public List<MockNetworkRankingData> rankingDatas;

	public List<MockShareForerunnerDataSetup> shareForerunnerDataSetups;

	public List<MockShareForerunnerDataSetup> testLeaveHistoryDataSetups;

	public List<MockShareForerunnerDataSetup> testGotHistoryDataSetups;

	public List<MockNetworkMailData> mailDatas;

	public List<MockPVPDefenderData> pvpDefenderDatas;

	public RankingManager.RankingData GetLocalBestRecord(ContractDef contract)
	{
		using (List<MockNetworkRankingData>.Enumerator enumerator = rankingDatas.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.GetLocalBestRecord();
			}
		}
		return null;
	}

	public List<ForerunnerLegacy> GeneratePresetMessageTestData()
	{
		List<TextDataPreset.ForerunnerPresetMessage> enabledMessages = ForerunnerPresetMessageResolver.GetEnabledMessages();
		if (enabledMessages.Count < 2)
		{
			return new List<ForerunnerLegacy>();
		}
		ForerunnerLegacy forerunnerLegacy = testGotHistoryDataSetups?.FirstOrDefault()?.ToData();
		return new List<ForerunnerLegacy>
		{
			CreatePresetMessageTestLegacy(forerunnerLegacy, enabledMessages[1].id, null),
			CreatePresetMessageTestLegacy(forerunnerLegacy, null, forerunnerLegacy?.note ?? string.Empty),
			CreatePresetMessageTestLegacy(forerunnerLegacy, "#MSG#FORERUNNER_99999", forerunnerLegacy?.note ?? string.Empty),
			CreatePresetMessageTestLegacy(forerunnerLegacy, "#MSG#FORERUNNER_99998", null)
		};
	}

	private static ForerunnerLegacy CreatePresetMessageTestLegacy(ForerunnerLegacy template, string presetMessageId, string note)
	{
		return new ForerunnerLegacy
		{
			guid = Guid.NewGuid().ToString(),
			sourcePlayerId = 888888888L,
			legacyItem = template?.legacyItem,
			legacySkill = template?.legacySkill,
			stage = (template?.stage ?? 1),
			sourceDraftMap = template?.sourceDraftMap,
			note = note,
			presetMessageId = presetMessageId,
			signature = (template?.signature ?? string.Empty),
			shareLegacy = true
		};
	}
}
