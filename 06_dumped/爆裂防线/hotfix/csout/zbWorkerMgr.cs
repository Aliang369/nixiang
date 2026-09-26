using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine;

public class zbWorkerMgr
{
	public List<zbWorker> workerList = new List<zbWorker>();

	private GameObject m_objScene;

	private GameObject m_prefab;

	private Transform m_rootTransform;

	private bool m_buildInit;

	public void Init(GameObject objScene)
	{
		m_objScene = objScene;
		m_prefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/worker/worker.prefab");
		m_rootTransform = ((Component)m_objScene.transform.Find("homeMap")).transform.Find("front").Find("root_worker").Find("worker");
	}

	public void Free()
	{
		foreach (zbWorker worker in workerList)
		{
			if ((Object)(object)worker != (Object)null && (Object)(object)((Component)worker).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)worker).gameObject);
			}
		}
		workerList.Clear();
		m_prefab = null;
	}

	public IEnumerator BuildAsync(int batchSize)
	{
		m_buildInit = false;
		RepeatedField<WorkerDetail> workerList = zbServerData.sGameData.HomelandInfo.WorkerList;
		for (int i = 0; i < workerList.Count; i++)
		{
			WorkerDetail workerDetail = workerList[i];
			zbRoom zbRoom2 = ((workerDetail.RoomId == 0) ? zbHomeMgr.Instance.roomMgr.GetRoomDating() : zbHomeMgr.Instance.roomMgr.GetRoom(workerDetail.RoomId));
			if (!((Object)(object)zbRoom2 == (Object)null))
			{
				bool faceLeft = Rand.Range(0, 2) == 0;
				Vector3 randomWalkablePos = zbRoom2.GetRandomWalkablePos();
				AddWorker(workerDetail, randomWalkablePos, faceLeft, isBuild: true);
				if ((i + 1) % batchSize == 0)
				{
					yield return null;
				}
			}
		}
		m_buildInit = true;
	}

	public void Build()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		m_buildInit = false;
		foreach (WorkerDetail worker in zbServerData.sGameData.HomelandInfo.WorkerList)
		{
			zbRoom zbRoom2 = null;
			zbRoom2 = ((worker.RoomId == 0) ? zbHomeMgr.Instance.roomMgr.GetRoomDating() : zbHomeMgr.Instance.roomMgr.GetRoom(worker.RoomId));
			_ = (Object)(object)zbRoom2 == (Object)null;
			bool faceLeft = Rand.Range(0, 2) == 0;
			Vector3 randomWalkablePos = zbRoom2.GetRandomWalkablePos();
			AddWorker(worker, randomWalkablePos, faceLeft, isBuild: true);
		}
		m_buildInit = true;
	}

	public void Refresh()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		if (!m_buildInit)
		{
			return;
		}
		List<zbWorker> list = new List<zbWorker>();
		foreach (zbWorker worker in workerList)
		{
			if (zbSelf.Instance.homelandHandler.GetHomelandWorkerDetail(worker.m_homelandWorkerDetail.WorkerId) == null)
			{
				list.Add(worker);
				continue;
			}
			WorkerDetail homelandWorkerDetail = zbSelf.Instance.homelandHandler.GetHomelandWorkerDetail(worker.m_homelandWorkerDetail.WorkerId);
			worker.Refresh(homelandWorkerDetail);
		}
		foreach (zbWorker item in list)
		{
			zbHomeMgr.Instance.leaverMgr.AddLeaver(item.m_homelandWorkerDetail, item.initStyleInfo, ((Component)item).transform.position, item.IsFaceLeft());
			RemoveWorker(item.m_homelandWorkerDetail.WorkerId);
		}
		int num = 0;
		foreach (WorkerDetail worker2 in zbServerData.sGameData.HomelandInfo.WorkerList)
		{
			if (!HasCreate(worker2.WorkerId))
			{
				zbHomeMgr.Instance.roomMgr.GetRoom(worker2.RoomId);
				bool faceLeft = Rand.Range(0, 2) == 0;
				Vector3 position = zbRoomDefine.Instance.RecruiterOuterPosition - new Vector3((float)num * 1.8f, 0f, 0f);
				AddWorker(worker2, position, faceLeft);
				num++;
			}
		}
	}

	public void OnRoomDestroy()
	{
		foreach (zbWorker worker in workerList)
		{
			worker.fsm.OnRoomDestroyNotfiy();
		}
	}

	public void Update()
	{
	}

	public void AddWorker(WorkerDetail workerDetail, Vector3 position, bool faceLeft, bool isBuild = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		zbWorker component = Object.Instantiate<GameObject>(m_prefab).GetComponent<zbWorker>();
		((Component)component).transform.SetParent(m_rootTransform);
		((Component)component).transform.position = position;
		component.SetFacing(faceLeft);
		component.Refresh(workerDetail);
		if (isBuild)
		{
			component.RefreshRoomSuit();
		}
		workerList.Add(component);
	}

	public void RemoveWorker(int workerId)
	{
		workerList.RemoveAll(delegate(zbWorker worker)
		{
			if ((Object)(object)worker != (Object)null && (Object)(object)((Component)worker).gameObject != (Object)null && worker.m_homelandWorkerDetail.WorkerId == workerId)
			{
				Object.Destroy((Object)(object)((Component)worker).gameObject);
				return true;
			}
			return false;
		});
	}

	public bool HasCreate(int workerId)
	{
		foreach (zbWorker worker in workerList)
		{
			if (worker.m_homelandWorkerDetail.WorkerId == workerId)
			{
				return true;
			}
		}
		return false;
	}

	public zbWorker FindWorker(int workerId)
	{
		foreach (zbWorker worker in workerList)
		{
			if (worker.m_homelandWorkerDetail.WorkerId == workerId)
			{
				return worker;
			}
		}
		return null;
	}
}
