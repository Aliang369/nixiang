using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbTipCommonItem : zbViewBaseItem
{
	private zbTipCommonWnd m_wndParent;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Button m_btnGoto;

	private int m_getwayType;

	private zbOperateCsvRecord m_operateRecord;

	private zbGetWayCsvRecord m_record;

	private int m_levelId;

	public zbTipCommonItem(zbTipCommonWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tip/tb_tipCommon.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_goto", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int getwayType)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		m_getwayType = getwayType;
		if (getwayType == 0)
		{
			m_spriteIcon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbName).text = zbUtil.Language("lang.10432");
			return;
		}
		m_spriteIcon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		string text = "ui/image/icon/getway/1000.png";
		if (m_getwayType > 1000)
		{
			m_operateRecord = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(getwayType - 1000);
			((TextElement)m_lbName).text = m_operateRecord.name;
			text = m_operateRecord.icon2;
		}
		else
		{
			m_record = zbDataConfig.Instance.getWayCsvTable.GetRecordByKey(getwayType);
			switch (getwayType)
			{
			case 7:
				m_levelId = zbSelf.Instance.getWayHandler.GetWayMaxMainQuest(m_wndParent.m_type, m_wndParent.m_id);
				m_levelId = ((m_levelId == 0) ? zbServerData.sGameData.MainQuestInfo.NextLevel : m_levelId);
				((TextElement)m_lbName).text = m_record.name + ": " + zbResourceHelper.GetMainLevelName(m_levelId);
				break;
			case 9:
				m_levelId = zbSelf.Instance.getWayHandler.GetWayMaxElite(m_wndParent.m_type, m_wndParent.m_id);
				m_levelId = ((m_levelId == 0) ? zbServerData.sGameData.EliteQuestInfo.NextLevel : m_levelId);
				((TextElement)m_lbName).text = m_record.name + ": " + zbResourceHelper.GetEliteLevelName(m_levelId);
				break;
			default:
				((TextElement)m_lbName).text = m_record.name;
				break;
			}
			text = m_record.icon;
		}
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(text);
		m_spriteIcon.style.backgroundImage = new StyleBackground(val);
		if (m_getwayType <= 1000 && m_record.funcType != 0 && zbSelf.Instance.funHandler.IsLockNoTip((EFuncType)m_record.funcType))
		{
			((VisualElement)m_btnGoto).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_180x70.png"));
		}
		else
		{
			((VisualElement)m_btnGoto).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180x70.png"));
		}
	}

	public void OnGoto(ClickEvent evt)
	{
		zbSelf.Instance.shopHandler.getWayItem = new zbItemData(m_wndParent.m_type, m_wndParent.m_id, 1L);
		if (zbServerData.sGameData.WorldInfo.DungeonBaseInfo.PassState == EPassState.InProgress)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10284"));
			return;
		}
		if (zbUIMgr.Instance.IsShow(EWndId.CombatPause))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10285"));
			return;
		}
		if (zbUIMgr.Instance.IsShow(EWndId.CombatBalance))
		{
			zbUIMgr.Instance.DestroyWnd(EWndId.CombatBalance);
			if (zbUIMgr.Instance.IsShow(EWndId.Combat))
			{
				zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
			}
		}
		if (zbUIMgr.Instance.IsShow(EWndId.CrossLadderBalance))
		{
			zbUIMgr.Instance.DestroyWnd(EWndId.CrossLadderBalance);
			if (zbUIMgr.Instance.IsShow(EWndId.Combat))
			{
				zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
			}
		}
		if (m_getwayType > 1000)
		{
			zbLinkHelper.OpenOperateByType((EOperateType)(m_getwayType - 1000), m_wndParent.m_type, m_wndParent.m_id);
		}
		else if (m_getwayType == 7)
		{
			int level = Mathf.Min(m_levelId, zbServerData.sGameData.MainQuestInfo.NextLevel);
			zbUIMgr.Instance.ShowMainLevel(0, level);
		}
		else if (m_getwayType == 9)
		{
			if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.EliteQuest))
			{
				int level2 = Mathf.Min(m_levelId, zbServerData.sGameData.EliteQuestInfo.NextLevel);
				zbUIMgr.Instance.ShowEliteQuest(level2);
			}
		}
		else
		{
			zbLinkHelper.OpenLink(m_record.link, 0);
		}
	}
}
