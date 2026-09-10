using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class GlobalGameRecordManager : GlobalGameManager
{
	public class GameRecordPreviewInfo
	{
		[SaveField(null, null)]
		public string gameGuid;

		[SaveField(null, null)]
		public string playerName;

		[SaveField(null, null)]
		public PlayerCharacterDef character;

		[NonSerialized]
		public Sprite avatar;

		[SaveField(null, null)]
		public bool success;

		[SaveField(null, null)]
		public Contract contract;

		[SaveField(null, null)]
		public int progress;

		[SaveField(null, null)]
		public long gameTime;

		[SaveField(null, null)]
		public int rankingScore;

		[SaveField(null, null)]
		public PVPChanllengeGameRecord pvpChanllengeRecord;

		[SaveField(null, null)]
		public long finishDateTime;

		public Sprite CharacterRecordIcon
		{
			get
			{
				object obj = character?.obsession?.icon;
				if (obj == null)
				{
					obj = character?.globalItemIcon;
					if (obj == null)
					{
						PlayerCharacterDef playerCharacterDef = character;
						if ((object)playerCharacterDef == null)
						{
							return null;
						}
						obj = playerCharacterDef.headIcon;
					}
				}
				return (Sprite)obj;
			}
		}

		public GameRecordPreviewInfo()
		{
		}

		public GameRecordPreviewInfo(GameRecord gameRecord)
		{
			gameGuid = gameRecord.gameGuid;
			playerName = gameRecord.playerName;
			character = gameRecord.character;
			avatar = CharacterRecordIcon;
			success = gameRecord.success;
			contract = gameRecord.contract;
			progress = gameRecord.progress;
			gameTime = gameRecord.gameTime;
			rankingScore = gameRecord.rankScore;
			pvpChanllengeRecord = gameRecord.pvpChallengeGameRecord;
			finishDateTime = gameRecord.finishTime.Ticks;
		}

		public void LoadGameRecord(ApplicationStateInfo applicationState, Action<GameRecord> onFinish)
		{
			if (GlobalConfigManager.instance.enableNetwork)
			{
				FetchGameRecordDetailRequest request = new FetchGameRecordDetailRequest(this);
				GeneralGameAsyncOperation<FetchGameRecordDetailRequest, FetchGameRecordDetailResult> op = new GeneralGameAsyncOperation<FetchGameRecordDetailRequest, FetchGameRecordDetailResult>(request, applicationState.ChillyRoomSDK.GameRecord.FetchGameRecordDetailAsync);
				op.OnComplete(delegate
				{
					GameRecord obj = op.OpResult.GameRecord.ToClient(new NetworkConvertContext(applicationState));
					onFinish?.Invoke(obj);
				}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_game_record_detail);
			}
		}
	}

	private const int ProfileRecordPageSize = 100;

	[SaveField(null, null)]
	public List<EnemyInfoData> cutsceneDebutFinishedEnemies;

	[SaveField(null, null)]
	public List<GameRecordPreviewInfo> records;

	[NonSerialized]
	public bool recordProfileDataLoaded;

	[NonSerialized]
	public bool recordProfileDataLoading;

	[NonSerialized]
	public GameAsyncOperation recordProfileDataLoadingOperation;

	public int FinishedGameCount => base.applicationState.gameStatisticsManager.gameCount;

	public int SuccessfulGameCount => base.applicationState.gameStatisticsManager.winGameCount;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (records == null)
		{
			records = new List<GameRecordPreviewInfo>();
		}
		if (cutsceneDebutFinishedEnemies == null)
		{
			cutsceneDebutFinishedEnemies = new List<EnemyInfoData>();
		}
		recordProfileDataLoaded = !GlobalConfigManager.instance.enableNetwork;
		recordProfileDataLoading = false;
		recordProfileDataLoadingOperation = null;
	}

	public GameAsyncOperation RequestRecordProfileDataIfNeeded()
	{
		if (recordProfileDataLoaded)
		{
			return null;
		}
		if (recordProfileDataLoading)
		{
			return recordProfileDataLoadingOperation;
		}
		FetchGameRecordBriefPageRequest request = new FetchGameRecordBriefPageRequest
		{
			Page = 1,
			Size = 100,
			AllowStaleRead = true
		};
		GeneralGameAsyncOperation<FetchGameRecordBriefPageRequest, FetchGameRecordBriefPageResult> op = new GeneralGameAsyncOperation<FetchGameRecordBriefPageRequest, FetchGameRecordBriefPageResult>(request, base.applicationState.ChillyRoomSDK.GameRecord.FetchGameRecordBriefPageAsync);
		recordProfileDataLoading = true;
		recordProfileDataLoadingOperation = op;
		op.OnComplete(delegate
		{
			FetchRecordProfileData(op.OpResult);
			recordProfileDataLoaded = true;
		}).OnOver(delegate
		{
			recordProfileDataLoading = false;
			recordProfileDataLoadingOperation = null;
		}).HideFaultPanel()
			.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_game_records);
		return op;
		void FetchRecordProfileData(FetchGameRecordBriefPageResult profileData)
		{
			NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
			records = profileData.GameRecordBriefs?.Select((GameRecordBrief r) => r.ToClient(context)).WhereNotNull().ToList() ?? new List<GameRecordPreviewInfo>();
		}
	}

	public void SaveGameRecord(GameRecord gameRecord)
	{
		records.Add(new GameRecordPreviewInfo(gameRecord));
		SavingUtil.SaveGameRecord(gameRecord, Bootstrap.instance.CurrentAccountID);
	}
}
