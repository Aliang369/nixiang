using System.Collections;
using System.IO;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbSpineCapture : MonoBehaviour
{
	private Camera m_renderCamera;

	private GameObject m_renderTarget;

	private RenderTexture m_renderTexture;

	public int textureWidth = 128;

	public int textureHeight = 128;

	private void Awake()
	{
		m_renderCamera = ((Component)((Component)this).transform.Find("Camera")).GetComponent<Camera>();
	}

	public bool IsNeedCaptureWorker(WorkerDetail workerDetail)
	{
		if (File.Exists(zbUtil.GetWokerCaptureFileName(workerDetail.WorkerId)))
		{
			string wokerCaptureFileNameStr = zbUtil.GetWokerCaptureFileNameStr(workerDetail.WorkerId);
			if (File.Exists(wokerCaptureFileNameStr))
			{
				string text = File.ReadAllText(wokerCaptureFileNameStr);
				if (workerDetail.ToString() == text)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsNeedCaptureReconer(WorkerDetail workerDetail)
	{
		if (File.Exists(zbUtil.GetWokerCaptureFileName(workerDetail.WorkerId)))
		{
			string wokerCaptureFileNameStr = zbUtil.GetWokerCaptureFileNameStr(workerDetail.WorkerId);
			if (File.Exists(wokerCaptureFileNameStr))
			{
				string text = File.ReadAllText(wokerCaptureFileNameStr);
				if (workerDetail.ToString() == text)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void StartCaptureSpine(SkeletonAnimation oriSkeleAnim, WorkerDetail workerDetail, WorkerDetail reconerDetail)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		int width = 128;
		int height = 128;
		int num = 0;
		string text = "";
		WorkerStyleInfo workerStyleInfo = null;
		if (workerDetail != null)
		{
			num = workerDetail.WorkerId;
			workerStyleInfo = workerDetail.StyleInfo;
			text = workerDetail.ToString();
		}
		else
		{
			if (reconerDetail == null)
			{
				return;
			}
			num = reconerDetail.WorkerId;
			workerStyleInfo = reconerDetail.StyleInfo;
			text = reconerDetail.ToString();
		}
		GameObject val = new GameObject();
		val.transform.SetParent(((Component)this).transform);
		val.transform.localPosition = new Vector3(-1000f, 0f, 0f);
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		SkeletonAnimation obj = val.AddComponent<SkeletonAnimation>();
		((SkeletonRenderer)obj).skeletonDataAsset = ((SkeletonRenderer)oriSkeleAnim).skeletonDataAsset;
		((SkeletonRenderer)obj).initialSkinName = ((SkeletonRenderer)oriSkeleAnim).initialSkinName;
		((SkeletonRenderer)obj).Initialize(true, false);
		((Component)this).gameObject.SetActive(true);
		CreateRenderTexture();
		zbStyleInfoHelper.RefreshStyleInfo(obj, workerStyleInfo);
		((MonoBehaviour)this).StartCoroutine(CaptureAfterFrame(val, width, height, num, text));
	}

	public void SetTipAnimation(SkeletonAnimation skeleAnim, string animationName)
	{
		if (animationName != "")
		{
			skeleAnim.AnimationState.SetAnimation(1, animationName, true);
		}
		else
		{
			skeleAnim.AnimationState.ClearTrack(1);
		}
	}

	private IEnumerator CaptureAfterFrame(GameObject target, int width, int height, int workerId, string saveDetailInfo)
	{
		yield return null;
		yield return null;
		target.SetActive(true);
		target.transform.localPosition = Vector3.zero;
		StartCapture(target, width, height, workerId, saveDetailInfo);
		target.transform.localPosition = new Vector3(-1000f, 0f, 0f);
		Object.Destroy((Object)(object)target);
	}

	public void StartCapture(GameObject renderTarget, int textureW, int textureH, int workerId, string saveDetailInfo)
	{
		textureWidth = textureW;
		textureHeight = textureH;
		m_renderTarget = renderTarget;
		if ((Object)(object)m_renderTarget == (Object)null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(true);
		AdjustCameraToFit();
		SaveCaptureInfo(workerId, saveDetailInfo);
	}

	private void CreateRenderTexture()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if ((Object)(object)m_renderTexture != (Object)null)
		{
			m_renderTexture.Release();
			Object.Destroy((Object)(object)m_renderTexture);
		}
		m_renderTexture = new RenderTexture(textureWidth, textureHeight, 16, (RenderTextureFormat)0);
		m_renderTexture.Create();
		m_renderCamera.targetTexture = m_renderTexture;
	}

	private void AdjustCameraToFit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetBounds();
		Vector3 center = ((Bounds)(ref bounds)).center;
		Vector3 size = ((Bounds)(ref bounds)).size;
		((Component)m_renderCamera).transform.position = new Vector3(center.x, center.y, -10f);
		float num = (float)m_renderCamera.pixelWidth / (float)m_renderCamera.pixelHeight;
		float num2 = size.y / 2f;
		float num3 = size.x / (2f * num);
		m_renderCamera.orthographicSize = Mathf.Max(num2, num3);
	}

	private Bounds GetBounds()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Renderer[] componentsInChildren = m_renderTarget.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(m_renderTarget.transform.position, Vector3.zero);
		}
		Bounds bounds = componentsInChildren[0].bounds;
		Renderer[] array = componentsInChildren;
		foreach (Renderer val in array)
		{
			((Bounds)(ref bounds)).Encapsulate(val.bounds);
		}
		return bounds;
	}

	private void SaveCaptureInfo(int workerId, string saveDetailInfo)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00bd: Expected O, but got Unknown
		if (!((Object)(object)m_renderTexture == (Object)null))
		{
			m_renderCamera.Render();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = m_renderTexture;
			Texture2D val = new Texture2D(((Texture)m_renderTexture).width, ((Texture)m_renderTexture).height, (TextureFormat)4, false);
			val.ReadPixels(new Rect(0f, 0f, (float)((Texture)m_renderTexture).width, (float)((Texture)m_renderTexture).height), 0, 0);
			val.Apply();
			byte[] bytes = ImageConversion.EncodeToPNG(val);
			string wokerCaptureFileName = zbUtil.GetWokerCaptureFileName(workerId);
			string directoryName = Path.GetDirectoryName(wokerCaptureFileName);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllBytes(wokerCaptureFileName, bytes);
			File.WriteAllText(zbUtil.GetWokerCaptureFileNameStr(workerId), saveDetailInfo);
			RenderTexture.active = active;
			Object.Destroy((Object)val);
		}
	}

	public void StopCapture()
	{
		if ((Object)(object)m_renderCamera != (Object)null)
		{
			m_renderCamera.targetTexture = null;
		}
		if ((Object)(object)m_renderTexture != (Object)null)
		{
			m_renderTexture.Release();
			Object.Destroy((Object)(object)m_renderTexture);
			m_renderTexture = null;
		}
		((Component)this).gameObject.SetActive(false);
	}

	public RenderTexture GetRenderTexture()
	{
		return m_renderTexture;
	}
}
