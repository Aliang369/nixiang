using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRechargeRebateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeRebateReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlJlY2hhcmdlUmViYXRlLnByb3RvEgV0eXBlcxoXZW51bS5FUmVi" + "YXRlU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwucHJvdG8icgoOUmVj" + "aGFyZ2VSZWJhdGUSKQoFU3RhdGUYASABKA4yGi5FUmViYXRlU3RhdGUuRVJl" + "YmF0ZVN0YXRlEg4KBkFtb3VudBgCIAEoAhIlCglBd2FyZExpc3QYAyADKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumERebateStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeRebate), (MessageParser)(object)RechargeRebate.Parser, new string[3] { "State", "Amount", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
