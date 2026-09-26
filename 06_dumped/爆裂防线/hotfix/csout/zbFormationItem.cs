using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbFormationItem>
	{
	}

	private zbFormationWnd m_wndParent;

	private int m_skillType;

	private Transform m_nodePlayerAvatar;

	private zbPlayerAvatar m_playerAvatar;

	private zbFightAvatar m_fightAvatar;

	private Transform m_nodeAvatar;

	private SkeletonAnimation m_spineAvatar;

	private SkeletonAnimation m_spineChuChang;

	private SkeletonAnimation m_spineSelect;

	private VisualElement m_bg;

	private VisualElement m_spriteSkillType;

	private Label m_lbLevel;

	private VisualElement m_spriteLock;

	private Button m_btnTouch;

	private Button m_btnNone;

	private VisualElement m_notify;

	private VisualElement m_new;

	private zbHeroCsvRecord m_record;

	public zbFormationItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/formation/tb_formation.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbFormationWnd wndParent, int skillType)
	{
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_skillType = skillType;
		m_nodePlayerAvatar = m_wndParent.GetMyGameObjectSceneRoot().transform.Find("qiangAvatar");
		if (skillType == 1)
		{
			m_playerAvatar = ((Component)((Component)m_nodePlayerAvatar).transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
			m_fightAvatar = ((Component)((Component)m_nodePlayerAvatar).transform.Find("fightAvatar")).GetComponent<zbFightAvatar>();
			m_spineChuChang = ((Component)m_nodePlayerAvatar.Find("chuChang")).GetComponent<SkeletonAnimation>();
			m_spineSelect = ((Component)m_nodePlayerAvatar.Find("select")).GetComponent<SkeletonAnimation>();
		}
		else
		{
			Transform transform = m_wndParent.GetMyGameObjectSceneRoot().transform;
			int num = skillType;
			m_nodeAvatar = transform.Find("hero" + num);
			m_spineAvatar = ((Component)m_nodeAvatar.Find("heroAvatar")).GetComponent<SkeletonAnimation>();
			m_spineChuChang = ((Component)m_nodeAvatar.Find("chuChang")).GetComponent<SkeletonAnimation>();
			m_spineSelect = ((Component)m_nodeAvatar.Find("select")).GetComponent<SkeletonAnimation>();
		}
		((Component)m_spineChuChang).gameObject.SetActive(false);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skillType", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_spriteLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_lock", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_btnNone = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_none", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNone, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag(m_bg, (Vector2?)new Vector2(-10f, -18f), 1f, true);
		m_new = zbUtil.CreateNewTag(m_bg, (Vector2?)new Vector2(-20f, -22f), 0.8f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void RefreshWithoutSpine()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)m_skillType);
		if (warRoomDetailBySkillType != null)
		{
			if (warRoomDetailBySkillType.HeroTid > 0)
			{
				m_record = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
			}
			HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(warRoomDetailBySkillType.RoomId);
			((TextElement)m_lbLevel).text = "Lv." + homelandRoomDetail.Level;
			((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(warRoomDetailBySkillType.HeroTid <= 0));
			RefreshDetail();
			RefreshNew();
			bool flag = m_skillType == 1;
			zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord record) => record.skillType == m_skillType);
			if (zbPlayerSuitCsvRecord2 != null)
			{
				flag = flag || zbSelf.Instance.suitGunHandler.IsPlayerSuitSkillUsed(zbPlayerSuitCsvRecord2.id);
			}
			m_spriteLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (zbSelf.Instance.heroHandler.heroSetTid > 0 && m_record != null && zbSelf.Instance.heroHandler.heroSetTid == m_record.id)
			{
				ActionChuchangAnim();
				zbSelf.Instance.heroHandler.heroSetTid = 0;
			}
		}
	}

	public void RefreshSpinePart()
	{
		RefreshSelect(m_wndParent.m_skillType);
	}

	public void RefreshSpine(bool isSelect)
	{
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if (m_skillType == 1)
		{
			if (isSelect)
			{
				((Component)m_fightAvatar).gameObject.SetActive(false);
				((Component)m_playerAvatar).gameObject.SetActive(true);
				m_playerAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
			}
			else
			{
				((Component)m_fightAvatar).gameObject.SetActive(true);
				((Component)m_playerAvatar).gameObject.SetActive(false);
				m_fightAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
			}
		}
		else if (m_record != null)
		{
			m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((Component)m_nodeAvatar).gameObject.SetActive(true);
			zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(m_record.avatarId);
			if (recordByKey != null)
			{
				if (isSelect)
				{
					zbUtil.RefreshSpine(m_spineAvatar, recordByKey.frontBone + "_SkeletonData.asset", "idle", loop: true);
					((Component)m_spineAvatar).transform.localScale = Vector3.one * 0.8f;
				}
				else
				{
					zbUtil.RefreshSpine(m_spineAvatar, recordByKey.fightBone + "_SkeletonData.asset", "idle", loop: true);
					((Component)m_spineAvatar).transform.localScale = Vector3.one;
				}
			}
		}
		else
		{
			m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((Component)m_nodeAvatar).gameObject.SetActive(false);
		}
	}

	private void RefreshDetail()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (m_record != null)
		{
			ERarityType heroRarity = zbSelf.Instance.heroHandler.GetHeroDetail(m_record.id)?.Rarity ?? ERarityType.EmptyValue;
			string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(m_record, heroRarity);
			m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		}
	}

	public void RefreshNew()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		List<zbHeroCsvRecord> heroRecordsBySkillType = zbSelf.Instance.heroHandler.GetHeroRecordsBySkillType(m_skillType);
		bool flag = false;
		foreach (zbHeroCsvRecord item in heroRecordsBySkillType)
		{
			if (zbSelf.Instance.newTagHandler.IsNewHero(item.id))
			{
				flag = true;
				break;
			}
		}
		bool flag2 = false;
		if (m_record != null && m_record.type != 1)
		{
			flag2 = zbSelf.Instance.heroHandler.HasHeroUpgradeNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroAwakenNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id) || zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)m_record.type, m_record.id) || zbSelf.Instance.heroHandler.HasHeroFollowNotify((ESkillType)m_record.type);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		m_new.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(!flag2 & flag)));
	}

	public void RefreshSelect(int skillType)
	{
		((Component)m_spineSelect).gameObject.SetActive(m_skillType == skillType);
		RefreshSpine(m_skillType == skillType);
	}

	public void ActionChuchangAnim()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		((Component)m_spineChuChang).gameObject.SetActive(true);
		TrackEntry val = m_spineChuChang.AnimationState.SetAnimation(0, "animation", false);
		if (val != null)
		{
			val.Complete += (TrackEntryDelegate)delegate
			{
				((Component)m_spineChuChang).gameObject.SetActive(false);
			};
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_record == null)
		{
			zbSkillTypeCsvRecord recordByKey = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(m_skillType);
			if (recordByKey != null)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10771"), recordByKey.unlockLevel));
			}
		}
		else
		{
			m_wndParent.RefreshSelectList(m_skillType);
			m_wndParent.SetEditPanelVisible(vis: true);
			zbSelf.Instance.newTagHandler.RemoveNewHero(m_record.id);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Hero);
			zbProgressEventHelper.Instance.SetProgressTrack(618);
		}
	}
}
