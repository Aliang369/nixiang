using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairHelpInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairHelpInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLldvcmxkTGFpckhlbHBJbmZvLnByb3RvEgV0eXBlcxoXdHlwZS5S" + "b2xlQmFzZUluZm8ucHJvdG8aGnR5cGUuV29ybGRMYWlyRGV0YWlsLnByb3Rv" + "ImwKEVdvcmxkTGFpckhlbHBJbmZvEikKDEhlbHBSb2xlSW5mbxgBIAEoCzIT" + "LnR5cGVzLlJvbGVCYXNlSW5mbxIsCgxIZWxwTGFpckluZm8YAiABKAsyFi50" + "eXBlcy5Xb3JsZExhaXJEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeRoleBaseInfoReflection.Descriptor,
			TypeWorldLairDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairHelpInfo), (MessageParser)(object)WorldLairHelpInfo.Parser, new string[2] { "HelpRoleInfo", "HelpLairInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
