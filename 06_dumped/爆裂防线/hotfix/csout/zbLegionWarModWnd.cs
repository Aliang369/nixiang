using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarModWnd : zbBaseWnd
{
	[NonSerialized]
	private zbLegionWarModPanel[] m_ModPanels = new zbLegionWarModPanel[3];

	private Button m_btnModList;

	private int selectNum;

	private LegionWarSelfTeamInfo m_selfInfo;

	private zbPlayerAvatar[] m_spineAvatar = new zbPlayerAvatar[3];

	private SkeletonAnimation[] m_heroAvatar = (SkeletonAnimation[])(object)new SkeletonAnimation[3];

	protected GameObject m_BaseObject;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnModList = UQueryExtensions.Q<Button>(m_RootElement, "btn_modList", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnModList, (Action)OnModList, "", (TrickleDown)0);
		m_BaseObject = ((Component)((Component)this).transform.Find("baseObject")).gameObject;
		for (int i = 0; i < 3; i++)
		{
			zbLegionWarModPanel zbLegionWarModPanel2 = new zbLegionWarModPanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "mod" + i, (string)null));
			m_ModPanels[i] = zbLegionWarModPanel2;
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_ModPanels[i].btnSelect, (Action<ClickEvent>)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
			m_spineAvatar[i] = ((Component)m_BaseObject.transform.Find("GameObject" + i).Find("playerAvatar" + i)).GetComponent<zbPlayerAvatar>();
			m_heroAvatar[i] = ((Component)m_BaseObject.transform.Find("GameObject" + i).Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		}
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarModSelect, OnEventUpdate);
	}

	public void RefreshCustom(LegionWarSelfTeamInfo legionWarSelfInfo)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		m_selfInfo = legionWarSelfInfo;
		m_BaseObject.transform.position = new Vector3(((Component)zbCameraHelper.Instance.mainCamera).transform.position.x, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.y - 0.7f, -2f);
		zbLegionWarZhanlingWnd zbLegionWarZhanlingWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegionWarZhanling) as zbLegionWarZhanlingWnd;
		if ((Object)(object)zbLegionWarZhanlingWnd2 != (Object)null)
		{
			zbLegionWarZhanlingWnd2.SetVisibleUI(flag: false);
		}
		RefreshModList();
		m_BaseObject.SetActive(true);
	}

	private void RefreshModList()
	{
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (m_selfInfo == null)
			{
				break;
			}
			if (m_selfInfo.ModSelectList == null)
			{
				break;
			}
			if (m_selfInfo.ModSelectList.Count > i)
			{
				int key = m_selfInfo.ModSelectList[i];
				zbModCsvRecord modRecord = zbDataConfig.Instance.modCsvTable.GetRecordByKey(key);
				m_ModPanels[i].RefreshDetail(modRecord, ((IEnumerable<SkillDetail>)m_selfInfo.SkillList).ToArray());
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				bool flag = modRecord.skillType[0] == 1;
				((Component)m_spineAvatar[i]).gameObject.SetActive(flag);
				((Component)m_heroAvatar[i]).gameObject.SetActive(!flag);
				if (!flag)
				{
					SkillDetail skillDetail = ((IEnumerable<SkillDetail>)m_selfInfo.SkillList).First((SkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
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

	private void OnSelect(int index)
	{
		int selectId = m_selfInfo.ModSelectList[index];
		zbWsMsgHandler.Instance.LegionWar_ModSelect_Req(selectId);
	}

	private void OnModList()
	{
		zbSelf.Instance.legionWarHandler.OnModList();
	}

	private void OnEventUpdate(object data)
	{
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		_ = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		zbDataConfig.Instance.legionWarLevelCsvTable.GetRecordByKey(legionWarZhanLingInfo.Level + 1);
		zbSelf.Instance.legionWarHandler.GetLegionWarState();
		if (zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.ModSelectList != null && zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.ModSelectList.Count > 0)
		{
			m_selfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo;
			RefreshModList();
			return;
		}
		zbLegionWarZhanlingWnd zbLegionWarZhanlingWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegionWarZhanling) as zbLegionWarZhanlingWnd;
		if ((Object)(object)zbLegionWarZhanlingWnd2 != (Object)null)
		{
			zbLegionWarZhanlingWnd2.SetVisibleUI(flag: true);
		}
		Hide();
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_BaseObject.SetActive(false);
	}
}
