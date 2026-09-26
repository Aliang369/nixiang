using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossPeakRankItem : zbViewBaseItem
{
	private VisualElement m_bg;

	private Label m_lbRank;

	private zbCrossPeakRankWnd m_wndParent;

	private zbPeakRankAwardCsvRecord m_rankRecord;

	private zbItem[] m_awards = new zbItem[5];

	public zbCrossPeakRankItem(VisualTreeAsset tAsset, zbCrossPeakRankWnd wndParent)
	{
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_awards[i].OnEnable(m_wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbPeakRankAwardCsvRecord rankRecord, zbPeakRankAwardCsvRecord nextRankRecord)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		m_rankRecord = rankRecord;
		if (m_rankRecord.rank <= 3)
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossPeak/crossPeakRank_bg" + m_rankRecord.rank + ".png"));
			zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_rankRecord.rank);
		}
		else
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossPeak/crossPeakRank_bg.png"));
			((VisualElement)m_lbRank).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			if (nextRankRecord != null)
			{
				if (nextRankRecord.rank - 1 > m_rankRecord.rank)
				{
					((TextElement)m_lbRank).text = m_rankRecord.rank + "-" + (nextRankRecord.rank - 1);
				}
				else
				{
					((TextElement)m_lbRank).text = m_rankRecord.rank.ToString();
				}
			}
			else if (rankRecord.rank < 50)
			{
				((TextElement)m_lbRank).text = m_rankRecord.rank + "-50";
			}
			else
			{
				((TextElement)m_lbRank).text = m_rankRecord.rank.ToString();
			}
		}
		((VisualElement)m_lbRank).style.scale = StyleScale.op_Implicit(new Vector2(1.2f, 1.2f));
		int[][] awardList = rankRecord.awardList;
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < awardList.Length;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				m_awards[i].Refresh(new zbItemData((EResourceType)awardList[i][0], awardList[i][1], awardList[i][2]));
			}
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
