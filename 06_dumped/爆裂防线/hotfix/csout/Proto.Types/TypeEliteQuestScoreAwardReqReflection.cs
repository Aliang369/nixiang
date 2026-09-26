using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestScoreAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestScoreAwardReqReflection()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkVsaXRlUXVlc3RTY29yZUF3YXJkUmVxLnByb3RvEgV0eXBlcyIq" + "ChdFbGl0ZVF1ZXN0U2NvcmVBd2FyZFJlcRIPCgdMZXZlbElkGAEgASgFQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestScoreAwardReq), (MessageParser)(object)EliteQuestScoreAwardReq.Parser, new string[1] { "LevelId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
