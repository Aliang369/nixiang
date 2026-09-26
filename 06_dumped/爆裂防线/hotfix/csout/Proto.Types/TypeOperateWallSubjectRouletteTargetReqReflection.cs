using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectRouletteTargetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectRouletteTargetReqReflection()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci50eXBlLk9wZXJhdGVXYWxsU3ViamVjdFJvdWxldHRlVGFyZ2V0UmVxLnBy" + "b3RvEgV0eXBlcyIzCiNPcGVyYXRlV2FsbFN1YmplY3RSb3VsZXR0ZVRhcmdl" + "dFJlcRIMCgRVVUlEGAEgASgJQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectRouletteTargetReq), (MessageParser)(object)OperateWallSubjectRouletteTargetReq.Parser, new string[1] { "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
