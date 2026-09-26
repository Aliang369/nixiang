using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePayCreateOrderSignReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePayCreateOrderSignReqReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlBheUNyZWF0ZU9yZGVyU2lnblJlcS5wcm90bxIFdHlwZXMaF2Vu" + "dW0uRUNoYW5uZWxUeXBlLnByb3RvGhhlbnVtLkVQbGF0Zm9ybVR5cGUucHJv" + "dG8ilgEKFVBheUNyZWF0ZU9yZGVyU2lnblJlcRIuCghQbGF0Zm9ybRgBIAEo" + "DjIcLkVQbGF0Zm9ybVR5cGUuRVBsYXRmb3JtVHlwZRIuCgpBcHBDaGFubmVs" + "GAIgASgOMhouRUNoYW5uZWxUeXBlLkVDaGFubmVsVHlwZRIPCgdHb29kc0lk" + "GAMgASgFEgwKBERhdGEYBCABKAlCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PayCreateOrderSignReq), (MessageParser)(object)PayCreateOrderSignReq.Parser, new string[4] { "Platform", "AppChannel", "GoodsId", "Data" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
