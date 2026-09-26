using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsApplyListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsApplyListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkZyaWVuZHNBcHBseUxpc3RBY2sucHJvdG8SBXR5cGVzGh10eXBl" + "LkZyaWVuZHNBcHBseURldGFpbC5wcm90byJDChNGcmllbmRzQXBwbHlMaXN0" + "QWNrEiwKCUFwcGx5TGlzdBgBIAMoCzIZLnR5cGVzLkZyaWVuZHNBcHBseURl" + "dGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFriendsApplyDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsApplyListAck), (MessageParser)(object)FriendsApplyListAck.Parser, new string[1] { "ApplyList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
