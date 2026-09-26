using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaRankAwardItem : zbViewBaseItem
{
	private VisualElement m_bg;

	private Label m_lbRank;

	private zbArenaRankAwardWnd m_wndParent;

	private zbArenaDailyAwardCsvRecord m_rankRecord;

	private zbItem[] m_awards = new zbItem[5];

	public zbArenaRankAwardItem(VisualTreeAsset tAsset, zbArenaRankAwardWnd wndParent)
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

	public void Refresh(zbArenaDailyAwardCsvRecord rankRecord)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
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
			((TextElement)m_lbRank).text = m_rankRecord.showRank;
		}
		((VisualElement)m_lbRank).style.scale = StyleScale.op_Implicit(new Vector2(1.2f, 1.2f));
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < 1;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)m_awards[i]).style.scale = StyleScale.op_Implicit(new Vector2(0.8f, 0.8f));
			if (flag)
			{
				m_awards[i].Refresh(new zbItemData(EResourceType.Money, 14L, rankRecord.rankMoney));
			}
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
