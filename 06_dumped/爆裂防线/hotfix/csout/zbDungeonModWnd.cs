using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonModWnd : zbBaseWnd
{
	[NonSerialized]
	private zbDungeonModPanel[] m_ModPanels = new zbDungeonModPanel[3];

	private Button m_btnModList;

	private int selectNum;

	private DungeonTriggerEventInfo m_eventInfo;

	private DungeonBaseInfo m_dungeonBaseInfo;

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
			zbDungeonModPanel zbDungeonModPanel2 = new zbDungeonModPanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "mod" + i, (string)null));
			m_ModPanels[i] = zbDungeonModPanel2;
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_ModPanels[i].btnSelect, (Action<ClickEvent>)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
			m_spineAvatar[i] = ((Component)m_BaseObject.transform.Find("GameObject" + i).Find("playerAvatar" + i)).GetComponent<zbPlayerAvatar>();
			m_heroAvatar[i] = ((Component)m_BaseObject.transform.Find("GameObject" + i).Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		}
	}

	public void RefreshCustom(DungeonTriggerEventInfo info, DungeonBaseInfo dungeonBaseInfo)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		m_BaseObject.transform.position = new Vector3(((Component)zbCameraHelper.Instance.mainCamera).transform.position.x, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.y - 0.7f, -2f);
		m_eventInfo = info;
		(zbUIMgr.Instance.GetWndById(EWndId.Dungeon) as zbDungeonWnd).SetActionUiDisplay(vis: false);
		m_dungeonBaseInfo = dungeonBaseInfo;
		if (m_eventInfo == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			if (info.EventModList.Count > i)
			{
				int key = info.EventModList[i];
				zbModCsvRecord modRecord = zbDataConfig.Instance.modCsvTable.GetRecordByKey(key);
				m_ModPanels[i].RefreshDetail(modRecord);
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((Component)m_spineAvatar[i]).gameObject.SetActive(modRecord.skillType[0] == 1);
				((Component)m_heroAvatar[i]).gameObject.SetActive(modRecord.skillType[0] != 1);
				if (modRecord.skillType[0] != 1)
				{
					SkillDetail skillDetail = ((IEnumerable<SkillDetail>)dungeonBaseInfo.SkillList).First((SkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
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
		m_BaseObject.SetActive(true);
	}

	private void OnSelect(int index)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		int modId = m_eventInfo.EventModList[index];
		zbDungeonMgr.Instance.dungeonHandler.DungeonEventModSelect_Req(modId);
		zbDungeonWnd obj = zbUIMgr.Instance.GetWndById(EWndId.Dungeon) as zbDungeonWnd;
		m_BaseObject.transform.position = new Vector3(0f, 0f, 0f);
		obj.SetActionUiDisplay(vis: true);
		Hide();
	}

	private void OnModList()
	{
		zbUIMgr.Instance.ShowSkillFightModWnd(0, ((IEnumerable<SkillDetail>)m_dungeonBaseInfo.SkillList).ToArray(), ((IEnumerable<int>)m_dungeonBaseInfo.ModList).ToList(), isResumeFight: false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_BaseObject.SetActive(false);
	}
}
