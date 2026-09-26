using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePushSetDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePushSetDataAckReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlB1c2hTZXREYXRhQWNrLnByb3RvEgV0eXBlcxoWdHlwZS5QdXNo" + "U2V0SW5mby5wcm90byI5Cg5QdXNoU2V0RGF0YUFjaxInCgtQdXNoU2V0SW5m" + "bxgBIAEoCzISLnR5cGVzLlB1c2hTZXRJbmZvQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypePushSetInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PushSetDataAck), (MessageParser)(object)PushSetDataAck.Parser, new string[1] { "PushSetInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
