using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleTipsDetailAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleTipsDetailAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlJvbGVUaXBzRGV0YWlsQWNrLnByb3RvEgV0eXBlcxoZdHlwZS5S" + "b2xlVGlwc0RldGFpbC5wcm90byI+ChFSb2xlVGlwc0RldGFpbEFjaxIpCgpU" + "aXBzRGV0YWlsGAEgASgLMhUudHlwZXMuUm9sZVRpcHNEZXRhaWxCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleTipsDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleTipsDetailAck), (MessageParser)(object)RoleTipsDetailAck.Parser, new string[1] { "TipsDetail" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
