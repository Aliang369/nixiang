using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestAccountSelector : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public class OptionData : TMP_Dropdown.OptionData
	{
		public AccountData accountData;

		public OptionData(string s)
			: base(s)
		{
		}
	}

	[SerializeField]
	private TMP_Dropdown dropdown;

	[SerializeField]
	private AccountData account;

	private void Start()
	{
		List<AccountData> list = Bootstrap.instance.globalSaveConfigData.accounts.ToList();
		list.Add(new AccountData
		{
			playerID = "Default",
			nickName = "Default",
			password = "123456"
		});
		list.Sort((AccountData a, AccountData b) => string.Compare(a.playerID, b.playerID, StringComparison.Ordinal));
		account = list.FirstOrDefault();
		dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (AccountData item in list)
		{
			dropdown.options.Add(new OptionData(item.playerID)
			{
				accountData = item
			});
		}
		dropdown.onValueChanged.AddListener(_OnChange);
		dropdown.value = 0;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Bootstrap.instance.InitLoad(account.playerID, dropCurrentGame: false);
	}

	private void _OnChange(int index)
	{
		account = (dropdown.options[0] as OptionData).accountData;
	}
}
