using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneComposeWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private Label m_lbName;

	private Label m_lbAttr;

	private Button m_btnGene;

	private VisualElement m_spriteGene;

	private Button[] m_btnGenes = (Button[])(object)new Button[4];

	private VisualElement[] m_spriteGenes = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_spriteAdds = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbGenes = (Label[])(object)new Label[4];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[4];

	private Button m_btnUp;

	private Label m_lbUp;

	private Button m_btnDown;

	private Label m_lbDown;

	private Button m_btnUpgrade;

	private Button m_btnClose;

	public int[] m_geneTids = new int[4];

	public int m_geneTid;

	public ESkillType m_skillType;

	private int m_index;

	private int m_flag;

	private zbGeneCsvRecord m_record;

	private Coroutine m_composeCoroutine;

	protected override void OnEnable()
	{
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = m_GameObjectSceneRoot.GetComponentInChildren<zbSpineScene>();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_btnGene = UQueryExtensions.Q<Button>(m_RootElement, "btn_gene", (string)null);
		m_spriteGene = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnGene, "bg_gene", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_btnGenes[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_gene" + i, (string)null);
			m_spriteGenes[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnGenes[i], "bg_gene", (string)null);
			m_spriteAdds[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnGenes[i], "bg_add", (string)null);
			m_lbGenes[i] = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnGenes[i], "lb_gene", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGenes[curIndex], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnGenes(evt, curIndex);
			}, "", (TrickleDown)0);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGenes[i], (Vector2?)new Vector2(50f, 50f), 1f, true);
		}
		m_btnUp = UQueryExtensions.Q<Button>(m_RootElement, "btn_up", (string)null);
		m_lbUp = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnUp, "lb_up", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>(m_RootElement, "btn_down", (string)null);
		m_lbDown = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDown, "lb_down", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUp, (Action<ClickEvent>)OnUp, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action<ClickEvent>)OnDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Bag_GeneCompose_Ack, OnBagGeneComposeAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_HeroAwaken_GeneCompose_Ack, OnHeroAwakenGeneComposeAck);
	}

	public void RefreshCustom(int geneTid, ESkillType skillType, int index)
	{
		m_spineScene.SetAnimation("idle", isLoop: true);
		m_geneTid = geneTid;
		m_skillType = skillType;
		m_index = index;
		zbSelf.Instance.geneHandler.composeIndex = -1;
		RefreshInfo();
	}

	public void RefreshInfo(int flag = 0)
	{
		m_flag = flag;
		if (flag != 0 && zbSelf.Instance.geneHandler.composeIndex == -1)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
			PlayComposeAnimation(OnCallBack);
		}
		else
		{
			OnCallBack();
		}
	}

	private void OnCallBack(bool isOK = true)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.geneHandler.composeIndex == -1)
		{
			if (m_flag == 2)
			{
				m_geneTid = zbSelf.Instance.geneHandler.composeAckGeneTid;
			}
			m_geneTids = new int[4];
		}
		else
		{
			m_geneTids[zbSelf.Instance.geneHandler.composeIndex] = zbSelf.Instance.geneHandler.composeAckGeneTid;
		}
		if (m_skillType != ESkillType.EmptyValue)
		{
			GeneSlotDetail geneSlotDetail = zbSelf.Instance.geneHandler.GetGeneSlotDetail(m_skillType, m_index);
			if (geneSlotDetail != null)
			{
				m_geneTid = geneSlotDetail.GeneTid;
			}
		}
		m_record = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(m_geneTid);
		m_spriteGene.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteGene.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
		((VisualElement)m_lbName).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lbName).text = m_record.name;
		((TextElement)m_lbAttr).text = zbAttrHelper.GetTeamAttrName((int)m_record.attr[0]) + "+" + zbSelf.Instance.geneHandler.GetGeneResonanceAttr(m_skillType, m_record.attr[1]);
		RefreshDetail();
		zbSelf.Instance.geneHandler.composeIndex = -1;
	}

	public void RefreshDetail()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (m_geneTids[i] != 0)
			{
				num++;
			}
		}
		if (m_skillType == ESkillType.EmptyValue)
		{
			num++;
		}
		for (int j = 0; j < 4; j++)
		{
			m_spriteGenes[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteAdds[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbGenes[j]).text = "";
			m_notifys[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (m_geneTids[j] != 0)
			{
				zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(m_geneTids[j]);
				if (recordByKey != null)
				{
					m_spriteGenes[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					m_spriteGenes[j].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
					((TextElement)m_lbGenes[j]).text = zbAttrHelper.GetTeamAttrName((int)recordByKey.attr[0]) + zbAttrHelper.FormatAttrValue(recordByKey.attr[1], isPercentageDisplay: false, isDisplayAdd: true);
				}
			}
			else
			{
				m_spriteAdds[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_notifys[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.geneHandler.HasGeneNotify(m_record.type, m_record.geneShape, num + 1)));
			}
		}
	}

	private void OnGenes(ClickEvent evt, int index)
	{
		if (m_geneTids[index] != 0)
		{
			m_geneTids[index] = 0;
			RefreshDetail();
		}
		else
		{
			zbUIMgr.Instance.ShowGeneSelect(m_geneTid, index);
		}
	}

	public void OnUp(ClickEvent evt)
	{
		List<ResourceDetail> list = (from v in zbSelf.Instance.geneHandler.GetGenesByTid(m_geneTid)
			select v.Clone()).ToList();
		RemoveSameGene(list, m_geneTid);
		m_geneTids = BuildGeneTidArray(list);
		RefreshDetail();
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_success.ogg");
	}

	private void RemoveSameGene(List<ResourceDetail> list, int tid)
	{
		if (tid == 0)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ResourceDetail resourceDetail = list[num];
			if (resourceDetail.Tid == tid)
			{
				if (m_skillType == ESkillType.EmptyValue)
				{
					resourceDetail.Count--;
				}
				if (resourceDetail.Count <= 0)
				{
					list.RemoveAt(num);
				}
				break;
			}
		}
	}

	private int[] BuildGeneTidArray(List<ResourceDetail> list)
	{
		List<int> list2 = new List<int>(4);
		foreach (ResourceDetail item in list)
		{
			for (int i = 0; i < item.Count; i++)
			{
				if (list2.Count >= 4)
				{
					break;
				}
				list2.Add(item.Tid);
			}
			if (list2.Count >= 4)
			{
				break;
			}
		}
		while (list2.Count < 4)
		{
			list2.Add(0);
		}
		return list2.ToArray();
	}

	public void OnDown(ClickEvent evt)
	{
		m_geneTids = new int[4];
		RefreshDetail();
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_success.ogg");
	}

	public void OnUpgrade(ClickEvent evt)
	{
		if (m_record.type >= 12)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10188"));
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			if (m_geneTids[i] == 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10939"));
				return;
			}
		}
		if (m_skillType != ESkillType.EmptyValue)
		{
			zbWsMsgHandler.Instance.HeroAwaken_GeneCompose_Req(m_skillType, m_index, m_geneTids);
			return;
		}
		List<BagSelectDetail> list = new List<BagSelectDetail>();
		list.Add(new BagSelectDetail
		{
			Tid = m_geneTid,
			Count = 1L
		});
		int[] geneTids = m_geneTids;
		foreach (int num in geneTids)
		{
			list.Add(new BagSelectDetail
			{
				Tid = num,
				Count = 1L
			});
		}
		zbWsMsgHandler.Instance.Bag_GeneCompose_Req(list.ToArray(), -1);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnBagGeneComposeAck(object data)
	{
		RefreshInfo(2);
	}

	private void OnHeroAwakenGeneComposeAck(object data)
	{
		RefreshInfo(1);
	}

	private void PlayComposeAnimation(Action<bool> callback)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		m_spriteGene.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbName).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		for (int i = 0; i < 4; i++)
		{
			m_spriteGenes[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteAdds[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_notifys[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbGenes[i]).text = "";
		}
		zbUIMgr.Instance.ShowModel();
		m_spineScene.PlaySequenceAnimation("act", "idle");
		if (m_composeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_composeCoroutine);
		}
		m_composeCoroutine = ((MonoBehaviour)this).StartCoroutine(ComposeDelay(callback));
	}

	private IEnumerator ComposeDelay(Action<bool> callback)
	{
		yield return (object)new WaitForSeconds(1.1333333f);
		zbUIMgr.Instance.HideModel();
		callback?.Invoke(obj: true);
		m_composeCoroutine = null;
	}
}
