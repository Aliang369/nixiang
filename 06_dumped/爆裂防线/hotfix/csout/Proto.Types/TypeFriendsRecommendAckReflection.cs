using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsRecommendAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsRecommendAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkZyaWVuZHNSZWNvbW1lbmRBY2sucHJvdG8SBXR5cGVzGh50eXBl" + "LkZyaWVuZHNDb21tb25EZXRhaWwucHJvdG8iSAoTRnJpZW5kc1JlY29tbWVu" + "ZEFjaxIxCg1SZWNvbW1lbmRMaXN0GAEgAygLMhoudHlwZXMuRnJpZW5kc0Nv" + "bW1vbkRldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFriendsCommonDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsRecommendAck), (MessageParser)(object)FriendsRecommendAck.Parser, new string[1] { "RecommendList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
