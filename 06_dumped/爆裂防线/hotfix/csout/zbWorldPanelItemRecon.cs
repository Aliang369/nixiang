using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWorldPanelItemRecon : zbViewBaseItem
{
	private zbWorldWnd m_wndParent;

	private VisualElement m_icon;

	private Label m_lbName;

	private Label m_lbState;

	private Button m_btnGoto;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private int m_reconId;

	private WorldReconDetail m_reconDetail;

	public zbWorldPanelItemRecon(zbWorldWnd wndParent)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldPanelRecon.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_icon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbState = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_state", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_goto", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-5f, -4f), 0.9f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int reconId)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		m_reconId = reconId;
		m_icon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_team" + (m_reconId - 1) + ".png"));
		((TextElement)m_lbName).text = zbUtil.Language("lang.10254") + m_reconId;
		m_reconDetail = zbSelf.Instance.worldHandler.GetWorldReconDetail(m_reconId);
		((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_reconDetail == null));
		if (m_reconDetail != null)
		{
			((TextElement)m_lbState).text = zbSelf.Instance.worldHandler.GetStateString(m_reconDetail.State);
			((VisualElement)m_lbState).style.color = StyleColor.op_Implicit(Color.white);
		}
		else
		{
			((TextElement)m_lbState).text = zbUtil.Language("lang.10481");
			((VisualElement)m_lbState).style.color = StyleColor.op_Implicit(new Color(1f, 43f / 51f, 0f));
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.worldHandler.HasWorldReconNotify(reconId)));
	}

	private void OnGoto(ClickEvent evt)
	{
		if (m_reconDetail == null)
		{
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldRecon2, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.WorldRecon2)) + "【" + zbUtil.Language("lang.11495") + "】");
			}
			return;
		}
		WorldMapGridDetail worldMapGridDetailByGridId = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(m_reconDetail.GridId);
		if (worldMapGridDetailByGridId != null)
		{
			m_wndParent.OnGotoGrid(worldMapGridDetailByGridId);
			m_wndParent.SetPanelVisible(vis: false);
		}
		else
		{
			zbUIMgr.Instance.ShowWorldRecon(m_reconDetail);
		}
	}
}
