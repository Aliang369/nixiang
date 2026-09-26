using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsActionGiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsActionGiveAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkZyaWVuZHNBY3Rpb25HaXZlQWNrLnByb3RvEgV0eXBlcxoYdHlw" + "ZS5GcmllbmRzRGV0YWlsLnByb3RvIkMKFEZyaWVuZHNBY3Rpb25HaXZlQWNr" + "EisKDUZyaWVuZHNEZXRhaWwYASABKAsyFC50eXBlcy5GcmllbmRzRGV0YWls" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFriendsDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsActionGiveAck), (MessageParser)(object)FriendsActionGiveAck.Parser, new string[1] { "FriendsDetail" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
