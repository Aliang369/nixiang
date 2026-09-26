using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbZhanLingKillItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbZhanLingKillItem>
	{
	}

	private zbZhanLingKillWnd m_wndParent;

	private VisualElement m_progressVal;

	private VisualElement m_grayMask;

	private VisualElement m_iconLevel;

	private Label m_lbLevel;

	private VisualElement m_bg;

	private zbItem[] m_items = new zbItem[3];

	public Sprite mainQuestIcon_Cur;

	public Sprite mainQuestIcon_Pre;

	public Sprite mainQuestIcon_Next;

	private zbZhanLingKillTargetCsvRecord m_record;

	public zbZhanLingKillItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_zhanLingKill.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbZhanLingKillWnd wndParent)
	{
		m_wndParent = wndParent;
		m_progressVal = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressVal", (string)null);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_iconLevel = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_level", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		mainQuestIcon_Cur = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_current.png");
		mainQuestIcon_Next = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_next.png");
		mainQuestIcon_Pre = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_pre.png");
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item{i}", (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbZhanLingKillTargetCsvRecord record)
	{
		m_record = record;
		if (m_record != null && m_wndParent.m_data != null)
		{
			((TextElement)m_lbLevel).text = m_record.lv.ToString();
			bool flag = m_wndParent.m_data.ZhanLingInfo.Level >= m_record.lv;
			SetMaskVisible(!flag);
			RefreshDetail();
		}
	}

	private void RefreshDetail()
	{
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_record.lv > m_wndParent.m_data.ZhanLingInfo.Level;
		bool flag2 = m_wndParent.m_data.ZhanLingInfo.Active == EBoolState.True;
		bool flag3 = m_record.lv <= m_wndParent.m_data.ZhanLingInfo.LowAward;
		bool flag4 = flag2 && m_record.lv <= m_wndParent.m_data.ZhanLingInfo.HighAward;
		bool flag5 = IsCurrentZhanling(m_record.lv);
		bool canReceiveLowAward = !flag3 && !flag;
		bool canReceiveHighAward = (!flag4 && !flag) & flag2;
		bool flag6 = IsGrandPrize(m_record.lv);
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag6 ? "ui/image/operate/zhanLingKill/zhanLingKill_grand.jpg" : "ui/image/operate/zhanLingKill/zhanLingKill_cell.jpg"));
		m_progressVal.style.height = new StyleLength(Length.Percent((float)((!flag) ? ((flag5 && !flag6) ? 50 : 100) : 0)));
		m_iconLevel.style.backgroundImage = new StyleBackground(flag ? mainQuestIcon_Next : mainQuestIcon_Pre);
		for (int i = 0; i < 3; i++)
		{
			bool flag7 = i == 0;
			if (flag7)
			{
				bool flag8 = m_record.lowAward.Length != 0;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag8));
				if (flag8)
				{
					int[] lowAward = m_record.lowAward[i];
					zbItemData data = new zbItemData((EResourceType)lowAward[0], lowAward[1], lowAward[2]);
					m_items[i].Refresh(data);
					m_items[i].SetGettedVisible(flag3);
					m_items[i].SetSpineVisible(canReceiveLowAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveLowAward)
						{
							zbWsMsgHandler.Instance.Operate_ZhanLingKillAward_Req(m_wndParent.m_data.BaseInfo.UUID, m_record.lv);
						}
						else
						{
							zbUIMgr.Instance.ShowTip((EResourceType)lowAward[0], lowAward[1], 0L);
						}
					});
				}
			}
			else
			{
				int num = i - m_record.lowAward.Length;
				int[][] highAwards = GetHighAwards();
				bool flag9 = highAwards.Length != 0 && highAwards.Length > num;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag9));
				if (flag9)
				{
					int[] highAward = highAwards[num];
					zbItemData data2 = new zbItemData((EResourceType)highAward[0], highAward[1], highAward[2]);
					m_items[i].Refresh(data2);
					m_items[i].SetGettedVisible(!flag7 & flag4);
					m_items[i].SetSpineVisible(canReceiveHighAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveHighAward)
						{
							zbWsMsgHandler.Instance.Operate_ZhanLingKillAward_Req(m_wndParent.m_data.BaseInfo.UUID, m_wndParent.m_data.ZhanLingInfo.Level);
						}
						else
						{
							zbUIMgr.Instance.ShowTip((EResourceType)highAward[0], highAward[1], 0L);
						}
					});
				}
			}
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_items[i], "icon_lock", (string)null)).visible = ((i == 0) ? flag : (flag || !flag2));
		}
	}

	private int[][] GetHighAwards()
	{
		List<int[]> list = new List<int[]>();
		if (m_record.highAward != null)
		{
			list.AddRange(m_record.highAward);
		}
		if (m_wndParent.m_data.GrandExtraList != null)
		{
			foreach (OperateZhanLingExtraDetail grandExtra in m_wndParent.m_data.GrandExtraList)
			{
				if (m_record.lv == grandExtra.Level)
				{
					list.Add(new int[3]
					{
						(int)grandExtra.Award.Type,
						(int)grandExtra.Award.Tid,
						(int)grandExtra.Award.Count
					});
				}
			}
		}
		return list.ToArray();
	}

	private bool IsCurrentZhanling(int level)
	{
		if (m_wndParent.m_records == null || m_wndParent.m_records.Length == 0)
		{
			return false;
		}
		int reach = m_wndParent.m_data?.ZhanLingInfo.Reach ?? 0;
		zbZhanLingKillTargetCsvRecord zbZhanLingKillTargetCsvRecord2 = (from x in m_wndParent.m_records
			where x != null && x.lv <= reach
			orderby x.lv descending
			select x).FirstOrDefault();
		zbZhanLingKillTargetCsvRecord zbZhanLingKillTargetCsvRecord3 = m_wndParent.m_records.OrderBy((zbZhanLingKillTargetCsvRecord x) => x?.lv).LastOrDefault();
		if (zbZhanLingKillTargetCsvRecord2 != null && zbZhanLingKillTargetCsvRecord3 != null && zbZhanLingKillTargetCsvRecord2.lv == zbZhanLingKillTargetCsvRecord3.lv)
		{
			return false;
		}
		if (zbZhanLingKillTargetCsvRecord2 != null)
		{
			return zbZhanLingKillTargetCsvRecord2.lv == level;
		}
		return false;
	}

	public bool IsGrandPrize(int level)
	{
		if (m_wndParent.m_records == null || m_wndParent.m_records.Length == 0)
		{
			return false;
		}
		zbZhanLingKillTargetCsvRecord[] records = m_wndParent.m_records;
		foreach (zbZhanLingKillTargetCsvRecord zbZhanLingKillTargetCsvRecord2 in records)
		{
			if (zbZhanLingKillTargetCsvRecord2.lv == level && zbZhanLingKillTargetCsvRecord2.isGrand > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void SetMaskVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}
}
