using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFirstRechargeTab
{
	private const int DAY_COUNT = 3;

	private const int ITEM_COUNT_PER_DAY = 2;

	private VisualElement m_RootElement;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private SpineVisualElement m_tabSpines;

	private readonly VisualElement[] m_dayRedDots;

	private readonly VisualElement[] m_dayLayers;

	private readonly zbItem[,] m_items;

	private readonly SpineVisualElement[] m_spineDays;

	private Button m_btnAvatar;

	private Button m_btnGem;

	public zbFirstRechargeTab(VisualElement root, zbFirstRechargeWnd parent)
	{
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		m_dayRedDots = (VisualElement[])(object)new VisualElement[3];
		m_dayLayers = (VisualElement[])(object)new VisualElement[3];
		m_items = new zbItem[3, 2];
		m_spineDays = (SpineVisualElement[])(object)new SpineVisualElement[3];
		base._002Ector();
		m_RootElement = root;
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnAvatar = UQueryExtensions.Q<Button>(m_RootElement, "btn_avatar", (string)null);
		m_btnGem = UQueryExtensions.Q<Button>(m_RootElement, "btn_gem", (string)null);
		m_tabSpines = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineBg", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_dayLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "day" + i, (string)null);
			m_spineDays[i] = UQueryExtensions.Q<SpineVisualElement>(m_dayLayers[i], "spineDay", (string)null);
			for (int j = 0; j < 2; j++)
			{
				m_items[i, j] = UQueryExtensions.Q<zbItem>(m_dayLayers[i], "item" + j, (string)null);
				m_items[i, j].OnEnable(parent);
			}
			m_dayRedDots[i] = zbUtil.CreateNotifyTag(m_dayLayers[i], (Vector2?)new Vector2(0f, 0f), 1f, true);
		}
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAvatar, (Action)OnTip, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGem, (Action)OnGem, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		m_tabSpines.AnimationState.SetAnimation(0, "in", false);
		m_tabSpines.AnimationState.AddAnimation(0, "loop", true, 0f);
		SetFirstRechargeUIText();
		RefreshDayAwardList();
		UpdateDayRedDots();
		int currentDisplayDay = GetCurrentDisplayDay();
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_spineDays[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((currentDisplayDay != i) ? 1 : 0));
		}
		bool flag = zbSelf.Instance.operateHandler.IsNovicePackPurchased();
		int novicePackClaimedDays = zbSelf.Instance.operateHandler.GetNovicePackClaimedDays();
		for (int j = 0; j < 3; j++)
		{
			for (int k = 0; k < 2; k++)
			{
				m_items[j, k].SetGettedVisible(flag && novicePackClaimedDays > j);
			}
		}
	}

	private void SetFirstRechargeUIText()
	{
		string format = zbUtil.Language("lang.10425");
		string arg = zbUtil.AddHighlightStyle(zbUtil.Language("lang.10424"), "FF8C00", 24);
		((TextElement)m_lbDesc).text = string.Format(format, arg);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.10722");
	}

	private void RefreshDayAwardList()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		JToken val = zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["novicePack"];
		if (val == null)
		{
			HideAllItems();
			return;
		}
		JArray val2 = (JArray)val[(object)"dayAwardList"];
		HideAllItems();
		for (int i = 0; i < 3; i++)
		{
			int num = 0;
			foreach (JArray item in (JArray)val2[i])
			{
				JArray val3 = item;
				((VisualElement)m_items[i, num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[i, num].Refresh(new zbItemData((EResourceType)(int)val3[0], (long)val3[1], (long)val3[2]));
				m_items[i, num].SetZhenGuiVisible(num == 0);
				num++;
			}
		}
	}

	private void UpdateDayRedDots()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (m_dayRedDots[i] != null)
			{
				m_dayRedDots[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		int novicePackClaimedDays = zbSelf.Instance.operateHandler.GetNovicePackClaimedDays();
		if (zbSelf.Instance.operateHandler.IsNovicePackClaimableToday() && novicePackClaimedDays >= 0 && novicePackClaimedDays < 3 && m_dayRedDots[novicePackClaimedDays] != null)
		{
			m_dayRedDots[novicePackClaimedDays].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private int GetCurrentDisplayDay()
	{
		if (!zbSelf.Instance.operateHandler.IsNovicePackPurchased())
		{
			return 0;
		}
		return Math.Min(zbSelf.Instance.operateHandler.GetNovicePackClaimedDays(), 2);
	}

	private void HideAllItems()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				((VisualElement)m_items[i, j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private void OnTip()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		JArray val = (JArray)((JArray)zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["novicePack"][(object)"dayAwardList"])[0];
		zbUIMgr.Instance.ShowTip(EResourceType.Prop, Extensions.Value<int>((IEnumerable<JToken>)val[0][(object)1]), 0L);
	}

	private void OnGem()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		JArray val = (JArray)((JArray)zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["novicePack"][(object)"dayAwardList"])[0];
		zbUIMgr.Instance.ShowTip(EResourceType.Gem, Extensions.Value<int>((IEnumerable<JToken>)val[1][(object)1]), 0L);
	}
}
