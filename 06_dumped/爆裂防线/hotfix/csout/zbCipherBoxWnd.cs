using System;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCipherBoxWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnGif;

	private Button m_btnConfirm;

	private Button[] m_btnsUp = (Button[])(object)new Button[4];

	private Button[] m_btnsDown = (Button[])(object)new Button[4];

	private Label m_lbName;

	private Label m_lbTime;

	private VisualElement m_rollWrapper;

	private Label m_lbRoll;

	private Label m_lbCount;

	private Label m_lbConfirm;

	private Label[] m_lbNums = (Label[])(object)new Label[4];

	private int[] m_nums = new int[4];

	private Sprite[] m_numsIcon = (Sprite[])(object)new Sprite[5];

	private Sprite m_iconDefault;

	private CipherBoxDetail m_cipherBoxDetail;

	private float scrollSpeed = 50f;

	private float currentX;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnGif = UQueryExtensions.Q<Button>(m_RootElement, "btn_gif", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_rollWrapper = UQueryExtensions.Q<VisualElement>(m_RootElement, "rollWrapper", (string)null);
		m_lbRoll = UQueryExtensions.Q<Label>(m_RootElement, "lb_roll", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_numsIcon[i] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/cipherbox/cipherbox_" + (i + 1) + ".png");
		}
		m_iconDefault = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/cipherbox/cipherbox_0.png");
		for (int j = 0; j < 4; j++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "node" + j, (string)null);
			m_btnsUp[j] = UQueryExtensions.Q<Button>(val, "btn_up", (string)null);
			m_btnsDown[j] = UQueryExtensions.Q<Button>(val, "btn_down", (string)null);
			m_lbNums[j] = UQueryExtensions.Q<Label>(val, "icon_num", (string)null);
			int tempIndex = j;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsUp[j], (Action)delegate
			{
				OnUp(tempIndex);
			}, "", (TrickleDown)0);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsDown[j], (Action)delegate
			{
				OnDown(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGif, (Action)OnClickGif, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
	}

	public void RefreshCustom(CipherBoxDetail cipherBoxDetail)
	{
		m_cipherBoxDetail = cipherBoxDetail;
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10048") + ": " + $"{m_cipherBoxDetail.LimitNum - m_cipherBoxDetail.Count} / {m_cipherBoxDetail.LimitNum}";
	}

	protected override void UpdateAtShow()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long startTm = m_cipherBoxDetail.BaseInfo.StartTm;
		long endTm = m_cipherBoxDetail.BaseInfo.EndTm;
		bool flag = startTm < serverTime && serverTime < endTm;
		((TextElement)m_lbTime).text = (flag ? zbUtil.FormatTimeHHMMSS(endTm - serverTime) : zbUtil.Language("lang.10043"));
		currentX -= scrollSpeed * Time.deltaTime;
		if (currentX < 0f - ((VisualElement)m_lbRoll).resolvedStyle.width)
		{
			currentX = m_rollWrapper.resolvedStyle.width;
		}
		((VisualElement)m_lbRoll).style.translate = new StyleTranslate(new Translate(Length.op_Implicit(currentX), Length.op_Implicit(0f), 0f));
	}

	private void OnUpdateEventOperate(object data)
	{
		CipherBoxDetail cipherBoxInfoByUUID = zbSelf.Instance.operateHandler.GetCipherBoxInfoByUUID(m_cipherBoxDetail.BaseInfo.UUID);
		RefreshCustom(cipherBoxInfoByUUID);
	}

	private void OnUp(int index)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (m_cipherBoxDetail.LimitNum - m_cipherBoxDetail.Count != 0)
		{
			_ = m_nums[index];
			if (m_nums[index] == 5)
			{
				m_nums[index] = 1;
				((VisualElement)m_lbNums[index]).style.backgroundImage = new StyleBackground(m_numsIcon[0]);
			}
			else
			{
				m_nums[index]++;
				int num = m_nums[index];
				((VisualElement)m_lbNums[index]).style.backgroundImage = new StyleBackground(m_numsIcon[num - 1]);
			}
		}
	}

	private void OnDown(int index)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (m_cipherBoxDetail.LimitNum - m_cipherBoxDetail.Count != 0)
		{
			_ = m_nums[index];
			if (m_nums[index] == 1 || m_nums[index] == 0)
			{
				m_nums[index] = 5;
				((VisualElement)m_lbNums[index]).style.backgroundImage = new StyleBackground(m_numsIcon[4]);
			}
			else
			{
				int num = --m_nums[index];
				((VisualElement)m_lbNums[index]).style.backgroundImage = new StyleBackground(m_numsIcon[num - 1]);
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnClickGif()
	{
		zbUIMgr.Instance.ShowCipherBoxPreview(m_cipherBoxDetail);
	}

	private void OnConfirm()
	{
		if (m_cipherBoxDetail.LimitNum <= m_cipherBoxDetail.Count)
		{
			zbUIMgr.Instance.BubbleInfo("lang.10160");
		}
		else if (m_nums.All((int n) => n > 0))
		{
			string cipher = string.Concat(m_nums);
			zbWsMsgHandler.Instance.Operate_CipherBox_Req(m_cipherBoxDetail.BaseInfo.UUID, cipher);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10276"));
		}
	}
}
