using System;
using System.IO;
using Best.HTTP.Shared.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestTestWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnSave;

	private int[,] ints = new int[5, 21]
	{
		{
			2, 3, 4, 5, 6, 8, 10, 12, 14, 14,
			17, 21, 25, 29, 32, 42, 48, 52, 68, 80,
			100
		},
		{
			0, 0, 0, 0, 0, 0, 2, 3, 4, 5,
			6, 8, 10, 12, 14, 14, 17, 21, 25, 29,
			32
		},
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 2, 3, 4, 5, 6, 8, 10, 12,
			14
		},
		{
			0, 0, 0, 0, 0, 0, 0, 0, 1, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0
		},
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 1, 0,
			0
		}
	};

	private TextField[,] m_spriteForce = new TextField[5, 21];

	private string strFilePath;

	protected override void OnEnable()
	{
		base.OnEnable();
		strFilePath = Path.Combine(Application.persistentDataPath, "zbMainQuestTestWnd.json");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnSave = UQueryExtensions.Q<Button>(m_RootElement, "btn_save", (string)null);
		for (int i = 0; i < 5; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "mst" + (i + 1), (string)null);
			for (int j = 0; j < 21; j++)
			{
				m_spriteForce[i, j] = UQueryExtensions.Q<TextField>(val, "field_ResetName" + (j + 1), (string)null);
			}
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSave, (Action)OnSave, "", (TrickleDown)0);
	}

	public void RefreshCustom()
	{
		string text = zbUtil.LoadFromFile(strFilePath);
		if (!string.IsNullOrEmpty(text))
		{
			ints = JsonConvert.DeserializeObject<int[,]>(text);
		}
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 21; j++)
			{
				((BaseField<string>)(object)m_spriteForce[i, j]).value = ints[i, j].ToString();
			}
		}
		OnSave();
	}

	public void SaveToFile(string strContent)
	{
		try
		{
			File.WriteAllText(strFilePath, strContent);
			zbFileEncrypUtil.EncryptFile(strFilePath);
		}
		catch (Exception)
		{
		}
	}

	public void OnSave()
	{
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 21; j++)
			{
				ints[i, j] = Extensions.ToInt32(((BaseField<string>)(object)m_spriteForce[i, j]).value, 0);
			}
		}
		string strContent = JsonConvert.SerializeObject((object)ints, (Formatting)1);
		SaveToFile(strContent);
	}

	public void OnClose(ClickEvent evt)
	{
		OnSave();
		Hide();
	}
}
