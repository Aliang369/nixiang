using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbResetNameWnd : zbBaseWnd
{
	private Button m_Button_close;

	private Button m_Button_AllClose;

	private Button m_Button_RandomName;

	private Button m_Button_ok;

	private TextField m_field_ResetName;

	private zbDebouncedPnl m_inputTouch;

	private string m_strName;

	private string m_tempName;

	private Label m_lbCostDiamond;

	private Label m_lbCostItem;

	private Label m_Lb_FirstFree;

	private const int RenameCostDiamond = 100;

	private zbPlayerNameCsvRecord[] m_records;

	private string[] m_firstNameList;

	private string[] m_lastNameList;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_Button_close = UQueryExtensions.Q<Button>(m_RootElement, "Button_close", (string)null);
		m_Button_AllClose = UQueryExtensions.Q<Button>(m_RootElement, "Button_AllClose", (string)null);
		m_Button_RandomName = UQueryExtensions.Q<Button>(m_RootElement, "Button_RandomName", (string)null);
		m_Button_ok = UQueryExtensions.Q<Button>(m_RootElement, "Button_ok", (string)null);
		m_field_ResetName = UQueryExtensions.Q<TextField>(m_RootElement, "field_ResetName", (string)null);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_lbCostDiamond = UQueryExtensions.Q<Label>(m_RootElement, "Lb_costDiamond", (string)null);
		m_lbCostItem = UQueryExtensions.Q<Label>(m_RootElement, "lb_costItem", (string)null);
		m_Lb_FirstFree = UQueryExtensions.Q<Label>(m_RootElement, "Lb_firstFree", (string)null);
		m_records = zbDataConfig.Instance.playerNameCsvTable.GetRecords().records.Values.ToArray();
		m_firstNameList = (from record in m_records
			where !string.IsNullOrEmpty(record.firstname)
			select record.firstname).ToArray();
		m_lastNameList = (from record in m_records
			where !string.IsNullOrEmpty(record.lastname)
			select record.lastname).ToArray();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_close, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_AllClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_RandomName, (Action<ClickEvent>)OnRandomName, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_ok, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_RoleName_Changed, OnRoleNameChanged);
		AddDark();
	}

	private void OnInputClick()
	{
		((Focusable)m_field_ResetName).Focus();
	}

	private void OnOk(ClickEvent evt)
	{
		string input = ((BaseField<string>)(object)m_field_ResetName)?.value?.Trim();
		if (!ValidateName(input, out var newName) || (zbServerData.sGameData.RoleCommonInfo.RoleNameChange != 0 && !HasEnoughRenameCost()))
		{
			return;
		}
		if (string.Equals(zbSelf.Instance.GetRoleName(), newName, StringComparison.OrdinalIgnoreCase))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10446"));
			return;
		}
		string title = zbUtil.Language("lang.10620");
		string arg = "<color=#D97904>" + newName + "</color>";
		string message = string.Format(zbUtil.Language("lang.10621"), arg);
		zbUIMgr.Instance.ShowMessageBox(title, message, delegate(bool isOk)
		{
			if (isOk)
			{
				m_strName = newName;
				zbWsMsgHandler.Instance.Reset_RoleName_Req(newName);
			}
		});
	}

	private bool ValidateName(string input, out string normalized)
	{
		string reason = "";
		if (!TryValidateName(input, out reason, out normalized))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language(reason));
			return false;
		}
		if (normalized.Length > 14)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10073"));
			return false;
		}
		if (!zbCharChecker.IsSafeChatMessage(normalized))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10074"));
			return false;
		}
		return true;
	}

	private bool HasEnoughRenameCost()
	{
		if (zbSelf.Instance.itemHandler.GetItemCount(EItemType.RoleNameCard) > 0)
		{
			return true;
		}
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(2, 100L))
		{
			return true;
		}
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10275"));
		return false;
	}

	private bool IsRenameSuccessful(string submittedName, string serverReturnedName)
	{
		return string.Equals(submittedName?.Trim(), serverReturnedName?.Trim(), StringComparison.Ordinal);
	}

	private void OnRoleNameChanged(object obj)
	{
		string roleName = zbServerData.sGameData.RoleBaseInfo.RoleName;
		if (IsRenameSuccessful(m_strName, roleName))
		{
			Hide();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10392"));
		}
	}

	private void OnRandomName(ClickEvent evt)
	{
		m_tempName = GetRandomName();
		int num = 0;
		while (m_tempName.Length > 14)
		{
			m_tempName = GetRandomName();
			num++;
			if (num >= 5)
			{
				break;
			}
		}
		((BaseField<string>)(object)m_field_ResetName).value = m_tempName;
	}

	private string GetRandomName()
	{
		string obj = m_firstNameList[Random.Range(0, m_firstNameList.Length)];
		string text = m_lastNameList[Random.Range(0, m_lastNameList.Length)];
		return obj + text;
	}

	public void RefreshCustom()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		m_tempName = zbSelf.Instance.GetRoleName();
		((BaseField<string>)(object)m_field_ResetName).value = m_tempName;
		bool flag = zbServerData.sGameData.RoleCommonInfo.RoleNameChange == 0;
		bool flag2 = zbSelf.Instance.itemHandler.GetItemCount(EItemType.RoleNameCard) > 0;
		bool flag3 = zbSelf.Instance.moneyHandler.HasEnoughMoney(2, 100L);
		((TextElement)m_Lb_FirstFree).text = zbUtil.Language("lang.10471");
		((VisualElement)m_Lb_FirstFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbCostItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(!flag & flag2)));
		((VisualElement)m_lbCostDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag || flag2) ? 1 : 0));
		if (m_lbCostDiamond != null)
		{
			((TextElement)m_lbCostDiamond).text = 100.ToString();
			((VisualElement)m_lbCostDiamond).style.color = StyleColor.op_Implicit(flag3 ? Color.green : Color.red);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public static bool TryValidateName(string input, out string reason, out string normalized)
	{
		reason = "";
		normalized = "";
		if (string.IsNullOrWhiteSpace(input))
		{
			reason = "lang.10085";
			return false;
		}
		normalized = input.Normalize(NormalizationForm.FormKC).Trim();
		if (string.IsNullOrEmpty(normalized))
		{
			reason = "lang.10085";
			return false;
		}
		string text = normalized;
		foreach (char c in text)
		{
			UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if (unicodeCategory == UnicodeCategory.Control || unicodeCategory == UnicodeCategory.Format || unicodeCategory == UnicodeCategory.Surrogate || unicodeCategory == UnicodeCategory.PrivateUse || unicodeCategory == UnicodeCategory.OtherNotAssigned)
			{
				reason = "lang.10074";
				return false;
			}
			if (!IsAllowedNameChar(c))
			{
				reason = "lang.10074";
				return false;
			}
		}
		return true;
	}

	private static bool IsAllowedNameChar(char c)
	{
		if (c >= '一' && c <= '鿿')
		{
			return true;
		}
		if (c >= '㐀' && c <= '䶿')
		{
			return true;
		}
		if (c >= 'a' && c <= 'z')
		{
			return true;
		}
		if (c >= 'A' && c <= 'Z')
		{
			return true;
		}
		if (c >= '0' && c <= '9')
		{
			return true;
		}
		if (c == '_')
		{
			return true;
		}
		return false;
	}
}
