using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGemInsertWnd : zbBaseWnd
{
	private VisualElement m_root;

	private Button m_btnTouchTest;

	private Button m_btnTop;

	private Button m_btnDown;

	private VisualElement m_bgFrame;

	private Label m_lbName;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteGem;

	private Button m_btnIcon;

	private Button m_btnLock;

	private Button m_btnDecompose;

	private Label m_lbInterface;

	private Label m_lbCount;

	private Label m_lbDesc;

	private Label m_lbNoSkill;

	private ScrollView m_scrollView;

	private Button[] m_btnSkillTypes = (Button[])(object)new Button[14];

	private Button m_btnCast;

	private Button m_btnInsert;

	private VisualElement[] m_nodeGems = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_nodeElements = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteGems = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbGems = (Label[])(object)new Label[5];

	private VisualElement[] m_spriteWarnings = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_btnGems = (Button[])(object)new Button[5];

	private ScrollView[] m_nodeGemsScrollView = (ScrollView[])(object)new ScrollView[5];

	private ScrollView m_descScrollView;

	private ResourceDetail m_data;

	private EGemType m_gemType;

	private EEquipType m_equipType;

	private int m_count;

	private List<GemSlotDetail> m_gemListDatas = new List<GemSlotDetail>();

	private List<int> m_skillTypeList = new List<int>();

	private Coroutine warningCoroutine;

	public RepeatedField<GemSlotDetail> GemSlotList;

	protected override void OnEnable()
	{
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_root = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnTop = UQueryExtensions.Q<Button>(m_RootElement, "btn_top", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>(m_RootElement, "btn_down", (string)null);
		m_bgFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "frame", (string)null);
		m_spriteGem = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_btnIcon = UQueryExtensions.Q<Button>(m_RootElement, "btn_icon", (string)null);
		m_btnLock = UQueryExtensions.Q<Button>(m_RootElement, "btn_lock", (string)null);
		m_btnDecompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_decompose", (string)null);
		m_lbInterface = UQueryExtensions.Q<Label>(m_RootElement, "lb_interface", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_descScrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_lbNoSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_noSkill", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "node_skills", (string)null);
		for (int i = 0; i < 14; i++)
		{
			m_btnSkillTypes[i] = UQueryExtensions.Q<Button>((VisualElement)(object)m_scrollView, "btn_skillType" + i, (string)null);
			int currentIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkillTypes[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillTypes(evt, currentIndex);
			}, "", (TrickleDown)0);
		}
		m_btnCast = UQueryExtensions.Q<Button>(m_RootElement, "btn_cast", (string)null);
		m_btnInsert = UQueryExtensions.Q<Button>(m_RootElement, "btn_insert", (string)null);
		for (int num = 0; num < 5; num++)
		{
			m_nodeGems[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "gem" + num, (string)null);
			m_nodeGemsScrollView[num] = UQueryExtensions.Q<ScrollView>(m_nodeGems[num], (string)null, (string)null);
			m_nodeElements[num] = UQueryExtensions.Q<VisualElement>(m_nodeGems[num], "node", (string)null);
			m_spriteGems[num] = UQueryExtensions.Q<VisualElement>(m_nodeGems[num], "icon_gem", (string)null);
			m_lbGems[num] = UQueryExtensions.Q<Label>(m_nodeGems[num], "lb_desc", (string)null);
			m_spriteWarnings[num] = UQueryExtensions.Q<VisualElement>(m_nodeGems[num], "warning", (string)null);
			m_spriteWarnings[num].style.opacity = StyleFloat.op_Implicit(0f);
			m_btnGems[num] = UQueryExtensions.Q<Button>(m_nodeGems[num], "gem_touch", (string)null);
			int curIndex = num;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGems[num], (Action<ClickEvent>)delegate
			{
				OnGems(curIndex);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTop, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnIcon, (Action)OnIcon, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLock, (Action)OnLock, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompose, (Action)OnDecompose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCast, (Action)OnCast, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnInsert, (Action)OnInsert, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EquipSlotInfo, OnEventUpdateEquipSlot);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gem, OnEventUpdateGem);
	}

	public void RefreshCustom(ResourceDetail data)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		for (int i = 0; i < 14; i++)
		{
			((VisualElement)m_btnSkillTypes[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		ScrollView[] nodeGemsScrollView = m_nodeGemsScrollView;
		foreach (ScrollView sv in nodeGemsScrollView)
		{
			((VisualElement)sv).contentContainer.MarkDirtyRepaint();
			((VisualElement)sv).schedule.Execute((Action)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				sv.scrollOffset = Vector2.zero;
				if (sv.horizontalScroller != null)
				{
					sv.horizontalScroller.value = 0f;
				}
			}).StartingIn(1L);
		}
		((VisualElement)m_descScrollView).contentContainer.MarkDirtyRepaint();
		((VisualElement)m_descScrollView).schedule.Execute((Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			m_descScrollView.scrollOffset = Vector2.zero;
			if (m_descScrollView.horizontalScroller != null)
			{
				m_descScrollView.horizontalScroller.value = 0f;
			}
		}).StartingIn(1L);
		zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(m_data.Tid);
		if (recordByKey != null)
		{
			m_gemType = (EGemType)recordByKey.type;
			string resourceBgTex = zbResourceHelper.GetResourceBgTex((EQuality)recordByKey.quality);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceBgTex);
			if ((Object)(object)val != (Object)null)
			{
				m_bgFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_bgFrame.MarkDirtyRepaint();
			}
			string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)recordByKey.quality);
			Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
			if ((Object)(object)val2 != (Object)null)
			{
				m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
				m_spriteFrame.MarkDirtyRepaint();
			}
			Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
			if ((Object)(object)val3 != (Object)null)
			{
				m_spriteGem.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
				m_spriteGem.MarkDirtyRepaint();
			}
			((TextElement)m_lbName).text = recordByKey.name;
			int num = 0;
			m_skillTypeList.Clear();
			int[] skillTypeId = recordByKey.skillTypeId;
			foreach (int num2 in skillTypeId)
			{
				zbSkillTypeCsvRecord recordByKey2 = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(num2);
				if (recordByKey2 != null && num < 14)
				{
					m_skillTypeList.Add(num2);
					Texture2D val4 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey2.icon1);
					((VisualElement)m_btnSkillTypes[num]).style.backgroundImage = StyleBackground.op_Implicit(val4);
					((VisualElement)m_btnSkillTypes[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					num++;
				}
			}
			((VisualElement)m_lbNoSkill).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num != 0) ? 1 : 0));
			((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num <= 0));
			zbLabelLinkUtil.BuildModLink(zbResourceHelper.GetResorceIconPath(EResourceType.Gem, m_data.Tid).strDesc, m_lbDesc);
			((TextElement)m_lbDesc).text = ((TextElement)m_lbDesc).text.Replace("<color=#64FF26>", "<color=#3CB317>");
			switch (recordByKey.type)
			{
			case 1:
			case 2:
				m_equipType = EEquipType.Neuron;
				break;
			case 3:
			case 4:
				m_equipType = EEquipType.Mnsr;
				break;
			case 5:
			case 6:
				m_equipType = EEquipType.ExoSkeleton;
				break;
			}
			Texture2D val5 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/icon/gemstone/gem_type" + recordByKey.type + ".png");
			if ((Object)(object)val5 != (Object)null)
			{
				((VisualElement)m_lbInterface).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val5));
				((VisualElement)m_lbInterface).MarkDirtyRepaint();
			}
			JObject record = zbDataConfig.Instance.resourceBaseJsonTable.GetRecord();
			((VisualElement)m_btnDecompose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(recordByKey.quality >= ((record != null) ? new int?(Extensions.Value<int>((IEnumerable<JToken>)record["gemDecompose"][(object)"qualityLimit"])) : ((int?)null)))));
			((VisualElement)m_btnCast).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(recordByKey.quality >= ((record != null) ? new int?(Extensions.Value<int>((IEnumerable<JToken>)record["gemCast"][(object)"qualityLimit"])) : ((int?)null)))));
			if (recordByKey.quality >= ((record != null) ? new int?(Extensions.Value<int>((IEnumerable<JToken>)record["gemCast"][(object)"qualityLimit"])) : ((int?)null)))
			{
				((VisualElement)m_btnInsert).transform.position = new Vector3(-90f, 415f, 0f);
			}
			else
			{
				((VisualElement)m_btnInsert).transform.position = new Vector3(-175f, 415f, 0f);
			}
		}
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10031") + ": " + zbSelf.Instance.equipHandler.getGemCount(m_data.Tid);
		if (zbSelf.Instance.equipHandler.IsGemLock(m_data.Tid))
		{
			((VisualElement)m_btnLock).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_lock.png"));
		}
		else
		{
			((VisualElement)m_btnLock).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_unLock.png"));
		}
		((VisualElement)m_btnTouchTest).visible = false;
		m_count = 0;
		RefreshGems();
		RefreshBtnDownContent();
	}

	private void RefreshBtnDownContent()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.CeilToInt((float)zbServerData.sGameData.GemList.Count / 6f);
		float num2 = 280f;
		switch (num)
		{
		case 1:
			num2 = 800f;
			break;
		case 2:
			num2 = 680f;
			break;
		case 3:
			num2 = 560f;
			break;
		case 4:
			num2 = 440f;
			break;
		case 5:
			num2 = 280f;
			break;
		}
		((VisualElement)m_btnDown).style.height = StyleLength.op_Implicit(num2);
	}

	private void RefreshGems()
	{
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		zbSelf.Instance.equipHandler.GetEquipSlotInfo(m_equipType);
		Button[] btnGems = m_btnGems;
		for (int i = 0; i < btnGems.Length; i++)
		{
			((VisualElement)btnGems[i]).pickingMode = (PickingMode)1;
		}
		m_gemListDatas.Clear();
		GemSlotList = zbSelf.Instance.equipHandler.GetGemSlotList(m_equipType);
		foreach (GemSlotDetail gemSlot in GemSlotList)
		{
			if (gemSlot.GemType == m_gemType)
			{
				m_gemListDatas.Add(gemSlot);
			}
		}
		int num = 0;
		foreach (GemSlotDetail gemListData in m_gemListDatas)
		{
			string resName = "ui/image/leader/leader_gem1.png";
			string resName2 = "ui/image/icon/gemstone/gem_type" + (int)gemListData.GemType + ".png";
			if (gemListData.GemTid > 0)
			{
				m_count++;
				zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemListData.GemTid);
				if (recordByKey != null)
				{
					resName = "ui/image/leader/leader_gem" + recordByKey.quality + ".png";
					resName2 = recordByKey.icon;
					((VisualElement)m_lbGems[num]).style.unityTextAlign = StyleEnum<TextAnchor>.op_Implicit((TextAnchor)3);
				}
				zbLabelLinkUtil.BuildModLink(zbResourceHelper.GetResorceIconPath(EResourceType.Gem, gemListData.GemTid).strDesc, m_lbGems[num]);
				((TextElement)m_lbGems[num]).text = ((TextElement)m_lbGems[num]).text.Replace("<color=#64FF26>", "<color=#3CB317>");
			}
			else
			{
				((VisualElement)m_lbGems[num]).style.unityTextAlign = StyleEnum<TextAnchor>.op_Implicit((TextAnchor)4);
				((TextElement)m_lbGems[num]).text = zbUtil.Language("lang.10131");
			}
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName);
			m_nodeGems[num].style.backgroundImage = StyleBackground.op_Implicit(val);
			Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName2);
			m_spriteGems[num].style.backgroundImage = StyleBackground.op_Implicit(val2);
			num++;
		}
		ScrollView[] nodeGemsScrollView = m_nodeGemsScrollView;
		foreach (ScrollView scrollView in nodeGemsScrollView)
		{
			((VisualElement)scrollView).schedule.Execute((Action)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				scrollView.scrollOffset = Vector2.zero;
				if (scrollView.verticalScroller != null)
				{
					scrollView.verticalScroller.value = 0f;
				}
			}).StartingIn(0L);
		}
	}

	public void OnGems(int index)
	{
		if (m_count >= 5 && ((VisualElement)m_btnTouchTest).visible)
		{
			GemSlotDetail gemSlotDetail = m_gemListDatas[index];
			if (gemSlotDetail != null)
			{
				zbWsMsgHandler.Instance.EquipSlot_GemReplace_Req(zbServerData.sGameData.EquipSlotInfo.GemPlanId, m_equipType, gemSlotDetail.Index, m_data.Tid);
			}
		}
	}

	public void OnSkillTypes(ClickEvent evt, int index)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (m_skillTypeList[index] > 0)
		{
			Vector2 val = VisualElementExtensions.WorldToLocal(m_root, Vector2.op_Implicit(((PointerEventBase<ClickEvent>)(object)evt).position));
			val.x = Mathf.Clamp(val.x - 160f, 20f, 340f);
			val.y = ((val.y < 840f) ? (val.y + 50f) : (val.y - 280f));
			zbUIMgr.Instance.ShowTipSkillType(m_skillTypeList[index], val);
		}
	}

	private void ActionWarning(int groupGemIndex)
	{
		if (warningCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(warningCoroutine);
			warningCoroutine = null;
		}
		warningCoroutine = ((MonoBehaviour)this).StartCoroutine(WarningBlinkCoroutine(groupGemIndex));
	}

	private IEnumerator WarningBlinkCoroutine(int index)
	{
		float fadeDuration = 0.08f;
		float stayDuration = 0.03f;
		for (int i = 0; i < 3; i++)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(FadeCoroutine(0f, 1f, fadeDuration, index));
			yield return (object)new WaitForSeconds(stayDuration);
			yield return ((MonoBehaviour)this).StartCoroutine(FadeCoroutine(1f, 0f, fadeDuration, index));
		}
		m_spriteWarnings[index].style.opacity = StyleFloat.op_Implicit(0f);
		warningCoroutine = null;
	}

	private IEnumerator FadeCoroutine(float startOpacity, float endOpacity, float duration, int index)
	{
		float time = 0f;
		while (time < duration)
		{
			float num = time / duration;
			float num2 = Mathf.Lerp(startOpacity, endOpacity, num);
			m_spriteWarnings[index].style.opacity = StyleFloat.op_Implicit(num2);
			time += Time.deltaTime;
			yield return null;
		}
		m_spriteWarnings[index].style.opacity = StyleFloat.op_Implicit(endOpacity);
	}

	private void OnIcon()
	{
		zbUIMgr.Instance.ShowTip(EResourceType.Gem, m_data.Tid, 0L);
	}

	private void OnLock()
	{
		if (!zbSelf.Instance.equipHandler.IsGemLock(m_data.Tid))
		{
			zbWsMsgHandler.Instance.Gem_Lock_Req(m_data.Tid, EBoolState.True);
		}
		else
		{
			zbWsMsgHandler.Instance.Gem_Lock_Req(m_data.Tid, EBoolState.False);
		}
	}

	private void OnDecompose()
	{
		if (zbSelf.Instance.equipHandler.IsGemLock(m_data.Tid))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.31107"));
		}
		else if (zbSelf.Instance.equipHandler.getGemCount(m_data.Tid) == 1 && zbSelf.Instance.equipHandler.IsGemInsert(m_data.Tid, zbServerData.sGameData.EquipSlotInfo.GemPlanId))
		{
			zbUIMgr.Instance.ShowGemDecompose(m_data);
			OnClose();
		}
		else if (zbSelf.Instance.equipHandler.getGemCount(m_data.Tid) == 1 && zbSelf.Instance.equipHandler.IsGemInsert(m_data.Tid))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10674"));
		}
		else
		{
			zbUIMgr.Instance.ShowGemDecompose(m_data);
			OnClose();
		}
	}

	private void OnCast()
	{
		if (zbSelf.Instance.equipHandler.IsGemLock(m_data.Tid))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.31107"));
		}
		else if (zbSelf.Instance.equipHandler.getGemCount(m_data.Tid) == 1 && zbSelf.Instance.equipHandler.IsGemInsert(m_data.Tid, zbServerData.sGameData.EquipSlotInfo.GemPlanId))
		{
			zbUIMgr.Instance.ShowGemCast(m_data);
			OnClose();
		}
		else if (zbSelf.Instance.equipHandler.getGemCount(m_data.Tid) == 1 && zbSelf.Instance.equipHandler.IsGemInsert(m_data.Tid))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10674"));
		}
		else
		{
			zbUIMgr.Instance.ShowGemCast(m_data);
			OnClose();
		}
	}

	private void OnInsert()
	{
		Button[] btnGems = m_btnGems;
		for (int i = 0; i < btnGems.Length; i++)
		{
			((VisualElement)btnGems[i]).pickingMode = (PickingMode)0;
		}
		int groupGemIndex = zbSelf.Instance.equipHandler.GetGroupGemIndex(m_data.Tid);
		if (zbSelf.Instance.equipHandler.CanGemReplace(m_data.Tid))
		{
			zbWsMsgHandler.Instance.EquipSlot_GemReplace_Req(zbServerData.sGameData.EquipSlotInfo.GemPlanId, m_equipType, groupGemIndex, m_data.Tid);
		}
		else if (groupGemIndex >= 0 && !zbSelf.Instance.equipHandler.CanGemReplace(m_data.Tid))
		{
			if (groupGemIndex >= 5)
			{
				ActionWarning(groupGemIndex - 5);
			}
			else
			{
				ActionWarning(groupGemIndex);
			}
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10272"));
		}
		else if (m_count < 5)
		{
			zbWsMsgHandler.Instance.EquipSlot_GemSet_Req(zbServerData.sGameData.EquipSlotInfo.GemPlanId, m_equipType, m_data.Tid);
		}
		else
		{
			((VisualElement)m_btnTouchTest).visible = true;
		}
	}

	private void OnClose()
	{
		Hide();
		zbLeaderWnd zbLeaderWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Leader) as zbLeaderWnd;
		if ((Object)(object)zbLeaderWnd2 != (Object)null)
		{
			zbLeaderWnd2.m_moduleJingti.RefreshSelect(-1);
		}
	}

	private void OnEventUpdateGem(object data)
	{
		if (m_data != null)
		{
			if (zbSelf.Instance.equipHandler.getGemCount(m_data.Tid) == 0L)
			{
				OnClose();
			}
			else
			{
				RefreshCustom(m_data);
			}
		}
	}

	private void OnEventUpdateEquipSlot(object data)
	{
		Hide();
	}
}
