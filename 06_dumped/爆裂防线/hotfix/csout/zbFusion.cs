using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFusion : zbBaseWnd
{
	private zbFusionTopPanel m_topPanel = new zbFusionTopPanel();

	private VisualElement m_mapLayer;

	private Label m_lbLevelUp;

	private Label m_lbClean;

	private Button m_btnTip;

	private Label m_lbTipDesc;

	private AutoHideTipController m_tipController;

	private Button m_btnClose;

	private Button m_btnAward;

	private Button m_btnRank;

	private VisualElement m_notifyAward;

	public Dictionary<int, Dictionary<int, zbFusionItem>> data = new Dictionary<int, Dictionary<int, zbFusionItem>>();

	public Dictionary<int, float> pointX = new Dictionary<int, float>();

	public Dictionary<int, float> pointY = new Dictionary<int, float>();

	private Dictionary<string, VisualElement> m_colMask = new Dictionary<string, VisualElement>();

	private Dictionary<string, VisualElement> m_rowMask = new Dictionary<string, VisualElement>();

	private Dictionary<string, VisualElement> m_holeMask = new Dictionary<string, VisualElement>();

	private List<Vector2Int> m_nodeList = new List<Vector2Int>();

	private FusionInfo m_fusionInfo;

	private JObject m_fusinbasetable;

	private List<int> m_cleanScores;

	public zbFusionLevelCsvTable fusionLevelCsvTable;

	private Coroutine m_randItemCoroutine;

	private Coroutine m_levelUpCoroutine;

	private int m_commitTm = 1;

	protected override void OnEnable()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_topPanel", (string)null);
		m_topPanel.OnEnable(this, root);
		m_mapLayer = UQueryExtensions.Q<VisualElement>(m_RootElement, "mapLayer", (string)null);
		m_lbLevelUp = UQueryExtensions.Q<Label>(m_RootElement, "lb_levelup", (string)null);
		((VisualElement)m_lbLevelUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_mapLayer.Clear();
		data.Clear();
		pointX.Clear();
		pointY.Clear();
		for (int i = 1; i <= 5; i++)
		{
			Dictionary<int, zbFusionItem> dictionary = new Dictionary<int, zbFusionItem>();
			for (int j = 1; j <= 5; j++)
			{
				zbFusionItem zbFusionItem2 = new zbFusionItem(this, i, j);
				((VisualElement)zbFusionItem2).style.left = StyleLength.op_Implicit((float)(i * 119 - 50 - 55));
				((VisualElement)zbFusionItem2).style.bottom = StyleLength.op_Implicit((float)(j * 120 - 54 - 58 + 550));
				dictionary[j] = zbFusionItem2;
				m_mapLayer.Add((VisualElement)(object)zbFusionItem2);
				pointY[j] = j * 120 - 54 - 58;
			}
			data[i] = dictionary;
			pointX[i] = i * 119 - 50 - 55;
		}
		m_colMask.Clear();
		for (int k = 1; k <= 4; k++)
		{
			for (int l = 1; l <= 5; l++)
			{
				float num = 24f;
				float num2 = 116f;
				VisualElement val = CreateVisualElement("ui/image/operate/fusion/colcolor_2.jpg", num, num2);
				m_colMask[string.Format("{0}{1}{2}{3}", new object[4]
				{
					k,
					k + 1,
					l,
					l
				})] = val;
				val.style.left = StyleLength.op_Implicit((float)(k * 119 + 10) - num / 2f);
				val.style.bottom = StyleLength.op_Implicit((float)(l * 120 - 54) - num2 / 2f);
				val.pickingMode = (PickingMode)0;
				int clickX = k;
				int clickY = l;
				((CallbackEventHandler)val).RegisterCallback<ClickEvent>((EventCallback<ClickEvent>)delegate
				{
					OnColMaskClick(clickX, clickY);
				}, (TrickleDown)0);
				m_mapLayer.Add(val);
			}
		}
		m_rowMask.Clear();
		for (int num3 = 1; num3 <= 5; num3++)
		{
			for (int num4 = 1; num4 <= 4; num4++)
			{
				float num5 = 110f;
				float num6 = 28f;
				VisualElement val2 = CreateVisualElement("ui/image/operate/fusion/rowcolor_2.jpg", num5, num6);
				m_rowMask[string.Format("{0}{1}{2}{3}", new object[4]
				{
					num3,
					num3,
					num4,
					num4 + 1
				})] = val2;
				val2.style.left = StyleLength.op_Implicit((float)(num3 * 119 - 50) - num5 / 2f);
				val2.style.bottom = StyleLength.op_Implicit((float)(num4 * 120 + 10) - num6 / 2f);
				val2.pickingMode = (PickingMode)0;
				int clickX2 = num3;
				int clickY2 = num4 + 1;
				((CallbackEventHandler)val2).RegisterCallback<ClickEvent>((EventCallback<ClickEvent>)delegate
				{
					OnRowMaskClick(clickX2, clickY2);
				}, (TrickleDown)0);
				m_mapLayer.Add(val2);
			}
		}
		m_holeMask.Clear();
		for (int num7 = 1; num7 <= 4; num7++)
		{
			for (int num8 = 1; num8 <= 4; num8++)
			{
				float num9 = 24f;
				float num10 = 28f;
				VisualElement val3 = CreateVisualElement("ui/image/operate/fusion/hole_1.jpg", num9, num10);
				m_holeMask[$"{num7}{num8}"] = val3;
				val3.style.left = StyleLength.op_Implicit((float)(num7 * 119 + 10) - num9 / 2f);
				val3.style.bottom = StyleLength.op_Implicit((float)(num8 * 120 + 10) - num10 / 2f);
				m_mapLayer.Add(val3);
			}
		}
		m_lbClean = UQueryExtensions.Q<Label>(m_RootElement, "lb_clean", (string)null);
		m_btnTip = UQueryExtensions.Q<Button>(m_RootElement, "btn_tipDesc", (string)null);
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_tipDesc", (string)null);
		m_tipController = new AutoHideTipController(m_uiEvent, m_btnTip, m_lbTipDesc);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_award", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_notifyAward = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnAward, (Vector2?)new Vector2(-5f, -3f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action)OnAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_FusionAwardList, RefreshNotify);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, RefreshNotify);
		InitRecords();
	}

	private void InitRecords()
	{
		m_fusinbasetable = zbDataConfig.Instance.fusionBaseJsonTable.GetRecord();
		m_cleanScores = m_fusinbasetable["cleanScore"].ToObject<List<int>>();
		fusionLevelCsvTable = zbDataConfig.Instance.fusionLevelCsvTable;
	}

	private VisualElement CreateVisualElement(string path, float width, float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		VisualElement val = new VisualElement();
		val.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val.style.width = StyleLength.op_Implicit(width);
		val.style.height = StyleLength.op_Implicit(height);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(path);
		val.style.backgroundImage = new StyleBackground(val2);
		return val;
	}

	public override void Refresh()
	{
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		m_fusionInfo = zbServerData.sGameData.OperateInfo.FusionInfo;
		if (zbSelf.Instance.operateHandler.fusionData.Count == 0)
		{
			if (m_fusinbasetable != null)
			{
				m_fusionInfo.FusionBase.Clean = Extensions.Value<int>((IEnumerable<JToken>)m_fusinbasetable["cleanBase"]);
			}
			m_fusionInfo.FusionBase.BestScore = 0;
			m_fusionInfo.FusionBase.Score = 0;
			for (int i = 1; i <= 5; i++)
			{
				for (int j = 1; j <= 5; j++)
				{
					data[i][j].Reset();
					(int, int) tuple = RandomCurItem(0, i, j);
					zbFusionLevelCsvRecord recordByKey = fusionLevelCsvTable.GetRecordByKey(tuple.Item1);
					data[i][j].Refresh(tuple.Item1, tuple.Item2, recordByKey.exp);
					data[i][j].EffectNode(j);
				}
			}
			CommitData();
		}
		else
		{
			foreach (List<int> fusionDatum in zbSelf.Instance.operateHandler.fusionData)
			{
				int num = fusionDatum[0];
				int num2 = fusionDatum[1];
				int curExp = fusionDatum[2];
				int curLevel = fusionDatum[3];
				int curColor = fusionDatum[4];
				((VisualElement)data[num][num2]).style.left = StyleLength.op_Implicit((float)(num * 119 - 50 - 55));
				((VisualElement)data[num][num2]).style.bottom = StyleLength.op_Implicit((float)(num2 * 120 - 54 - 58 + 550));
				data[num][num2].Refresh(curLevel, curColor, curExp);
				data[num][num2].EffectNode(num2);
			}
		}
		m_topPanel.Refresh(m_fusionInfo);
		((MonoBehaviour)this).StartCoroutine(DelayRefreshRowCol());
		RefreshNotify();
	}

	private IEnumerator DelayRefreshRowCol()
	{
		ResetRowCol();
		yield return (object)new WaitForSeconds(0.2f);
		RefreshRowCol();
	}

	private void OnColMaskClick(int x, int y)
	{
		if (data.ContainsKey(x) && data[x].ContainsKey(y) && data[x][y].curLevel > 0)
		{
			CaclNodeExp(x, y);
		}
	}

	private void OnRowMaskClick(int x, int y)
	{
		if (data.ContainsKey(x) && data[x].ContainsKey(y) && data[x][y].curLevel > 0)
		{
			CaclNodeExp(x, y);
		}
	}

	public void RandItem(int maxLevel)
	{
		if (m_randItemCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_randItemCoroutine);
			m_randItemCoroutine = null;
		}
		m_randItemCoroutine = ((MonoBehaviour)this).StartCoroutine(RandItemCoroutine(maxLevel));
	}

	private IEnumerator RandItemCoroutine(int maxLevel)
	{
		for (int i = 1; i <= 5; i++)
		{
			for (int j = 1; j <= 5; j++)
			{
				if (data[i][j].curLevel == 0)
				{
					(int, int) tuple = RandomCurItem(maxLevel, i, j);
					zbFusionLevelCsvRecord recordByKey = fusionLevelCsvTable.GetRecordByKey(tuple.Item1);
					data[i][j].Refresh(tuple.Item1, tuple.Item2, recordByKey.exp);
					data[i][j].EffectNode(j);
				}
			}
		}
		yield return (object)new WaitForSeconds(0.2f);
		ResetRowCol();
		RefreshRowCol();
	}

	private void TryCleanBlock(int x, int y, int maxlevel)
	{
		if (m_fusionInfo.FusionBase.Clean <= 0)
		{
			return;
		}
		string message = zbUtil.Language("lang.10322");
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), message, delegate(bool result)
		{
			if (result)
			{
				m_fusionInfo.FusionBase.Clean--;
				data[x][y].Reset();
				FillOtherItem();
				RandItem(maxlevel);
				zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_FUSION);
			}
		});
	}

	public void CaclNodeExp(int x, int y)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (KeyValuePair<int, Dictionary<int, zbFusionItem>> datum in data)
		{
			foreach (KeyValuePair<int, zbFusionItem> item in datum.Value)
			{
				zbFusionItem value = item.Value;
				if (num < value.curLevel)
				{
					num = value.curLevel;
				}
			}
		}
		if (data[x][y].curColor > 5)
		{
			TryCleanBlock(x, y, num);
			return;
		}
		m_nodeList.Clear();
		m_nodeList.Add(new Vector2Int(x, y));
		IterFunc(x, y);
		int num2 = 0;
		foreach (Vector2Int node in m_nodeList)
		{
			Vector2Int current = node;
			int x2 = ((Vector2Int)(ref current)).x;
			int y2 = ((Vector2Int)(ref current)).y;
			num2 += data[x2][y2].curExp;
			if (x2 != x || y2 != y)
			{
				data[x2][y2].Reset();
				SafeSetNodeVisible(m_colMask, string.Format("{0}{1}{2}{3}", new object[4]
				{
					x2,
					x2 + 1,
					y2,
					y2
				}), visible: false);
				SafeSetNodeVisible(m_colMask, string.Format("{0}{1}{2}{3}", new object[4]
				{
					x2 - 1,
					x2,
					y2,
					y2
				}), visible: false);
				SafeSetNodeVisible(m_rowMask, string.Format("{0}{1}{2}{3}", new object[4]
				{
					x2,
					x2,
					y2,
					y2 + 1
				}), visible: false);
				SafeSetNodeVisible(m_rowMask, string.Format("{0}{1}{2}{3}", new object[4]
				{
					x2,
					x2,
					y2 - 1,
					y2
				}), visible: false);
				SafeSetNodeVisible(m_holeMask, $"{x2 - 1}{y2}", visible: false);
				SafeSetNodeVisible(m_holeMask, $"{x2}{y2}", visible: false);
				SafeSetNodeVisible(m_holeMask, $"{x2}{y2 - 1}", visible: false);
			}
		}
		if (data[x][y].curColor < 5)
		{
			num2 = Math.Min(num2, fusionLevelCsvTable.GetRecordByKey(10).exp);
		}
		if (num2 == data[x][y].curExp)
		{
			TryCleanBlock(x, y, num);
			return;
		}
		zbFusionLevelCsvRecord levelRecordByExp = zbFusionUtil.GetLevelRecordByExp(num2);
		int curColor = data[x][y].curColor;
		if (levelRecordByExp.lv == 10)
		{
			curColor = 5;
		}
		else if (levelRecordByExp.lv > 10)
		{
			curColor = 6;
		}
		data[x][y].Refresh(levelRecordByExp.lv, curColor, num2);
		FillOtherItem(x, y);
		RandItem(num);
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_FUSION_COMPOSE);
	}

	private void SafeSetNodeVisible(Dictionary<string, VisualElement> dict, string key, bool visible)
	{
		if (dict.TryGetValue(key, out var value))
		{
			SetNodeVisible(value, visible);
		}
	}

	public void SetNodeVisible(VisualElement element, bool visible)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		DisplayStyle val = (DisplayStyle)(!visible);
		if (element != null)
		{
			element.style.display = StyleEnum<DisplayStyle>.op_Implicit(val);
		}
	}

	public void IterFunc(int x, int y)
	{
		CheckAndAddNeighbor(x, y, x + 1, y);
		CheckAndAddNeighbor(x, y, x - 1, y);
		CheckAndAddNeighbor(x, y, x, y + 1);
		CheckAndAddNeighbor(x, y, x, y - 1);
	}

	private void CheckAndAddNeighbor(int originX, int originY, int checkX, int checkY)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (data.ContainsKey(checkX) && data[checkX].ContainsKey(checkY) && data[checkX][checkY].curColor == data[originX][originY].curColor)
		{
			Vector2Int item = default(Vector2Int);
			((Vector2Int)(ref item))._002Ector(checkX, checkY);
			if (!m_nodeList.Contains(item))
			{
				m_nodeList.Add(item);
				IterFunc(checkX, checkY);
			}
		}
	}

	public void FillOtherItem(int curX = -1, int curY = -1)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		for (int i = 1; i <= 5; i++)
		{
			for (int j = 1; j <= 5; j++)
			{
				if (data[i][j].curLevel != 0)
				{
					continue;
				}
				for (int k = j + 1; k <= 5; k++)
				{
					if (data[i][k].curLevel != 0)
					{
						data[i][j].CopyItem(data[i][k]);
						data[i][k].Reset();
						((VisualElement)data[i][j]).style.bottom = StyleLength.op_Implicit(pointY[k]);
						data[i][j].EffectNode(1);
						SafeSetNodeVisible(m_colMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i,
							i + 1,
							k,
							k
						}), visible: false);
						SafeSetNodeVisible(m_colMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i - 1,
							i,
							k,
							k
						}), visible: false);
						SafeSetNodeVisible(m_rowMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i,
							i,
							k,
							k + 1
						}), visible: false);
						SafeSetNodeVisible(m_rowMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i,
							i,
							k - 1,
							k
						}), visible: false);
						SafeSetNodeVisible(m_holeMask, $"{i}{k}", visible: false);
						SafeSetNodeVisible(m_holeMask, $"{i - 1}{k}", visible: false);
						SafeSetNodeVisible(m_holeMask, $"{i}{k - 1}", visible: false);
						SafeSetNodeVisible(m_colMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i,
							i + 1,
							j,
							j
						}), visible: false);
						SafeSetNodeVisible(m_colMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i - 1,
							i,
							j,
							j
						}), visible: false);
						SafeSetNodeVisible(m_rowMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i,
							i,
							j,
							j + 1
						}), visible: false);
						SafeSetNodeVisible(m_rowMask, string.Format("{0}{1}{2}{3}", new object[4]
						{
							i,
							i,
							j - 1,
							j
						}), visible: false);
						SafeSetNodeVisible(m_holeMask, $"{i}{j}", visible: false);
						SafeSetNodeVisible(m_holeMask, $"{i - 1}{j}", visible: false);
						SafeSetNodeVisible(m_holeMask, $"{i}{j - 1}", visible: false);
						if (curX == i && k == curY)
						{
							data[i][j].WeaponScale();
							ShowLevelUp(i, j);
							flag = true;
						}
						break;
					}
				}
			}
		}
		if (curX != -1 && curY != -1 && !flag)
		{
			data[curX][curY].WeaponScale();
			ShowLevelUp(curX, curY);
		}
	}

	public void ShowLevelUp(int x, int y)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbLevelUp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lbLevelUp).text = $"Lv {data[x][y].curLevel}";
		((VisualElement)m_lbLevelUp).schedule.Execute((Action)delegate
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			float width = ((VisualElement)m_lbLevelUp).resolvedStyle.width;
			((VisualElement)m_lbLevelUp).style.left = StyleLength.op_Implicit(pointX[x] + 55f - width / 2f);
		});
		float num = pointY[y] + 28f;
		((VisualElement)m_lbLevelUp).style.bottom = StyleLength.op_Implicit(num);
		((VisualElement)m_lbLevelUp).style.opacity = StyleFloat.op_Implicit(1f);
		if (m_levelUpCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_levelUpCoroutine);
		}
		m_levelUpCoroutine = ((MonoBehaviour)this).StartCoroutine(ShowLevelUpCoroutine(num));
	}

	private IEnumerator ShowLevelUpCoroutine(float startBottom)
	{
		yield return null;
		float endBottom = startBottom + 80f;
		float duration = 1.5f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			float num2 = Mathf.Lerp(startBottom, endBottom, num);
			float num3 = Mathf.Lerp(1f, 0f, num);
			((VisualElement)m_lbLevelUp).style.bottom = StyleLength.op_Implicit(num2);
			((VisualElement)m_lbLevelUp).style.opacity = StyleFloat.op_Implicit(num3);
			yield return null;
		}
	}

	public void ResetRowCol()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<string, VisualElement> item in m_colMask)
		{
			item.Value.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		foreach (KeyValuePair<string, VisualElement> item2 in m_rowMask)
		{
			item2.Value.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		foreach (KeyValuePair<string, VisualElement> item3 in m_holeMask)
		{
			item3.Value.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public void RefreshRowCol()
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		for (int i = 1; i <= 4; i++)
		{
			for (int j = 1; j <= 5; j++)
			{
				if (data[i + 1][j].curColor == data[i][j].curColor && data[i][j].curColor <= 5)
				{
					string resName = $"ui/image/operate/fusion/colcolor_{data[i][j].curColor}.jpg";
					Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName);
					m_colMask[string.Format("{0}{1}{2}{3}", new object[4]
					{
						i,
						i + 1,
						j,
						j
					})].style.backgroundImage = new StyleBackground(val);
					m_colMask[string.Format("{0}{1}{2}{3}", new object[4]
					{
						i,
						i + 1,
						j,
						j
					})].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					flag = true;
				}
			}
		}
		for (int k = 1; k <= 5; k++)
		{
			for (int l = 1; l <= 4; l++)
			{
				if (data[k][l].curColor == data[k][l + 1].curColor && data[k][l + 1].curColor <= 5)
				{
					string resName2 = $"ui/image/operate/fusion/rowcolor_{data[k][l].curColor}.jpg";
					Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName2);
					m_rowMask[string.Format("{0}{1}{2}{3}", new object[4]
					{
						k,
						k,
						l,
						l + 1
					})].style.backgroundImage = new StyleBackground(val2);
					m_rowMask[string.Format("{0}{1}{2}{3}", new object[4]
					{
						k,
						k,
						l,
						l + 1
					})].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					flag = true;
				}
			}
		}
		for (int m = 1; m <= 4; m++)
		{
			for (int n = 1; n <= 4; n++)
			{
				if (data[m][n].curColor == data[m][n + 1].curColor && data[m + 1][n].curColor == data[m][n].curColor && data[m][n].curColor == data[m + 1][n + 1].curColor && data[m][n + 1].curColor <= 5)
				{
					string resName3 = $"ui/image/operate/fusion/hole_{data[m][n].curColor}.jpg";
					Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName3);
					m_holeMask[$"{m}{n}"].style.backgroundImage = new StyleBackground(val3);
					m_holeMask[$"{m}{n}"].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
			}
		}
		CaclScore();
		((TextElement)m_lbClean).text = zbUtil.Language("lang.11090") + ": " + m_fusionInfo.FusionBase.Clean;
		int num = m_cleanScores.FirstOrDefault((int v) => v > m_fusionInfo.FusionBase.BestScore);
		((TextElement)m_lbTipDesc).text = string.Format(zbUtil.Language("lang.11091"), zbUtil.ConvertToTBMK(num)) + "\n" + zbUtil.Language("lang.11092");
		m_topPanel.Refresh(m_fusionInfo);
		if (flag)
		{
			return;
		}
		if (m_fusionInfo.FusionBase.Clean > 0)
		{
			zbUIMgr.Instance.ShowMessageConfirm(zbUtil.Language("lang.10323"), null);
			return;
		}
		string message = zbUtil.Language("lang.10324") + m_fusionInfo.FusionBase.Score;
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), message, delegate(bool result)
		{
			if (result)
			{
				zbSelf.Instance.operateHandler.fusionData.Clear();
				zbWsMsgHandler.Instance.Operate_FusionUpdate_Req();
				Hide();
				zbUIMgr.Instance.ShowFusionEnter();
			}
		});
	}

	public void CaclScore(bool checkDailyState = true)
	{
		int num = 0;
		foreach (KeyValuePair<int, Dictionary<int, zbFusionItem>> datum in data)
		{
			foreach (KeyValuePair<int, zbFusionItem> item in datum.Value)
			{
				zbFusionLevelCsvRecord recordByKey = fusionLevelCsvTable.GetRecordByKey(item.Value.curLevel);
				num += recordByKey.score;
			}
		}
		if (m_fusionInfo == null)
		{
			return;
		}
		m_fusionInfo.FusionBase.Score = num;
		foreach (int cleanScore in m_cleanScores)
		{
			if (m_fusionInfo.FusionBase.BestScore < cleanScore && m_fusionInfo.FusionBase.Score >= cleanScore)
			{
				m_fusionInfo.FusionBase.Clean++;
			}
		}
		m_fusionInfo.FusionBase.DailyBestScore = Math.Max(m_fusionInfo.FusionBase.Score, m_fusionInfo.FusionBase.DailyBestScore);
		m_fusionInfo.FusionBase.BestScore = Math.Max(m_fusionInfo.FusionBase.Score, m_fusionInfo.FusionBase.BestScore);
		m_fusionInfo.FusionBase.HistoryScore = Math.Max(m_fusionInfo.FusionBase.BestScore, m_fusionInfo.FusionBase.HistoryScore);
		if (!checkDailyState)
		{
			return;
		}
		foreach (OperateTargetDetail dailyAward in m_fusionInfo.FusionBase.DailyAwardList)
		{
			if (dailyAward.State == ECommonState.No && m_fusionInfo.FusionBase.Score >= dailyAward.Target)
			{
				CommitData();
				break;
			}
		}
	}

	public void CommitData()
	{
		CaclScore(checkDailyState: false);
		zbSelf.Instance.operateHandler.fusionData.Clear();
		foreach (KeyValuePair<int, Dictionary<int, zbFusionItem>> datum in data)
		{
			foreach (KeyValuePair<int, zbFusionItem> item2 in datum.Value)
			{
				zbFusionItem value = item2.Value;
				List<int> item = new List<int> { value.x, value.y, value.curExp, value.curLevel, value.curColor };
				zbSelf.Instance.operateHandler.fusionData.Add(item);
			}
		}
		zbWsMsgHandler.Instance.Operate_FusionUpdate_Req();
	}

	public (int level, int color) RandomCurItem(int level, int x, int y)
	{
		int num = 1;
		int num2 = 30;
		if (fusionLevelCsvTable != null && fusionLevelCsvTable.GetRecords() != null)
		{
			num2 = fusionLevelCsvTable.GetRecords().records.Count;
		}
		for (int i = 1; i <= num2 && level >= fusionLevelCsvTable.GetRecordByKey(i).filltime; i++)
		{
			num = i;
		}
		int num3 = 3;
		if (m_fusionInfo != null && Extensions.Value<int>((IEnumerable<JToken>)m_fusinbasetable["whiteScore"]) <= m_fusionInfo.FusionBase.Score)
		{
			num3 = 4;
		}
		int[] array = new int[5];
		if (data.ContainsKey(x - 1) && data[x - 1].ContainsKey(y) && data[x - 1][y].curLevel > 0 && data[x - 1][y].curColor <= 4)
		{
			array[data[x - 1][y].curColor] = array[data[x - 1][y].curColor] + 1;
		}
		else if (data.ContainsKey(x - 1) && data[x - 1].ContainsKey(y - 1) && data[x - 1][y - 1].curLevel > 0 && data[x - 1][y - 1].curColor <= 4)
		{
			array[data[x - 1][y - 1].curColor] = array[data[x - 1][y - 1].curColor] + 1;
		}
		else if (data.ContainsKey(x + 1) && data[x + 1].ContainsKey(y) && data[x + 1][y].curLevel > 0 && data[x + 1][y].curColor <= 4)
		{
			array[data[x + 1][y].curColor] = array[data[x + 1][y].curColor] + 1;
		}
		int[] array2 = new int[5];
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		for (int j = 1; j <= num3; j++)
		{
			int num7 = Extensions.Value<int>((IEnumerable<JToken>)m_fusinbasetable["easy"]);
			array2[j] = array[j] * num7 + 100;
			num4 = num4 + array[j] * num7 + 100;
		}
		int num8 = Rand.Range(1, num4 + 1);
		int item = 1;
		for (int k = 1; k <= num3; k++)
		{
			num6 += array2[k];
			if (num6 == num4 && num8 == num6)
			{
				item = k;
				break;
			}
			if (num5 < num8 && num8 <= num6)
			{
				item = k;
				break;
			}
			num5 += array2[k];
		}
		return (level: Rand.Range(1, num + 1), color: item);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		m_topPanel.RefresLeftTime();
		m_commitTm++;
		if (m_commitTm >= 60)
		{
			m_commitTm = 1;
			CommitData();
		}
	}

	private void OnClose()
	{
		CommitData();
		Hide();
		zbUIMgr.Instance.ShowFusionEnter();
	}

	private void OnAward()
	{
		zbUIMgr.Instance.ShowFusionAward();
	}

	private void OnRank()
	{
		zbWsMsgHandler.Instance.Operate_FusionFriendsRank_Req();
	}

	private void RefreshNotify(object data = null)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.operateHandler.HasFusionTargetNotify(isHoliday: true) || zbSelf.Instance.operateHandler.HasFusionTargetNotify();
		m_notifyAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (data != null)
		{
			m_fusionInfo.FusionBase.DailyAwardList.Clear();
			m_fusionInfo.FusionBase.DailyAwardList.AddRange((IEnumerable<OperateTargetDetail>)zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.DailyAwardList);
			m_topPanel.Refresh(m_fusionInfo);
		}
	}
}
