using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarServerItem : zbViewBaseItem
{
	private zbMineWarServerWnd m_Parent;

	private VisualElement sp_sprite;

	private Label m_lbName;

	private Label m_lbCost;

	private Label m_lbSelf;

	private Button m_btnTouch;

	private VisualElement m_lbCross;

	private VisualElement m_lbState;

	private CrossMineServerDetail server;

	private VisualElement sp_NewServer;

	private Label m_lbNewServer;

	public zbMineWarServerItem(zbMineWarServerWnd parent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_Parent = parent;
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Server", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Cost", (string)null);
		m_lbCross = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "cross", (string)null);
		m_lbSelf = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Self", (string)null);
		m_lbState = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_State", (string)null);
		sp_sprite = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbNewServer = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_NewServerTime", (string)null);
		sp_NewServer = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "newServer", (string)null);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(CrossMineServerDetail info)
	{
		server = info;
		refreshItems();
	}

	private void refreshItems()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		if (server.ServerId == zbSelf.Instance.GetServerId())
		{
			((VisualElement)m_lbSelf).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_lbCross.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			sp_NewServer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			sp_sprite.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mineWar/contentSelf_bg.png"));
		}
		else
		{
			if (server.CrossOpenTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				sp_NewServer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_lbCross.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				sp_NewServer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_lbCross.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			((VisualElement)m_lbSelf).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			sp_sprite.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legion/content_bg.png"));
		}
		if (server.State == EMineWarServerState.Barren)
		{
			((VisualElement)m_lbName).style.color = StyleColor.op_Implicit(new Color(0.9137255f, 1f / 51f, 1f / 51f, 1f));
			((VisualElement)m_lbName).style.color = StyleColor.op_Implicit(Color.red);
			((TextElement)m_lbName).text = server.ServerName + "(" + zbUtil.Language("lang.10984") + ")";
			m_lbState.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mineWar/state_3.png"));
		}
		else if (server.State == EMineWarServerState.Normal)
		{
			((VisualElement)m_lbName).style.color = StyleColor.op_Implicit(Color.white);
			((TextElement)m_lbName).text = server.ServerName + "(" + zbUtil.Language("lang.10985") + ")";
			m_lbState.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mineWar/state_2.png"));
		}
		else if (server.State == EMineWarServerState.Prosperity)
		{
			((VisualElement)m_lbName).style.color = StyleColor.op_Implicit(new Color(20f / 51f, 1f, 0.14901961f, 1f));
			((VisualElement)m_lbName).style.color = StyleColor.op_Implicit(Color.green);
			((TextElement)m_lbName).text = server.ServerName + "(" + zbUtil.Language("lang.10986") + ")";
			m_lbState.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mineWar/state_1.png"));
		}
		((TextElement)m_lbCost).text = "X" + server.CostCombatNum;
	}

	private void OnTouch(ClickEvent evt)
	{
		if (server.ServerId == zbSelf.Instance.GetServerId())
		{
			zbWsMsgHandler.Instance.MineWar_Data_Req();
		}
		else
		{
			if (server.CrossOpenTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				string arg = zbUtil.FormatTimeSmart(server.CrossOpenTm - zbSelf.Instance.timeHandler.GetServerTime());
				string msg = string.Format(zbUtil.Language("lang.11028"), arg);
				zbUIMgr.Instance.BubbleInfo(msg);
				return;
			}
			zbWsMsgHandler.Instance.CrossMine_Data_Req(server.ServerId);
		}
		m_Parent.Hide();
	}

	public void UpdatePerSecond()
	{
		if (server.CrossOpenTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_lbNewServer).text = zbUtil.FormatTimeSmart(server.CrossOpenTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}
}
