using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsActionGiveAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsActionGiveAllAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkZyaWVuZHNBY3Rpb25HaXZlQWxsQWNrLnByb3RvEgV0eXBlcxoY" + "dHlwZS5GcmllbmRzRGV0YWlsLnByb3RvGhZ0eXBlLkZyaWVuZHNJbmZvLnBy" + "b3RvIm0KF0ZyaWVuZHNBY3Rpb25HaXZlQWxsQWNrEicKC0ZyaWVuZHNJbmZv" + "GAEgASgLMhIudHlwZXMuRnJpZW5kc0luZm8SKQoLRnJpZW5kc0xpc3QYAiAD" + "KAsyFC50eXBlcy5GcmllbmRzRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeFriendsDetailReflection.Descriptor,
			TypeFriendsInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsActionGiveAllAck), (MessageParser)(object)FriendsActionGiveAllAck.Parser, new string[2] { "FriendsInfo", "FriendsList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
