using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsActionCollectAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsActionCollectAllAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkZyaWVuZHNBY3Rpb25Db2xsZWN0QWxsQWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnBy" + "b3RvGhh0eXBlLkZyaWVuZHNEZXRhaWwucHJvdG8aFnR5cGUuRnJpZW5kc0lu" + "Zm8ucHJvdG8ivgEKGkZyaWVuZHNBY3Rpb25Db2xsZWN0QWxsQWNrEicKC0Zy" + "aWVuZHNJbmZvGAEgASgLMhIudHlwZXMuRnJpZW5kc0luZm8SKQoLRnJpZW5k" + "c0xpc3QYAiADKAsyFC50eXBlcy5GcmllbmRzRGV0YWlsEiUKCUF3YXJkTGlz" + "dBgDIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYBCAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFriendsDetailReflection.Descriptor,
			TypeFriendsInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsActionCollectAllAck), (MessageParser)(object)FriendsActionCollectAllAck.Parser, new string[4] { "FriendsInfo", "FriendsList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
