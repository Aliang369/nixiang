using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPeerageInfoWnd : zbBaseWnd
{
	private class PeerageData
	{
		public string TitleName;

		public string PeerageName;

		public Sprite PeerageIcon;

		public string UnlockFunctionDesc;

		public zbItemData[] Awards;

		public zbItemData[] Blueprints;
	}

	private Button m_btnTouch;

	private Label m_lbTitleName;

	private Label m_lbPeerageLevel;

	private Button m_btnLeft;

	private Button m_btnRight;

	private VisualElement m_nodeAwards;

	private SpineVisualElement m_spineShuzi;

	private SpineVisualElement m_spineShuziCut;

	private readonly zbItem[] m_awards = new zbItem[3];

	private VisualElement m_nodeUnlock;

	private Label m_lbUnlockFunctionDesc;

	private VisualElement m_nodeBlueprint;

	private VisualElement m_nodeItem2;

	private readonly zbItem[] m_itemBlueprints = new zbItem[12];

	private VisualElement m_nodeRoomBlueprint;

	private VisualElement m_container;

	private VisualTreeAsset treeAsset;

	private int m_curPeerageLevel;

	private zbPeerageCsvRecord m_curPeerageRecord;

	private int? m_prevPeerageLevel;

	private bool m_showPeerageup;

	private readonly List<zbHomeRoomTypeCsvRecord> m_roomBlueprintList = new List<zbHomeRoomTypeCsvRecord>();

	private readonly List<zbFunctionUnlockCsvRecord> m_cachedFunctionUnlockList = new List<zbFunctionUnlockCsvRecord>();

	private readonly List<int> m_peerageLevelList = new List<int>();

	protected override void OnEnable()
	{
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		treeAsset = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/peerage/tb_peerageInfoRoomItem.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitleName = UQueryExtensions.Q<Label>(m_RootElement, "lb_titleName", (string)null);
		m_lbPeerageLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_peerageLevel", (string)null);
		m_spineShuzi = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineShuzi", (string)null);
		m_spineShuziCut = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineShuziCut", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_nodeAwards = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodeAwards", (string)null);
		for (int i = 0; i < m_awards.Length; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_nodeAwards, "awardItem" + i, (string)null);
			m_awards[i].OnEnable(this);
		}
		m_nodeUnlock = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodeUnlock", (string)null);
		m_lbUnlockFunctionDesc = UQueryExtensions.Q<Label>(m_nodeUnlock, "lb_unlockFunctionDesc", (string)null);
		m_nodeBlueprint = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodeBlueprint", (string)null);
		m_nodeItem2 = UQueryExtensions.Q<VisualElement>(m_nodeBlueprint, "noodeItem2", (string)null);
		for (int j = 0; j < m_itemBlueprints.Length; j++)
		{
			m_itemBlueprints[j] = UQueryExtensions.Q<zbItem>(m_nodeBlueprint, "item" + j, (string)null);
			m_itemBlueprints[j].OnEnable(this);
		}
		m_nodeRoomBlueprint = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodeRoomBlueprint", (string)null);
		m_container = UQueryExtensions.Q<VisualElement>(m_RootElement, "container", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action<ClickEvent>)delegate
		{
			OnGotoNext(-1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action<ClickEvent>)delegate
		{
			OnGotoNext(1);
		}, "", (TrickleDown)0);
		UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null).style.backgroundColor = StyleColor.op_Implicit(new Color(0f, 0f, 0f, 0.8f));
	}

	public void refreshCustom()
	{
		m_showPeerageup = false;
		m_curPeerageLevel = zbServerData.sGameData.PeerageInfo.PeerageId;
		RefreshView();
	}

	private void RefreshView()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		bool hasValue = m_prevPeerageLevel.HasValue;
		((VisualElement)m_spineShuzi).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(hasValue ? 1 : 0));
		((VisualElement)m_spineShuziCut).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!hasValue));
		if (hasValue)
		{
			((MonoBehaviour)this).StartCoroutine(PlayUpgradeEffect());
		}
		else
		{
			string text = m_curPeerageLevel.ToString();
			if (m_spineShuzi.skeleton.Data.FindAnimation(text) != null)
			{
				m_spineShuzi.AnimationState.SetAnimation(0, text, true);
			}
		}
		((TextElement)m_lbPeerageLevel).text = string.Format(zbUtil.Language("lang.10719"), m_curPeerageLevel);
		m_curPeerageRecord = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(m_curPeerageLevel);
		((TextElement)m_lbTitleName).text = zbUtil.Language("lang.10453");
		if (m_prevPeerageLevel.HasValue && m_prevPeerageLevel.Value != m_curPeerageLevel)
		{
			((TextElement)m_lbTitleName).text = zbUtil.Language("lang.10454");
		}
		for (int i = 0; i < m_awards.Length; i++)
		{
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		for (int j = 0; j < m_curPeerageRecord.awardList.Length; j++)
		{
			if (j < m_awards.Length)
			{
				((VisualElement)m_awards[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_awards[j].Refresh(new zbItemData((EResourceType)m_curPeerageRecord.awardList[j][0], m_curPeerageRecord.awardList[j][1], m_curPeerageRecord.awardList[j][2]));
			}
		}
		m_nodeAwards.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_curPeerageRecord.awardList.Length == 0));
		List<zbFunctionUnlockCsvRecord> functionUnlockRecords = GetFunctionUnlockRecords();
		if (functionUnlockRecords.Count > 0)
		{
			m_nodeUnlock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (zbFunctionUnlockCsvRecord item in functionUnlockRecords)
			{
				string text2 = zbUtil.AddHighlightStyle(zbUtil.Language(item.name), "008415", 26) + ": " + zbUtil.Language(item.desc);
				if (zbSelf.Instance.funHandler.IsLockNoTip((EFuncType)item.id) && zbSelf.Instance.funHandler.GetFuncOpenLeftTime(item.serverHours) > 0)
				{
					text2 += string.Format(zbUtil.Language("lang.11048"), zbUtil.FormatTimeHHMMSS(zbSelf.Instance.funHandler.GetFuncOpenLeftTime(item.serverHours)));
				}
				stringBuilder.AppendLine(text2);
			}
			((TextElement)m_lbUnlockFunctionDesc).text = stringBuilder.ToString().TrimEnd('\n');
		}
		else
		{
			m_nodeUnlock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_nodeBlueprint.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshRoomBlueprintList();
		m_container.Clear();
		if (m_roomBlueprintList.Count > 0)
		{
			m_nodeRoomBlueprint.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			for (int k = 0; k < m_roomBlueprintList.Count; k++)
			{
				VisualElement val = (VisualElement)(object)treeAsset.CloneTree();
				UQueryExtensions.Q<VisualElement>(val, "roomIcon", (string)null).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_roomBlueprintList[k].iconType));
				((TextElement)UQueryExtensions.Q<Label>(val, "lb_roomDesc", (string)null)).text = m_roomBlueprintList[k].name;
				m_container.Add(val);
			}
		}
		else
		{
			m_nodeRoomBlueprint.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = zbDataConfig.Instance.peerageCsvTable.GetRecords().records.Keys.Min();
		int num2 = zbServerData.sGameData.PeerageInfo.PeerageId + 1;
		if (m_prevPeerageLevel.HasValue)
		{
			((VisualElement)m_btnLeft).SetEnabled(false);
			((VisualElement)m_btnRight).SetEnabled(false);
			((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnLeft).SetEnabled(m_curPeerageLevel > num);
			bool flag = m_curPeerageRecord.ifMax == 1;
			((VisualElement)m_btnRight).SetEnabled(!flag || m_curPeerageLevel < num2);
			((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_curPeerageLevel == 1));
			((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag || m_curPeerageLevel >= num2) ? 1 : 0));
		}
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "unity-content-and-vertical-scroll-container", (string)null);
		if (val2 != null)
		{
			val2.pickingMode = (PickingMode)1;
		}
	}

	protected override void UpdatePerSecond()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePerSecond();
		List<zbFunctionUnlockCsvRecord> functionUnlockRecords = GetFunctionUnlockRecords();
		if (functionUnlockRecords.Count <= 0 || (int)m_nodeUnlock.resolvedStyle.display != 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (zbFunctionUnlockCsvRecord item in functionUnlockRecords)
		{
			string text = zbUtil.AddHighlightStyle(zbUtil.Language(item.name), "008415", 26) + ": " + zbUtil.Language(item.desc);
			if (zbSelf.Instance.funHandler.IsLockNoTip((EFuncType)item.id))
			{
				text += string.Format(zbUtil.Language("lang.11048"), zbUtil.FormatTimeHHMMSS(zbSelf.Instance.funHandler.GetFuncOpenLeftTime(item.serverHours)));
			}
			stringBuilder.AppendLine(text);
		}
		((TextElement)m_lbUnlockFunctionDesc).text = stringBuilder.ToString().TrimEnd('\n');
	}

	private IEnumerator PlayUpgradeEffect()
	{
		yield return null;
		yield return null;
		yield return null;
		((VisualElement)m_spineShuzi).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_spineShuziCut).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_spineShuziCut.AnimationState.SetAnimation(0, "animation", false) != null)
		{
			yield return (object)new WaitForSeconds(8f / 15f);
		}
		((VisualElement)m_spineShuziCut).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_spineShuzi).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		string text = m_curPeerageLevel.ToString();
		if (m_spineShuzi.skeleton.Data.FindAnimation(text) != null)
		{
			m_spineShuzi.AnimationState.SetAnimation(0, text, true);
		}
		yield return null;
	}

	public List<zbFunctionUnlockCsvRecord> GetFunctionUnlockRecords()
	{
		m_cachedFunctionUnlockList.Clear();
		foreach (zbFunctionUnlockCsvRecord value in zbDataConfig.Instance.functionUnlockCsvTable.GetRecords().records.Values)
		{
			if (value != null && value.unlockType == 2 && value.unlockVal == m_curPeerageLevel && value.showTime != 2)
			{
				m_cachedFunctionUnlockList.Add(value);
			}
		}
		return m_cachedFunctionUnlockList;
	}

	private void RefreshRoomBlueprintList()
	{
		m_roomBlueprintList.Clear();
		foreach (zbHomeRoomTypeCsvRecord value in zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecords().records.Values)
		{
			if (value != null && value.peerageId == m_curPeerageLevel && zbVersionMgr.IsReleased(value.preShow))
			{
				m_roomBlueprintList.Add(value);
			}
		}
	}

	private void InitPeerageLevelList()
	{
		m_peerageLevelList.Clear();
		foreach (KeyValuePair<int, zbPeerageCsvRecord> record in zbDataConfig.Instance.peerageCsvTable.GetRecords().records)
		{
			m_peerageLevelList.Add(record.Key);
		}
	}

	public void OnGotoNext(int index)
	{
		InitPeerageLevelList();
		int num = m_peerageLevelList.IndexOf(m_curPeerageLevel);
		if (num < 0)
		{
			return;
		}
		int num2 = num + index;
		if (num2 < 0 || num2 >= m_peerageLevelList.Count)
		{
			return;
		}
		int num3 = m_peerageLevelList[num2];
		int num4 = zbServerData.sGameData.PeerageInfo.PeerageId + 1;
		if (num3 <= num4)
		{
			zbPeerageCsvRecord recordByKey = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(num3);
			if (recordByKey != null)
			{
				m_curPeerageLevel = num3;
				m_curPeerageRecord = recordByKey;
				RefreshView();
			}
		}
	}

	public void ShowPeerageUp()
	{
		m_showPeerageup = true;
		m_curPeerageLevel = zbServerData.sGameData.PeerageInfo.PeerageId;
		m_prevPeerageLevel = m_curPeerageLevel - 1;
		RefreshView();
	}

	private void OnClose()
	{
		m_prevPeerageLevel = null;
		Hide();
		OnShowAppReview();
	}

	private void OnShowAppReview()
	{
		bool flag = false;
		string key = "appreview_key";
		int num = zbGameSave.Instance.GetInt(key);
		if (num < 3)
		{
			if (m_showPeerageup && (zbServerData.sGameData.PeerageInfo.PeerageId == 3 || zbServerData.sGameData.PeerageInfo.PeerageId == 8 || zbServerData.sGameData.PeerageInfo.PeerageId == 15))
			{
				flag = true;
			}
			if (flag && (zbLoginUtil.appChannel == EChannelType.Ios || zbLoginUtil.appChannel == EChannelType.TapTap || zbLoginUtil.appChannel == EChannelType.Hykb || zbLoginUtil.appChannel == EChannelType.Dyunion))
			{
				zbUIMgr.Instance.ShowAppReview();
				zbGameSave.Instance.SetInt(key, num + 1);
			}
		}
	}
}
