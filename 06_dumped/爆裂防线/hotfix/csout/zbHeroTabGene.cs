using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroTabGene
{
	private zbHeroWnd m_wndParent;

	private VisualElement m_RootElement;

	private SkeletonAnimation m_spineBigCircle;

	private SkeletonAnimation[] m_spineCircles = (SkeletonAnimation[])(object)new SkeletonAnimation[5];

	private SkeletonAnimation[] m_spineGenes = (SkeletonAnimation[])(object)new SkeletonAnimation[5];

	private Button[] m_btnGenes = (Button[])(object)new Button[5];

	private VisualElement[] m_spriteLights = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteGenes = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbNames = (Label[])(object)new Label[5];

	private SpineVisualElement[] m_spineUnlocks = (SpineVisualElement[])(object)new SpineVisualElement[5];

	private VisualElement[] m_notifyGenes = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_nodeAttr;

	private Label m_lbAttr;

	private Label m_lbValue;

	private Label m_lbValueAdd;

	private Button m_btnAttr;

	private Button m_btnRule;

	public Button m_btnGeneList;

	private Label m_lbCost;

	private zbItem m_item = new zbItem();

	public Button m_btnGene;

	private Label m_lbLimit;

	private VisualElement m_nodeAuto;

	private Label m_lbAuto;

	private VisualElement m_spriteAuto;

	private VisualElement m_notify;

	private GeneSlotDetail[] m_geneSlotList;

	private int m_cost;

	private int m_slotOpenCount;

	private GeneResonanceResult m_geneResonanceResult;

	public int m_autoIndex = -1;

	public void OnEnable(zbHeroWnd wndParent, VisualElement root)
	{
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_spineBigCircle = ((Component)((Component)m_wndParent.m_spriteGeneBg).transform.Find("circleBg").Find("circleSpineAnimation")).GetComponent<SkeletonAnimation>();
		for (int i = 0; i < 5; i++)
		{
			m_spineCircles[i] = ((Component)((Component)m_wndParent.m_spriteGeneBg).transform.Find("circleBg").Find("circle" + i)).GetComponent<SkeletonAnimation>();
			m_spineGenes[i] = ((Component)((Component)m_wndParent.m_spriteGeneBg).transform.Find("circleBg").Find("gene" + i)).GetComponent<SkeletonAnimation>();
			m_btnGenes[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_gene" + i, (string)null);
			m_spriteLights[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnGenes[i], "light", (string)null);
			m_spriteGenes[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnGenes[i], "icon", (string)null);
			m_lbNames[i] = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnGenes[i], "lb_name", (string)null);
			m_spineUnlocks[i] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnGenes[i], "spine_unlock", (string)null);
			int curIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGenes[curIndex], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnGenes(evt, curIndex);
			}, "", (TrickleDown)0);
			m_notifyGenes[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGenes[i], (Vector2?)new Vector2(10f, 5f), 1f, true);
		}
		m_nodeAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_attr", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_value", (string)null);
		m_lbValueAdd = UQueryExtensions.Q<Label>(m_RootElement, "lb_valueAdd", (string)null);
		m_btnAttr = UQueryExtensions.Q<Button>(m_RootElement, "btn_attr", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnGeneList = UQueryExtensions.Q<Button>(m_RootElement, "btn_geneList", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_btnGene = UQueryExtensions.Q<Button>(m_RootElement, "btn_gene", (string)null);
		m_lbLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_limit", (string)null);
		m_nodeAuto = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_auto", (string)null);
		m_lbAuto = UQueryExtensions.Q<Label>(m_nodeAuto, "lb_auto", (string)null);
		m_spriteAuto = UQueryExtensions.Q<VisualElement>(m_nodeAuto, "sprite_ok", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGene);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_nodeAttr, (Action)OnRule, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttr, (Action)OnAttr, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGeneList, (Action)OnGeneList, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGene, (Action)OnGene, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_nodeAuto, (Action)OnAuto, "", (TrickleDown)0);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_HeroAwaken_GeneOpenHole_Ack, OnHeroAwakenGeneOpenHoleAck);
	}

	public void Refresh()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_warRoomDetail != null && m_wndParent.m_roomDetail != null)
		{
			m_geneSlotList = zbSelf.Instance.geneHandler.GetGeneSlotListBySkillType(m_wndParent.m_warRoomDetail.SkillType);
			m_slotOpenCount = zbSelf.Instance.geneHandler.GetGeneSlotListOpenCount(m_wndParent.m_warRoomDetail.SkillType);
			((VisualElement)m_btnRule).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_slotOpenCount != 5));
			m_nodeAttr.pickingMode = (PickingMode)(m_slotOpenCount != 5);
			m_nodeAuto.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_slotOpenCount != 5 || zbSelf.Instance.geneHandler.IsAllSlotSameShape(m_wndParent.m_warRoomDetail.SkillType)) ? 1 : 0));
			RefreshCost();
			RefreshSpineAnimations();
			RefreshGenes();
			RefreshCircles();
			RefreshAuto();
		}
	}

	private void RefreshCost()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.geneHandler.IsAllSlotSameShape(m_wndParent.m_warRoomDetail.SkillType))
		{
			((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnGene).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbLimit).text = zbUtil.Language("lang.10918");
		}
		else if (m_slotOpenCount == 5)
		{
			m_cost = zbSelf.Instance.geneHandler.geneRefreshCost;
			((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_btnGene).text = zbUtil.Language("lang.10903");
			((VisualElement)m_btnGene).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLimit).text = "";
		}
		else
		{
			m_cost = zbSelf.Instance.geneHandler.geneOpenCost;
			((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int num = zbSelf.Instance.geneHandler.geneUnlockLevels[m_slotOpenCount];
			if (zbSelf.Instance.heroHandler.GetHeroAwakenLevel(m_wndParent.m_warRoomDetail.SkillType) >= num)
			{
				((TextElement)m_btnGene).text = zbUtil.Language("lang.10786");
				((VisualElement)m_btnGene).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbLimit).text = "";
			}
			else
			{
				((VisualElement)m_btnGene).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.10940"), num);
			}
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Money, 19);
		m_item.Refresh(new zbItemData(EResourceType.Money, 19L, m_cost), isNameVis: false, isCountVis: false);
		((TextElement)m_lbCost).text = zbUtil.GetNeedHaveRichString(resourceCount, m_cost);
		if (m_cost > resourceCount)
		{
			m_item.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 19L, m_cost);
			});
		}
		else
		{
			m_item.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 19L, 0L);
			});
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.geneHandler.HasAnyGeneUnlockNotify(m_wndParent.m_warRoomDetail.SkillType)));
	}

	private void RefreshSpineAnimations()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			if (zbSelf.Instance.geneHandler.ackNeedAnimIndex == i)
			{
				((VisualElement)m_spineUnlocks[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spineUnlocks[i].AnimationState.SetAnimation(0, "animation", false);
				zbSelf.Instance.geneHandler.ackNeedAnimIndex = -1;
			}
			else
			{
				((VisualElement)m_spineUnlocks[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private void RefreshGenes()
	{
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			if (i < m_geneSlotList.Length && m_geneSlotList[i].GeneShape != EGeneShape.EmptyValue)
			{
				m_spineGenes[i].AnimationState.SetAnimation(0, ((int)m_geneSlotList[i].GeneShape).ToString(), true);
				if (m_geneSlotList[i].GeneTid > 0)
				{
					zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(m_geneSlotList[i].GeneTid);
					if (recordByKey != null)
					{
						m_spriteLights[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
						m_spriteLights[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/hero/hero_geneLight{recordByKey.geneShape}.png"));
						m_spriteGenes[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
						m_spriteGenes[i].style.scale = StyleScale.op_Implicit(new Vector2(1.2f, 1.2f));
						((TextElement)m_lbNames[i]).text = zbAttrHelper.GetTeamAttrName((int)recordByKey.attr[0]) + "+" + zbSelf.Instance.geneHandler.GetGeneResonanceAttr(m_wndParent.m_warRoomDetail.SkillType, recordByKey.attr[1]);
					}
				}
				else
				{
					m_spriteLights[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					m_spriteGenes[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/hero/hero_gene{(int)m_geneSlotList[i].GeneShape}.png"));
					m_spriteGenes[i].style.scale = StyleScale.op_Implicit(new Vector2(1f, 1f));
					((TextElement)m_lbNames[i]).text = "";
				}
			}
			else
			{
				m_spriteLights[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_spriteGenes[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/hero/hero_gene_lock.png"));
				m_spriteGenes[i].style.scale = StyleScale.op_Implicit(new Vector2(1f, 1f));
				((TextElement)m_lbNames[i]).text = "";
			}
			bool flag = zbSelf.Instance.geneHandler.HasGeneUnlockNotify(m_wndParent.m_warRoomDetail.SkillType, i);
			bool flag2 = zbSelf.Instance.geneHandler.HasGeneSetNotify(m_wndParent.m_warRoomDetail.SkillType, i);
			bool flag3 = zbSelf.Instance.geneHandler.HasGeneExchangeNotify(m_wndParent.m_warRoomDetail.SkillType, i);
			bool flag4 = zbSelf.Instance.geneHandler.HasGeneUpgradeNotify(m_wndParent.m_warRoomDetail.SkillType, i);
			m_notifyGenes[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(flag | flag2 | flag3 | flag4)));
		}
	}

	private void RefreshCircles()
	{
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			((Component)m_spineCircles[i]).gameObject.SetActive(false);
			((Component)m_spineGenes[i]).gameObject.SetActive(false);
		}
		m_geneResonanceResult = zbSelf.Instance.geneHandler.GetGeneResonanceResult(m_wndParent.m_warRoomDetail.SkillType);
		((Component)m_spineBigCircle).gameObject.SetActive(m_geneResonanceResult.MaxCount == 5);
		if (m_geneResonanceResult.MaxPotentialCount > 0)
		{
			((TextElement)m_lbAttr).text = string.Format(zbUtil.Language("lang.10902"), m_geneResonanceResult.MaxPotentialCount);
			if (m_geneResonanceResult.MaxCount > 0)
			{
				((TextElement)m_lbValue).text = "+" + zbSelf.Instance.geneHandler.geneShapeResonance[m_geneResonanceResult.MaxCount - 1] * 100f + "%";
			}
			else
			{
				((TextElement)m_lbValue).text = "+0%";
			}
			if (m_geneResonanceResult.MaxPotentialCount > m_geneResonanceResult.MaxCount)
			{
				((VisualElement)m_lbValueAdd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbValueAdd).text = string.Format(zbUtil.Language("lang.10938"), zbSelf.Instance.geneHandler.geneShapeResonance[m_geneResonanceResult.MaxPotentialCount - 1] * 100f);
			}
			else
			{
				((VisualElement)m_lbValueAdd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		else
		{
			((TextElement)m_lbAttr).text = "";
			((TextElement)m_lbValue).text = "";
			((TextElement)m_lbValueAdd).text = "";
		}
		foreach (int resonanceEdge in m_geneResonanceResult.ResonanceEdges)
		{
			((Component)m_spineCircles[resonanceEdge]).gameObject.SetActive(true);
			((SkeletonRenderer)m_spineCircles[resonanceEdge]).skeleton.A = 1f;
		}
		foreach (int openResonanceEdge in m_geneResonanceResult.OpenResonanceEdges)
		{
			((Component)m_spineCircles[openResonanceEdge]).gameObject.SetActive(true);
			((SkeletonRenderer)m_spineCircles[openResonanceEdge]).skeleton.A = 0.4f;
		}
		foreach (int filledResonanceSlot in m_geneResonanceResult.FilledResonanceSlots)
		{
			((Component)m_spineGenes[filledResonanceSlot]).gameObject.SetActive(true);
			((SkeletonRenderer)m_spineGenes[filledResonanceSlot]).skeleton.A = 1f;
			m_spriteLights[filledResonanceSlot].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		foreach (int emptyResonanceSlot in m_geneResonanceResult.EmptyResonanceSlots)
		{
			((Component)m_spineGenes[emptyResonanceSlot]).gameObject.SetActive(true);
			((SkeletonRenderer)m_spineGenes[emptyResonanceSlot]).skeleton.A = 0.4f;
		}
	}

	private void RefreshAuto()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_spriteAuto.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.geneHandler.selAutoIndex <= -1));
		if (zbSelf.Instance.geneHandler.selAutoIndex > -1)
		{
			((TextElement)m_lbAuto).text = zbUtil.Language("lang.10975") + "(" + zbSelf.Instance.geneHandler.autoIndexs[zbSelf.Instance.geneHandler.selAutoIndex] * 100f + "%)";
		}
		else
		{
			((TextElement)m_lbAuto).text = zbUtil.Language("lang.10975");
		}
	}

	private void OnGenes(ClickEvent evt, int index)
	{
		if (index < m_geneSlotList.Length && m_geneSlotList[index].GeneShape != EGeneShape.EmptyValue)
		{
			if (m_geneSlotList[index].GeneTid == 0)
			{
				zbUIMgr.Instance.ShowHeroGeneSelect(m_geneSlotList[index], m_wndParent.m_warRoomDetail.SkillType, index);
				return;
			}
			long newCount = zbSelf.Instance.geneHandler.getBagGeneCount(m_geneSlotList[index].GeneTid) + 1;
			zbItemData data = new zbItemData(EResourceType.Gene, m_geneSlotList[index].GeneTid, newCount);
			zbUIMgr.Instance.ShowBagItemDetail(data, m_wndParent.m_warRoomDetail.SkillType, index);
			return;
		}
		if (index >= m_slotOpenCount)
		{
			int num = zbSelf.Instance.geneHandler.geneUnlockLevels[index];
			if (num > zbSelf.Instance.heroHandler.GetHeroAwakenLevel(m_wndParent.m_warRoomDetail.SkillType))
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10901"), num));
				return;
			}
		}
		OnGene();
	}

	private void OnAttr()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		List<Dictionary<string, object>> genesInfoByType = zbSelf.Instance.geneHandler.GetGenesInfoByType(m_wndParent.m_warRoomDetail.SkillType);
		Vector2 val = VisualElementExtensions.LocalToWorld((VisualElement)(object)m_btnAttr, Vector2.zero);
		val.x = 102f;
		val.y += 20f;
		Vector2 vector = VisualElementExtensions.WorldToLocal(UQueryExtensions.Q<VisualElement>(m_wndParent.GetRootElement(), "root", (string)null), val);
		vector.x = 102f;
		zbUIMgr.Instance.ShowGeneTip(genesInfoByType, vector);
	}

	private void OnRule()
	{
		zbUIMgr.Instance.ShowGeneRule();
	}

	private void OnGeneList()
	{
		zbUIMgr.Instance.ShowGene();
	}

	private void OnGene()
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 19, m_cost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 19L, m_cost);
			return;
		}
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_success.ogg");
		if (m_slotOpenCount == 5)
		{
			zbWsMsgHandler.Instance.HeroAwaken_GeneRefreshHole_Req(m_wndParent.m_warRoomDetail.SkillType);
		}
		else
		{
			zbWsMsgHandler.Instance.HeroAwaken_GeneOpenHole_Req(m_wndParent.m_warRoomDetail.SkillType);
		}
	}

	private void OnAuto()
	{
		zbUIMgr.Instance.ShowGeneAuto(m_wndParent.m_warRoomDetail.SkillType, delegate(bool isOK)
		{
			if (isOK)
			{
				RefreshAuto();
			}
		});
	}

	private void OnHeroAwakenGeneOpenHoleAck(object obj)
	{
		RefreshSpineAnimations();
		((MonoBehaviour)m_wndParent).StartCoroutine(PlayUnlockAnimation(obj));
	}

	private IEnumerator PlayUnlockAnimation(object obj)
	{
		zbUIMgr.Instance.ShowModel();
		yield return (object)new WaitForSeconds(0.4f);
		zbUIMgr.Instance.HideModel();
		if (obj is HeroAwakenGeneOpenHoleAck heroAwakenGeneOpenHoleAck)
		{
			zbServerData.sGameData.HeroAwakenInfo = heroAwakenGeneOpenHoleAck.HeroAwakenInfo;
			zbSelf.Instance.dataHandler.DataChange(heroAwakenGeneOpenHoleAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gene);
		}
	}
}
