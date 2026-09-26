using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRoomWorkerRefreshWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnRefresh;

	private Button m_btnSave;

	private Label m_lbWorkdesc;

	private Label m_lbCost;

	private VisualElement[] m_nodeWorkers = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_spriteWorkers = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_selects = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_btnSelects = (Button[])(object)new Button[3];

	private int m_workerId;

	private int m_selectedWorkerId = -1;

	private int[] m_replaceCost;

	private GameObject m_captureRoot;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_btnSave = UQueryExtensions.Q<Button>(m_RootElement, "btn_save", (string)null);
		m_lbWorkdesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_workdesc", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		for (int i = 0; i < 3; i++)
		{
			int index = i;
			m_nodeWorkers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"node_worker{i}", (string)null);
			m_spriteWorkers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"bg_worker{i}", (string)null);
			m_btnSelects[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn_select{i}", (string)null);
			m_selects[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"select{i}", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelects[i], (Action<ClickEvent>)delegate
			{
				OnSelect(index);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action<ClickEvent>)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSave, (Action)OnSave, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorkerReplace_Refresh, RefreshList);
		m_gameEevent.RegisterEvent(EGameEvent.Event_ReplaceWorkerSelect_Ack, OnRealClose);
		InitRecord();
	}

	private void InitRecord()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		m_replaceCost = zbSelf.Instance.homelandHandler.replaceCost;
		if (m_replaceCost != null)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)m_replaceCost[0], m_replaceCost[1]);
			((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
			((TextElement)m_lbCost).text = "x" + m_replaceCost[2];
			CreateMoneyPanel(new EMoneyType[1] { (EMoneyType)m_replaceCost[1] }, isBgVis: true);
		}
	}

	public void RefreshCustom(int workerId)
	{
		m_workerId = workerId;
		m_selectedWorkerId = -1;
		((TextElement)m_lbWorkdesc).text = zbUtil.Language("lang.11470");
		RefreshList();
		RefreshSelect();
	}

	private void RefreshList(object obj = null)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		List<WorkerDetail> replaceList = zbSelf.Instance.homelandHandler.replaceList;
		List<(WorkerDetail, int)> list = new List<(WorkerDetail, int)>();
		for (int i = 0; i < 3; i++)
		{
			m_spriteWorkers[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/home/worker/workerNone.png"));
			if (replaceList != null && i < replaceList.Count)
			{
				WorkerDetail workerDetail = replaceList[i];
				Texture2D texture2DByName = zbUtil.GetTexture2DByName(workerDetail.WorkerId);
				if ((Object)(object)texture2DByName != (Object)null)
				{
					m_spriteWorkers[i].style.backgroundImage = StyleBackground.op_Implicit(texture2DByName);
				}
				else
				{
					list.Add((workerDetail, i));
				}
			}
		}
		if (list.Count > 0)
		{
			((MonoBehaviour)this).StartCoroutine(CaptureWorkersSequential(list));
		}
		((VisualElement)m_btnSave).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((replaceList == null || replaceList.Count <= 0) ? 1 : 0));
	}

	private void RefreshSelect()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (m_selects[i] != null)
			{
				m_selects[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i != m_selectedWorkerId) ? 1 : 0));
			}
		}
	}

	private void OnRefresh(ClickEvent evt)
	{
		if (m_replaceCost != null && !zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)m_replaceCost[0], m_replaceCost[1], m_replaceCost[2]))
		{
			zbUIMgr.Instance.ShowTip((EResourceType)m_replaceCost[0], m_replaceCost[1], m_replaceCost[2]);
		}
		else
		{
			zbWsMsgHandler.Instance.HomelandV2_ReplaceWorker_Req(m_workerId);
		}
	}

	private void OnSave()
	{
		if (m_selectedWorkerId <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10988"));
		}
		else
		{
			zbWsMsgHandler.Instance.HomelandV2_ReplaceWorkerSelect_Req(m_workerId, m_selectedWorkerId);
		}
	}

	private void OnClose()
	{
		if (zbSelf.Instance.homelandHandler.replaceList == null || zbSelf.Instance.homelandHandler.replaceList.Count == 0)
		{
			OnRealClose();
			return;
		}
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.11236"), zbUtil.Language("lang.11463"), delegate(bool isOk)
		{
			if (isOk)
			{
				OnRealClose();
			}
		}, "1", zbUtil.Language("lang.10550"));
	}

	private void OnRealClose(object obj = null)
	{
		zbSelf.Instance.homelandHandler.replaceList.Clear();
		Hide();
	}

	private void OnEventUpdate(object obj)
	{
		RefreshList();
	}

	private void OnSelect(int index)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		List<WorkerDetail> replaceList = zbSelf.Instance.homelandHandler.replaceList;
		if (replaceList == null || replaceList.Count == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11464"));
		}
		else
		{
			if (replaceList == null || index < 0 || index >= replaceList.Count)
			{
				return;
			}
			m_selectedWorkerId = replaceList[index].WorkerId;
			zbNpcDescCsvRecord recordByKey = zbDataConfig.Instance.npcDescCsvTable.GetRecordByKey(replaceList[index].StyleInfo.Desc);
			((TextElement)m_lbWorkdesc).text = recordByKey.desc;
			for (int i = 0; i < 3; i++)
			{
				if (m_selects[i] != null)
				{
					m_selects[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i != index) ? 1 : 0));
				}
			}
		}
	}

	private IEnumerator CaptureWorkersSequential(List<(WorkerDetail worker, int slotIndex)> pending)
	{
		foreach (var item in pending)
		{
			yield return CaptureWorkerCoroutine(item.worker, item.slotIndex);
		}
	}

	private IEnumerator CaptureWorkerCoroutine(WorkerDetail worker, int slotIndex)
	{
		if ((Object)(object)m_captureRoot == (Object)null)
		{
			m_captureRoot = new GameObject("WorkerCaptureRoot");
			m_captureRoot.transform.position = new Vector3(-1000f, 0f, 0f);
			Object.DontDestroyOnLoad((Object)(object)m_captureRoot);
		}
		string resName = ((worker.StyleInfo.Gender == EGender.Female) ? "animation/citizen/citizen_woman_01/citizen_woman_01_SkeletonData.asset" : "animation/citizen/citizen_man_01/citizen_man_01_SkeletonData.asset");
		SkeletonDataAsset val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(resName);
		if ((Object)(object)val == (Object)null)
		{
			yield break;
		}
		GameObject captureObj = new GameObject($"WorkerCapture_{worker.WorkerId}");
		captureObj.transform.SetParent(m_captureRoot.transform);
		captureObj.transform.localPosition = Vector3.zero;
		captureObj.transform.localRotation = Quaternion.identity;
		captureObj.transform.localScale = Vector3.one;
		SkeletonAnimation obj = captureObj.AddComponent<SkeletonAnimation>();
		((SkeletonRenderer)obj).skeletonDataAsset = val;
		((SkeletonRenderer)obj).Initialize(true, false);
		zbStyleInfoHelper.RefreshStyleInfo(obj, worker.StyleInfo);
		yield return null;
		zbRenderTarget renderTarget = zbHomeMgr.Instance?.renderTarget;
		if ((Object)(object)renderTarget == (Object)null)
		{
			Object.Destroy((Object)(object)captureObj);
			yield break;
		}
		renderTarget.SetCameraPadding(1.2f);
		renderTarget.StartRender(captureObj, 192, 233);
		yield return null;
		RenderTexture renderTexture = renderTarget.GetRenderTexture();
		if ((Object)(object)renderTexture == (Object)null)
		{
			renderTarget.StopRender();
			Object.Destroy((Object)(object)captureObj);
			yield break;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		Texture2D val2 = new Texture2D(((Texture)renderTexture).width, ((Texture)renderTexture).height, (TextureFormat)4, false);
		val2.ReadPixels(new Rect(0f, 0f, (float)((Texture)renderTexture).width, (float)((Texture)renderTexture).height), 0, 0);
		val2.Apply();
		string wokerCaptureFileName = zbUtil.GetWokerCaptureFileName(worker.WorkerId);
		string directoryName = Path.GetDirectoryName(wokerCaptureFileName);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		byte[] bytes = ImageConversion.EncodeToPNG(val2);
		File.WriteAllBytes(wokerCaptureFileName, bytes);
		File.WriteAllText(zbUtil.GetWokerCaptureFileNameStr(worker.WorkerId), worker.ToString());
		RenderTexture.active = active;
		if (m_spriteWorkers != null && slotIndex < m_spriteWorkers.Length && m_spriteWorkers[slotIndex] != null)
		{
			m_spriteWorkers[slotIndex].style.backgroundImage = StyleBackground.op_Implicit(val2);
		}
		else
		{
			Object.Destroy((Object)(object)val2);
		}
		renderTarget.StopRender();
		Object.Destroy((Object)(object)captureObj);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((Object)(object)m_captureRoot != (Object)null)
		{
			Object.Destroy((Object)(object)m_captureRoot);
			m_captureRoot = null;
		}
	}
}
