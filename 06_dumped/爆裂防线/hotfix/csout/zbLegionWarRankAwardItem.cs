using Best.HTTP.Shared.Extensions;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarRankAwardItem : zbViewBaseItem
{
	private zbLegionWarRankAwardWnd m_wndParent;

	private Label m_lbRank;

	private zbItem[] m_itemWins = new zbItem[4];

	private zbItem[] m_itemFails = new zbItem[4];

	public zbLegionWarRankAwardItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarRankAward.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbLegionWarRankAwardWnd wndParent)
	{
		m_wndParent = wndParent;
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_win", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_fail", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_itemWins[i] = UQueryExtensions.Q<zbItem>(val, "item" + i, (string)null);
			m_itemWins[i].OnEnable(m_wndParent);
			m_itemFails[i] = UQueryExtensions.Q<zbItem>(val2, "item" + i, (string)null);
			m_itemFails[i].OnEnable(m_wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(string rank, int[][] winAwardList, int[][] failAwardList)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		if (rank.Contains("~"))
		{
			((VisualElement)m_lbRank).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			((TextElement)m_lbRank).text = rank;
		}
		else
		{
			zbSelf.Instance.rankHandler.InitRank(m_lbRank, Extensions.ToInt32(rank, 0));
		}
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_itemWins[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_itemFails[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int[][] array = winAwardList;
		foreach (int[] array2 in array)
		{
			if (num < m_itemWins.Length)
			{
				((VisualElement)m_itemWins[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_itemWins[num].Refresh(new zbItemData((EResourceType)array2[0], array2[1], array2[2]));
			}
			num++;
		}
		int num2 = 0;
		array = failAwardList;
		foreach (int[] array3 in array)
		{
			if (num2 < m_itemWins.Length)
			{
				((VisualElement)m_itemFails[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_itemFails[num2].Refresh(new zbItemData((EResourceType)array3[0], array3[1], array3[2]));
			}
			num2++;
		}
	}
}
