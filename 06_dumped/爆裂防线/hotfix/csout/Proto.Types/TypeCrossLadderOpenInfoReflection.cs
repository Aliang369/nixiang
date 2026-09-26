using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderOpenInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderOpenInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkNyb3NzTGFkZGVyT3BlbkluZm8ucHJvdG8SBXR5cGVzImkKE0Ny" + "b3NzTGFkZGVyT3BlbkluZm8SDwoHR3JvdXBJZBgBIAEoBRISCgpHcm91cFJh" + "bmdlGAIgAygFEg8KB1N0YXJ0VG0YAyABKAMSDQoFRW5kVG0YBCABKAMSDQoF" + "TW9udGgYBSABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderOpenInfo), (MessageParser)(object)CrossLadderOpenInfo.Parser, new string[5] { "GroupId", "GroupRange", "StartTm", "EndTm", "Month" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
