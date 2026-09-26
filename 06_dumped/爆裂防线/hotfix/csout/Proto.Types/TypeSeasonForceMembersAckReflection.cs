using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonForceMembersAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonForceMembersAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlNlYXNvbkZvcmNlTWVtYmVyc0Fjay5wcm90bxIFdHlwZXMaG3R5" + "cGUuU2Vhc29uTWVtYmVySW5mby5wcm90byI+ChVTZWFzb25Gb3JjZU1lbWJl" + "cnNBY2sSJQoETGlzdBgBIAMoCzIXLnR5cGVzLlNlYXNvbk1lbWJlckluZm9C" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonMemberInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonForceMembersAck), (MessageParser)(object)SeasonForceMembersAck.Parser, new string[1] { "List" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
