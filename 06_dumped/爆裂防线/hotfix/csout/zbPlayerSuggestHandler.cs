using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerSuggestHandler
{
	public AuxPlayerSuggestAck playerSuggestAck;

	public void Init()
	{
	}

	public void InitPlayerInfo(PlayerSuggestDetail m_detail, VisualElement m_playerInfo)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_detail.TipsInfo.RoleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "header", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_detail.TipsInfo.RoleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null)).text = m_detail.TipsInfo.RoleBaseInfo.Level.ToString();
	}
}
