using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonLogItem : zbViewBaseItem
{
	private zbDungeonLogWnd m_parent;

	private Label m_lbContent;

	public zbDungeonLogItem(VisualTreeAsset tAsset, zbDungeonLogWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_parent = parent;
		m_lbContent = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_content", (string)null);
	}

	public void Init(DungeonLogDetail dungeonLogDetail, int index)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (index % 2 == 0)
		{
			((VisualElement)this).style.backgroundColor = StyleColor.op_Implicit(new Color(9f / 85f, 0.08627451f, 9f / 85f));
		}
		if (dungeonLogDetail != null)
		{
			string text = zbUtil.FormatUnixTimeToUtcString(dungeonLogDetail.LogTm);
			string text2 = ((dungeonLogDetail.EventId == 0 && dungeonLogDetail.EventType == EDungeonEventType.RiotFight) ? zbUtil.Language("lang.10185") : ((dungeonLogDetail.EventType != EDungeonEventType.BossFight) ? zbDataConfig.Instance.dungeonEventCsvTable.GetRecordByKey(dungeonLogDetail.EventId).desc : zbUtil.Language("lang.10186")));
			((TextElement)m_lbContent).text = text + " " + text2;
			zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		}
	}
}
