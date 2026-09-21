using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class SavingUtil
{
	public class ProfileSaveData
	{
		[SaveField(null, null)]
		public ApplicationStateInfo applicationState;

		[SaveField(null, null)]
		public string SaveDate;

		[SaveField(null, null)]
		public int version;

		[SaveField(null, null)]
		public string saveVersion;

		private ProfileSaveData()
		{
		}

		public ProfileSaveData(ApplicationStateInfo applicationState)
		{
			this.applicationState = applicationState;
			version = GlobalConfigManager.instance.localSaveVersion;
			saveVersion = Bootstrap.instance.globalSaveConfigData.saveVersion;
			SaveDate = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
		}
	}

	public class GlobalConfigSaveData
	{
		[SaveField(null, null)]
		public List<AccountData> accounts = new List<AccountData>();

		[SaveField(null, null)]
		public long game_active_times;

		[SaveField(null, null)]
		public List<string> viewedAnnouncementIds = new List<string>();

		[SaveField(null, null)]
		public bool privacyAgreed;

		[SaveField(null, null)]
		public string saveVersion;

		[SaveField(null, null)]
		public float sfxVolume = 1f;

		[SaveField(null, null)]
		public float musicVolume = 1f;

		[SaveField(null, null)]
		public float safeAreaLeft;

		[SaveField(null, null)]
		public bool safeAreaAutoInited;

		[SaveField(null, null)]
		public TimeScaleManager.TimeScaleType _battleTimeScaleTypeSetting;

		[SaveField(null, null)]
		public TimeScaleManager.TimeScaleType uiTimeScaleTypeSetting;

		[SaveField(null, null)]
		public bool enableCameraShake = true;

		[SaveField(null, null)]
		public bool quickUseSkill;

		[SaveField(null, null)]
		public AutoPlayButtonMode autoPlayButtonMode = AutoPlayButtonMode.Disabled;

		public bool renderSettingsDirty;

		[SaveField(null, null)]
		public RenderingManager.FrameRateMode _frameRateMode;

		[SaveField(null, null)]
		public RenderingManager.PerformanceMode _performanceMode;

		[SaveField(null, null)]
		public RenderingManager.ResolutionMode _resolutionMode;

		[SaveField(null, null)]
		public LocalizationManager.LanguageCode languageCode;

		public TimeScaleManager.TimeScaleType battleTimeScaleTypeSetting
		{
			get
			{
				return _battleTimeScaleTypeSetting;
			}
			set
			{
				_battleTimeScaleTypeSetting = value;
				Bootstrap.applicationState.eventManager.speedChanged.TriggerEvent();
			}
		}

		public RenderingManager.FrameRateMode frameRateMode
		{
			get
			{
				return _frameRateMode;
			}
			set
			{
				_frameRateMode = value;
				renderSettingsDirty = true;
			}
		}

		public RenderingManager.PerformanceMode performanceMode
		{
			get
			{
				return _performanceMode;
			}
			set
			{
				_performanceMode = value;
				renderSettingsDirty = true;
			}
		}

		public RenderingManager.ResolutionMode resolutionMode
		{
			get
			{
				return _resolutionMode;
			}
			set
			{
				_resolutionMode = value;
				renderSettingsDirty = true;
			}
		}

		private GlobalConfigSaveData()
		{
		}

		public GlobalConfigSaveData(string saveVersion)
		{
			this.saveVersion = saveVersion;
			UISafeArea.DeviceConfig deviceConfig = GlobalConfigManager.instance.safeAreaConfig.configs.Find((UISafeArea.DeviceConfig c) => c.id == SystemInfo.deviceModel);
			if (deviceConfig == null)
			{
				safeAreaLeft = 0f;
			}
			else
			{
				safeAreaLeft = deviceConfig.left;
			}
		}

		public void Save(ApplicationStateInfo applicationState = null)
		{
			SaveGlobalSaveConfigData(this);
			applicationState?.eventManager.GlobalConfigDataChanged.TriggerEvent(this);
		}
	}

	public class GameRecordSaveData
	{
		[SaveField(null, null)]
		public GameRecord gameRecord;

		private GameRecordSaveData()
		{
		}

		public GameRecordSaveData(GameRecord gameRecord)
		{
			this.gameRecord = gameRecord;
		}
	}

	public const string SaveFileName = "CurrentSave.json";

	public const string GlobalConfigFileName = "GlobalConfig.json";

	public const string TestAccount = "Default";

	private static readonly object PersistentWriteQueueLock = new object();

	private static Task persistentWriteTail = Task.CompletedTask;

	public static string GetAccountDirectory(string account)
	{
		return Path.Combine(Application.persistentDataPath, "Save_" + account);
	}

	public static string GetAccountProfileSavePath(string account)
	{
		return Path.Combine(GetAccountDirectory(account), "CurrentSave.json");
	}

	public static string GetAccountGameRecordSavePath(string account, string gameId)
	{
		return Path.Combine(GetAccountDirectory(account), "GameRecord-" + gameId + ".json");
	}

	public static List<string> GetAllAccountGameRecordsPath(string account)
	{
		string accountDirectory = GetAccountDirectory(account);
		if (!Directory.Exists(accountDirectory))
		{
			return new List<string>();
		}
		string[] files = Directory.GetFiles(accountDirectory);
		List<string> list = new List<string>();
		string[] array = files;
		foreach (string text in array)
		{
			if (text.Contains("GameRecord-"))
			{
				list.Add(text);
			}
		}
		return list;
	}

	public static string GetGlobalSaveConfigDataPath()
	{
		return Path.Combine(Application.persistentDataPath, "GlobalConfig.json");
	}

	public static void SaveProfileData(ApplicationStateInfo applicationState, string accountID)
	{
		try
		{
			string content = SerializeProfileData(applicationState);
			WriteToPersistent(GetAccountProfileSavePath(accountID), content);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "存档错误" + accountID, ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
		}
	}

	public static Task SaveProfileDataAsync(ApplicationStateInfo applicationState, string accountID)
	{
		try
		{
			string content = SerializeProfileData(applicationState);
			return WriteToPersistentAtomicAsync(GetAccountProfileSavePath(accountID), content);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "存档错误" + accountID, ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
			return Task.CompletedTask;
		}
	}

	private static string SerializeProfileData(ApplicationStateInfo applicationState)
	{
		ProfileSaveData obj = new ProfileSaveData(applicationState);
		return new SerializationExecutor(Bootstrap.instance.assetCatalog).Serialize(obj);
	}

	public static ProfileSaveData LoadOrCreateProfile(string accountID)
	{
		if (IsAccountProfileExist(accountID))
		{
			try
			{
				return LoadProfileData(accountID);
			}
			catch (Exception e)
			{
				TraceLog.Exception(e, "载入本地存档错误", ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
			}
		}
		return CreateProfileData(accountID);
	}

	public static ProfileSaveData LoadProfileData(string accountID)
	{
		try
		{
			string text = NormalizeProfileSaveDataJson(LoadFormPersistent(GetAccountProfileSavePath(accountID)));
			ProfileSaveData profileSaveData = new SerializationExecutor(Bootstrap.instance.assetCatalog).Deserialize<ProfileSaveData>(text);
			if (profileSaveData.saveVersion != Bootstrap.instance.globalSaveConfigData.saveVersion)
			{
				throw new Exception("存档版本不符合");
			}
			if (profileSaveData.version < GlobalConfigManager.instance.localSaveVersion)
			{
				TraceLog.Warn($"本地存档版本过低，重建存档({accountID})。存档版本:{profileSaveData.version} 当前版本:{GlobalConfigManager.instance.localSaveVersion}");
				return CreateProfileData(accountID);
			}
			profileSaveData.version = GlobalConfigManager.instance.localSaveVersion;
			return profileSaveData;
		}
		catch (Exception ex)
		{
			TraceLog.Exception(ex, "读档错误(" + accountID + ")", ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
			throw ex;
		}
	}

	private static string NormalizeProfileSaveDataJson(string text)
	{
		if (text.IsNullOrEmpty())
		{
			return text;
		}
		JObject jObject = JObject.Parse(text);
		JToken jToken = jObject["version"];
		if (jToken != null && jToken.Type == JTokenType.String)
		{
			jObject["saveVersion"] = jToken.ToString();
			jObject["version"] = 0;
		}
		return jObject.ToString();
	}

	public static ProfileSaveData CreateProfileData(string accountID)
	{
		DeleteProfileData(accountID);
		ProfileSaveData profileSaveData = new ProfileSaveData(null);
		string content = new SerializationExecutor(Bootstrap.instance.assetCatalog).Serialize(profileSaveData);
		WriteToPersistent(GetAccountProfileSavePath(accountID), content);
		return profileSaveData;
	}

	public static void DeleteProfileData(string accountID)
	{
		string accountDirectory = GetAccountDirectory(accountID);
		if (Directory.Exists(accountDirectory))
		{
			Directory.Delete(accountDirectory, recursive: true);
		}
	}

	public static GlobalConfigSaveData LoadOrCreateGlobalSaveConfigData()
	{
		if (!IsGlobalSaveConfigExist())
		{
			return new GlobalConfigSaveData(GlobalConfigManager.instance.saveVersion);
		}
		try
		{
			string text = LoadFormPersistent(GetGlobalSaveConfigDataPath());
			GlobalConfigSaveData globalConfigSaveData = new SerializationExecutor(null).Deserialize<GlobalConfigSaveData>(text);
			if (globalConfigSaveData == null || globalConfigSaveData.saveVersion != Bootstrap.instance.globalConfigManager.saveVersion)
			{
				globalConfigSaveData = new GlobalConfigSaveData(GlobalConfigManager.instance.saveVersion);
			}
			return globalConfigSaveData;
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "载入本地全局配置错误", ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
			return new GlobalConfigSaveData(GlobalConfigManager.instance.saveVersion);
		}
	}

	public static void SaveGlobalSaveConfigData(GlobalConfigSaveData configData)
	{
		string content = new SerializationExecutor(Bootstrap.instance.assetCatalog).Serialize(configData);
		WriteToPersistent(GetGlobalSaveConfigDataPath(), content);
	}

	public static void DeleteGlobalSaveConfigData()
	{
		string globalSaveConfigDataPath = GetGlobalSaveConfigDataPath();
		if (File.Exists(globalSaveConfigDataPath))
		{
			File.Delete(globalSaveConfigDataPath);
		}
	}

	public static void WipeAllLocalSaves()
	{
		string[] directories = Directory.GetDirectories(Application.persistentDataPath, "Save_*");
		for (int i = 0; i < directories.Length; i++)
		{
			Directory.Delete(directories[i], recursive: true);
		}
	}

	public static void SaveGameRecord(GameRecord gameRecord, string accountID)
	{
		GameRecordSaveData obj = new GameRecordSaveData(gameRecord);
		string content = new SerializationExecutor(Bootstrap.instance.assetCatalog).Serialize(obj);
		WriteToPersistent(GetAccountGameRecordSavePath(accountID, gameRecord.gameGuid), content);
	}

	public static GameRecord LoadGameRecord(string accountID, string gameId)
	{
		try
		{
			string text = LoadFormPersistent(GetAccountGameRecordSavePath(accountID, gameId));
			GameRecordSaveData gameRecordSaveData = new SerializationExecutor(Bootstrap.instance.assetCatalog).Deserialize<GameRecordSaveData>(text);
			if (gameRecordSaveData != null)
			{
				return gameRecordSaveData.gameRecord;
			}
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "读档错误(" + accountID + ")", ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
		}
		return null;
	}

	public static Task<GameRecord> LoadGameRecordAsync(string accountID, string gameId)
	{
		string path = GetAccountGameRecordSavePath(accountID, gameId);
		return Task.Run(delegate
		{
			try
			{
				string text = LoadFormPersistent(path);
				GameRecordSaveData gameRecordSaveData = new SerializationExecutor(Bootstrap.instance.assetCatalog).Deserialize<GameRecordSaveData>(text);
				if (gameRecordSaveData != null)
				{
					return gameRecordSaveData.gameRecord;
				}
			}
			catch (Exception ex)
			{
				TraceLog.Exception(ex, "读档错误(" + accountID + ")", ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
				throw ex;
			}
			return (GameRecord)null;
		});
	}

	public static List<GameRecord> LoadAllGameRecords(string accountID)
	{
		List<string> allAccountGameRecordsPath = GetAllAccountGameRecordsPath(accountID);
		List<GameRecord> list = new List<GameRecord>();
		foreach (string item in allAccountGameRecordsPath.OrEmptyIfNull())
		{
			try
			{
				string text = LoadFormPersistent(item);
				GameRecordSaveData gameRecordSaveData = new SerializationExecutor(Bootstrap.instance.assetCatalog).Deserialize<GameRecordSaveData>(text);
				if (gameRecordSaveData != null)
				{
					list.Add(gameRecordSaveData.gameRecord);
				}
			}
			catch (Exception e)
			{
				TraceLog.Exception(e, "读档错误(" + accountID + ")", ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
			}
		}
		return list;
	}

	public static string LoadFormPersistent(string path)
	{
		try
		{
			using StreamReader streamReader = new StreamReader(path);
			return streamReader.ReadToEnd();
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "文件读取错误(" + path + ")", ThinkingAnalyticsExtends.ExceptionType.Other);
			return null;
		}
	}

	public static bool IsFileExist(string path)
	{
		return File.Exists(path);
	}

	public static bool IsAccountProfileExist(string accountID)
	{
		return IsFileExist(GetAccountProfileSavePath(accountID));
	}

	public static bool IsGlobalSaveConfigExist()
	{
		return IsFileExist(GetGlobalSaveConfigDataPath());
	}

	public static void WriteToPersistent(string path, string content)
	{
		EnqueuePersistentWrite(delegate
		{
			WriteToPersistentInternal(path, content);
		}).GetAwaiter().GetResult();
	}

	public static Task WriteToPersistentAtomicAsync(string path, string content)
	{
		return EnqueuePersistentWrite(delegate
		{
			WriteToPersistentAtomicInternal(path, content);
		});
	}

	private static Task EnqueuePersistentWrite(Action writeAction)
	{
		lock (PersistentWriteQueueLock)
		{
			persistentWriteTail = persistentWriteTail.ContinueWith(delegate(Task previousWrite)
			{
				if (previousWrite.IsFaulted)
				{
					_ = previousWrite.Exception;
				}
				writeAction();
			}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
			return persistentWriteTail;
		}
	}

	private static void WriteToPersistentInternal(string path, string content)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		using StreamWriter streamWriter = new StreamWriter(path, append: false);
		streamWriter.Write(content);
	}

	private static void WriteToPersistentAtomicInternal(string path, string content)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		string text = path + ".tmp";
		using (StreamWriter streamWriter = new StreamWriter(text, append: false))
		{
			streamWriter.Write(content);
		}
		if (!File.Exists(path))
		{
			File.Move(text, path);
			return;
		}
		try
		{
			File.Replace(text, path, null);
		}
		catch (PlatformNotSupportedException)
		{
			File.Copy(text, path, overwrite: true);
			File.Delete(text);
		}
	}
}
