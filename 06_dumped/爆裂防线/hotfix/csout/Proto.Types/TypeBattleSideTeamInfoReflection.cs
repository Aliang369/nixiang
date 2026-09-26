using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleSideTeamInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleSideTeamInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkJhdHRsZVNpZGVUZWFtSW5mby5wcm90bxIFdHlwZXMaGXR5cGUu" + "QmF0dGxlVGVhbUluZm8ucHJvdG8icgoSQmF0dGxlU2lkZVRlYW1JbmZvEi0K" + "Dk1hc3RlclRlYW1JbmZvGAEgASgLMhUudHlwZXMuQmF0dGxlVGVhbUluZm8S" + "LQoOSGVscGVyVGVhbUluZm8YAiABKAsyFS50eXBlcy5CYXR0bGVUZWFtSW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleTeamInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleSideTeamInfo), (MessageParser)(object)BattleSideTeamInfo.Parser, new string[2] { "MasterTeamInfo", "HelperTeamInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
