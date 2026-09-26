using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsActionCollectAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsActionCollectAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkZyaWVuZHNBY3Rpb25Db2xsZWN0QWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3Rv" + "Ghh0eXBlLkZyaWVuZHNEZXRhaWwucHJvdG8aFnR5cGUuRnJpZW5kc0luZm8u" + "cHJvdG8ivQEKF0ZyaWVuZHNBY3Rpb25Db2xsZWN0QWNrEicKC0ZyaWVuZHNJ" + "bmZvGAEgASgLMhIudHlwZXMuRnJpZW5kc0luZm8SKwoNRnJpZW5kc0RldGFp" + "bBgCIAEoCzIULnR5cGVzLkZyaWVuZHNEZXRhaWwSJQoJQXdhcmRMaXN0GAMg" + "AygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIR" + "LnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFriendsDetailReflection.Descriptor,
			TypeFriendsInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsActionCollectAck), (MessageParser)(object)FriendsActionCollectAck.Parser, new string[4] { "FriendsInfo", "FriendsDetail", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
