using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionTipsInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionTipsInfoAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkxlZ2lvblRpcHNJbmZvQWNrLnByb3RvEgV0eXBlcxoZdHlwZS5M" + "ZWdpb25UaXBzSW5mby5wcm90byJCChFMZWdpb25UaXBzSW5mb0FjaxItCg5M" + "ZWdpb25UaXBzSW5mbxgBIAEoCzIVLnR5cGVzLkxlZ2lvblRpcHNJbmZvQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionTipsInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionTipsInfoAck), (MessageParser)(object)LegionTipsInfoAck.Parser, new string[1] { "LegionTipsInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
