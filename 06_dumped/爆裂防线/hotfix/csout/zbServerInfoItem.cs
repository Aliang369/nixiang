using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServerInfoItem : VisualElement
{
	private Label m_lblTile;

	private Label m_lblRoleLv;

	private Button m_btnTouch;

	private ServerRoleDetail m_serverDetail;

	private zbServerListWnd m_parent;

	private VisualElement m_bgNormal;

	private VisualElement m_bgCur;

	private VisualElement m_state;

	public zbServerInfoItem(VisualTreeAsset tAsset)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		((VisualElement)this).style.backgroundColor = new StyleColor(Color.clear);
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
	}

	public void Init(zbServerListWnd parent, ServerRoleDetail data)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		m_serverDetail = data;
		m_parent = parent;
		((VisualElement)this).visible = data != null;
		if (data != null)
		{
			m_bgNormal = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg1", (string)null);
			m_bgCur = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg2", (string)null);
			bool flag = zbLoginUtil.selectServerId == data.m_serverDetail.ServerId;
			m_bgNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			m_bgCur.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			m_lblTile = UQueryExtensions.Q<Label>((VisualElement)(object)this, "title", (string)null);
			m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
			m_lblRoleLv = UQueryExtensions.Q<Label>((VisualElement)(object)this, "rolelv", (string)null);
			m_state = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "state", (string)null);
			m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
			((TextElement)m_lblTile).text = data.m_serverDetail.ServerName;
			if (data.m_roleInfo != null)
			{
				((TextElement)m_lblRoleLv).text = "Lv." + data.m_roleInfo.Level;
			}
			else
			{
				((TextElement)m_lblRoleLv).text = "";
			}
			EServerState status = data.m_serverDetail.Status;
			Texture2D val = null;
			switch (status)
			{
			case EServerState.Close:
				val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/login/serverlist/close.png");
				break;
			case EServerState.Limit:
			case EServerState.Normal:
			case EServerState.Recommend:
				val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/login/serverlist/normal.png");
				break;
			case EServerState.Hot:
				val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/login/serverlist/hot.png");
				break;
			}
			m_state.style.backgroundImage = new StyleBackground(val);
			zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		if (!((Object)(object)m_parent != (Object)null) || m_serverDetail == null)
		{
			return;
		}
		if (m_serverDetail.m_roleInfo == null && m_serverDetail.m_serverDetail.RegisterState == EBoolState.False)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10768"), delegate(bool isOK)
			{
				if (isOK)
				{
					OnSelectServer(zbLoginUtil.serverListAckData.ServerId);
				}
			});
		}
		else
		{
			OnSelectServer(m_serverDetail.m_serverDetail.ServerId);
		}
	}

	private void OnSelectServer(int serverId)
	{
		zbLoginUtil.selectServerId = serverId;
		m_parent.Hide();
		(zbUIMgr.Instance.GetWndById(EWndId.Login) as zbLoginWnd).OnServer_List_Ack(null);
	}
}
