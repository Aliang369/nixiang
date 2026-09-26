using System;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaTabStar
{
	private zbMechaWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_nodeCurStar;

	private VisualElement m_nodeNextStar;

	private VisualElement m_arrowStar;

	private VisualElement[] m_curStars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_nextStars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_nodeAttrs = (VisualElement[])(object)new VisualElement[1];

	private Label[] m_lbCurValues = (Label[])(object)new Label[1];

	private Label[] m_lbNextValues = (Label[])(object)new Label[1];

	private VisualElement[] m_spriteArrows = (VisualElement[])(object)new VisualElement[1];

	private VisualElement m_nodeNextStarSkill;

	private Label m_lbNextStarOpen;

	private VisualElement m_spriteNextStarSkill;

	private Button m_btnNextStarSkill;

	private Label[] m_costLabels = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private zbItem m_convertItem;

	private Label m_lbConvertItem;

	private Label m_lbOr;

	private Button m_btnStar;

	private Label m_lbMax;

	private Label m_lbMaxDesc;

	private VisualElement m_notify;

	private zbMechaStarUpgradeCsvRecord m_curStarRecord;

	private zbMechaStarUpgradeCsvRecord m_nextStarRecord;

	private int[][] m_starCostList;

	private bool m_isUpgradeReq;

	public void OnEnable(zbMechaWnd wndParent, VisualElement root)
	{
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "star", (string)null);
		m_nodeCurStar = UQueryExtensions.Q<VisualElement>(val, "cur_stars", (string)null);
		m_nodeNextStar = UQueryExtensions.Q<VisualElement>(val, "next_stars", (string)null);
		m_arrowStar = UQueryExtensions.Q<VisualElement>(val, "arrow", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_curStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeCurStar, "star" + i, (string)null);
			m_nextStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeNextStar[i], "star" + i, (string)null);
		}
		for (int j = 0; j < 1; j++)
		{
			m_nodeAttrs[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + j, (string)null);
			m_lbCurValues[j] = UQueryExtensions.Q<Label>(m_nodeAttrs[j], "lb_curValue", (string)null);
			m_lbNextValues[j] = UQueryExtensions.Q<Label>(m_nodeAttrs[j], "lb_nextValue", (string)null);
			m_spriteArrows[j] = UQueryExtensions.Q<VisualElement>(m_nodeAttrs[j], "arrow", (string)null);
		}
		m_nodeNextStarSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr1", (string)null);
		m_lbNextStarOpen = UQueryExtensions.Q<Label>(m_nodeNextStarSkill, "lb_key", (string)null);
		m_spriteNextStarSkill = UQueryExtensions.Q<VisualElement>(m_nodeNextStarSkill, "skill", (string)null);
		m_btnNextStarSkill = UQueryExtensions.Q<Button>(m_nodeNextStarSkill, "btn_attr", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNextStarSkill, (Action<ClickEvent>)OnNextStarSkill, "", (TrickleDown)0);
		for (int k = 0; k < 2; k++)
		{
			m_costLabels[k] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + k, (string)null);
			m_items[k] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + k, (string)null);
			m_items[k].OnEnable(m_wndParent);
			((VisualElement)m_costLabels[k]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_lbConvertItem = UQueryExtensions.Q<Label>(m_RootElement, "lb_convertItem", (string)null);
		m_convertItem = UQueryExtensions.Q<zbItem>(m_RootElement, "convertItem", (string)null);
		m_convertItem.OnEnable(m_wndParent);
		m_lbOr = UQueryExtensions.Q<Label>(m_RootElement, "lb_or", (string)null);
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "btn_star", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action)OnStar, "", (TrickleDown)0);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_lbMaxDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_maxDesc", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar, (Vector2?)new Vector2(-3f, -5f), 1f, true);
	}

	public void Refresh()
	{
		if (m_wndParent.m_mechaDetail != null)
		{
			((TextElement)m_lbMaxDesc).text = zbSelf.Instance.itemHandler.GetStarMaxDesc(m_wndParent.m_record.propId);
			m_curStarRecord = zbDataConfig.Instance.mechaStarUpgradeCsvTable.GetRecordByKey(m_wndParent.m_mechaDetail.Star);
			m_nextStarRecord = zbDataConfig.Instance.mechaStarUpgradeCsvTable.GetRecordByKey(m_wndParent.m_mechaDetail.Star + 1);
			RefreshAttrs();
			RefreshCostList();
			if (m_isUpgradeReq)
			{
				m_wndParent.ActionChuchangAnim();
				m_isUpgradeReq = false;
			}
		}
	}

	private void RefreshAttrs()
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		if (m_curStarRecord != null)
		{
			zbUIEffect.SetStars(m_curStars, m_curStarRecord.star);
			((TextElement)m_lbCurValues[0]).text = m_wndParent.m_mechaDetail.Attack.ToString();
		}
		if (m_nextStarRecord != null)
		{
			zbUIEffect.SetStars(m_nextStars, m_nextStarRecord.star);
			((TextElement)m_lbNextValues[0]).text = (m_wndParent.m_mechaDetail.Attack + m_nextStarRecord.attack - m_curStarRecord.attack).ToString();
			((TextElement)m_lbNextStarOpen).text = "解锁技能升级";
			int[] array = m_wndParent.m_record.skillList.ToArray();
			int key = array[Mathf.Clamp(m_nextStarRecord.star - 1, 0, array.Length - 1)];
			zbMechaSkillCsvRecord recordByKey = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(key);
			if (recordByKey != null)
			{
				m_spriteNextStarSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			}
		}
		m_nodeNextStarSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_nodeNextStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_arrowStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		((VisualElement)m_lbNextValues[0]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_spriteArrows[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
	}

	private void RefreshCostList()
	{
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (m_nextStarRecord != null)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbMaxDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_starCostList = new int[1][] { new int[3]
			{
				2,
				m_wndParent.m_record.propId,
				m_nextStarRecord.costPropNum
			} };
			bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, m_wndParent.m_record.convertPropId, 1);
			((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData(EResourceType.Prop, m_wndParent.m_record.convertPropId, 1L);
				m_convertItem.Refresh(data, isNameVis: false, isCountVis: false);
				int[][] costLists = new int[1][] { new int[3]
				{
					2,
					m_wndParent.m_record.convertPropId,
					m_nextStarRecord.costPropNum
				} };
				zbSelf.Instance.homelandHandler.UpdateCostUI(costLists, (Label[])(object)new Label[1] { m_lbConvertItem }, new zbItem[1] { m_convertItem });
			}
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbMaxDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_starCostList = new int[0][];
			((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_starCostList, m_costLabels, m_items);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasMechaStarNotify(m_wndParent.m_record.id)));
	}

	private void OnNextStarSkill(ClickEvent evt)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (m_nextStarRecord != null)
		{
			int[] array = new int[5]
			{
				m_wndParent.m_record.skillList[0],
				m_wndParent.m_record.skill2,
				m_wndParent.m_record.skill3,
				m_wndParent.m_record.skill4,
				m_wndParent.m_record.skill5
			};
			int num = array[Mathf.Clamp(m_nextStarRecord.star - 1, 0, array.Length - 1)];
			if (zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(num) != null)
			{
				IEventHandler target = ((EventBase)evt).target;
				Vector2 pos = VisualElementExtensions.LocalToWorld((VisualElement)(object)((target is VisualElement) ? target : null), Vector2.zero);
				pos.y += 200f;
				zbUIMgr.Instance.ShowTipMod(zbSelf.Instance.mechaHandler.GetCurMechaSkillDesc(num), pos);
			}
		}
	}

	private void OnStar()
	{
		if (zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Mecha) || m_wndParent.m_mechaDetail == null)
		{
			return;
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_wndParent.m_record.propId);
		if (m_nextStarRecord != null && resourceCount < m_nextStarRecord.costPropNum)
		{
			long resourceCount2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_wndParent.m_record.convertPropId);
			if (resourceCount2 > 0 && resourceCount2 + resourceCount >= m_nextStarRecord.costPropNum)
			{
				zbWsMsgHandler.Instance.Mecha_UpgradeStar_Req(m_wndParent.m_record.id);
				m_isUpgradeReq = true;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_wndParent.m_record.propId, m_nextStarRecord.costPropNum);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.Mecha_UpgradeStar_Req(m_wndParent.m_record.id);
			m_isUpgradeReq = true;
		}
	}
}
