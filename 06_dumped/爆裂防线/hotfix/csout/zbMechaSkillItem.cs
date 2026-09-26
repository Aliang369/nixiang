using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSkillItem : zbViewBaseItem
{
	private VisualElement m_spriteBg;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbLevel;

	private VisualElement m_lbWrapper;

	private Label m_lbDesc;

	private Label m_lbMax;

	private Button m_btnUpgrade;

	private VisualElement m_notify;

	private zbMechaWnd m_wndParent;

	private MechaSkillDetail m_skillDetail;

	private zbMechaSkillCsvRecord m_skillRecord;

	private int m_index;

	public zbMechaSkillItem(VisualTreeAsset tAsset, zbMechaWnd wndParent)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbWrapper = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "lb_wrapper", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_max", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_upgrade", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(3f, 3f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int skillId, int index)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		m_index = index;
		m_skillDetail = zbSelf.Instance.mechaHandler.GetMechaSkillDetailBySkillId(skillId);
		m_skillRecord = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(skillId);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_skillRecord.icon));
		((TextElement)m_lbName).text = m_skillRecord.name;
		((TextElement)m_lbDesc).text = zbSelf.Instance.mechaHandler.GetCurMechaSkillDesc(skillId);
		int num = m_index + 1;
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MechaSkill))
		{
			if (num <= m_wndParent.m_mechaDetail.Star)
			{
				((TextElement)m_lbLevel).text = "";
				((VisualElement)m_lbLevel).style.top = StyleLength.op_Implicit(18f);
			}
			else
			{
				((TextElement)m_lbLevel).text = string.Format(zbUtil.Language("lang.10801"), num);
				((VisualElement)m_lbLevel).style.top = StyleLength.op_Implicit(52f);
				((VisualElement)m_lbLevel).style.color = StyleColor.op_Implicit(Color.red);
			}
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_lbWrapper.style.width = StyleLength.op_Implicit(600f);
			((VisualElement)m_lbDesc).style.width = StyleLength.op_Implicit(600f);
			bool flag = zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MechaSkill);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_gray.png"));
			return;
		}
		m_lbWrapper.style.width = StyleLength.op_Implicit(480f);
		((VisualElement)m_lbDesc).style.width = StyleLength.op_Implicit(480f);
		bool flag2 = m_skillDetail != null && m_wndParent.m_mechaDetail != null && num <= m_wndParent.m_mechaDetail.Star;
		if (flag2)
		{
			((TextElement)m_lbLevel).text = "Lv." + m_skillDetail.Level;
			((VisualElement)m_lbLevel).style.color = StyleColor.op_Implicit(Color.white);
			zbMechaSkillUpgradeCsvRecord zbMechaSkillUpgradeCsvRecord2 = zbDataConfig.Instance.mechaSkillUpgradeCsvTable.GetRecords()?.records.FirstOrDefault((zbMechaSkillUpgradeCsvRecord r) => r.quality == m_wndParent.m_record.quality && r.lv == m_skillDetail.Level + 1);
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbMechaSkillUpgradeCsvRecord2 != null));
		}
		else
		{
			((TextElement)m_lbLevel).text = string.Format(zbUtil.Language("lang.10801"), num);
			((VisualElement)m_lbLevel).style.color = StyleColor.op_Implicit(Color.red);
			((VisualElement)m_lbLevel).style.top = StyleLength.op_Implicit(52f);
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>((flag2 && !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MechaSkill)) ? "ui/image/button/btn_180_80_yellow.png" : "ui/image/button/btn_180_80_gray.png"));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasSkillUpgradeNotify(m_wndParent.m_record.id, index + 1)));
	}

	private void OnUpgrade()
	{
		int num = m_index + 1;
		if (m_skillDetail != null && m_wndParent.m_mechaDetail != null && num <= m_wndParent.m_mechaDetail.Star && !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MechaSkill))
		{
			zbUIMgr.Instance.ShowMechaSkillUpgrade(m_wndParent.m_record, m_skillDetail);
		}
	}
}
