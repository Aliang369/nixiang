using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeCreateItem : zbViewBaseItem
{
	private Label m_lbName;

	private VisualElement m_spriteIcon;

	private Label m_lbCount;

	private Label m_lbDesc;

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private Button m_btnTouch;

	private zbHomeCreateWnd m_wndParent;

	private zbHomeRoomTypeCsvRecord m_record;

	private int[][] m_costList;

	public zbHomeCreateItem(zbHomeCreateWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_homeCreate.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_lbCosts[i] = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_cost" + i, (string)null);
		}
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbHomeRoomTypeCsvRecord record)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		((TextElement)m_lbName).text = record.name;
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(m_record.iconBuild);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteIcon.MarkDirtyRepaint();
		}
		int roomCountByType = zbSelf.Instance.homelandHandler.GetRoomCountByType(record.id);
		int maxNum = record.maxNum;
		if (roomCountByType >= maxNum)
		{
			((VisualElement)m_lbCount).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCount).style.color = StyleColor.op_Implicit(Color.white);
		}
		((TextElement)m_lbCount).text = roomCountByType + "/" + maxNum;
		((TextElement)m_lbDesc).text = record.desc;
		for (int i = 0; i < 2; i++)
		{
			((VisualElement)m_lbCosts[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbRoomUpgradeEffectCsvRecord[] records = zbDataConfig.Instance.roomUpgradeEffectCsvTable.GetRecords().records;
		foreach (zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 in records)
		{
			if (zbRoomUpgradeEffectCsvRecord2.id == record.length && zbRoomUpgradeEffectCsvRecord2.lv == 1)
			{
				m_costList = zbSelf.Instance.homelandHandler.AppendExtraCostItem(zbRoomUpgradeEffectCsvRecord2.costList, record.itemId, zbRoomUpgradeEffectCsvRecord2.costItemNum);
			}
		}
		if (m_costList != null)
		{
			int num = 0;
			int[][] costList = m_costList;
			foreach (int[] array in costList)
			{
				if (num < m_lbCosts.Length)
				{
					((VisualElement)m_lbCosts[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)array[0], array[1]);
					((VisualElement)m_lbCosts[num]).style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath));
					((TextElement)m_lbCosts[num]).text = array[2].ToString();
					if (zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)array[0], array[1]) < array[2])
					{
						((VisualElement)m_lbCosts[num]).style.color = StyleColor.op_Implicit(Color.red);
					}
					else
					{
						((VisualElement)m_lbCosts[num]).style.color = StyleColor.op_Implicit(Color.white);
					}
				}
				num++;
			}
		}
		VisualElement[] lbCosts = (VisualElement[])(object)m_lbCosts;
		zbUtil.CenterNodeItems(lbCosts, 110f);
	}

	public void OnTouch(ClickEvent evt)
	{
		zbSelf.Instance.homeEditHandler.createRoomRecord = m_record;
		zbSelf.Instance.homeEditHandler.createRoomCostList = m_costList;
		if (!zbSelf.Instance.homeEditHandler.HaveEmptyRoom())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10270"));
			zbSelf.Instance.homeEditHandler.createRoomRecord = null;
			return;
		}
		if (zbSelf.Instance.homelandHandler.GetRoomCountByType(m_record.id) >= m_record.maxNum)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10133"));
			zbSelf.Instance.homeEditHandler.createRoomRecord = null;
			return;
		}
		if (m_costList != null)
		{
			int[][] costList = m_costList;
			foreach (int[] array in costList)
			{
				if (zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)array[0], array[1]) < array[2])
				{
					zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
					zbSelf.Instance.homeEditHandler.createRoomRecord = null;
					return;
				}
			}
		}
		zbUIMgr.Instance.ShowHomeCreateDetail();
		m_wndParent.Hide();
		zbHomeMgr.Instance.floorMgr.Refresh();
		zbHomeMgr.Instance.roomMgr.Refresh();
		zbHomeWnd zbHomeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Home) as zbHomeWnd;
		if ((Object)(object)zbHomeWnd2 != (Object)null)
		{
			zbHomeWnd2.SetUIVisible(vis: false);
		}
	}
}
