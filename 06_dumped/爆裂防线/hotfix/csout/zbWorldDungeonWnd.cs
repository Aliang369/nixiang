using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldDungeonWnd : zbBaseWnd
{
	private Button m_btnTouch;

	public Button m_btnClose;

	private Label m_lbName;

	private Label m_lbDesc;

	private VisualElement m_spriteIcon;

	private Label m_lbAttack;

	private Label m_lbRisk;

	private Label m_lbZombie;

	private Label m_lbBuildSize;

	private Label m_lbTili;

	private Button m_btnConfirm;

	private int m_gridId;

	private zbWorldBuildingCsvRecord m_buildRecord;

	private zbDungeonCsvRecord m_dungeonRecord;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_lbAttack = UQueryExtensions.Q<Label>(m_RootElement, "lb_attack", (string)null);
		m_lbRisk = UQueryExtensions.Q<Label>(m_RootElement, "lb_risk", (string)null);
		m_lbZombie = UQueryExtensions.Q<Label>(m_RootElement, "lb_zombie", (string)null);
		m_lbBuildSize = UQueryExtensions.Q<Label>(m_RootElement, "lb_buildSize", (string)null);
		m_lbTili = UQueryExtensions.Q<Label>(m_RootElement, "lb_tili", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Tili }, isBgVis: true);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
	}

	public void RefreshCustom(int gridId, int buildId)
	{
		m_gridId = gridId;
		m_buildRecord = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(buildId);
		((TextElement)m_lbName).text = zbUtil.Language(m_buildRecord.name);
		((TextElement)m_lbDesc).text = zbUtil.Language(m_buildRecord.desc);
		RefreshDungeon();
	}

	private void RefreshDungeon()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		m_dungeonRecord = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(m_buildRecord.easyDungeonId);
		if (m_dungeonRecord != null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_dungeonRecord.icon));
			((TextElement)m_lbAttack).text = zbUtil.ConvertToTBMK(m_dungeonRecord.atkShow);
			((TextElement)m_lbTili).text = "x" + m_dungeonRecord.costTili;
			if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_dungeonRecord.costTili))
			{
				((VisualElement)m_lbTili).style.color = StyleColor.op_Implicit(Color.red);
			}
			else
			{
				((VisualElement)m_lbTili).style.color = StyleColor.op_Implicit(Color.white);
			}
			string text = "";
			Color white = Color.white;
			int atkShow = m_dungeonRecord.atkShow;
			if ((float)atkShow * 0.8f > (float)zbServerData.sGameData.RoleBaseInfo.TeamAttack)
			{
				text = zbUtil.Language("lang.10567");
				white = Color.red;
			}
			else if ((float)atkShow * 1.2f > (float)zbServerData.sGameData.RoleBaseInfo.TeamAttack && (float)zbServerData.sGameData.RoleBaseInfo.TeamAttack > (float)atkShow * 0.8f)
			{
				text = zbUtil.Language("lang.10568");
				white = Color32.op_Implicit(new Color32((byte)55, (byte)57, (byte)70, byte.MaxValue));
			}
			else if ((float)zbServerData.sGameData.RoleBaseInfo.TeamAttack > (float)atkShow * 1.2f)
			{
				text = zbUtil.Language("lang.10569");
				white = Color32.op_Implicit(new Color32((byte)58, (byte)161, (byte)0, byte.MaxValue));
			}
			else
			{
				text = zbUtil.Language("lang.10569");
				white = Color.green;
			}
			((TextElement)m_lbRisk).text = zbUtil.Language(text);
			((VisualElement)m_lbRisk).style.color = StyleColor.op_Implicit(white);
			string key;
			string key2;
			switch (m_dungeonRecord.size)
			{
			case 1:
				key = "lang.10460";
				key2 = "lang.10463";
				break;
			case 2:
				key = "lang.10461";
				key2 = "lang.10464";
				break;
			case 3:
				key = "lang.10462";
				key2 = "lang.10465";
				break;
			default:
				key = "lang.10460";
				key2 = "lang.10463";
				break;
			}
			((TextElement)m_lbZombie).text = zbUtil.Language(key);
			((TextElement)m_lbBuildSize).text = zbUtil.Language(key2);
		}
	}

	private void OnConfirm()
	{
		if (m_dungeonRecord != null && !zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_dungeonRecord.costTili))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, m_dungeonRecord.costTili);
			return;
		}
		zbSelf.Instance.worldDungeonHandler.OpenDungeon(m_gridId, EDifficultyLevel.Easy);
		OnClose();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object obj)
	{
		RefreshDungeon();
	}
}
