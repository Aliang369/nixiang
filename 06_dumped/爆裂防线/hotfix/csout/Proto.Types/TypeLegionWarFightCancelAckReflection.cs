using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarFightCancelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarFightCancelAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxlZ2lvbldhckZpZ2h0Q2FuY2VsQWNrLnByb3RvEgV0eXBlcxoc" + "dHlwZS5MZWdpb25XYXJCYXNlSW5mby5wcm90byJOChdMZWdpb25XYXJGaWdo" + "dENhbmNlbEFjaxIzChFMZWdpb25XYXJCYXNlSW5mbxgBIAEoCzIYLnR5cGVz" + "LkxlZ2lvbldhckJhc2VJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionWarBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarFightCancelAck), (MessageParser)(object)LegionWarFightCancelAck.Parser, new string[1] { "LegionWarBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
