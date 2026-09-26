using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBaseWnd : MonoBehaviour
{
	private EWndId m_eWndId;

	[SerializeField]
	private VisualTreeAsset sourceAsset;

	[NonSerialized]
	public zbUIEvent m_uiEvent;

	[NonSerialized]
	public zbGameEvent m_gameEevent;

	private GameObject m_prefabObject;

	protected VisualElement m_RootElement;

	[NonSerialized]
	public zbMoneyPanel m_moneyPanel;

	[NonSerialized]
	public zbMainBottom m_MainBottom;

	protected GameObject m_GameObjectSceneRoot;

	private bool m_bAutoHide;

	[NonSerialized]
	public Vector3 m_LastCameraPosition;

	[NonSerialized]
	public float m_LastCameraOrthographicSize;

	private float m_refreshTimer;

	private static readonly float m_refreshInterval = 1f;

	private bool m_isPaused;

	private int m_delayFrames;

	private int m_delayFrameTarget;

	public void Init(EWndId id, GameObject prefabObject)
	{
		m_eWndId = id;
		m_prefabObject = prefabObject;
		zbUIMgr.Instance.AddWndElement(this);
	}

	public EWndId GetWndId()
	{
		return m_eWndId;
	}

	public VisualElement GetRootElement()
	{
		return m_RootElement;
	}

	public GameObject GetPrefabObject()
	{
		return m_prefabObject;
	}

	public bool IsCascadWnd()
	{
		if (m_eWndId != EWndId.Bubble)
		{
			return m_eWndId != EWndId.TipConnect;
		}
		return false;
	}

	protected virtual void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Transform val = ((Component)this).transform.Find("GameObjectSceneRoot");
		if ((Object)(object)val != (Object)null)
		{
			m_GameObjectSceneRoot = ((Component)val).gameObject;
		}
		m_LastCameraPosition = new Vector3(0f, 0f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
	}

	protected virtual void OnEnable()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		m_RootElement = (VisualElement)(object)sourceAsset.Instantiate();
		m_RootElement.name = ((Object)sourceAsset).name;
		m_RootElement.userData = this;
		m_RootElement.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_RootElement.pickingMode = (PickingMode)1;
		m_RootElement.style.width = StyleLength.op_Implicit(720f);
		m_RootElement.style.height = StyleLength.op_Implicit(1480f);
		m_RootElement.style.left = StyleLength.op_Implicit(-360f);
		m_RootElement.style.top = StyleLength.op_Implicit(-740f);
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
		zbUtil.FixAllBackgrounds(m_RootElement);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		if (val != null)
		{
			((CallbackEventHandler)val).RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)OnRootGeometryChanged, (TrickleDown)0);
		}
		if (HasScene())
		{
			((Component)zbCameraHelper.Instance.mainCamera).transform.position = m_LastCameraPosition;
			zbWorldScaler.AdaptCamera(zbCameraHelper.Instance.mainCamera);
			m_LastCameraOrthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		}
	}

	protected virtual void OnRootGeometryChanged(GeometryChangedEvent evt)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		IEventHandler target = ((EventBase)evt).target;
		VisualElement val = (VisualElement)(object)((target is VisualElement) ? target : null);
		if (val != null)
		{
			if (val.resolvedStyle.top == 0f)
			{
				val.style.top = StyleLength.op_Implicit(new Length(50f, (LengthUnit)1));
			}
			val.style.translate = StyleTranslate.op_Implicit(new Translate(Length.Percent(-50f), Length.Percent(-50f)));
			zbUtil.FixAllBackgrounds(m_RootElement);
			((CallbackEventHandler)val).UnregisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)OnRootGeometryChanged, (TrickleDown)0);
		}
	}

	protected virtual void OnDisable()
	{
		m_uiEvent.UnRegisterCallback();
		m_gameEevent.UnRegisterCallback();
	}

	protected virtual void OnDestroy()
	{
		zbUIMgr.Instance.OnNotifyDestroyWnd(m_eWndId);
		sourceAsset = null;
		m_RootElement = null;
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			m_isPaused = true;
			return;
		}
		m_isPaused = false;
		m_delayFrames = 0;
	}

	private void Update()
	{
		if (!IsShow() || m_isPaused)
		{
			return;
		}
		if (m_delayFrames < m_delayFrameTarget)
		{
			m_delayFrames++;
			return;
		}
		UpdateAtShow();
		m_refreshTimer += Time.unscaledDeltaTime;
		if (m_refreshTimer >= m_refreshInterval)
		{
			m_refreshTimer = 0f;
			UpdatePerSecond();
		}
	}

	protected virtual void UpdateAtShow()
	{
		if (m_MainBottom != null)
		{
			m_MainBottom.UpdateRedPoint();
		}
	}

	protected virtual void UpdatePerSecond()
	{
	}

	protected virtual void RefreshWhenAutoVisible()
	{
	}

	protected virtual void RefreshWhenAutoVisibleCaseNoScene()
	{
	}

	protected void AutoWndSceneWhenShow()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!HasScene())
		{
			return;
		}
		foreach (zbBaseWnd value in zbUIMgr.Instance.m_kDictWnd.Values)
		{
			if (value.IsShow() && value.IsCascadWnd() && value.m_eWndId != m_eWndId)
			{
				value.GetRootElement().style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				value.SetMySceneVisible(visible: false);
				value.m_bAutoHide = true;
			}
		}
		SetMySceneVisible(visible: true);
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = m_LastCameraPosition;
		zbCameraHelper.Instance.mainCamera.orthographicSize = m_LastCameraOrthographicSize;
	}

	protected void AutoWndSceneWhenHide()
	{
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		if (!IsCascadWnd())
		{
			return;
		}
		if (!HasScene())
		{
			foreach (VisualElement item in GetChildrenTopToBottom(zbUIMgr.Instance.GetRootCascadWndElement()))
			{
				if (item.userData != null)
				{
					zbBaseWnd zbBaseWnd2 = item.userData as zbBaseWnd;
					if (Object.op_Implicit((Object)(object)zbBaseWnd2) && zbBaseWnd2.IsShow())
					{
						zbBaseWnd2.RefreshWhenAutoVisibleCaseNoScene();
						break;
					}
				}
			}
			return;
		}
		foreach (VisualElement item2 in GetChildrenTopToBottom(zbUIMgr.Instance.GetRootCascadWndElement()))
		{
			if (item2.userData == null)
			{
				continue;
			}
			zbBaseWnd zbBaseWnd3 = item2.userData as zbBaseWnd;
			if (!Object.op_Implicit((Object)(object)zbBaseWnd3))
			{
				continue;
			}
			if (zbBaseWnd3.IsShow() && zbBaseWnd3.HasScene())
			{
				((Component)zbCameraHelper.Instance.mainCamera).transform.position = zbBaseWnd3.m_LastCameraPosition;
				zbCameraHelper.Instance.mainCamera.orthographicSize = zbBaseWnd3.m_LastCameraOrthographicSize;
				zbBaseWnd3.RefreshWhenAutoVisible();
				break;
			}
			if (zbBaseWnd3.m_bAutoHide)
			{
				zbBaseWnd3.GetRootElement().style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbBaseWnd3.m_bAutoHide = false;
				if (zbBaseWnd3.HasScene())
				{
					zbBaseWnd3.SetMySceneVisible(visible: true);
					((Component)zbCameraHelper.Instance.mainCamera).transform.position = zbBaseWnd3.m_LastCameraPosition;
					zbCameraHelper.Instance.mainCamera.orthographicSize = zbBaseWnd3.m_LastCameraOrthographicSize;
					zbBaseWnd3.RefreshWhenAutoVisible();
					break;
				}
			}
		}
	}

	public static List<VisualElement> GetChildrenBottomToTop(VisualElement parent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Hierarchy hierarchy = parent.hierarchy;
		return ((Hierarchy)(ref hierarchy)).Children().ToList();
	}

	public static List<VisualElement> GetChildrenTopToBottom(VisualElement parent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Hierarchy hierarchy = parent.hierarchy;
		return ((Hierarchy)(ref hierarchy)).Children().Reverse().ToList();
	}

	public virtual void Show()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		GetRootElement().style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (IsCascadWnd())
		{
			GetRootElement().BringToFront();
		}
		m_bAutoHide = false;
		AutoWndSceneWhenShow();
	}

	public virtual void Hide()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (GetRootElement() != null)
		{
			GetRootElement().style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (IsCascadWnd())
			{
				GetRootElement().SendToBack();
			}
			SetMySceneVisible(visible: false);
			m_bAutoHide = false;
			AutoWndSceneWhenHide();
			OnHide();
		}
	}

	protected virtual void OnHide()
	{
	}

	public void HideForPreload()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (GetRootElement() != null)
		{
			GetRootElement().style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if ((Object)(object)m_GameObjectSceneRoot != (Object)null)
		{
			m_GameObjectSceneRoot.SetActive(false);
		}
	}

	public bool IsShow()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || (Object)(object)((Component)this).gameObject == (Object)null || GetRootElement() == null)
		{
			return false;
		}
		return GetRootElement().style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public bool IsAutoHide()
	{
		return m_bAutoHide;
	}

	public void AddDark()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		if (val != null)
		{
			val.style.backgroundColor = StyleColor.op_Implicit(new Color(0f, 0f, 0f, 0.8f));
		}
	}

	public void DelDark()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		if (val != null)
		{
			val.style.backgroundColor = StyleColor.op_Implicit(new Color(0f, 0f, 0f, 0f));
		}
	}

	public void CreateMoneyPanel(EMoneyType[] moneyList, bool isBgVis = false)
	{
		if (m_moneyPanel == null)
		{
			m_moneyPanel = new zbMoneyPanel(this);
		}
		if (m_moneyPanel != null)
		{
			m_moneyPanel.Refresh(moneyList, isBgVis);
		}
	}

	public void CreateMainBottom(int index)
	{
		if (m_MainBottom == null)
		{
			m_MainBottom = new zbMainBottom(this, index);
		}
		if (m_MainBottom != null)
		{
			m_MainBottom.Refresh();
		}
	}

	public virtual void Refresh()
	{
	}

	public GameObject GetMyGameObjectSceneRoot()
	{
		return m_GameObjectSceneRoot;
	}

	public void SetMySceneVisible(bool visible)
	{
		if ((Object)(object)m_GameObjectSceneRoot != (Object)null)
		{
			m_GameObjectSceneRoot.SetActive(visible);
		}
	}

	private bool HasScene()
	{
		return (Object)(object)m_GameObjectSceneRoot != (Object)null;
	}

	public void DestroySelfWnd()
	{
		zbUIMgr.Instance.DestroyWnd(m_eWndId);
	}

	public virtual void OnScreenSizeChnaged()
	{
	}

	public zbBaseWnd()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		m_uiEvent = new zbUIEvent();
		m_gameEevent = new zbGameEvent();
		m_LastCameraPosition = Vector3.zero;
		m_LastCameraOrthographicSize = 5f;
		m_delayFrameTarget = 3;
		((MonoBehaviour)this)._002Ector();
	}
}
