using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAuxExchangeMoneyReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxExchangeMoneyReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkF1eEV4Y2hhbmdlTW9uZXlSZXEucHJvdG8SBXR5cGVzGhVlbnVt" + "LkVNb25leVR5cGUucHJvdG8iTwoTQXV4RXhjaGFuZ2VNb25leVJlcRIpCglN" + "b25leVR5cGUYASABKA4yFi5FTW9uZXlUeXBlLkVNb25leVR5cGUSDQoFQ291" + "bnQYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEMoneyTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxExchangeMoneyReq), (MessageParser)(object)AuxExchangeMoneyReq.Parser, new string[2] { "MoneyType", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
