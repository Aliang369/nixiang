using System.Collections.Generic;
using Proto.Types;
using UnityEngine;

public class zbLeaverMgr
{
	public List<zbLeaver> workerList = new List<zbLeaver>();

	private GameObject m_objScene;

	private GameObject m_prefab;

	private Transform m_rootTransform;

	public void Init(GameObject objScene)
	{
		m_objScene = objScene;
		m_prefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/worker/leaver.prefab");
		m_rootTransform = ((Component)m_objScene.transform.Find("homeMap")).transform.Find("front").Find("root_worker").Find("leaver");
	}

	public void Free()
	{
	}

	public void Build()
	{
	}

	public void Refresh()
	{
	}

	public void Update()
	{
	}

	public void OnRoomDestroy()
	{
		foreach (zbLeaver worker in workerList)
		{
			worker.m_fsm.SetState(ELeaverState.RoomDestroyNotfiy);
		}
	}

	public zbLeaver AddLeaver(WorkerDetail workerDetail, WorkerStyleInfo initStyleInfo, Vector3 position, bool faceLeft)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		zbLeaver component = Object.Instantiate<GameObject>(m_prefab).GetComponent<zbLeaver>();
		((Component)component).transform.SetParent(m_rootTransform);
		component.Init(workerDetail, initStyleInfo, position, faceLeft);
		workerList.Add(component);
		return component;
	}

	public void RemoveLeaver(int workerId)
	{
		workerList.RemoveAll(delegate(zbLeaver worker)
		{
			if ((Object)(object)worker != (Object)null && (Object)(object)((Component)worker).gameObject != (Object)null && worker.m_homelandWorkerDetail.WorkerId == workerId)
			{
				Object.Destroy((Object)(object)((Component)worker).gameObject);
				return true;
			}
			return false;
		});
	}
}
