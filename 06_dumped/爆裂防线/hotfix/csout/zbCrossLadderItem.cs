using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbCrossLadderItem>
	{
	}

	private VisualElement m_nodeProgress;

	private VisualElement m_spriteProgress;

	private VisualElement m_spritePoint0;

	private Button m_btnLast;

	private Label m_lbScore;

	private zbItem m_item = new zbItem();

	private int m_ladderId;

	public zbCrossLadderItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/crossLadder/tb_crossLadder.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbCrossLadderWnd wndParent)
	{
		m_nodeProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress_bg", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_spritePoint0 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_point0", (string)null);
		m_btnLast = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_last", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLast, (Action)OnLast, "", (TrickleDown)0);
		m_lbScore = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_score", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(wndParent);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int ladderId, bool isGrandPrize = false)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		m_ladderId = ladderId;
		zbLadderCsvRecord recordByKey = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(ladderId);
		if (recordByKey == null)
		{
			return;
		}
		((TextElement)m_lbScore).text = recordByKey.score.ToString();
		int[][] ladderTargetAwards = zbSelf.Instance.crossLadderHandler.GetLadderTargetAwards(ladderId);
		int[] award = ladderTargetAwards[0];
		if (award == null)
		{
			return;
		}
		m_item.Refresh(new zbItemData((EResourceType)award[0], award[1], award[2]));
		zbLadderTargetAwardCsvRecord recordByKey2 = zbDataConfig.Instance.ladderTargetAwardCsvTable.GetRecordByKey(ladderId);
		m_item.SetRareVisible(recordByKey2 != null && recordByKey2.reward > 0);
		bool isGetted = ladderId <= zbServerData.sGameData.CrossLadderInfo.SelfInfo.TargetAward;
		m_item.SetGettedVisible(isGetted);
		if (isGrandPrize)
		{
			m_nodeProgress.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spritePoint0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_nodeProgress.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spritePoint0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(ladderId <= 2));
			float num = 1f;
			if (!isGetted)
			{
				zbLadderCsvRecord recordByKey3 = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(ladderId - 1);
				num = ((recordByKey3 == null || zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score <= recordByKey3.score) ? 0f : Mathf.Min((float)(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score - recordByKey3.score) / (float)(recordByKey.score - recordByKey3.score), 1f));
			}
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 110f, (LengthUnit)0));
		}
		bool canGet = !isGetted && ladderId <= zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId;
		m_item.SetSpineVisible(canGet);
		m_item.SetTouchCallback(delegate
		{
			if (!canGet | isGetted)
			{
				zbUIMgr.Instance.ShowTip((EResourceType)award[0], award[1], 0L);
			}
			else
			{
				zbWsMsgHandler.Instance.CrossLadder_TargetAward_Req();
			}
		});
	}

	public void OnLast()
	{
		int num = m_ladderId - 1;
		int[] array = zbSelf.Instance.crossLadderHandler.GetLadderTargetAwards(num)[0];
		if (array != null)
		{
			bool flag = num <= zbServerData.sGameData.CrossLadderInfo.SelfInfo.TargetAward;
			if ((flag || num > zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId) | flag)
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], 0L);
			}
			else
			{
				zbWsMsgHandler.Instance.CrossLadder_TargetAward_Req();
			}
		}
	}
}
