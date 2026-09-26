using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTrainRobFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainRobFightResultReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlRyYWluUm9iRmlnaHRSZXN1bHRSZXEucHJvdG8SBXR5cGVzGht0" + "eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8iXAoWVHJhaW5Sb2JGaWdodFJl" + "c3VsdFJlcRITCgtUcmFuc3BvcnRJZBgBIAEoBRItCgxCYXR0bGVSZXN1bHQY" + "AiABKAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZvQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainRobFightResultReq), (MessageParser)(object)TrainRobFightResultReq.Parser, new string[2] { "TransportId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
