using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServerListWnd : zbBaseWnd
{
	public VisualTreeAsset treeAsset_Index;

	public VisualTreeAsset treeAsset_Server;

	private List<zbServerIndexData> m_listIndexDatas;

	private ScrollView m_listViewIndex;

	private zbServerInfoPnl m_infoPnl;

	private Button m_BtnClose;

	private VisualElement m_mask;

	[NonSerialized]
	public int rowNum = 10;

	public ServerListAck m_serverListAck;

	protected override void OnEnable()
	{
		base.OnEnable();
		InitElement();
		m_gameEevent.RegisterEvent(EGameEvent.Server_List_Ack, OnServer_List_Ack);
	}

	private void InitElement()
	{
		m_BtnClose = UQueryExtensions.Q<Button>(m_RootElement, "close", (string)null);
		m_mask = UQueryExtensions.Q<VisualElement>(m_RootElement, "mask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_BtnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_mask, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_listViewIndex = UQueryExtensions.Q<ScrollView>(m_RootElement, "serverindex", (string)null);
		((VisualElement)m_listViewIndex).visible = false;
		m_listIndexDatas = new List<zbServerIndexData>();
		ListView listview = UQueryExtensions.Q<ListView>(m_RootElement, "serverinfo", (string)null);
		m_infoPnl = new zbServerInfoPnl(listview, this);
		AddDark();
	}

	public void RefreshCustom()
	{
		zbHttpMsgHandler.Instance.Server_List_Req();
	}

	private void OnServer_List_Ack(object ackData)
	{
		m_serverListAck = zbLoginUtil.serverListAckData;
		int count = m_serverListAck.ServerList.Count;
		if (count == 0)
		{
			return;
		}
		((VisualElement)m_listViewIndex).visible = true;
		int num = count % rowNum;
		int num2 = (count - num) / rowNum;
		((VisualElement)m_listViewIndex).Clear();
		m_listIndexDatas.Clear();
		if (!((Object)(object)treeAsset_Index == (Object)null))
		{
			zbServerIndexData item = new zbServerIndexData(0, bLastTab: true);
			m_listIndexDatas.Add(item);
			if (num > 0)
			{
				zbServerIndexData item2 = new zbServerIndexData(num2 + 1, bLastTab: false, num);
				m_listIndexDatas.Add(item2);
			}
			for (int num3 = num2; num3 > 0; num3--)
			{
				zbServerIndexData item3 = new zbServerIndexData(num3, bLastTab: false);
				m_listIndexDatas.Add(item3);
			}
			for (int i = 0; i < m_listIndexDatas.Count; i++)
			{
				zbServerIndexItem zbServerIndexItem2 = new zbServerIndexItem(treeAsset_Index, this);
				zbServerIndexItem2.Init(m_listIndexDatas[i]);
				((VisualElement)m_listViewIndex).Add((VisualElement)(object)zbServerIndexItem2);
			}
			ScrollToLoginServer();
		}
	}

	private void ScrollToLoginServer()
	{
		if (m_serverListAck == null)
		{
			return;
		}
		ServerDetail serverDetailById = GetServerDetailById(zbLoginUtil.selectServerId);
		if (serverDetailById == null)
		{
			serverDetailById = GetServerDetailById(zbLoginUtil.loginServerId);
			if (serverDetailById == null)
			{
				serverDetailById = GetServerDetailById(m_serverListAck.ServerId);
			}
		}
		if (serverDetailById != null)
		{
			int num = Mathf.FloorToInt((float)((serverDetailById.SortId - 1) / rowNum));
			OnServerSel(num + 1);
		}
	}

	public void OnServerSel(int selectIndex)
	{
		m_infoPnl.RefreshServer(selectIndex);
		OnSelectionChanged(selectIndex);
	}

	private void OnSelectionChanged(int index)
	{
		foreach (zbServerIndexItem item in ((VisualElement)m_listViewIndex).Children().OfType<zbServerIndexItem>())
		{
			item.OnSelected(index);
		}
	}

	public void OnLastLoginServer()
	{
		m_infoPnl.RefreshLastLogin();
		OnSelectionChanged(0);
	}

	public ServerDetail GetServerDetailById(int id)
	{
		ServerDetail result = null;
		foreach (ServerDetail server in m_serverListAck.ServerList)
		{
			if (server.ServerId == id)
			{
				result = server;
				break;
			}
		}
		return result;
	}

	public List<ServerRoleDetail> GetServerRoleList()
	{
		List<ServerRoleDetail> list = new List<ServerRoleDetail>();
		if (m_serverListAck != null)
		{
			foreach (RoleBaseInfo role in m_serverListAck.RoleList)
			{
				ServerDetail serverDetailById = GetServerDetailById(role.ServerId);
				if (serverDetailById != null)
				{
					ServerRoleDetail item = new ServerRoleDetail(serverDetailById, role);
					list.Add(item);
				}
			}
		}
		return list;
	}

	public RoleBaseInfo GetServerRoleInfoByServerId(int serverId)
	{
		if (m_serverListAck != null)
		{
			foreach (RoleBaseInfo role in m_serverListAck.RoleList)
			{
				if (role.ServerId == serverId)
				{
					return role;
				}
			}
		}
		return null;
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnSetLang_Ack(object data)
	{
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
	}
}
