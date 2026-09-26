using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldLairListItem : zbViewBaseItem
{
	private VisualElement m_iconBossAvatar;

	private VisualElement m_bossInfo;

	private VisualElement m_iconNew;

	private VisualElement m_playerInfo;

	private Button m_btnFight;

	private Label m_lbFrom;

	private Label m_lbName;

	private WorldLairHelpInfo m_worldLairInfo;

	private zbWorldLairCsvRecord m_record;

	public zbWorldLairListItem(VisualTreeAsset tAsset, zbWorldLairListWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_iconBossAvatar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_bossAvatar", (string)null);
		m_bossInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bossInfo", (string)null);
		m_iconNew = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_new", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		m_lbFrom = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_from", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_playerName", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_fight", (string)null);
		Button visualElement = UQueryExtensions.Q<Button>(m_playerInfo, "btn_head", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action)OnHead, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(WorldLairHelpInfo worldLairInfo)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		m_worldLairInfo = worldLairInfo;
		string uUID = m_worldLairInfo.HelpLairInfo.UUID;
		zbSelf.Instance.newTagHandler.AddNewLair(uUID);
		bool flag = zbSelf.Instance.newTagHandler.isNewLair(uUID);
		m_iconNew.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_record = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(worldLairInfo.HelpLairInfo.LairId);
		if (m_record != null)
		{
			InitBossInfo();
			InitPlayerInfo();
			((TextElement)m_lbFrom).text = (zbSelf.Instance.friendHandler.CheckIsFriend(worldLairInfo.HelpRoleInfo.RoleId) ? zbUtil.Language("lang.10592") : zbUtil.Language("lang.10593"));
		}
	}

	private void InitBossInfo()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon);
		m_iconBossAvatar.style.backgroundImage = new StyleBackground(val);
		int mstLv = zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey(m_worldLairInfo.HelpLairInfo.Level).mstLv;
		string text = "";
		Color white = Color.white;
		int num = mstLv - zbServerData.sGameData.RoleBaseInfo.Level;
		if (num >= 5)
		{
			text = zbUtil.Language("lang.10567");
			white = Color.red;
		}
		else if (num < 5 && num >= -3)
		{
			text = zbUtil.Language("lang.10568");
			white = Color.white;
		}
		else
		{
			text = zbUtil.Language("lang.10569");
			white = Color.green;
		}
		Label obj = UQueryExtensions.Q<Label>(m_iconBossAvatar, "lb_difficulty", (string)null);
		((VisualElement)obj).style.color = StyleColor.op_Implicit(white);
		((TextElement)obj).text = text;
		((TextElement)UQueryExtensions.Q<Label>(m_bossInfo, "lb_bossName", (string)null)).text = zbUtil.Language(m_record.name);
		VisualElement obj2 = UQueryExtensions.Q<VisualElement>(m_bossInfo, "progress", (string)null);
		Label val2 = UQueryExtensions.Q<Label>(m_bossInfo, "lb_progress", (string)null);
		float num2 = (float)m_worldLairInfo.HelpLairInfo.KillCount / (float)m_worldLairInfo.HelpLairInfo.PassTarget * 100f;
		obj2.style.width = StyleLength.op_Implicit(new Length((num2 > 100f) ? 100f : num2, (LengthUnit)1));
		((TextElement)val2).text = ((num2 > 100f) ? zbUtil.Language("lang.10570") : $"{m_worldLairInfo.HelpLairInfo.PassTarget - m_worldLairInfo.HelpLairInfo.KillCount}/{m_worldLairInfo.HelpLairInfo.PassTarget}");
	}

	private void OnFight()
	{
		zbWsMsgHandler.Instance.WorldLair_HelpData_Req(m_worldLairInfo.HelpLairInfo.UUID);
	}

	private void OnHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_worldLairInfo.HelpRoleInfo.RoleId);
	}

	private void InitPlayerInfo()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_worldLairInfo.HelpRoleInfo.RoleName;
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_worldLairInfo.HelpRoleInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "header", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_worldLairInfo.HelpRoleInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null)).text = m_worldLairInfo.HelpRoleInfo.Level.ToString();
	}
}
