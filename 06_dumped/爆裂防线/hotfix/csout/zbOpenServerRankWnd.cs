using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbOpenServerRankWnd : zbBaseWnd
{
	private class ActiveSpineBinding
	{
		public VisualElement placeholder;

		public GameObject spineGO;

		public int dataIndex;
	}

	private VisualElement m_mine;

	private Label m_lbLimitedTime;

	private Button m_btnBack;

	private Button m_btnRule;

	private ScrollView m_scroll;

	private VisualElement m_container;

	private readonly List<ActiveSpineBinding> m_activeSpines = new List<ActiveSpineBinding>();

	private readonly Queue<GameObject> m_spinePool = new Queue<GameObject>();

	private Label m_lbRank;

	private Label m_lbNoRank;

	private VisualElement m_noRank;

	private VisualElement m_rankInfo;

	private VisualElement m_playerVE;

	[SerializeField]
	public GameObject m_spinePrefab;

	private OpsRankInfo m_opsRankInfo;

	[SerializeField]
	private VisualTreeAsset m_rankItemTemplate;

	public float m_savedScrollOffsetY;

	protected override void OnEnable()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_lbLimitedTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_limitedTime", (string)null);
		m_scroll = UQueryExtensions.Q<ScrollView>(m_RootElement, "listView", (string)null);
		m_scroll.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scroll.horizontalScrollerVisibility = (ScrollerVisibility)2;
		m_container = new VisualElement();
		m_container.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)0);
		((VisualElement)m_scroll).Add(m_container);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scroll, "unity-content-viewport", (string)null).style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)0);
		m_lbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
		m_lbNoRank = UQueryExtensions.Q<Label>(m_mine, "lb_noRank", (string)null);
		m_noRank = UQueryExtensions.Q<VisualElement>(m_mine, "noRankInfo", (string)null);
		m_rankInfo = UQueryExtensions.Q<VisualElement>(m_mine, "rankInfo", (string)null);
		m_playerVE = UQueryExtensions.Q<VisualElement>(m_mine, "player", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
	}

	public void RefreshCustom(OpsRankInfo opsRankInfo)
	{
		m_opsRankInfo = opsRankInfo;
		InitSelfInfo();
		UpdateListView();
		zbWsMsgHandler.Instance.OpenServer_RankTop_Req();
	}

	private void UpdateListView()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		ClearActiveSpines();
		m_container.Clear();
		int num = 10 - m_opsRankInfo.RankList.Count;
		for (int i = 0; i < num; i++)
		{
			m_opsRankInfo.RankList.Add(new OpsRankDetail());
		}
		for (int j = 0; j < m_opsRankInfo.RankList.Count; j++)
		{
			OpsRankDetail opsRankDetail = m_opsRankInfo.RankList[j];
			zbOpenServerRankItem zbOpenServerRankItem2 = new zbOpenServerRankItem(m_rankItemTemplate, this);
			zbOpenServerRankItem2.Init(opsRankDetail.RankInfo, j, opsRankDetail.RankLimit);
			((VisualElement)zbOpenServerRankItem2).style.height = StyleLength.op_Implicit(280f);
			bool flag = j == 0;
			zbOpenServerRankItem2.m_spinePlaceholder.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			zbOpenServerRankItem2.m_spinePlaceholderTop.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			m_container.Add((VisualElement)(object)zbOpenServerRankItem2);
			VisualElement placeholder = (flag ? zbOpenServerRankItem2.m_spinePlaceholderTop : zbOpenServerRankItem2.m_spinePlaceholder);
			bool hasPlayer = opsRankDetail.RankInfo != null && opsRankDetail.RankInfo.RoleBaseInfo != null;
			BindOrReuseSpine(placeholder, j, hasPlayer);
		}
	}

	private void BindOrReuseSpine(VisualElement placeholder, int index, bool hasPlayer)
	{
		ActiveSpineBinding activeSpineBinding = m_activeSpines.FirstOrDefault((ActiveSpineBinding s) => s.dataIndex == index);
		if (activeSpineBinding != null)
		{
			activeSpineBinding.placeholder = placeholder;
			zbOpenServerRankPlayerAvatar component = activeSpineBinding.spineGO.GetComponent<zbOpenServerRankPlayerAvatar>();
			SetupSpineAvatar(component, index + 1, hasPlayer);
			activeSpineBinding.spineGO.SetActive(true);
			return;
		}
		ActiveSpineBinding activeSpineBinding2 = m_activeSpines.FirstOrDefault((ActiveSpineBinding s) => s.placeholder == placeholder);
		if (activeSpineBinding2 == null)
		{
			GameObject orCreateSpine = GetOrCreateSpine(out var _);
			zbOpenServerRankPlayerAvatar component2 = orCreateSpine.GetComponent<zbOpenServerRankPlayerAvatar>();
			SetupSpineAvatar(component2, index + 1, hasPlayer);
			orCreateSpine.transform.SetParent(((Component)this).transform.Find("GameObjectSceneRoot"), false);
			orCreateSpine.SetActive(true);
			m_activeSpines.Add(new ActiveSpineBinding
			{
				placeholder = placeholder,
				spineGO = orCreateSpine,
				dataIndex = index
			});
		}
		else
		{
			activeSpineBinding2.dataIndex = index;
			zbOpenServerRankPlayerAvatar component3 = activeSpineBinding2.spineGO.GetComponent<zbOpenServerRankPlayerAvatar>();
			SetupSpineAvatar(component3, index + 1, hasPlayer);
			activeSpineBinding2.spineGO.SetActive(true);
		}
	}

	private void SetupSpineAvatar(zbOpenServerRankPlayerAvatar avatar, int rank, bool visible)
	{
		RoleBaseInfo roleBaseInfo = m_opsRankInfo.RankList[rank - 1].RankInfo?.RoleBaseInfo;
		avatar.RefreshCustom(rank, visible, roleBaseInfo?.AvatarId ?? 0, roleBaseInfo?.GunId ?? 0, roleBaseInfo?.MechaId ?? 0);
	}

	private GameObject GetOrCreateSpine(out bool isNew)
	{
		isNew = m_spinePool.Count == 0;
		if (!isNew)
		{
			return m_spinePool.Dequeue();
		}
		return Object.Instantiate<GameObject>(m_spinePrefab);
	}

	private void ReturnSpineToPool(GameObject go)
	{
		go.SetActive(false);
		m_spinePool.Enqueue(go);
	}

	private void ClearActiveSpines()
	{
		foreach (ActiveSpineBinding activeSpine in m_activeSpines)
		{
			ReturnSpineToPool(activeSpine.spineGO);
		}
		m_activeSpines.Clear();
	}

	private void InitSelfInfo()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		RoleBaseInfo role = zbServerData.sGameData.RoleBaseInfo;
		((TextElement)UQueryExtensions.Q<Label>(m_playerVE, "lb_name", (string)null)).text = role.RoleName;
		((TextElement)UQueryExtensions.Q<Label>(m_playerVE, "lb_main", (string)null)).text = "主线: Day" + zbServerData.sGameData.MainQuestInfo.MainLevel;
		zbPlayerHeadCsvRecord recordByKey = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(role.HeadId);
		if (recordByKey != null)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
			UQueryExtensions.Q<VisualElement>(m_playerVE, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		}
		zbPlayerHeadFrameCsvRecord recordByKey2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(role.HeadFrameId);
		if (recordByKey2 != null)
		{
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon);
			UQueryExtensions.Q<VisualElement>(m_playerVE, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		}
		((TextElement)UQueryExtensions.Q<Label>(m_playerVE, "lb_level", (string)null)).text = role.Level.ToString();
		OpsRankDetail opsRankDetail = ((IEnumerable<OpsRankDetail>)m_opsRankInfo.RankList).FirstOrDefault((OpsRankDetail r) => r.RankInfo != null && r.RankInfo.RoleBaseInfo != null && r.RankInfo.RoleBaseInfo?.RoleId == role.RoleId);
		bool flag = opsRankDetail != null;
		m_noRank.visible = !flag;
		m_rankInfo.visible = flag;
		((VisualElement)m_lbRank).visible = flag;
		((VisualElement)m_lbNoRank).visible = !flag;
		if (flag)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_rankInfo, "lb_time", (string)null)).text = zbUtil.FormatUnixTimeToUtcString(opsRankDetail.RankInfo.RankData.RankTm);
			zbSelf.Instance.rankHandler.InitRankInOpenServerRank(m_lbRank, opsRankDetail.RankInfo.RankData.Rank);
		}
		else
		{
			bool flag2 = zbServerData.sGameData.MainQuestInfo.MainLevel >= 10;
			((TextElement)UQueryExtensions.Q<Label>(m_noRank, "lb_text", (string)null)).text = (flag2 ? "未进入前10名" : "未达到主线: Day10");
			((TextElement)m_lbNoRank).text = zbUtil.Language("lang.10512");
		}
	}

	protected override void UpdateAtShow()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		foreach (ActiveSpineBinding activeSpine in m_activeSpines)
		{
			VisualElement placeholder = activeSpine.placeholder;
			if (((placeholder != null) ? placeholder.panel : null) == null || (int)placeholder.resolvedStyle.display != 0)
			{
				activeSpine.spineGO.SetActive(false);
				continue;
			}
			Vector3 worldPositionFromVisualElement = zbUtil.GetWorldPositionFromVisualElement(placeholder, zbCameraHelper.Instance.mainCamera);
			worldPositionFromVisualElement.z = -2f;
			activeSpine.spineGO.transform.position = worldPositionFromVisualElement;
		}
		UpdateLimitedTime();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		SaveScrollOffset();
	}

	private void UpdateLimitedTime()
	{
		if (zbServerData.sGameData.OpenServerInfo != null && zbServerData.sGameData.OpenServerInfo.RankEndTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			bool flag = zbServerData.sGameData.OpenServerInfo.RankBaseList[0].SettleTm - zbSelf.Instance.timeHandler.GetServerTime() < 0;
			((TextElement)m_lbLimitedTime).text = (flag ? zbUtil.Language("lang.10397") : string.Format(zbUtil.Language("lang.10490"), zbUtil.FormatTimeHHMMSS(zbServerData.sGameData.OpenServerInfo.RankBaseList[0].SettleTm - zbSelf.Instance.timeHandler.GetServerTime())));
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		if (!(m_savedScrollOffsetY > 0f))
		{
			return;
		}
		((VisualElement)m_scroll).schedule.Execute((Action)delegate
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (m_scroll != null)
			{
				m_scroll.scrollOffset = new Vector2(0f, m_savedScrollOffsetY);
			}
		}).ExecuteLater(10L);
	}

	private void SaveScrollOffset()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (m_scroll != null)
		{
			m_savedScrollOffsetY = m_scroll.scrollOffset.y;
		}
	}

	public override void Hide()
	{
		base.Hide();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnRule()
	{
		zbUIMgr.Instance.ShowOpenServerRankRule();
	}
}
