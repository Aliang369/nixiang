using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeErrorCodeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeErrorCodeAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkVycm9yQ29kZUFjay5wcm90bxIFdHlwZXMaFWVudW0uRUVycm9y" + "Q29kZS5wcm90byJtCgxFcnJvckNvZGVBY2sSKQoJRXJyb3JDb2RlGAEgASgO" + "MhYuRUVycm9yQ29kZS5FRXJyb3JDb2RlEhMKC0Vycm9yUGFyYW1zGAIgAygJ" + "Eg0KBVJlcUlkGAMgASgFEg4KBlN5bmNJZBgEIAEoBUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEErrorCodeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ErrorCodeAck), (MessageParser)(object)ErrorCodeAck.Parser, new string[4] { "ErrorCode", "ErrorParams", "ReqId", "SyncId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
