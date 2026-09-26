using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestFightStartReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestFightStartReqReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkVsaXRlUXVlc3RGaWdodFN0YXJ0UmVxLnByb3RvEgV0eXBlcyJL" + "ChdFbGl0ZVF1ZXN0RmlnaHRTdGFydFJlcRIPCgdMZXZlbElkGAEgASgFEg0K" + "BUNvdW50GAIgASgFEhAKCFBsYXllcklkGAMgASgDQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestFightStartReq), (MessageParser)(object)EliteQuestFightStartReq.Parser, new string[3] { "LevelId", "Count", "PlayerId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
