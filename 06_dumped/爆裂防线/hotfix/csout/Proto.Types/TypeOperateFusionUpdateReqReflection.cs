using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateFusionUpdateReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateFusionUpdateReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLk9wZXJhdGVGdXNpb25VcGRhdGVSZXEucHJvdG8SBXR5cGVzIlcK" + "Fk9wZXJhdGVGdXNpb25VcGRhdGVSZXESEQoJQmVzdFNjb3JlGAEgASgFEg0K" + "BVNjb3JlGAIgASgFEg0KBUNsZWFuGAMgASgFEgwKBERhdGEYBCABKAlCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateFusionUpdateReq), (MessageParser)(object)OperateFusionUpdateReq.Parser, new string[4] { "BestScore", "Score", "Clean", "Data" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
