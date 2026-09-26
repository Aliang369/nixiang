using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldItemRes : zbViewBaseItem
{
	protected zbWorldWnd m_wndParent;

	public WorldMapGridDetail gridDetail;

	private WorldBuildingResDetail m_resData;

	private zbWorldBuildingCsvRecord m_record;

	private VisualElement m_spriteFrame;

	public VisualElement m_spriteIcon;

	private Label m_lbBuilding;

	private Label m_lbUnBuild;

	public zbWorldItemRes(zbWorldWnd wndParent, WorldMapGridDetail data)
	{
		m_wndParent = wndParent;
		gridDetail = data;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldRes.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbBuilding = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_building", (string)null);
		m_lbUnBuild = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_unBuild", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		zbUtil.SetPickingModeIgnoreRecursive((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		gridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(gridDetail.GridId);
		WorldReconDetail worldReconDetailByGridId = zbSelf.Instance.worldHandler.GetWorldReconDetailByGridId(gridDetail.GridId);
		m_record = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(gridDetail.BuildingId);
		if (gridDetail.State == EWorldMapGridState.Explored && m_record != null && worldReconDetailByGridId == null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
			((TextElement)m_lbBuilding).text = m_record.name;
			m_resData = zbSelf.Instance.worldHandler.GetWorldBuildingResDetail(gridDetail.GridId);
			if (m_resData == null)
			{
				((VisualElement)m_lbUnBuild).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_spriteFrame.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_frame4.png"));
			}
			else if (m_resData.ResLevel == 0)
			{
				((VisualElement)m_lbUnBuild).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spriteFrame.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				((VisualElement)m_lbUnBuild).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_spriteFrame.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_frame3.png"));
			}
		}
		else
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public void OnTouch()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!(((VisualElement)this).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1)))
		{
			if (m_resData == null)
			{
				zbUIMgr.Instance.ShowWorldDungeon(gridDetail.GridId, gridDetail.BuildingId);
			}
			else if (m_resData.ResLevel == 0)
			{
				zbUIMgr.Instance.ShowWorldResourceCreate(gridDetail);
			}
			else
			{
				zbUIMgr.Instance.ShowWorldResourceDetail(gridDetail);
			}
		}
	}
}
