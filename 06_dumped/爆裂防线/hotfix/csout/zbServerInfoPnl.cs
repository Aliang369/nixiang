using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServerInfoPnl
{
	private zbServerListWnd m_parent;

	private ListView m_listViewInfo;

	private List<ServerRoleDetail> m_listInfoDatas;

	public zbServerInfoPnl(ListView listview, zbServerListWnd parent)
	{
		m_listViewInfo = listview;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listViewInfo, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listViewInfo, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listViewInfo).horizontalScrollingEnabled = false;
		m_parent = parent;
		m_listInfoDatas = new List<ServerRoleDetail>();
		SetListVisible(bVisible: false);
	}

	public void SetListVisible(bool bVisible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listViewInfo).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!bVisible));
	}

	public void RefreshLastLogin()
	{
		m_listInfoDatas.Clear();
		((VisualElement)m_listViewInfo).Clear();
		if ((Object)(object)m_parent != (Object)null)
		{
			m_listInfoDatas = m_parent.GetServerRoleList();
		}
		SetListVisible(m_listInfoDatas.Count > 0);
		if (m_listInfoDatas.Count != 0)
		{
			((BaseVerticalCollectionView)m_listViewInfo).itemsSource = m_listInfoDatas;
			((BaseVerticalCollectionView)m_listViewInfo).Rebuild();
		}
	}

	public void RefreshServer(int index)
	{
		m_listInfoDatas.Clear();
		((VisualElement)m_listViewInfo).Clear();
		if ((Object)(object)m_parent != (Object)null && m_parent.m_serverListAck != null)
		{
			ServerListAck serverListAck = m_parent.m_serverListAck;
			int count = serverListAck.ServerList.Count;
			int num = (index - 1) * m_parent.rowNum;
			for (int num2 = m_parent.rowNum - 1; num2 >= 0; num2--)
			{
				int num3 = num + num2;
				if (num3 < count)
				{
					ServerDetail serverDetail = serverListAck.ServerList[num3];
					RoleBaseInfo serverRoleInfoByServerId = m_parent.GetServerRoleInfoByServerId(serverDetail.ServerId);
					ServerRoleDetail item = new ServerRoleDetail(serverDetail, serverRoleInfoByServerId);
					m_listInfoDatas.Add(item);
				}
			}
		}
		SetListVisible(m_listInfoDatas.Count > 0);
		if (m_listInfoDatas.Count == 0)
		{
			return;
		}
		((BaseVerticalCollectionView)m_listViewInfo).itemsSource = m_listInfoDatas;
		m_listViewInfo.makeItem = () => (VisualElement)(object)new zbServerInfoItem(m_parent.treeAsset_Server);
		m_listViewInfo.bindItem = delegate(VisualElement element, int index2)
		{
			if (m_listInfoDatas[index2] != null)
			{
				(element as zbServerInfoItem).Init(m_parent, m_listInfoDatas[index2]);
			}
		};
		((BaseVerticalCollectionView)m_listViewInfo).Rebuild();
	}
}
