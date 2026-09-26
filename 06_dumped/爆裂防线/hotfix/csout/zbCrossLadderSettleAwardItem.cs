using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderSettleAwardItem : zbViewBaseItem
{
	private VisualElement m_spriteLadder;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbLadder;

	private VisualElement m_spriteArrow;

	private VisualElement m_spriteCur;

	private VisualElement m_spriteCell;

	private Label m_lbScore;

	private zbItem[] m_items = new zbItem[3];

	private Label m_lbScoreName;

	private zbLadderCsvRecord m_record;

	public zbCrossLadderSettleAwardItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/crossLadder/tb_crossLadderSettleAward.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbCrossLadderSettleAwardWnd wndParent)
	{
		m_spriteLadder = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_ladder", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "star" + i, (string)null);
		}
		m_lbLadder = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_ladder", (string)null);
		m_spriteArrow = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "arrow", (string)null);
		m_spriteCur = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_cur", (string)null);
		m_spriteCell = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_score", (string)null);
		m_lbScoreName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_scoreName", (string)null);
		((TextElement)m_lbScoreName).text = zbUtil.Language("lang.10900");
		for (int j = 0; j < 3; j++)
		{
			m_items[j] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + j, (string)null);
			m_items[j].OnEnable(wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbLadderCsvRecord record)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_spriteLadder.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(record.icon));
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((record.id == zbSelf.Instance.crossLadderHandler.maxLadderId) ? 1 : 0));
			if (record.id != zbSelf.Instance.crossLadderHandler.maxLadderId)
			{
				if (i < record.star)
				{
					m_spriteStars[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_star1.png"));
				}
				else
				{
					m_spriteStars[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_star0.png"));
				}
			}
		}
		((TextElement)m_lbLadder).text = record.name;
		m_spriteArrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((record.id >= zbSelf.Instance.crossLadderHandler.maxLadderId) ? 1 : 0));
		((TextElement)m_lbScore).text = record.score.ToString();
		for (int j = 0; j < 3; j++)
		{
			if (record.settleAward[j] != null)
			{
				((VisualElement)m_items[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[j].Refresh(new zbItemData((EResourceType)record.settleAward[j][0], record.settleAward[j][1], record.settleAward[j][2]));
			}
			else
			{
				((VisualElement)m_items[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	public void Refresh()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (m_record.id == zbServerData.sGameData.CrossLadderInfo.AchieveInfo.BestLadderId)
		{
			m_spriteCur.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteCell.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_settleAward_cell1.png"));
		}
		else
		{
			m_spriteCur.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteCell.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_settleAward_cell.png"));
		}
	}
}
