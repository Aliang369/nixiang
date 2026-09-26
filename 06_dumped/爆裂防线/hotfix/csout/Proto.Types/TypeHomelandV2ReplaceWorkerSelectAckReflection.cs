using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2ReplaceWorkerSelectAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2ReplaceWorkerSelectAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLkhvbWVsYW5kVjJSZXBsYWNlV29ya2VyU2VsZWN0QWNrLnByb3Rv" + "EgV0eXBlcxoXdHlwZS5Ib21lbGFuZEluZm8ucHJvdG8iTQogSG9tZWxhbmRW" + "MlJlcGxhY2VXb3JrZXJTZWxlY3RBY2sSKQoMSG9tZWxhbmRJbmZvGAEgASgL" + "MhMudHlwZXMuSG9tZWxhbmRJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeHomelandInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2ReplaceWorkerSelectAck), (MessageParser)(object)HomelandV2ReplaceWorkerSelectAck.Parser, new string[1] { "HomelandInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
