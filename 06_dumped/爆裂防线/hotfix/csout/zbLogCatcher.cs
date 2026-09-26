using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Proto.Enums;
using UnityEngine;
using UnityEngine.Networking;

public class zbLogCatcher : MonoBehaviour
{
	[Serializable]
	private class ExceptionRecord
	{
		public string eventId;

		public string exceptionKey;

		public string firstTime;

		public string lastTime;

		public int count;

		public string type;

		public string errorMsg;

		public string stack;

		public ExceptionExtraInfo extraInfo;
	}

	[Serializable]
	private class ExceptionExtraInfo
	{
		public string roleId;

		public string serverId;

		public string appResVersion;

		public string appVersion;

		public string appChannel;

		public string platform;
	}

	private static zbLogCatcher instance;

	private const string DumpUrl = "https://blfx.mydreamjoy.com/dump/clientdump2file.php";

	private const long MaxFileSize = 2097152L;

	private const int MaxExceptionCount = 50;

	private const float WriteInterval = 3f;

	private const float FirstUploadDelay = 5f;

	private const float UploadInterval = 30f;

	private const float UploadRetryDelay = 60f;

	private const float BattleCheckInterval = 3f;

	private WaitForSeconds writeWait;

	private WaitForSeconds firstUploadWait;

	private WaitForSeconds uploadWait;

	private WaitForSeconds uploadRetryWait;

	private WaitForSeconds battleCheckWait;

	private string logDirectory;

	private string currentLogFile;

	private readonly Dictionary<string, ExceptionRecord> exceptionRecords = new Dictionary<string, ExceptionRecord>();

	private readonly object exceptionLock = new object();

	private bool recordsDirty;

	private int recordsVersion;

	private void Awake()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		if ((Object)(object)instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		writeWait = new WaitForSeconds(3f);
		firstUploadWait = new WaitForSeconds(5f);
		uploadWait = new WaitForSeconds(30f);
		uploadRetryWait = new WaitForSeconds(60f);
		battleCheckWait = new WaitForSeconds(3f);
		logDirectory = Path.Combine(Application.persistentDataPath, "ExceptionLogs");
		Directory.CreateDirectory(logDirectory);
		SealPreviousCurrentFile();
		currentLogFile = Path.Combine(logDirectory, "exception_current.dump");
		Application.logMessageReceived += new LogCallback(HandleMessage);
		((MonoBehaviour)this).StartCoroutine(WriteLoop());
		((MonoBehaviour)this).StartCoroutine(UploadLoop());
	}

	private void OnDestroy()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if ((Object)(object)instance == (Object)(object)this)
		{
			Application.logMessageReceived -= new LogCallback(HandleMessage);
			instance = null;
		}
	}

	private void OnApplicationQuit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		Application.logMessageReceived -= new LogCallback(HandleMessage);
		FlushAllRecords();
		SealCurrentFile();
	}

	private void HandleMessage(string logString, string stackTrace, LogType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if ((int)type != 4)
		{
			return;
		}
		if (zbLoginUtil.appChannel == EChannelType.Zb)
		{
			string logEntry = $"[{type}] {logString}\n" + stackTrace + "\n\n";
			((MonoBehaviour)this).StartCoroutine(DelayShowErrorLog(logEntry));
			return;
		}
		string exceptionKey = GetExceptionKey(type, logString, stackTrace);
		lock (exceptionLock)
		{
			if (exceptionRecords.TryGetValue(exceptionKey, out var value))
			{
				value.count++;
				value.lastTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
				recordsDirty = true;
				recordsVersion++;
			}
			else if (exceptionRecords.Count < 50)
			{
				value = CreateExceptionRecord(exceptionKey, logString, stackTrace, type);
				exceptionRecords.Add(exceptionKey, value);
				recordsDirty = true;
				recordsVersion++;
			}
		}
	}

	private unsafe ExceptionRecord CreateExceptionRecord(string exceptionKey, string logString, string stackTrace, LogType type)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		string roleId = "-1";
		string serverId = "-1";
		if (zbGameStateMachine.Instance != null && zbGameStateMachine.Instance.GetStateId() == EGameState.Main)
		{
			if (zbSelf.Instance != null)
			{
				roleId = zbSelf.Instance.GetRoleId().ToString();
			}
			serverId = zbLoginUtil.loginServerId.ToString();
		}
		ExceptionExtraInfo extraInfo = new ExceptionExtraInfo
		{
			roleId = roleId,
			serverId = serverId,
			appResVersion = SafeGetResVersion(),
			appVersion = Application.version,
			appChannel = zbLoginUtil.appChannel.ToString(),
			platform = ((object)Application.platform/*cast due to constrained. prefix*/).ToString()
		};
		string text = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
		return new ExceptionRecord
		{
			eventId = Guid.NewGuid().ToString("N"),
			exceptionKey = exceptionKey,
			firstTime = text,
			lastTime = text,
			count = 1,
			type = ((object)(*(LogType*)(&type))/*cast due to constrained. prefix*/).ToString(),
			errorMsg = logString,
			stack = stackTrace,
			extraInfo = extraInfo
		};
	}

	private string SafeGetResVersion()
	{
		try
		{
			return zbSdkHelper.GetResVersion();
		}
		catch
		{
			return string.Empty;
		}
	}

	private unsafe static string GetExceptionKey(LogType type, string logString, string stackTrace)
	{
		return GetHash(((object)(*(LogType*)(&type))/*cast due to constrained. prefix*/).ToString() + "\n" + logString + "\n" + stackTrace);
	}

	private static string GetHash(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		using MD5 mD = MD5.Create();
		return BitConverter.ToString(mD.ComputeHash(bytes)).Replace("-", "").Substring(0, 16);
	}

	private IEnumerator WriteLoop()
	{
		while (true)
		{
			if (recordsDirty)
			{
				RewriteCurrentFile();
			}
			yield return writeWait;
		}
	}

	private void RewriteCurrentFile()
	{
		List<ExceptionRecord> list;
		int num;
		lock (exceptionLock)
		{
			if (!recordsDirty || exceptionRecords.Count == 0)
			{
				return;
			}
			list = new List<ExceptionRecord>(exceptionRecords.Values);
			num = recordsVersion;
		}
		try
		{
			EnsureCurrentFile();
			StringBuilder stringBuilder = new StringBuilder(4096);
			for (int i = 0; i < list.Count; i++)
			{
				AppendRecord(stringBuilder, list[i]);
			}
			string contents = stringBuilder.ToString();
			File.WriteAllText(currentLogFile, contents, Encoding.UTF8);
			lock (exceptionLock)
			{
				if (recordsVersion == num)
				{
					recordsDirty = false;
				}
			}
			if (new FileInfo(currentLogFile).Length >= 2097152)
			{
				SealCurrentFile();
			}
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"[zbLogCatcher] Rewrite failed: {arg}");
		}
	}

	private void AppendRecord(StringBuilder sb, ExceptionRecord record)
	{
		sb.AppendLine("============================================================");
		sb.AppendLine("Exception");
		sb.AppendLine("============================================================");
		sb.AppendLine("EventId: " + record.eventId);
		sb.AppendLine("ExceptionKey: " + record.exceptionKey);
		sb.AppendLine($"Count: {record.count}");
		sb.AppendLine("FirstTime: " + record.firstTime);
		sb.AppendLine("LastTime: " + record.lastTime);
		sb.AppendLine("Type: " + record.type);
		sb.AppendLine();
		sb.AppendLine("Error:");
		sb.AppendLine(record.errorMsg);
		sb.AppendLine();
		sb.AppendLine("StackTrace:");
		sb.AppendLine(record.stack);
		sb.AppendLine();
		sb.AppendLine("ExtraInfo:");
		ExceptionExtraInfo extraInfo = record.extraInfo;
		sb.AppendLine("roleId: " + extraInfo.roleId);
		sb.AppendLine("serverId: " + extraInfo.serverId);
		sb.AppendLine("appResVersion: " + extraInfo.appResVersion);
		sb.AppendLine("appVersion: " + extraInfo.appVersion);
		sb.AppendLine("appChannel: " + extraInfo.appChannel);
		sb.AppendLine("platform: " + extraInfo.platform);
		sb.AppendLine();
		sb.AppendLine();
	}

	private void EnsureCurrentFile()
	{
		if (string.IsNullOrEmpty(currentLogFile))
		{
			currentLogFile = Path.Combine(logDirectory, "exception_current.dump");
		}
		if (!File.Exists(currentLogFile))
		{
			using (File.Create(currentLogFile))
			{
			}
		}
	}

	private void FlushAllRecords()
	{
		try
		{
			lock (exceptionLock)
			{
				if (exceptionRecords.Count <= 0)
				{
					return;
				}
			}
			EnsureCurrentFile();
			RewriteCurrentFile();
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"[zbLogCatcher] Flush failed: {arg}");
		}
	}

	private void SealPreviousCurrentFile()
	{
		try
		{
			string text = Path.Combine(logDirectory, "exception_current.dump");
			if (File.Exists(text))
			{
				if (new FileInfo(text).Length <= 0)
				{
					File.Delete(text);
					return;
				}
				string newDumpFileName = GetNewDumpFileName();
				File.Move(text, newDumpFileName);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"[zbLogCatcher] Seal previous file failed: {arg}");
		}
	}

	private void SealCurrentFile()
	{
		try
		{
			if (string.IsNullOrEmpty(currentLogFile) || !File.Exists(currentLogFile) || new FileInfo(currentLogFile).Length <= 0)
			{
				return;
			}
			string newDumpFileName = GetNewDumpFileName();
			File.Move(currentLogFile, newDumpFileName);
			currentLogFile = Path.Combine(logDirectory, "exception_current.dump");
			lock (exceptionLock)
			{
				exceptionRecords.Clear();
				recordsDirty = false;
				recordsVersion++;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"[zbLogCatcher] Seal file failed: {arg}");
		}
	}

	private string GetNewDumpFileName()
	{
		string text = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
		string path = "exception_" + text + ".dump";
		string text2 = Path.Combine(logDirectory, path);
		int num = 1;
		while (File.Exists(text2))
		{
			path = $"exception_{text}_{num}.dump";
			text2 = Path.Combine(logDirectory, path);
			num++;
		}
		return text2;
	}

	private IEnumerator UploadLoop()
	{
		yield return firstUploadWait;
		while (true)
		{
			if (IsFighting())
			{
				yield return battleCheckWait;
				continue;
			}
			string[] dumpFiles = GetDumpFiles();
			bool hasFailure = false;
			for (int i = 0; i < dumpFiles.Length; i++)
			{
				if (IsFighting())
				{
					break;
				}
				bool success = false;
				yield return ((MonoBehaviour)this).StartCoroutine(UploadSingleDump(dumpFiles[i], delegate(bool result)
				{
					success = result;
				}));
				if (!success)
				{
					hasFailure = true;
					break;
				}
			}
			if (hasFailure)
			{
				yield return uploadRetryWait;
			}
			else
			{
				yield return uploadWait;
			}
		}
	}

	private bool IsFighting()
	{
		if (zbGameStateMachine.Instance != null)
		{
			return zbGameStateMachine.Instance.IsFighting();
		}
		return false;
	}

	private string[] GetDumpFiles()
	{
		if (!Directory.Exists(logDirectory))
		{
			return Array.Empty<string>();
		}
		string[] files = Directory.GetFiles(logDirectory, "*.dump");
		Array.Sort(files);
		return files;
	}

	private IEnumerator UploadSingleDump(string filePath, Action<bool> callback)
	{
		if (!File.Exists(filePath))
		{
			callback?.Invoke(obj: true);
			yield break;
		}
		List<IMultipartFormSection> list = new List<IMultipartFormSection> { (IMultipartFormSection)new MultipartFormFileSection("upload_file_minidump", File.ReadAllBytes(filePath), Path.GetFileName(filePath), "text/plain") };
		UnityWebRequest request = UnityWebRequest.Post("https://blfx.mydreamjoy.com/dump/clientdump2file.php", list);
		try
		{
			request.timeout = 30;
			yield return request.SendWebRequest();
			if ((int)request.result == 1)
			{
				lock (exceptionLock)
				{
					exceptionRecords.Clear();
					recordsDirty = false;
					recordsVersion++;
				}
				try
				{
					File.Delete(filePath);
				}
				catch (Exception arg)
				{
					Debug.LogError((object)("[zbLogCatcher] " + $"Delete dump failed: {arg}"));
				}
				callback?.Invoke(obj: true);
			}
			else
			{
				callback?.Invoke(obj: false);
			}
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}

	private IEnumerator DelayShowErrorLog(string logEntry)
	{
		yield return null;
		if ((Object)(object)zbUIMgr.Instance != (Object)null)
		{
			zbUIMgr.Instance.ShowLogError(logEntry);
		}
	}
}
