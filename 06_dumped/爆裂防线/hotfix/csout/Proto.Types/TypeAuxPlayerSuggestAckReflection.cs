using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAuxPlayerSuggestAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxPlayerSuggestAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkF1eFBsYXllclN1Z2dlc3RBY2sucHJvdG8SBXR5cGVzGh50eXBl" + "LlBsYXllclN1Z2dlc3REZXRhaWwucHJvdG8iRgoTQXV4UGxheWVyU3VnZ2Vz" + "dEFjaxIvCgtTdWdnZXN0TGlzdBgBIAMoCzIaLnR5cGVzLlBsYXllclN1Z2dl" + "c3REZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypePlayerSuggestDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxPlayerSuggestAck), (MessageParser)(object)AuxPlayerSuggestAck.Parser, new string[1] { "SuggestList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
