using System.Collections;
using UnityEngine;

public class zbHomeMgr
{
	private static zbHomeMgr sInstance;

	private static bool m_buildInit;

	private Coroutine m_workerCoroutine;

	private bool m_needRefreshAfterBuild;

	private bool m_needCheckFloorUnlockAfterBuild;

	public zbWallMgr wallMgr;

	public zbFloorMgr floorMgr;

	public zbRoomMgr roomMgr;

	public zbWorkerMgr workerMgr;

	public zbLeaverMgr leaverMgr;

	public zbRoomHeroMgr roomHeroMgr;

	public zbZombieMgr zombieMgr;

	public zbPaotaiMgr paotaiMgr;

	public zbRenderTarget renderTarget;

	public static zbHomeMgr Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbHomeMgr();
			}
			return sInstance;
		}
	}

	public void Init(GameObject objScene)
	{
		renderTarget = ((Component)GameObject.Find("Renders").transform.Find("rendertarget")).GetComponent<zbRenderTarget>();
		wallMgr = new zbWallMgr();
		floorMgr = new zbFloorMgr();
		roomMgr = new zbRoomMgr();
		workerMgr = new zbWorkerMgr();
		leaverMgr = new zbLeaverMgr();
		roomHeroMgr = new zbRoomHeroMgr();
		zombieMgr = new zbZombieMgr();
		paotaiMgr = new zbPaotaiMgr();
		wallMgr.Init(objScene);
		floorMgr.Init(objScene);
		roomMgr.Init(objScene);
		workerMgr.Init(objScene);
		leaverMgr.Init(objScene);
		roomHeroMgr.Init(objScene);
		zombieMgr.Init(objScene);
		paotaiMgr.Init(objScene);
	}

	public void Free()
	{
		if (m_workerCoroutine != null)
		{
			if ((Object)(object)zbUIMgr.Instance != (Object)null)
			{
				((MonoBehaviour)zbUIMgr.Instance).StopCoroutine(m_workerCoroutine);
			}
			m_workerCoroutine = null;
		}
		paotaiMgr.Free();
		zombieMgr.Free();
		wallMgr.Free();
		roomMgr.Free();
		floorMgr.Free();
		workerMgr.Free();
		leaverMgr.Free();
		roomHeroMgr.Free();
		renderTarget = null;
		m_buildInit = false;
		sInstance = null;
	}

	public IEnumerator BuildAsync()
	{
		m_buildInit = false;
		m_needRefreshAfterBuild = false;
		m_needCheckFloorUnlockAfterBuild = false;
		roomMgr.BuildRealRooms();
		yield return null;
		floorMgr.Build();
		yield return null;
		roomMgr.BuildEmptyRooms();
		yield return null;
		if (zbServerData.sGameData.MainQuestInfo.MainLevel <= 3)
		{
			wallMgr.Build();
			yield return null;
			workerMgr.Build();
			yield return null;
			leaverMgr.Build();
			yield return null;
			roomHeroMgr.Build();
			yield return null;
			zombieMgr.Build();
			yield return null;
			paotaiMgr.Build();
		}
		else
		{
			yield return null;
			wallMgr.Build();
			yield return null;
			if (m_workerCoroutine != null)
			{
				((MonoBehaviour)zbUIMgr.Instance).StopCoroutine(m_workerCoroutine);
				m_workerCoroutine = null;
			}
			m_workerCoroutine = ((MonoBehaviour)zbUIMgr.Instance).StartCoroutine(workerMgr.BuildAsync(5));
			yield return m_workerCoroutine;
			leaverMgr.Build();
			yield return null;
			roomHeroMgr.Build();
			yield return null;
			zombieMgr.Build();
			yield return null;
			paotaiMgr.Build();
			yield return null;
		}
		m_buildInit = true;
		if (m_needRefreshAfterBuild)
		{
			Refresh();
			m_needRefreshAfterBuild = false;
		}
		if (m_needCheckFloorUnlockAfterBuild)
		{
			floorMgr.CheakFloorUnlock();
			m_needCheckFloorUnlockAfterBuild = false;
		}
	}

	public void Refresh()
	{
		if (m_buildInit)
		{
			wallMgr.Refresh();
			roomMgr.Refresh();
			floorMgr.Refresh();
			workerMgr.Refresh();
			leaverMgr.Refresh();
			roomHeroMgr.Refresh();
			paotaiMgr.Refresh();
		}
	}

	public void Update()
	{
		if (m_buildInit)
		{
			floorMgr.Update();
			roomMgr.Update();
			workerMgr.Update();
			leaverMgr.Update();
			roomHeroMgr.Update();
			zombieMgr.Update();
		}
	}

	public void MarkNeedRefreshAfterBuild()
	{
		m_needRefreshAfterBuild = true;
	}

	public void MarkNeedCheckFloorUnlockAfterBuild()
	{
		m_needCheckFloorUnlockAfterBuild = true;
	}

	public bool IsBuildOk()
	{
		return m_buildInit;
	}
}
