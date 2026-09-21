using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class WaitPanelBase : MonoBehaviour
{
	public sealed class Token
	{
		internal readonly long id;

		internal readonly string loadingContent;

		internal Token(long id, string loadingContent)
		{
			this.id = id;
			this.loadingContent = loadingContent;
		}
	}

	[SerializeField]
	public WaitPanel waitPanel;

	[NonSerialized]
	private List<Token> tokens = new List<Token>();

	[NonSerialized]
	private long nextTokenId;

	[NonSerialized]
	private bool showing;

	[NonSerialized]
	private string[] showingLoadingContents = Array.Empty<string>();

	[NonSerialized]
	private Sequence sequence;

	public void Init()
	{
		waitPanel.Init();
		waitPanel.gameObject.SetActive(value: false);
	}

	public Token Show(string loadingContent)
	{
		Token token = new Token(++nextTokenId, loadingContent);
		tokens.Add(token);
		return token;
	}

	public void Hide(Token token)
	{
		if (token != null)
		{
			tokens.RemoveAll((Token t) => t.id == token.id);
		}
	}

	public void Clear()
	{
		tokens.Clear();
		RefreshVisibleState();
	}

	private void Update()
	{
		RefreshVisibleState();
	}

	private void RefreshVisibleState()
	{
		bool flag = tokens.Count > 0;
		RefreshLoadingContentTexts();
		if (showing != flag)
		{
			if (flag)
			{
				ShowImpl();
			}
			else
			{
				HideImpl();
			}
		}
	}

	private void RefreshLoadingContentTexts()
	{
		string[] array = (from t in tokens
			select t.loadingContent into s
			where !string.IsNullOrEmpty(s)
			select s).ToArray();
		if (!Enumerable.SequenceEqual(showingLoadingContents, array))
		{
			showingLoadingContents = array;
			waitPanel.SyncLoadingContents(array);
		}
	}

	private void ShowImpl()
	{
		sequence?.Kill();
		waitPanel.Show();
		waitPanel.SetAlpha(0f);
		sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(waitPanel.CanvasGroup, 1f, 1f));
		showing = true;
	}

	private void HideImpl()
	{
		sequence?.Kill();
		sequence = null;
		waitPanel.SetAlpha(0f);
		waitPanel.Hide();
		showing = false;
	}
}
