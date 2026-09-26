using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPvpModSelectWnd : zbBaseWnd
{
	[NonSerialized]
	private VisualElement num_bg;

	[NonSerialized]
	private Button m_btnRefresh;

	private Button m_btnCancel;

	public Button m_btnGuide;

	[NonSerialized]
	private zbLegionWarModPanel[] m_ModPanels = new zbLegionWarModPanel[3];

	private int selectNum;

	private int selectMax;

	[NonSerialized]
	private VisualElement[] m_numBg = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbNum = (Label[])(object)new Label[5];

	private zbPlayerAvatar[] m_spineAvatar = new zbPlayerAvatar[3];

	private SkeletonAnimation[] m_heroAvatar = (SkeletonAnimation[])(object)new SkeletonAnimation[3];

	private List<SkillDetail> heroList = new List<SkillDetail>();

	private Label m_lbDiamond;

	private zbPvpModSelected m_ModSelected;

	protected override void OnEnable()
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		num_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "numBg", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnGuide = UQueryExtensions.Q<Button>(m_RootElement, "btn_guide", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnCancel, "", (TrickleDown)0);
		((VisualElement)m_btnCancel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_ModSelected = new zbPvpModSelected(UQueryExtensions.Q<VisualElement>(m_RootElement, "modSelected", (string)null), this);
		for (int i = 0; i < 3; i++)
		{
			zbLegionWarModPanel zbLegionWarModPanel2 = new zbLegionWarModPanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "mod" + i, (string)null));
			m_ModPanels[i] = zbLegionWarModPanel2;
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_ModPanels[i].btnSelect, (Action<ClickEvent>)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
			m_spineAvatar[i] = ((Component)m_GameObjectSceneRoot.transform.Find("GameObject" + i).Find("playerAvatar" + i)).GetComponent<zbPlayerAvatar>();
			m_heroAvatar[i] = ((Component)m_GameObjectSceneRoot.transform.Find("GameObject" + i).Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		}
		for (int num = 0; num < 5; num++)
		{
			m_numBg[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "num_" + num, (string)null);
			m_lbNum[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_num" + num, (string)null);
		}
		m_lbDiamond = UQueryExtensions.Q<Label>(m_RootElement, "lb_diamond", (string)null);
		((TextElement)m_lbDiamond).text = zbSelf.Instance.pvpHandler.modRefreshDiamond.ToString();
		m_gameEevent.RegisterEvent(EGameEvent.Event_PvpSet_ModRefresh, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_PvpSet_ModSelect, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_PvpSet_ModSubmit, OnClose);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
	}

	public void ChangeNum()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		if (selectMax <= 5 || selectNum < 3)
		{
			for (int i = 0; i < 5; i++)
			{
				((TextElement)m_lbNum[i]).text = (i + 1).ToString();
				if (i >= selectMax)
				{
					m_numBg[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
				else
				{
					m_numBg[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
				Sprite val = ((i != selectNum) ? zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_un.png") : zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_se.png"));
				m_numBg[i].style.backgroundImage = new StyleBackground(val);
			}
		}
		else if (selectNum > selectMax - 3)
		{
			for (int j = 0; j < 5; j++)
			{
				((TextElement)m_lbNum[j]).text = (selectMax - 5 + j + 1).ToString();
				m_numBg[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				Sprite val2 = ((selectMax - 5 + j != selectNum) ? zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_un.png") : zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_se.png"));
				m_numBg[j].style.backgroundImage = new StyleBackground(val2);
			}
		}
		else
		{
			for (int k = 0; k < 5; k++)
			{
				((TextElement)m_lbNum[k]).text = (selectNum - 2 + k + 1).ToString();
				m_numBg[k].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				Sprite val3 = ((selectNum - 2 + k != selectNum) ? zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_un.png") : zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_se.png"));
				m_numBg[k].style.backgroundImage = new StyleBackground(val3);
			}
		}
		zbUtil.CenterNodeItems(m_numBg, 44f, 30f);
	}

	public void RefreshDetail()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		ChangeNum();
		RefreshItems();
		m_ModSelected.Refresh(zbServerData.sGameData.PvpInfo.SetModInfo);
		int num = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.PvpModRefresh, zbSelf.Instance.GetVipLevel());
		if (num >= 10000)
		{
			((TextElement)m_btnRefresh).text = zbUtil.Language("刷新");
		}
		else
		{
			((TextElement)m_btnRefresh).text = zbUtil.Language("刷新") + "(" + (num - zbServerData.sGameData.PvpInfo.RefreshCount) + ")";
		}
		((VisualElement)m_btnCancel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.PvpInfo.ValidModInfo.SelectList.Count <= 0));
	}

	public void RefreshCustom()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		selectNum = zbServerData.sGameData.PvpInfo.SetModInfo.SelectList.Count;
		selectMax = zbSelf.Instance.pvpHandler.modSelectNum;
		if (selectNum >= selectMax)
		{
			zbUIMgr.Instance.ShowPvpModChange();
			Hide();
			return;
		}
		num_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(selectMax <= 1));
		heroList.Clear();
		foreach (int select in zbServerData.sGameData.PvpInfo.SetModInfo.SelectList)
		{
			foreach (SkillDetail skill in zbServerData.sGameData.PvpInfo.SetModInfo.SkillList)
			{
				if (skill.SkillType + 20010 == (ESkillType)select)
				{
					SkillDetail skillDetail = new SkillDetail();
					skillDetail.HeroTid = skill.HeroTid;
					skillDetail.SkillType = skill.SkillType;
					skillDetail.SkillId = skill.SkillId;
					heroList.Add(skillDetail);
				}
			}
		}
		RefreshDetail();
	}

	public void RefreshItems()
	{
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		RepeatedField<int> setModList = zbServerData.sGameData.PvpInfo.SetModList;
		for (int i = 0; i < 3; i++)
		{
			if (setModList.Count > i)
			{
				int key = setModList[i];
				zbModCsvRecord modRecord = zbDataConfig.Instance.modCsvTable.GetRecordByKey(key);
				m_ModPanels[i].RefreshDetail(modRecord, ((IEnumerable<SkillDetail>)zbServerData.sGameData.PvpInfo.SetModInfo.SkillList).ToArray());
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((Component)m_spineAvatar[i]).gameObject.SetActive(modRecord.skillType[0] == 1);
				((Component)m_heroAvatar[i]).gameObject.SetActive(modRecord.skillType[0] != 1);
				if (modRecord.skillType[0] != 1)
				{
					SkillDetail skillDetail = ((IEnumerable<SkillDetail>)zbServerData.sGameData.PvpInfo.SetModInfo.SkillList).First((SkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
					zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(skillDetail.HeroTid);
					zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
					zbUtil.RefreshSpine(m_heroAvatar[i], recordByKey2.frontBone + "_SkeletonData.asset", "idle", loop: true);
				}
				else
				{
					m_spineAvatar[i].RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
				}
			}
			else
			{
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((Component)m_spineAvatar[i]).gameObject.SetActive(false);
				((Component)m_heroAvatar[i]).gameObject.SetActive(false);
			}
		}
	}

	public void OnSelect(int index)
	{
		zbWsMsgHandler.Instance.PvpSet_ModSelect_Req(selectNum, zbServerData.sGameData.PvpInfo.SetModList[index]);
	}

	public void OnRefresh()
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.PvpModRefresh, zbSelf.Instance.GetVipLevel()) <= zbServerData.sGameData.PvpInfo.RefreshCount)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11496"));
		}
		else
		{
			zbWsMsgHandler.Instance.PvpSet_ModRefresh_Req();
		}
	}

	public void OnSkillClick(ClickEvent evt, int skillIndex)
	{
		List<int> modList = ((IEnumerable<int>)zbServerData.sGameData.PvpInfo.SetModInfo.SelectList).ToList();
		zbUIMgr.Instance.ShowSkillFightModWnd(skillIndex, heroList.ToArray(), modList, isResumeFight: false);
	}

	protected void OnSwitchCancel(bool isOk)
	{
		if (isOk)
		{
			zbWsMsgHandler.Instance.PvpSet_ModCancel_Req();
		}
	}

	public void OnCancel()
	{
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.10114"), OnSwitchCancel);
	}

	private void OnClose(object data = null)
	{
		Hide();
	}

	private void OnEventUpdate(object data = null)
	{
		RefreshCustom();
	}
}
