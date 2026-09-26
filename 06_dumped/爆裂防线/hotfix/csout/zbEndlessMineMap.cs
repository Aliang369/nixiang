using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEndlessMineMap
{
	private class BombEffectData
	{
		public Vector2 pos;

		public zbEndlessMineCell cell;
	}

	private const int INIT_ROW_NUM = 21;

	private const float ROW_HEIGHT = 90f;

	private zbEndlessMineWnd m_wndParent;

	private VisualElement m_root;

	public ScrollView m_scrollView;

	private SpineVisualElement m_plotEffectZhakai;

	private SpineVisualElement m_propEffectPickaxe;

	private SpineVisualElement m_propEffectDrill;

	private SpineVisualElement m_propEffectBomb;

	private SpineVisualElement[] m_nituEffects = (SpineVisualElement[])(object)new SpineVisualElement[5];

	private SpineVisualElement[] m_shitouEffects = (SpineVisualElement[])(object)new SpineVisualElement[5];

	private VisualElement m_spritePickProp;

	private zbEndlessMineGetItem[] m_getItems = new zbEndlessMineGetItem[5];

	private List<zbEndlessMineRow> m_rowList = new List<zbEndlessMineRow>();

	private List<zbEndlessMineCell> m_cellList = new List<zbEndlessMineCell>();

	private zbEndlessMineCell m_curCell;

	private zbEndlessMineCell m_lastCell;

	private EPropType m_lastProp;

	private int m_curDepth;

	private Vector2 m_downPos;

	private bool m_isDragging;

	private const float DRAG_THRESHOLD = 10f;

	private bool m_isPlayingEffect;

	private List<BombEffectData> m_bombEffectPos = new List<BombEffectData>();

	private HashSet<zbEndlessMineCell> m_effectCellSet = new HashSet<zbEndlessMineCell>();

	public void OnEnable(zbEndlessMineWnd wndParent, VisualElement rootElement)
	{
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_root = rootElement;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_root, "scrollView", (string)null);
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)2;
		m_scrollView.mode = (ScrollViewMode)0;
		m_plotEffectZhakai = UQueryExtensions.Q<SpineVisualElement>(m_root, "spine_zhakai", (string)null);
		m_propEffectPickaxe = UQueryExtensions.Q<SpineVisualElement>(m_root, "spine_pickaxe", (string)null);
		m_propEffectDrill = UQueryExtensions.Q<SpineVisualElement>(m_root, "spine_drill", (string)null);
		m_propEffectBomb = UQueryExtensions.Q<SpineVisualElement>(m_root, "spine_bomb", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_nituEffects[i] = UQueryExtensions.Q<SpineVisualElement>(m_root, "spine_nitu" + i, (string)null);
			m_shitouEffects[i] = UQueryExtensions.Q<SpineVisualElement>(m_root, "spine_shitou" + i, (string)null);
			m_getItems[i] = UQueryExtensions.Q<zbEndlessMineGetItem>(m_root, "getItem" + i, (string)null);
			m_getItems[i].OnEnable(m_wndParent, this);
		}
		m_spritePickProp = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "pick_prop", (string)null);
		m_spritePickProp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((CallbackEventHandler)m_scrollView).RegisterCallback<WheelEvent>((EventCallback<WheelEvent>)delegate(WheelEvent evt)
		{
			((EventBase)evt).StopImmediatePropagation();
		}, (TrickleDown)1);
		((CallbackEventHandler)m_scrollView).RegisterCallback<PointerDownEvent>((EventCallback<PointerDownEvent>)OnPointerDown, (TrickleDown)1);
		((CallbackEventHandler)m_scrollView).RegisterCallback<PointerMoveEvent>((EventCallback<PointerMoveEvent>)OnPointerMove, (TrickleDown)1);
		((CallbackEventHandler)m_scrollView).RegisterCallback<PointerUpEvent>((EventCallback<PointerUpEvent>)OnPointerUp, (TrickleDown)1);
		InitRows();
	}

	private void InitRows()
	{
		for (int i = 0; i < 21; i++)
		{
			zbEndlessMineRow zbEndlessMineRow2 = new zbEndlessMineRow();
			zbEndlessMineRow2.OnEnable();
			m_rowList.Add(zbEndlessMineRow2);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbEndlessMineRow2);
			zbEndlessMineCell[] cells = zbEndlessMineRow2.m_cells;
			foreach (zbEndlessMineCell item in cells)
			{
				m_cellList.Add(item);
			}
		}
		((VisualElement)m_plotEffectZhakai).BringToFront();
		((VisualElement)m_propEffectPickaxe).BringToFront();
		((VisualElement)m_propEffectDrill).BringToFront();
		((VisualElement)m_propEffectBomb).BringToFront();
		for (int k = 0; k < 5; k++)
		{
			((VisualElement)m_nituEffects[k]).BringToFront();
			((VisualElement)m_shitouEffects[k]).BringToFront();
		}
		for (int l = 0; l < 5; l++)
		{
			((VisualElement)m_getItems[l]).BringToFront();
		}
		m_spritePickProp.BringToFront();
	}

	public void Refresh()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (m_isPlayingEffect || zbSelf.Instance.endlessMineHandler.endlessMineInfo == null)
		{
			return;
		}
		m_curDepth = zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.Depth;
		int num = m_curDepth - 7;
		int num2 = 0;
		foreach (EndlessMineMapRowDetail item in zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineMap)
		{
			if (item.Row >= num && num2 < m_rowList.Count)
			{
				m_rowList[num2].Refresh(item);
				num2++;
			}
		}
		m_scrollView.scrollOffset = Vector2.zero;
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (m_isPlayingEffect)
		{
			((EventBase)evt).StopPropagation();
			return;
		}
		PickCell(Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).position));
		if (m_curCell != null)
		{
			m_downPos = Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).position);
			m_isDragging = false;
			UpdateSelection(Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).position));
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_scrollView, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
			((EventBase)evt).StopPropagation();
		}
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isPlayingEffect && PointerCaptureHelper.HasPointerCapture((IEventHandler)(object)m_scrollView, ((PointerEventBase<PointerMoveEvent>)(object)evt).pointerId))
		{
			if (Vector2.Distance(Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position), m_downPos) > 10f)
			{
				m_isDragging = true;
				PickCell(Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position));
				UpdateSelection(Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position));
			}
			((EventBase)evt).StopPropagation();
		}
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (PointerCaptureHelper.HasPointerCapture((IEventHandler)(object)m_scrollView, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId))
		{
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_scrollView, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
		}
		if (!m_isPlayingEffect)
		{
			if (!m_isDragging)
			{
				PickCell(Vector2.op_Implicit(((PointerEventBase<PointerUpEvent>)(object)evt).position));
			}
			if (m_curCell != null)
			{
				ProcessInteraction();
			}
			CancelSelection();
			m_isDragging = false;
			((EventBase)evt).StopPropagation();
		}
	}

	private void CancelSelection()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_spritePickProp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		ResetAllCellColor();
		m_curCell = null;
	}

	private void PickCell(Vector2 worldPos)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_curCell = null;
		foreach (zbEndlessMineCell cell in m_cellList)
		{
			Rect worldBound = ((VisualElement)cell).worldBound;
			if (((Rect)(ref worldBound)).Contains(worldPos))
			{
				m_curCell = cell;
				break;
			}
		}
	}

	public void SetProp(int propId)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey(propId);
		if (recordByKey != null)
		{
			m_spritePickProp.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
	}

	private void UpdateSelection(Vector2 worldPos)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		ResetAllCellColor();
		m_bombEffectPos.Clear();
		m_effectCellSet.Clear();
		if (m_curCell == null || m_curCell.m_cellDetail == null)
		{
			m_spritePickProp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_spritePickProp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		Rect worldBound = ((VisualElement)m_curCell).worldBound;
		Vector2 val = VisualElementExtensions.WorldToLocal(((VisualElement)m_scrollView).contentContainer, ((Rect)(ref worldBound)).center);
		m_spritePickProp.style.left = StyleLength.op_Implicit(val.x);
		m_spritePickProp.style.top = StyleLength.op_Implicit(val.y);
		switch (m_wndParent.m_selectProp)
		{
		case EPropType.Pickaxe:
			HandlePickaxe();
			break;
		case EPropType.Drill:
			HandleDrill();
			break;
		case EPropType.Bomb:
			HandleBomb();
			break;
		}
	}

	private void ProcessInteraction()
	{
		if (m_curCell == null || m_curCell.m_cellDetail == null || m_curCell.m_row > m_curDepth - 1)
		{
			return;
		}
		m_lastCell = m_curCell;
		m_lastProp = m_wndParent.m_selectProp;
		EndlessMinePropDetail endlessMinePropDetail = zbSelf.Instance.endlessMineHandler.GetEndlessMinePropDetail(m_wndParent.PropTypeIds[m_lastProp]);
		if (endlessMinePropDetail == null || endlessMinePropDetail.Count < 1)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_wndParent.PropTypeIds[m_lastProp], 0L);
			return;
		}
		switch (m_lastProp)
		{
		case EPropType.Pickaxe:
			if (m_curCell.m_cellDetail.Visible == EBoolState.False)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11124"));
				return;
			}
			if (m_curCell.m_cellDetail.Plot == EEndlessMinePlotType.EmptyValue)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11125"));
				return;
			}
			zbWsMsgHandler.Instance.EndlessMine_Dig_Req(m_curCell.m_cellDetail.Index);
			break;
		case EPropType.Drill:
			if (m_curCell.m_cellDetail.Visible == EBoolState.False)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11124"));
				return;
			}
			if (m_curCell.m_cellDetail.Plot != EEndlessMinePlotType.EmptyValue)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11126"));
				return;
			}
			zbWsMsgHandler.Instance.EndlessMine_Drill_Req(m_curCell.m_cellDetail.Index);
			break;
		case EPropType.Bomb:
			if (m_curCell.m_cellDetail.Visible == EBoolState.False)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11124"));
				return;
			}
			if (m_curCell.m_cellDetail.Plot != EEndlessMinePlotType.EmptyValue)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11126"));
				return;
			}
			zbWsMsgHandler.Instance.EndlessMine_Bomb_Req(m_curCell.m_cellDetail.Index);
			break;
		}
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_CLICK);
	}

	private void HandlePickaxe()
	{
		bool flag = m_curCell.m_cellDetail.Visible == EBoolState.True && m_curCell.m_cellDetail.Plot != EEndlessMinePlotType.EmptyValue;
		m_curCell.SetSelectState(flag ? 1 : 2);
	}

	private void HandleDrill()
	{
		int num = ((m_curCell.m_cellDetail.Plot == EEndlessMinePlotType.EmptyValue && m_curCell.m_cellDetail.Visible == EBoolState.True) ? 1 : 2);
		int index = m_curCell.m_cellDetail.Index;
		int row = m_curCell.m_row;
		int num2 = m_curDepth - 1 - row;
		int num3 = index + num2 * 6;
		foreach (zbEndlessMineCell cell in m_cellList)
		{
			if (cell.m_row <= m_curDepth - 1)
			{
				bool flag = false;
				int index2 = cell.m_cellDetail.Index;
				if ((index - index2) % 6 == 0)
				{
					cell.SetSelectState(num);
					flag = true;
				}
				else if (cell.m_row == m_curDepth - 1 && (index2 == num3 - 1 || index2 == num3 + 1))
				{
					cell.SetSelectState(num);
					flag = true;
				}
				if ((num == 1) & flag)
				{
					AddBombEffect(cell);
				}
			}
		}
	}

	private void HandleBomb()
	{
		int num = ((m_curCell.m_cellDetail.Plot == EEndlessMinePlotType.EmptyValue && m_curCell.m_cellDetail.Visible == EBoolState.True) ? 1 : 2);
		int index = m_curCell.m_cellDetail.Index;
		int row = m_curCell.m_row;
		m_curCell.SetSelectState(num);
		foreach (zbEndlessMineCell cell in m_cellList)
		{
			if (cell.m_row > m_curDepth - 1)
			{
				continue;
			}
			bool flag = false;
			int index2 = cell.m_cellDetail.Index;
			int row2 = cell.m_row;
			if (row2 == row)
			{
				int num2 = Mathf.Abs(index2 - index);
				if (num2 == 1 || num2 == 2)
				{
					flag = true;
				}
			}
			else if (row2 == row - 1)
			{
				if (index2 == index - 6 || index2 == index - 7 || index2 == index - 5)
				{
					flag = true;
				}
			}
			else if (row2 == row - 2)
			{
				if (index2 == index - 12)
				{
					flag = true;
				}
			}
			else if (row2 == row + 1)
			{
				if (index2 == index + 6 || index2 == index + 5 || index2 == index + 7)
				{
					flag = true;
				}
			}
			else if (row2 == row + 2 && index2 == index + 12)
			{
				flag = true;
			}
			if (flag)
			{
				cell.SetSelectState(num);
				if (num == 1)
				{
					AddBombEffect(cell);
				}
			}
		}
	}

	private void AddBombEffect(zbEndlessMineCell cell)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (cell != null && !m_effectCellSet.Contains(cell) && cell.m_cellDetail.Plot != EEndlessMinePlotType.EmptyValue)
		{
			m_effectCellSet.Add(cell);
			Rect worldBound = ((VisualElement)cell).worldBound;
			m_bombEffectPos.Add(new BombEffectData
			{
				cell = cell,
				pos = VisualElementExtensions.WorldToLocal(((VisualElement)m_scrollView).contentContainer, ((Rect)(ref worldBound)).center)
			});
		}
	}

	private List<BombEffectData> GetDrillCellsByRow(int idx)
	{
		List<BombEffectData> list = new List<BombEffectData>();
		int num = m_curDepth - 7;
		foreach (BombEffectData bombEffectPo in m_bombEffectPos)
		{
			if (bombEffectPo.cell != null && bombEffectPo.cell.m_cellDetail != null && bombEffectPo.cell.m_row - num == idx - 1)
			{
				list.Add(bombEffectPo);
			}
		}
		return list;
	}

	private void PlaySingleCellEffect(BombEffectData data)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (data != null && data.cell != null)
		{
			zbEndlessMineCell cell = data.cell;
			((VisualElement)m_plotEffectZhakai).style.left = StyleLength.op_Implicit(data.pos.x);
			((VisualElement)m_plotEffectZhakai).style.top = StyleLength.op_Implicit(data.pos.y);
			m_plotEffectZhakai.AnimationState.SetAnimation(0, "animation", false);
			int num = Random.Range(0, 5);
			if (cell.m_cellDetail.Plot == EEndlessMinePlotType.Clod)
			{
				SpineVisualElement obj = m_nituEffects[num];
				((VisualElement)obj).style.left = StyleLength.op_Implicit(data.pos.x);
				((VisualElement)obj).style.top = StyleLength.op_Implicit(data.pos.y);
				obj.AnimationState.SetAnimation(0, "animation", false);
			}
			else
			{
				SpineVisualElement obj2 = m_shitouEffects[num];
				((VisualElement)obj2).style.left = StyleLength.op_Implicit(data.pos.x);
				((VisualElement)obj2).style.top = StyleLength.op_Implicit(data.pos.y);
				obj2.AnimationState.SetAnimation(0, "animation", false);
			}
			((VisualElement)cell).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_BOMB);
		}
	}

	public void ShowPropsUseEffect(object data)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (m_lastCell == null)
		{
			return;
		}
		m_isPlayingEffect = true;
		SpineVisualElement propEffect = GetPropEffect(m_lastProp);
		propEffect.AnimationState.ClearTrack(0);
		float delay = 4f / 15f;
		Rect worldBound = ((VisualElement)m_lastCell).worldBound;
		Vector2 val = VisualElementExtensions.WorldToLocal(((VisualElement)m_scrollView).contentContainer, ((Rect)(ref worldBound)).center);
		if (propEffect != null)
		{
			((VisualElement)propEffect).style.left = StyleLength.op_Implicit(val.x);
			((VisualElement)propEffect).style.top = StyleLength.op_Implicit(val.y);
			((VisualElement)m_plotEffectZhakai).style.left = StyleLength.op_Implicit(val.x);
			((VisualElement)m_plotEffectZhakai).style.top = StyleLength.op_Implicit(val.y);
			if (m_lastProp == EPropType.Drill)
			{
				((VisualElement)propEffect).style.top = StyleLength.op_Implicit(630f);
				delay = 0.9f;
				PlayDrillPlotEffect();
			}
			else
			{
				propEffect.AnimationState.SetAnimation(0, "animation", false);
				if (m_lastProp == EPropType.Bomb)
				{
					delay = 7f / 15f;
				}
			}
		}
		((MonoBehaviour)m_wndParent).StartCoroutine(CoEffectSequence(delay, data));
	}

	private IEnumerator CoEffectSequence(float delay, object data)
	{
		yield return (object)new WaitForSeconds(delay);
		if (m_lastProp == EPropType.Bomb)
		{
			PlayBombPlotEffect();
		}
		else if (m_lastProp == EPropType.Pickaxe)
		{
			PlayPickaxeHit();
		}
		ShowAwardList(data);
		yield return (object)new WaitForSeconds(0.3f);
		m_isPlayingEffect = false;
		OnNewEndlessMineInfoAck();
	}

	private void PlayPickaxeHit()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		int num = Random.Range(0, 5);
		SpineVisualElement obj = ((m_lastCell.m_cellDetail.Plot == EEndlessMinePlotType.Stone) ? m_shitouEffects[num] : m_nituEffects[num]);
		Rect worldBound = ((VisualElement)m_lastCell).worldBound;
		Vector2 val = VisualElementExtensions.WorldToLocal(((VisualElement)m_scrollView).contentContainer, ((Rect)(ref worldBound)).center);
		((VisualElement)obj).style.left = StyleLength.op_Implicit(val.x);
		((VisualElement)obj).style.top = StyleLength.op_Implicit(val.y);
		obj.AnimationState.SetAnimation(0, "animation", false);
		string strAudioPath = ((m_lastCell.m_cellDetail.Plot == EEndlessMinePlotType.Stone) ? zbAudioDefine.UI_JIAN : zbAudioDefine.UI_BOMB);
		zbAudioMgr.Instance.PlayAudio(strAudioPath);
	}

	private void PlayBombPlotEffect()
	{
		m_plotEffectZhakai.AnimationState.SetAnimation(0, "animation", false);
		foreach (BombEffectData bombEffectPo in m_bombEffectPos)
		{
			PlaySingleCellEffect(bombEffectPo);
		}
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_BOMB);
	}

	private void PlayDrillPlotEffect()
	{
		if (m_lastCell != null)
		{
			m_propEffectDrill.AnimationState.ClearTrack(0);
			m_propEffectDrill.AnimationState.SetAnimation(0, "animation", false);
			((MonoBehaviour)m_wndParent).StartCoroutine(CoPlayDrillRowSequence());
		}
	}

	private IEnumerator CoPlayDrillRowSequence()
	{
		yield return (object)new WaitForSeconds(1f / 30f);
		for (int i = 1; i <= 7; i++)
		{
			foreach (BombEffectData item in GetDrillCellsByRow(i))
			{
				PlaySingleCellEffect(item);
			}
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_BOMB);
			yield return (object)new WaitForSeconds(2f / 15f);
		}
	}

	private void OnNewEndlessMineInfoAck()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		EndlessMineInfo endlessMineInfo = zbSelf.Instance.endlessMineHandler.endlessMineInfo;
		if (endlessMineInfo != null)
		{
			int num = endlessMineInfo.EndlessMineBaseInfo.Depth - m_curDepth;
			if (num <= 0)
			{
				Refresh();
				return;
			}
			float num2 = (float)num * 90f;
			Vector2 target = m_scrollView.scrollOffset + new Vector2(0f, num2);
			((MonoBehaviour)m_wndParent).StartCoroutine(CoScrollTo(target, 0.3f));
		}
	}

	private IEnumerator CoScrollTo(Vector2 target, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 start = m_scrollView.scrollOffset;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			m_scrollView.scrollOffset = Vector2.Lerp(start, target, elapsed / duration);
			yield return null;
		}
		m_scrollView.scrollOffset = target;
		Refresh();
	}

	private void ResetAllCellColor()
	{
		foreach (zbEndlessMineCell cell in m_cellList)
		{
			cell.SetSelectState(0);
		}
	}

	private SpineVisualElement GetPropEffect(EPropType type)
	{
		return (SpineVisualElement)(type switch
		{
			EPropType.Pickaxe => m_propEffectPickaxe, 
			EPropType.Drill => m_propEffectDrill, 
			EPropType.Bomb => m_propEffectBomb, 
			_ => null, 
		});
	}

	private void ShowAwardList(object data)
	{
		if (data is AwardDetail[] array)
		{
			int num = array.Length;
			bool flag = num > 1;
			if (num == 1 && array[0].Type == EResourceType.Box)
			{
				flag = true;
			}
			if (flag)
			{
				zbUIMgr.Instance.ShowAward(array);
				m_wndParent.m_isGetPropsAck = false;
				m_wndParent.RefreshExchange();
			}
			else
			{
				ShowGetProps(array);
			}
		}
	}

	private void ShowGetProps(AwardDetail[] awardList)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (awardList.Length != 0)
		{
			AwardDetail award = awardList[0];
			zbEndlessMineGetItem zbEndlessMineGetItem2 = null;
			for (int i = 0; i < 5; i++)
			{
				if (m_getItems[i] != null && !m_getItems[i].m_isAction)
				{
					zbEndlessMineGetItem2 = m_getItems[i];
					break;
				}
			}
			if (zbEndlessMineGetItem2 == null)
			{
				zbEndlessMineGetItem2 = m_getItems[0];
			}
			if (zbEndlessMineGetItem2 != null && m_lastCell != null)
			{
				Rect worldBound = ((VisualElement)m_lastCell).worldBound;
				Vector2 pos = VisualElementExtensions.WorldToLocal(((VisualElement)m_scrollView).contentContainer, ((Rect)(ref worldBound)).center);
				zbEndlessMineGetItem2.Refresh(award, pos);
			}
		}
		else
		{
			m_wndParent.m_isGetPropsAck = false;
			m_wndParent.RefreshExchange();
		}
	}
}
