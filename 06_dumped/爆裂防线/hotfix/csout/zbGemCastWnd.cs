using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGemCastWnd : zbBaseWnd
{
	private AudioSource m_audio;

	private Button m_btnTouch;

	private zbItem m_item;

	private VisualElement m_content;

	private VisualElement m_inner;

	private Label m_lbCost;

	private Button m_btnCast;

	private Button m_btnSkip;

	private VisualElement m_spriteOk;

	private List<zbGemCastItem> m_listItemDatas = new List<zbGemCastItem>();

	private readonly float m_itemHeight = 100f;

	private bool skipFlag;

	private int m_finalIndex;

	private float m_wheelRunTime = 2f;

	private zbItemData m_costItemData;

	private int m_curGemTid;

	private List<zbGemCsvRecord> m_gemRecords = new List<zbGemCsvRecord>();

	private float m_totalHeight;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_content = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_btnCast = UQueryExtensions.Q<Button>(m_RootElement, "btn_cast", (string)null);
		m_btnSkip = UQueryExtensions.Q<Button>(m_RootElement, "btn_skip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCast, (Action)OnCast, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkip, (Action)OnSkip, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_GemCast_Ack, OnEventGemCastAck);
	}

	public void RefreshCustom(ResourceDetail data)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		JObject record = zbDataConfig.Instance.resourceBaseJsonTable.GetRecord();
		m_costItemData = new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)record["gemCast"][(object)"cost"][(object)0]), Extensions.Value<long>((IEnumerable<JToken>)record["gemCast"][(object)"cost"][(object)1]), Extensions.Value<long>((IEnumerable<JToken>)record["gemCast"][(object)"cost"][(object)2]));
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_costItemData.type, m_costItemData.id);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val != (Object)null)
		{
			((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			((VisualElement)m_lbCost).MarkDirtyRepaint();
		}
		RefreshCost();
		RefreshSkip();
		m_curGemTid = data.Tid;
		RefreshGemRecords(m_curGemTid);
		m_item.Refresh(new zbItemData(EResourceType.Gem, m_curGemTid, 1L), isNameVis: false, isCountVis: false);
		m_finalIndex = m_gemRecords.FindIndex((zbGemCsvRecord zbGemCsvRecord2) => zbGemCsvRecord2.id == m_curGemTid);
		m_listItemDatas.Clear();
		m_content.Clear();
		if (m_inner != null && m_inner.parent != null)
		{
			m_inner.RemoveFromHierarchy();
		}
		m_inner = new VisualElement();
		m_inner.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_inner.style.top = StyleLength.op_Implicit(0f);
		m_inner.style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)0);
		m_content.Add(m_inner);
		m_totalHeight = (float)m_gemRecords.Count * m_itemHeight;
		for (int num = 0; num < m_gemRecords.Count; num++)
		{
			zbGemCastItem zbGemCastItem2 = new zbGemCastItem();
			zbGemCastItem2.Refresh(m_gemRecords[num].mod);
			((VisualElement)zbGemCastItem2).style.height = StyleLength.op_Implicit(m_itemHeight);
			m_listItemDatas.Add(zbGemCastItem2);
			m_inner.Add((VisualElement)(object)zbGemCastItem2);
		}
		float num2 = (float)m_finalIndex * m_itemHeight - 70f;
		m_inner.transform.position = new Vector3(0f, 0f - num2, 0f);
	}

	private void RefreshCost()
	{
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(m_costItemData.type, (int)m_costItemData.id);
		((TextElement)m_lbCost).text = zbUtil.GetNeedHaveRichString(resourceCount, m_costItemData.count);
	}

	private void RefreshSkip()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSetSave.Instance.GetGemCast() != 1));
	}

	private void OnEventGemCastAck(object data)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		skipFlag = false;
		RefreshCost();
		GemCastAck ack = data as GemCastAck;
		if (ack != null)
		{
			m_curGemTid = ack.DstTid;
			m_finalIndex = m_gemRecords.FindIndex((zbGemCsvRecord record) => record.id == ack.DstTid);
			if (zbSetSave.Instance.GetGemCast() == 1)
			{
				m_item.Refresh(new zbItemData(EResourceType.Gem, ack.DstTid, 1L), isNameVis: false, isCountVis: false);
				float num = (float)m_finalIndex * m_itemHeight - 70f;
				m_inner.transform.position = new Vector3(0f, 0f - num, 0f);
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(SpinReel(m_inner, ack.DstTid));
			}
		}
	}

	private void RefreshGemRecords(int gemTid)
	{
		Dictionary<int, zbGemCsvRecord>.ValueCollection values = zbDataConfig.Instance.gemCsvTable.GetRecords().records.Values;
		zbGemCsvRecord target = values.FirstOrDefault((zbGemCsvRecord r) => r.id == gemTid);
		m_gemRecords = ((target == null) ? new List<zbGemCsvRecord>() : (from r in values
			where r.type == target.type && r.quality == target.quality
			select (r)).ToList());
	}

	private IEnumerator SpinReel(VisualElement inner, int dstTid)
	{
		((VisualElement)m_btnCast).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/laohuji.ogg", loop: true);
		zbUIMgr.Instance.ShowModel();
		float speed = 1500f;
		float currentY = 0f;
		float elapsed = 0f;
		while (elapsed < m_wheelRunTime && !skipFlag)
		{
			elapsed += Time.unscaledDeltaTime;
			currentY += speed * Time.unscaledDeltaTime;
			float num = currentY % m_totalHeight;
			inner.transform.position = new Vector3(0f, 0f - num, 0f);
			yield return null;
		}
		yield return AlignToTarget(inner, currentY, dstTid);
		zbUIMgr.Instance.HideModel();
		((VisualElement)m_btnCast).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (Object.op_Implicit((Object)(object)m_audio))
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			m_audio = null;
		}
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
	}

	private IEnumerator AlignToTarget(VisualElement inner, float currentY, int dstTid)
	{
		float speed = 1500f;
		float targetY = (float)m_finalIndex * m_itemHeight - 70f;
		currentY %= m_totalHeight;
		float diff = targetY - currentY;
		if (diff < 0f)
		{
			diff += m_totalHeight;
		}
		while (diff > 1f)
		{
			float num = speed * Time.unscaledDeltaTime;
			if (num > diff)
			{
				num = diff;
			}
			currentY = (currentY + num) % m_totalHeight;
			inner.transform.position = new Vector3(0f, 0f - currentY, 0f);
			diff -= num;
			yield return null;
		}
		m_item.Refresh(new zbItemData(EResourceType.Gem, dstTid, 1L), isNameVis: false, isCountVis: false);
		inner.transform.position = new Vector3(0f, 0f - targetY, 0f);
	}

	private void OnCast()
	{
		if (zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Gem, m_curGemTid) == 1 && zbSelf.Instance.equipHandler.IsGemInsert(m_curGemTid))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10674"));
			OnClose();
		}
		else if (!zbSelf.Instance.dataHandler.HasResourceEnough(m_costItemData.type, (int)m_costItemData.id, (int)m_costItemData.count))
		{
			zbUIMgr.Instance.ShowTip(m_costItemData.type, m_costItemData.id, m_costItemData.count);
		}
		else
		{
			zbWsMsgHandler.Instance.Gem_Cast_Req(m_curGemTid);
		}
	}

	private void OnSkip()
	{
		zbSetSave.Instance.SetGemCast((zbSetSave.Instance.GetGemCast() == 0) ? 1 : 0);
		RefreshSkip();
		if (zbSetSave.Instance.GetGemCast() == 1)
		{
			skipFlag = true;
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
