using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleResultSideTeamInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleResultSideTeamInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkJhdHRsZVJlc3VsdFNpZGVUZWFtSW5mby5wcm90bxIFdHlwZXMa" + "H3R5cGUuQmF0dGxlUmVzdWx0U2lkZUluZm8ucHJvdG8aH3R5cGUuQmF0dGxl" + "UmVzdWx0VGVhbUluZm8ucHJvdG8iswEKGEJhdHRsZVJlc3VsdFNpZGVUZWFt" + "SW5mbxItCghTaWRlSW5mbxgBIAEoCzIbLnR5cGVzLkJhdHRsZVJlc3VsdFNp" + "ZGVJbmZvEjMKDk1hc3RlclRlYW1JbmZvGAIgASgLMhsudHlwZXMuQmF0dGxl" + "UmVzdWx0VGVhbUluZm8SMwoOSGVscGVyVGVhbUluZm8YAyABKAsyGy50eXBl" + "cy5CYXR0bGVSZXN1bHRUZWFtSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleResultSideInfoReflection.Descriptor,
			TypeBattleResultTeamInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleResultSideTeamInfo), (MessageParser)(object)BattleResultSideTeamInfo.Parser, new string[3] { "SideInfo", "MasterTeamInfo", "HelperTeamInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
