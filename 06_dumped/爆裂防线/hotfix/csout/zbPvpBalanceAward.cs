using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPvpBalanceAward
{
	private zbPvpBalanceWnd m_wndParent;

	private VisualElement m_rootElement;

	private VisualElement foldIconLeft;

	private Image m_descImgLeft;

	private VisualElement foldIconRight;

	private Image m_descImgRight;

	private Label m_lbNameLeft;

	private Label m_lbTypeLeft;

	private Label m_lbValueLeft;

	private Label m_lbNameRight;

	private Label m_lbTypeRight;

	private Label m_lbValueRight;

	private zbItem[] m_items = new zbItem[5];

	private VisualElement lb_award;

	public void OnEnable(zbPvpBalanceWnd wndParent, VisualElement root)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_rootElement = root;
		foldIconLeft = UQueryExtensions.Q<VisualElement>(m_rootElement, "foldIconLeft", (string)null);
		lb_award = UQueryExtensions.Q<VisualElement>(m_rootElement, "lb_award", (string)null);
		m_lbNameLeft = UQueryExtensions.Q<Label>(foldIconLeft, "lb_name", (string)null);
		m_lbTypeLeft = UQueryExtensions.Q<Label>(foldIconLeft, "lb_type", (string)null);
		m_lbValueLeft = UQueryExtensions.Q<Label>(foldIconLeft, "lb_value", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_rootElement, $"item{i}", (string)null);
			m_items[i]?.OnEnable(m_wndParent);
		}
		if (m_descImgLeft == null)
		{
			m_descImgLeft = new Image();
			((VisualElement)m_descImgLeft).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImgLeft).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImgLeft).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImgLeft).style.height = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImgLeft).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		foldIconLeft.Add((VisualElement)(object)m_descImgLeft);
		foldIconRight = UQueryExtensions.Q<VisualElement>(m_rootElement, "foldIconRight", (string)null);
		m_lbNameRight = UQueryExtensions.Q<Label>(foldIconRight, "lb_name", (string)null);
		m_lbTypeRight = UQueryExtensions.Q<Label>(foldIconRight, "lb_type", (string)null);
		m_lbValueRight = UQueryExtensions.Q<Label>(foldIconRight, "lb_value", (string)null);
		if (m_descImgRight == null)
		{
			m_descImgRight = new Image();
			((VisualElement)m_descImgRight).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImgRight).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImgRight).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImgRight).style.height = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImgRight).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		foldIconRight.Add((VisualElement)(object)m_descImgRight);
	}

	public void RefreshBalanceAward()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0659: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		BattleResultSideTeamInfo attackerTeamInfo = zbCombatSystem.Instance.battleResultInfo.AttackerTeamInfo;
		BattleResultSideTeamInfo defenderTeamInfo = zbCombatSystem.Instance.battleResultInfo.DefenderTeamInfo;
		m_wndParent.renderTargetLeft.StartRender(m_descImgLeft);
		m_wndParent.renderTargetLeft.RefreshAvatar(attackerTeamInfo.MasterTeamInfo.RoleBaseInfo.AvatarId, attackerTeamInfo.MasterTeamInfo.RoleBaseInfo.GunId);
		((TextElement)m_lbNameLeft).text = attackerTeamInfo.MasterTeamInfo.RoleBaseInfo.RoleName.ToString();
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(attackerTeamInfo.MasterTeamInfo.RoleBaseInfo.FootholdId);
		foldIconLeft.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		m_wndParent.renderTargetRight.StartRender(m_descImgRight);
		m_wndParent.renderTargetRight.RefreshAvatar(defenderTeamInfo.MasterTeamInfo.RoleBaseInfo.AvatarId, defenderTeamInfo.MasterTeamInfo.RoleBaseInfo.GunId);
		((TextElement)m_lbNameRight).text = defenderTeamInfo.MasterTeamInfo.RoleBaseInfo.RoleName.ToString();
		zbFootholdCsvRecord recordByKey2 = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(defenderTeamInfo.MasterTeamInfo.RoleBaseInfo.FootholdId);
		foldIconRight.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
		zbItem[] items = m_items;
		for (int i = 0; i < items.Length; i++)
		{
			((VisualElement)items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (AwardDetail battleAward in zbCombatSystem.Instance.battleAwardList)
		{
			if (num > 4)
			{
				break;
			}
			zbItemData data = new zbItemData(battleAward.Type, battleAward.Tid, battleAward.Count);
			m_items[num].Refresh(data);
			if (num == 0)
			{
				m_items[num].AddBestPrizeTag(vis: true);
			}
			else
			{
				m_items[num].AddBestPrizeTag(vis: false);
			}
			((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			num++;
		}
		((TextElement)m_lbTypeLeft).text = zbUtil.Language("lang.11466");
		((TextElement)m_lbTypeRight).text = zbUtil.Language("lang.11466");
		((TextElement)m_lbValueLeft).text = attackerTeamInfo.SideInfo.KillCount.ToString();
		((TextElement)m_lbValueRight).text = defenderTeamInfo.SideInfo.KillCount.ToString();
		((TextElement)m_wndParent.m_lbTipResult).text = zbUtil.Language("lang.10333");
		if (zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail)
		{
			lb_award.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/title_bg.png"));
			if (zbCombatSystem.Instance.battleResultInfo.TimeUsed >= zbCombatSystem.Instance.maxTime)
			{
				if (attackerTeamInfo.SideInfo.KillCount < defenderTeamInfo.SideInfo.KillCount)
				{
					((TextElement)m_wndParent.m_lbTipResult).text = zbUtil.Language("lang.11490");
				}
				else if (attackerTeamInfo.SideInfo.KillCount == defenderTeamInfo.SideInfo.KillCount)
				{
					((TextElement)m_wndParent.m_lbTipResult).text = zbUtil.Language("lang.11491");
				}
				((VisualElement)m_lbValueLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbValueRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbTypeLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbTypeRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				if (attackerTeamInfo.SideInfo.DefensePct <= 0f)
				{
					((TextElement)m_wndParent.m_lbTipResult).text = zbUtil.Language("lang.11489");
				}
				((VisualElement)m_lbValueLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbValueRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbTypeLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbTypeRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			((VisualElement)m_wndParent.m_lbTipResult).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			lb_award.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/elite/eliteQuest_titlebg.png"));
			if (zbCombatSystem.Instance.battleResultInfo.TimeUsed >= zbCombatSystem.Instance.maxTime)
			{
				if (attackerTeamInfo.SideInfo.KillCount > defenderTeamInfo.SideInfo.KillCount)
				{
					((TextElement)m_wndParent.m_lbTipResult).text = zbUtil.Language("lang.11488");
				}
				((VisualElement)m_lbValueLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbValueRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbTypeLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbTypeRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				if (defenderTeamInfo.SideInfo.DefensePct <= 0f)
				{
					((TextElement)m_wndParent.m_lbTipResult).text = zbUtil.Language("lang.11487");
				}
				((VisualElement)m_lbValueLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbValueRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbTypeLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbTypeRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			((VisualElement)m_wndParent.m_lbTipResult).style.color = StyleColor.op_Implicit(Color.green);
		}
		if (attackerTeamInfo.SideInfo.KillCount > defenderTeamInfo.SideInfo.KillCount)
		{
			((VisualElement)m_lbValueLeft).style.color = StyleColor.op_Implicit(Color.green);
			((VisualElement)m_lbValueRight).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbValueLeft).style.color = StyleColor.op_Implicit(Color.red);
			((VisualElement)m_lbValueRight).style.color = StyleColor.op_Implicit(Color.green);
		}
	}

	public void SetBackgroundImage(Sprite bg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_rootElement.style.backgroundImage = new StyleBackground(bg);
	}
}
