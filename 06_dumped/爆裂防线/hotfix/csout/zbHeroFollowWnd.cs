using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroFollowWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SpriteRenderer[] m_spriteYingzis = (SpriteRenderer[])(object)new SpriteRenderer[3];

	private SkeletonAnimation[] m_skeletonHeros = (SkeletonAnimation[])(object)new SkeletonAnimation[3];

	private Button[] m_btnHeros = (Button[])(object)new Button[3];

	private VisualElement[] m_nodeDamages = (VisualElement[])(object)new VisualElement[2];

	private Label[] m_lbAdds = (Label[])(object)new Label[2];

	private Button[] m_btnTips = (Button[])(object)new Button[2];

	private Label[] m_lbDamages = (Label[])(object)new Label[3];

	private Label m_lbTipDesc;

	private AutoHideTipController m_tipController1;

	private AutoHideTipController m_tipController2;

	private Button m_btnLock;

	private Label m_lbLevel;

	private Label m_lbAdd;

	private SpineVisualElement m_spine;

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private zbItem m_convertItem;

	private Label m_lbConvertItem;

	private Label m_lbOr;

	private Button m_btnFollow;

	private VisualElement m_notifyFollow;

	private Label m_lbMax;

	private Button m_btnClose;

	private Button m_btnRule;

	private ESkillType m_skillType;

	private int m_maxFollowLevel;

	private int m_level;

	private List<int> m_heroTids = new List<int>();

	private List<zbHeroCsvRecord> m_records;

	private int[][] m_costList;

	private float m_curDamageCoef;

	private bool m_isUpgradeReq;

	private zbHeroFollowCsvRecord m_nextRecord;

	private const int COMMONITEMID = 313000;

	protected override void OnEnable()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = m_GameObjectSceneRoot.GetComponentInChildren<zbSpineScene>();
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_tipDesc", (string)null);
		((VisualElement)m_lbTipDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		for (int i = 0; i < 3; i++)
		{
			m_spriteYingzis[i] = ((Component)m_GameObjectSceneRoot.transform.Find("yingzi" + i)).GetComponent<SpriteRenderer>();
			m_skeletonHeros[i] = ((Component)m_GameObjectSceneRoot.transform.Find("hero" + i)).GetComponent<SkeletonAnimation>();
			m_btnHeros[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_hero" + i, (string)null);
			int index = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHeros[i], (Action)delegate
			{
				OnHeros(index);
			}, "", (TrickleDown)0);
			m_lbDamages[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_damage" + i, (string)null);
			if (i > 0)
			{
				m_nodeDamages[i - 1] = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_damage" + i, (string)null);
				m_lbAdds[i - 1] = UQueryExtensions.Q<Label>(m_RootElement, "lb_add" + i, (string)null);
				m_btnTips[i - 1] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tipDesc" + i, (string)null);
			}
		}
		m_tipController1 = new AutoHideTipController(m_uiEvent, m_btnTips[0], m_lbTipDesc);
		m_tipController2 = new AutoHideTipController(m_uiEvent, m_btnTips[1], m_lbTipDesc);
		m_btnLock = UQueryExtensions.Q<Button>(m_RootElement, "btn_lock", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbAdd = UQueryExtensions.Q<Label>(m_RootElement, "lb_add", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		for (int num = 0; num < 2; num++)
		{
			m_lbCosts[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + num, (string)null);
			m_items[num] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + num, (string)null);
			m_items[num].OnEnable(this);
			((VisualElement)m_lbCosts[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_lbConvertItem = UQueryExtensions.Q<Label>(m_RootElement, "lb_convertItem", (string)null);
		m_convertItem = UQueryExtensions.Q<zbItem>(m_RootElement, "convertItem", (string)null);
		m_convertItem.OnEnable(this);
		m_lbOr = UQueryExtensions.Q<Label>(m_RootElement, "lb_or", (string)null);
		m_btnFollow = UQueryExtensions.Q<Button>(m_RootElement, "btn_follow", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLock, (Action)OnLock, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFollow, (Action)OnFollow, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_notifyFollow = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFollow);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
	}

	public void RefreshCustom(ESkillType skillType, bool isFirstIn = true)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		m_skillType = skillType;
		m_maxFollowLevel = zbSelf.Instance.heroHandler.GetHeroFollowMaxLevel(m_skillType);
		if (isFirstIn)
		{
			for (int i = 0; i < 3; i++)
			{
				SkeletonAnimation skeletonAnimation = m_spineScene.skeletonAnimation;
				Skeleton skeleton = ((SkeletonRenderer)skeletonAnimation).Skeleton;
				skeletonAnimation.AnimationState.SetEmptyAnimation(i, 0f);
				Enumerator<Slot> enumerator = skeleton.Slots.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						enumerator.Current.Attachment = null;
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
				((SkeletonRenderer)skeletonAnimation).LateUpdate();
				((Component)m_spriteYingzis[i]).gameObject.SetActive(false);
				((Component)m_skeletonHeros[i]).gameObject.SetActive(false);
				((VisualElement)m_btnHeros[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				if (i < 2)
				{
					m_nodeDamages[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
			}
		}
		RefreshLevelCost();
		RefreshMoneyPanel();
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType(skillType);
		HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(warRoomDetailBySkillType.HeroTid);
		if (heroDetail == null)
		{
			return;
		}
		m_heroTids.Clear();
		RefreshHero(0, heroDetail, isFirstIn);
		m_records = zbSelf.Instance.heroHandler.GetHeroRecordsBySkillType((int)skillType);
		int num = 1;
		foreach (zbHeroCsvRecord record in m_records)
		{
			if (heroDetail.HeroTid != record.id)
			{
				HeroDetail heroDetail2 = zbSelf.Instance.heroHandler.GetHeroDetail(record.id);
				if (heroDetail2 != null)
				{
					RefreshHero(num, heroDetail2, isFirstIn);
					num++;
				}
			}
		}
		((VisualElement)m_btnLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num != 2));
		zbSelf.Instance.heroHandler.CheckHeroFollowConvert(m_skillType);
	}

	private void RefreshHero(int index, HeroDetail heroDetail, bool isFirstIn)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		m_heroTids.Add(heroDetail.HeroTid);
		if (isFirstIn)
		{
			m_spineScene.skeletonAnimation.AnimationState.SetAnimation(index, (index + 1).ToString(), true);
			((Component)m_spriteYingzis[index]).gameObject.SetActive(true);
			((Component)m_skeletonHeros[index]).gameObject.SetActive(true);
			((VisualElement)m_btnHeros[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (index > 0)
			{
				m_nodeDamages[index - 1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroDetail.HeroTid);
		if (recordByKey == null)
		{
			return;
		}
		zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
		if (recordByKey2 != null)
		{
			zbUtil.RefreshSpine(m_skeletonHeros[index], recordByKey2.frontBone + "_SkeletonData.asset", "idle", loop: true);
		}
		if (index == 0)
		{
			float damage = zbSelf.Instance.heroHandler.GetDamage(heroDetail.Quality, heroDetail.Rarity, heroDetail.QualityStar);
			float followDamages = zbSelf.Instance.heroHandler.GetFollowDamages(m_skillType);
			string text = zbUtil.Language("ShangHaiXiShu") + $" {damage * 100f:0.##}%";
			if (followDamages > 0f)
			{
				text = text + "<color=#2DF421>+" + $"{followDamages * 100f:0.##}%" + "</color>";
			}
			((TextElement)m_lbDamages[index]).text = text;
		}
		else
		{
			float damage2 = zbSelf.Instance.heroHandler.GetDamage(heroDetail.Quality, heroDetail.Rarity, heroDetail.QualityStar);
			((TextElement)m_lbDamages[index]).text = zbUtil.Language("ShangHaiXiShu") + $": {damage2 * 100f:0.##}%";
			if (m_level == 0)
			{
				((TextElement)m_lbAdds[index - 1]).text = zbUtil.Language("lang.11050") + ": 0%";
				return;
			}
			float num = damage2 * m_curDamageCoef;
			((TextElement)m_lbAdds[index - 1]).text = zbUtil.Language("lang.11050") + $": {num * 100f:0.##}%";
		}
	}

	private void RefreshLevelCost()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		m_level = (((IEnumerable<HeroFollowDetail>)zbServerData.sGameData?.HeroFollowInfo?.FollowList).FirstOrDefault((HeroFollowDetail x) => x.SkillType == m_skillType)?.FollowLevel).GetValueOrDefault();
		((TextElement)m_lbLevel).text = string.Format(zbUtil.Language("lang.11053"), m_level);
		((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_level < m_maxFollowLevel) ? 1 : 0));
		((VisualElement)m_btnFollow).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_level >= m_maxFollowLevel) ? 1 : 0));
		if (m_level == 0)
		{
			m_curDamageCoef = 0f;
			((TextElement)m_lbAdd).text = zbUtil.Language("lang.11058");
		}
		else
		{
			zbHeroFollowCsvRecord heroFollowRecord = zbSelf.Instance.heroHandler.GetHeroFollowRecord(m_skillType, m_level);
			m_curDamageCoef = heroFollowRecord.damageCoef;
			((TextElement)m_lbAdd).text = zbUtil.Language("lang.11051") + ": " + $"{m_curDamageCoef * 100f:0.##}%";
		}
		if (m_level < m_maxFollowLevel)
		{
			m_nextRecord = zbSelf.Instance.heroHandler.GetHeroFollowRecord(m_skillType, m_level + 1);
			IEnumerable<int[]> source;
			if (m_nextRecord.spcCost == null || m_nextRecord.spcCost.Length == 0)
			{
				IEnumerable<int[]> enumerable = m_nextRecord.costList ?? Array.Empty<int[]>();
				source = enumerable;
			}
			else
			{
				source = new int[1][] { m_nextRecord.spcCost }.Concat(m_nextRecord.costList ?? Array.Empty<int[]>());
			}
			m_costList = source.ToArray();
			bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, 313000, 1) && m_nextRecord.spcCost.Length != 0;
			((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData(EResourceType.Item, 313000L, 1L);
				m_convertItem.Refresh(data, isNameVis: false, isCountVis: false);
				int[][] costLists = new int[1][] { new int[3]
				{
					3,
					313000,
					m_nextRecord.spcCost[2]
				} };
				zbSelf.Instance.homelandHandler.UpdateCostUI(costLists, (Label[])(object)new Label[1] { m_lbConvertItem }, new zbItem[1] { m_convertItem });
			}
		}
		else
		{
			m_nextRecord = null;
			m_costList = new int[0][];
			((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_costList, m_lbCosts, m_items);
		m_notifyFollow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroFollowNotify(m_skillType)));
	}

	private void RefreshMoneyPanel()
	{
		CreateMoneyPanel(new EMoneyType[0]);
		zbHeroFollowCsvRecord heroFollowRecord = zbSelf.Instance.heroHandler.GetHeroFollowRecord(m_skillType, 1);
		int[] commonCost = heroFollowRecord.costList[0];
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)commonCost[0], commonCost[1]);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)commonCost[0], commonCost[1]);
		m_moneyPanel.RefreshOther(0, resorceIconPath.strIconPath, resourceCount, delegate
		{
			zbUIMgr.Instance.ShowTip((EResourceType)commonCost[0], commonCost[1], 0L);
		});
	}

	private void OnHeros(int index)
	{
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_heroTids[index]);
		zbUIMgr.Instance.ShowHeroDetail(recordByKey);
	}

	private void OnLock()
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11057"));
	}

	private void OnFollow()
	{
		if (m_nextRecord == null)
		{
			return;
		}
		if (m_nextRecord.spcCost.Length != 0)
		{
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)m_nextRecord.spcCost[0], m_nextRecord.spcCost[1]);
			if (resourceCount < m_nextRecord.spcCost[2])
			{
				long resourceCount2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Item, 313000);
				long num = m_nextRecord.spcCost[2] - resourceCount - resourceCount2;
				if (num > 0)
				{
					zbUIMgr.Instance.ShowTip((EResourceType)m_nextRecord.spcCost[0], m_nextRecord.spcCost[1], num);
					return;
				}
			}
		}
		int[][] costList = m_nextRecord.costList;
		foreach (int[] array in costList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
				return;
			}
		}
		zbWsMsgHandler.Instance.HeroFollow_Upgrade_Req(m_skillType);
		m_isUpgradeReq = true;
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnRule()
	{
		string[] langKeys = new string[3] { "lang.11061", "lang.11062", "lang.11063" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_skillType, isFirstIn: false);
		if (m_isUpgradeReq)
		{
			ActionChuchangAnim();
			m_isUpgradeReq = false;
		}
	}

	public void ActionChuchangAnim()
	{
		zbUIMgr.Instance.ShowModel();
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		m_spine.AnimationState.SetAnimation(0, "animation", false);
		zbUIEffect.CreateScaleActionOnce((VisualElement)(object)m_lbLevel, 1.2f, 1f);
		zbUIEffect.CreateScaleActionOnce((VisualElement)(object)m_lbAdd, 1.2f, 1f);
		zbUIEffect.CreateScaleActionOnce((VisualElement)(object)m_lbDamages[0], 1.2f, 1f);
		zbUIEffect.CreateScaleActionOnce((VisualElement)(object)m_lbAdds[0], 1.2f, 1f);
		zbUIEffect.CreateScaleActionOnce((VisualElement)(object)m_lbAdds[1], 1.2f, 1f);
		((MonoBehaviour)this).StartCoroutine(DelayHideModel());
	}

	private IEnumerator DelayHideModel()
	{
		yield return (object)new WaitForSeconds(1f);
		zbUIMgr.Instance.HideModel();
	}
}
