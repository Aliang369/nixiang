using System;
using System.Collections.Generic;

public class WillPowerOverLimitConfirmInfo
{
	public int overLimitWillPower;

	public List<Influence> penaltyInfluences;

	public Action confirmCallback;

	public Action cancelCallback;
}
