using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldItemBuilding : zbViewBaseItem
{
	protected zbWorldWnd m_wndParent;

	public WorldMapGridDetail gridDetail;

	private zbWorldBuildingCsvRecord m_record;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbBuilding;

	private VisualElement[] m_spriteDifficulits = (VisualElement[])(object)new VisualElement[3];

	public zbWorldItemBuilding(zbWorldWnd wndParent, WorldMapGridDetail data)
	{
		m_wndParent = wndParent;
		gridDetail = data;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldBuilding.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbBuilding = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_building", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_spriteDifficulits[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "difficult" + i, (string)null);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		zbUtil.SetPickingModeIgnoreRecursive((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		gridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(gridDetail.GridId);
		WorldReconDetail worldReconDetailByGridId = zbSelf.Instance.worldHandler.GetWorldReconDetailByGridId(gridDetail.GridId);
		m_record = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(gridDetail.BuildingId);
		if (gridDetail.State == EWorldMapGridState.Explored && m_record != null && worldReconDetailByGridId == null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			for (int i = 0; i < 3; i++)
			{
				m_spriteDifficulits[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
			((TextElement)m_lbBuilding).text = m_record.name;
			if (m_record.type == 1)
			{
				m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_frame3.png"));
			}
			else
			{
				m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/world/world_frame4.png"));
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
			if (m_record.type == 1)
			{
				zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10304"), OnConfirm);
			}
			else
			{
				zbUIMgr.Instance.ShowWorldDungeon(gridDetail.GridId, gridDetail.BuildingId);
			}
		}
	}

	private void OnConfirm(bool isOk)
	{
		if (isOk)
		{
			m_wndParent.Hide();
			zbLinkHelper.OpenHome();
		}
	}
}
