using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterTwoExchangeReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterTwoExchangeReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkpvdXJuZXlDaGFwdGVyVHdvRXhjaGFuZ2VSZXEucHJvdG8SBXR5" + "cGVzIjkKHEpvdXJuZXlDaGFwdGVyVHdvRXhjaGFuZ2VSZXESCgoCSWQYASAB" + "KAUSDQoFQ291bnQYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterTwoExchangeReq), (MessageParser)(object)JourneyChapterTwoExchangeReq.Parser, new string[2] { "Id", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
