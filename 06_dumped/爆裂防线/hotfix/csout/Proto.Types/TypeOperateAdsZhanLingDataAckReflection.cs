using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsZhanLingDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsZhanLingDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVBZHNaaGFuTGluZ0RhdGFBY2sucHJvdG8SBXR5cGVz" + "Ghx0eXBlLkFkc1poYW5MaW5nRGV0YWlsLnByb3RvIk4KGU9wZXJhdGVBZHNa" + "aGFuTGluZ0RhdGFBY2sSMQoPQWRzWmhhbkxpbmdMaXN0GAEgAygLMhgudHlw" + "ZXMuQWRzWmhhbkxpbmdEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAdsZhanLingDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsZhanLingDataAck), (MessageParser)(object)OperateAdsZhanLingDataAck.Parser, new string[1] { "AdsZhanLingList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
