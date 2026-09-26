using System.Collections.Generic;
using System.Linq;
using Proto.Types;

public class zbHeadHandler
{
	public HashSet<int> localOwnedAvatars = new HashSet<int>();

	public void Init()
	{
		localOwnedAvatars = LoadLocalOwnedAvatars();
		SyncAvatarsFromServer(((IEnumerable<int>)zbServerData.sGameData.HeadList).ToList());
	}

	public bool HasHeadFrameNotify()
	{
		foreach (PropDetail prop in zbServerData.sGameData.PropList)
		{
			if (HasHeadFrame(prop.Tid))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHeadFrame(int id)
	{
		foreach (PropDetail prop in zbServerData.sGameData.PropList)
		{
			if (prop.Tid == id)
			{
				zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey(prop.Tid);
				if (recordByKey != null && recordByKey.type == 11)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasHeadFrameFirstNotify()
	{
		zbPlayerHeadFrameCsvTableData records = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecords();
		foreach (PropDetail prop in zbServerData.sGameData.PropList)
		{
			if (!HasHeadFrame(prop.Tid))
			{
				continue;
			}
			foreach (KeyValuePair<int, zbPlayerHeadFrameCsvRecord> record in records.records)
			{
				if (record.Value.propId == prop.Tid && !IsHeadFrameUnlocker(record.Key))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!HasHeadFrameFirstNotify())
		{
			return HasNewHead();
		}
		return true;
	}

	public bool IsHeadFrameUnlocker(int HeadFrameId)
	{
		foreach (int headFrame in zbServerData.sGameData.HeadFrameList)
		{
			if (headFrame == HeadFrameId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHeadUnlocker(int HeadId)
	{
		foreach (int head in zbServerData.sGameData.HeadList)
		{
			if (head == HeadId)
			{
				return true;
			}
		}
		return false;
	}

	public HashSet<int> LoadLocalOwnedAvatars()
	{
		string dataFromFile = zbGameSave.Instance.GetDataFromFile("ownedAvatars" + zbSelf.Instance.GetRoleId());
		HashSet<int> hashSet = new HashSet<int>();
		if (!string.IsNullOrEmpty(dataFromFile))
		{
			string[] array = dataFromFile.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (int.TryParse(array[i], out var result))
				{
					hashSet.Add(result);
				}
			}
		}
		return hashSet;
	}

	public void SyncAvatarsFromServer(List<int> ownedAvatarsFromServer)
	{
		foreach (int item in ownedAvatarsFromServer.Except(localOwnedAvatars))
		{
			if (item != 1)
			{
				zbSelf.Instance.newTagHandler.AddNewAvatar(item);
			}
		}
		localOwnedAvatars = new HashSet<int>(ownedAvatarsFromServer);
		SaveLocalOwnedAvatars(localOwnedAvatars);
	}

	public void OnAvatarViewed(int avatarId)
	{
		if (zbSelf.Instance.newTagHandler.IsNewAvatar(avatarId))
		{
			zbSelf.Instance.newTagHandler.RemoveNewAvatar(avatarId);
		}
	}

	private void SaveLocalOwnedAvatars(HashSet<int> avatars)
	{
		string jsonData = string.Join(",", avatars);
		zbGameSave.Instance.SaveDataToFile("ownedAvatars" + zbSelf.Instance.GetRoleId(), jsonData);
	}

	public bool HasNewHead()
	{
		foreach (int head in zbServerData.sGameData.HeadList)
		{
			if (zbSelf.Instance.newTagHandler.IsNewAvatar(head))
			{
				return true;
			}
		}
		return false;
	}
}
