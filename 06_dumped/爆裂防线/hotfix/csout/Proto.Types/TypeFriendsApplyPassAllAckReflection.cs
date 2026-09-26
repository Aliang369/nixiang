using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsApplyPassAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsApplyPassAllAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkZyaWVuZHNBcHBseVBhc3NBbGxBY2sucHJvdG8SBXR5cGVzGh10" + "eXBlLkZyaWVuZHNBcHBseURldGFpbC5wcm90bxoYdHlwZS5GcmllbmRzRGV0" + "YWlsLnByb3RvInEKFkZyaWVuZHNBcHBseVBhc3NBbGxBY2sSKQoLRnJpZW5k" + "c0xpc3QYASADKAsyFC50eXBlcy5GcmllbmRzRGV0YWlsEiwKCUFwcGx5TGlz" + "dBgCIAMoCzIZLnR5cGVzLkZyaWVuZHNBcHBseURldGFpbEIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeFriendsApplyDetailReflection.Descriptor,
			TypeFriendsDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsApplyPassAllAck), (MessageParser)(object)FriendsApplyPassAllAck.Parser, new string[2] { "FriendsList", "ApplyList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
