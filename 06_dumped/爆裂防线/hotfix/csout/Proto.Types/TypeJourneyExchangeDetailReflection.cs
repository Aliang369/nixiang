using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyExchangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyExchangeDetailReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkpvdXJuZXlFeGNoYW5nZURldGFpbC5wcm90bxIFdHlwZXMaI3R5" + "cGUuSm91cm5leUNvdW50UmVjb3JkRGV0YWlsLnByb3RvIk4KFUpvdXJuZXlF" + "eGNoYW5nZURldGFpbBI1CgxFeGNoYW5nZUxpc3QYASADKAsyHy50eXBlcy5K" + "b3VybmV5Q291bnRSZWNvcmREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeJourneyCountRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyExchangeDetail), (MessageParser)(object)JourneyExchangeDetail.Parser, new string[1] { "ExchangeList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
