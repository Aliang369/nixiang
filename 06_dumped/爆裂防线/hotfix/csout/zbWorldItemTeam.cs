using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldItemTeam : zbViewBaseItem
{
	private zbWorldWnd m_wndParent;

	private zbWorldItemMgr m_worldItemMgr;

	public WorldReconDetail reconDetail;

	public WorldMapGridDetail gridDetail;

	private VisualElement m_root;

	private VisualElement m_frame;

	private VisualElement m_spriteTeam;

	private Label m_lbState;

	private Label m_lbTime;

	private float m_cellWidth;

	private float m_cellHeight;

	public zbWorldItemTeam(zbWorldWnd wndParent, zbWorldItemMgr worldItemMgr, WorldReconDetail data)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		m_cellWidth = 140f;
		m_cellHeight = 140f;
		base._002Ector();
		m_wndParent = wndParent;
		m_worldItemMgr = worldItemMgr;
		reconDetail = data;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldTeam.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_frame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteTeam = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_team", (string)null);
		m_lbState = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_state", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		if (reconDetail.ReconId == 1)
		{
			m_spriteTeam.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_team0.png"));
		}
		else
		{
			m_spriteTeam.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_team1.png"));
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		zbUtil.SetPickingModeIgnoreRecursive((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		reconDetail = zbSelf.Instance.worldHandler.GetWorldReconDetail(reconDetail.ReconId);
		if (reconDetail != null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(reconDetail.GridId <= 1));
			((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(reconDetail.State != EWorldReconState.Explore));
			gridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(reconDetail.GridId);
			if (reconDetail.State > EWorldReconState.Explored)
			{
				((VisualElement)m_lbState).style.color = StyleColor.op_Implicit(Color.red);
			}
			else
			{
				((VisualElement)m_lbState).style.color = StyleColor.op_Implicit(Color.white);
			}
			((TextElement)m_lbState).text = zbSelf.Instance.worldHandler.GetStateString(reconDetail.State);
			m_frame.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteTeam.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbState).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (gridDetail != null)
			{
				var (num, num2) = zbSelf.Instance.worldHandler.GetColAndRow(gridDetail);
				((VisualElement)this).style.left = StyleLength.op_Implicit((float)num * m_cellWidth - m_cellWidth);
				((VisualElement)this).style.top = StyleLength.op_Implicit((float)num2 * m_cellHeight - m_cellHeight);
			}
		}
		Update();
	}

	public void OnTouch()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (reconDetail != null && !(((VisualElement)this).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1)))
		{
			if (reconDetail.State == EWorldReconState.Explored)
			{
				zbWsMsgHandler.Instance.World_ReconExplored_Req(reconDetail.ReconId);
			}
			else
			{
				zbUIMgr.Instance.ShowWorldRecon(reconDetail);
			}
		}
	}

	public void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (reconDetail == null)
		{
			return;
		}
		if (m_worldItemMgr.IsReconLineDrawing(reconDetail.ReconId))
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else if (reconDetail.State == EWorldReconState.Explore)
		{
			long num = reconDetail.StateTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
			if (num <= 0)
			{
				zbWsMsgHandler.Instance.World_Data_Req();
			}
		}
	}
}
