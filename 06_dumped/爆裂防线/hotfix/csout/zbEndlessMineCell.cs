using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEndlessMineCell : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbEndlessMineCell>
	{
	}

	private VisualElement m_node;

	private VisualElement m_spPlot;

	private VisualElement m_spColor;

	private VisualElement m_spVisible;

	private VisualElement m_nodeProgress;

	public EndlessMineMapCellDetail m_cellDetail;

	public int m_row;

	private Vector2 m_convertPos;

	private Vector2 m_contentSize;

	private static Dictionary<int, string> PlotSpritePath = new Dictionary<int, string>
	{
		{ 1, "ui/image/operate/endlessMine/endlessMine_plot1.png" },
		{ 2, "ui/image/operate/endlessMine/endlessMine_plot2.png" },
		{ 3, "ui/image/operate/endlessMine/endlessMine_plot3.png" },
		{ 4, "ui/image/operate/endlessMine/endlessMine_plot4.png" },
		{ 5, "ui/image/operate/endlessMine/endlessMine_plot5.png" },
		{ 6, "ui/image/operate/endlessMine/endlessMine_plot6.png" },
		{ 7, "ui/image/operate/endlessMine/endlessMine_plot7.png" },
		{ 8, "ui/image/operate/endlessMine/endlessMine_plot8.png" },
		{ 9, "ui/image/operate/endlessMine/endlessMine_plot9.png" },
		{ 10, "ui/image/operate/endlessMine/endlessMine_plot10.png" }
	};

	public zbEndlessMineCell()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		m_contentSize = new Vector2(90f, 90f);
		base._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/endlessMine/tb_endlessMineCell.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(VisualElement root)
	{
		m_node = root;
		m_spPlot = UQueryExtensions.Q<VisualElement>(root, "plot", (string)null);
		m_spColor = UQueryExtensions.Q<VisualElement>(root, "state", (string)null);
		m_spVisible = UQueryExtensions.Q<VisualElement>(root, "opacity", (string)null);
		m_nodeProgress = UQueryExtensions.Q<VisualElement>(root, "progress1", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void SetNodePosConvert(float y)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_convertPos.x = m_node.resolvedStyle.translate.x - m_contentSize.x / 2f;
		m_convertPos.y = y;
	}

	public void Refresh(EndlessMineMapCellDetail data, int row = 0)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		m_cellDetail = data;
		m_row = row;
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spPlot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(data == null));
		m_spColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spVisible.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeProgress.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_cellDetail == null)
		{
			return;
		}
		if (m_cellDetail.Plot != EEndlessMinePlotType.EmptyValue)
		{
			string resName = PlotSpritePath[(int)m_cellDetail.Plot];
			m_spPlot.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
			if (m_cellDetail.Plot == EEndlessMinePlotType.Stone && m_cellDetail.Prog > 0f)
			{
				m_spPlot.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/endlessMine/endlessMine_plot21.png"));
				m_nodeProgress.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
		m_spPlot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_cellDetail.Plot == EEndlessMinePlotType.EmptyValue));
		m_spVisible.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_cellDetail.Visible != EBoolState.False) ? 1 : 0));
	}

	public void SetSelectState(int type)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (type == 0)
		{
			m_spColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_spColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		string resName = ((type == 2) ? "ui/image/operate/endlessMine/endlessMine_bgRed.png" : "ui/image/operate/endlessMine/endlessMine_bgGreen.png");
		m_spColor.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
	}
}
