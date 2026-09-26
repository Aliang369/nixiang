using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroRarityWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SkeletonAnimation m_skeletonHero;

	private SkeletonAnimation m_spineChuChang;

	private VisualElement m_nodeNone;

	private VisualElement m_btnNoneAdd;

	private VisualElement m_nodeRarity;

	private Label m_lbName;

	private VisualElement m_spriteSkillType;

	private Button m_btnHero;

	private VisualElement m_nodeNotify;

	private VisualElement m_spriteNotifyOk;

	private VisualElement m_spriteRarity0;

	private VisualElement m_spriteRarity1;

	private VisualElement m_progress0;

	private VisualElement m_progress1;

	private Label m_lbProgress;

	private Label m_lbDamage0;

	private Label m_lbDamage1;

	private VisualElement[] m_spriteFrames = (VisualElement[])(object)new VisualElement[6];

	private Button[] m_btnAdds = (Button[])(object)new Button[6];

	private zbItem[] m_items = new zbItem[6];

	private Button m_btnRarity;

	private Button m_btnRule;

	private Button m_btnClose;

	private int m_heroTid;

	private HeroDetail m_heroDetail;

	private int m_upgradeFrag;

	private int m_needFrag;

	private string m_animName;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = m_GameObjectSceneRoot.GetComponentInChildren<zbSpineScene>();
		m_skeletonHero = ((Component)m_GameObjectSceneRoot.transform.Find("hero")).GetComponent<SkeletonAnimation>();
		m_spineChuChang = ((Component)m_GameObjectSceneRoot.transform.Find("chuChang")).GetComponent<SkeletonAnimation>();
		((Component)m_spineChuChang).gameObject.SetActive(false);
		m_nodeNone = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_none", (string)null);
		m_btnNoneAdd = (VisualElement)(object)UQueryExtensions.Q<Button>(m_nodeNone, "btn_noneAdd", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnNoneAdd, (Action)OnNoneAdd, "", (TrickleDown)0);
		m_nodeRarity = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_rarity", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_nodeRarity, "lb_name", (string)null);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>(m_nodeRarity, "bg_skillType", (string)null);
		m_btnHero = UQueryExtensions.Q<Button>(m_nodeRarity, "btn_hero", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHero, (Action)OnNoneAdd, "", (TrickleDown)0);
		m_nodeNotify = UQueryExtensions.Q<VisualElement>(m_nodeRarity, "node_notify", (string)null);
		m_spriteNotifyOk = UQueryExtensions.Q<VisualElement>(m_nodeNotify, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeNotify, (Action)OnNotify, "", (TrickleDown)0);
		m_spriteRarity0 = UQueryExtensions.Q<VisualElement>(m_nodeRarity, "bg_rarity0", (string)null);
		m_spriteRarity1 = UQueryExtensions.Q<VisualElement>(m_nodeRarity, "bg_rarity1", (string)null);
		m_lbDamage0 = UQueryExtensions.Q<Label>(m_nodeRarity, "lb_damage0", (string)null);
		m_lbDamage1 = UQueryExtensions.Q<Label>(m_nodeRarity, "lb_damage1", (string)null);
		m_progress0 = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress0", (string)null);
		m_progress1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress1", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_spriteFrames[i] = UQueryExtensions.Q<VisualElement>(m_nodeRarity, "item" + i, (string)null);
			m_btnAdds[i] = UQueryExtensions.Q<Button>(m_spriteFrames[i], "btn_add", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdds[i], (Action<ClickEvent>)delegate
			{
				OnAdds(curIndex);
			}, "", (TrickleDown)0);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_spriteFrames[i], "item", (string)null);
			m_items[i].OnEnable(this);
			m_items[i].SetTouchCallback(delegate
			{
				OnAdds(curIndex);
			});
		}
		m_btnRarity = UQueryExtensions.Q<Button>(m_nodeRarity, "btn_rarity", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRarity, (Action)OnRarity, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_HeroRarityUpgrade_Ack, OnEventUpdate);
	}

	public void RefreshCustom(int heroTid = -1)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (heroTid < 0)
		{
			m_nodeNone.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeRarity.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_animName = "unsel";
			m_spineScene.SetAnimation(m_animName, isLoop: true);
			((Component)m_skeletonHero).gameObject.SetActive(false);
			zbSelf.Instance.heroHandler.selectList.Clear();
			m_heroTid = -1;
		}
		else
		{
			m_heroTid = heroTid;
			RefreshRarityUpgradeHero();
		}
		m_spriteNotifyOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroTid > 0 && zbSelf.Instance.newTagHandler.IsNotNotifyHeroRarity(m_heroTid)) ? 1 : 0));
	}

	public void RefreshRarityUpgradeHero()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		if (m_heroTid <= 0)
		{
			RefreshCustom();
			return;
		}
		m_nodeNone.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeRarity.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_animName != "sel")
		{
			m_animName = "sel";
			m_spineScene.SetAnimation(m_animName, isLoop: true);
		}
		((Component)m_skeletonHero).gameObject.SetActive(true);
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_heroTid);
		m_heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(m_heroTid);
		if (recordByKey != null && m_heroDetail != null)
		{
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				zbUtil.RefreshSpine(m_skeletonHero, recordByKey2.frontBone + "_SkeletonData.asset", "idle", loop: true);
			}
			((TextElement)m_lbName).text = recordByKey.name;
			string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(recordByKey, m_heroDetail.Rarity);
			m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
			string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(m_heroDetail.Rarity);
			m_spriteRarity0.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
			float damage = zbSelf.Instance.heroHandler.GetDamage(m_heroDetail.Quality, m_heroDetail.Rarity, m_heroDetail.QualityStar);
			((TextElement)m_lbDamage0).text = $"{damage * 100f:0.##}%";
			ERarityType eRarityType = m_heroDetail.Rarity + 1;
			string heroRarityIconPath2 = zbResourceHelper.GetHeroRarityIconPath(eRarityType);
			m_spriteRarity1.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath2));
			float damage2 = zbSelf.Instance.heroHandler.GetDamage(m_heroDetail.Quality, eRarityType, m_heroDetail.QualityStar);
			((TextElement)m_lbDamage1).text = $"{damage2 * 100f:0.##}%";
			m_upgradeFrag = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey((int)eRarityType).upgradeFrag;
			RefreshSelectList();
		}
	}

	public void RefreshSelectList()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (BagSelectDetail select in zbSelf.Instance.heroHandler.selectList)
		{
			if (m_items[num] != null)
			{
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(new zbItemData(EResourceType.HeroFrag, select.Tid, select.Count));
			}
			num++;
		}
		long num2 = zbSelf.Instance.heroHandler.selectList.Sum((BagSelectDetail v) => v.Count);
		if (num2 > 0)
		{
			((TextElement)m_lbProgress).text = m_heroDetail.RarityUpgrade + "+" + num2 + "/" + m_upgradeFrag;
		}
		else
		{
			((TextElement)m_lbProgress).text = m_heroDetail.RarityUpgrade + "/" + m_upgradeFrag;
		}
		float num3 = Mathf.Clamp((float)(m_heroDetail.RarityUpgrade + num2) / (float)m_upgradeFrag, 0f, 1f);
		m_progress0.style.width = StyleLength.op_Implicit(new Length(num3 * 530f, (LengthUnit)0));
		float num4 = Mathf.Clamp((float)m_heroDetail.RarityUpgrade / (float)m_upgradeFrag, 0f, 1f);
		m_progress1.style.width = StyleLength.op_Implicit(new Length(num4 * 530f, (LengthUnit)0));
		m_needFrag = m_upgradeFrag - m_heroDetail.RarityUpgrade;
	}

	private void OnNoneAdd()
	{
		zbUIMgr.Instance.ShowHeroRaritySelect(m_heroTid, delegate(int newId)
		{
			if (newId != m_heroTid)
			{
				m_heroTid = newId;
				RefreshRarityUpgradeHero();
			}
		});
	}

	private void OnNotify()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (!zbSelf.Instance.newTagHandler.IsNotNotifyHeroRarity(m_heroTid))
		{
			zbSelf.Instance.newTagHandler.AddNotNotifyHeroRarity(m_heroTid);
		}
		else
		{
			zbSelf.Instance.newTagHandler.RemoveNotNotifyHeroRarity(m_heroTid);
		}
		m_spriteNotifyOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroTid > 0 && zbSelf.Instance.newTagHandler.IsNotNotifyHeroRarity(m_heroTid)) ? 1 : 0));
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Hero);
	}

	private void OnAdds(int index)
	{
		zbUIMgr.Instance.ShowHeroRarityFragSelect(m_needFrag, delegate(bool isOK)
		{
			if (isOK)
			{
				RefreshSelectList();
			}
		});
	}

	private void OnRarity()
	{
		if (zbSelf.Instance.heroHandler.selectList.Count == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10260"));
		}
		else
		{
			zbWsMsgHandler.Instance.Hero_RarityUpgrade_Req(m_heroTid, zbSelf.Instance.heroHandler.selectList.ToArray());
		}
	}

	private void OnRule()
	{
		string[] langKeys = new string[3] { "lang.11105", "lang.11106", "lang.11107" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(m_heroTid);
		if (heroDetail.Rarity > m_heroDetail.Rarity)
		{
			zbUIMgr.Instance.ShowHeroRaritySuccess(heroDetail);
		}
		else
		{
			ActionChuchangAnim();
		}
		if (heroDetail.Rarity >= ERarityType.Ss)
		{
			RefreshCustom();
			return;
		}
		zbSelf.Instance.heroHandler.selectList.Clear();
		RefreshRarityUpgradeHero();
	}

	private void ActionChuchangAnim()
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
				m_skeletonHero.AnimationState.SetAnimation(0, "show", false);
				m_skeletonHero.AnimationState.AddAnimation(0, "idle", true, 0f);
				((Component)m_spineChuChang).gameObject.SetActive(false);
			};
		}
	}
}
