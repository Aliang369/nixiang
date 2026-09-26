using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityEventRollingAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityEventRollingAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkZhY2lsaXR5RXZlbnRSb2xsaW5nQWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3Rv" + "Ghp0eXBlLkZhY2lsaXR5TWFwSW5mby5wcm90bxogdHlwZS5GYWNpbGl0eVJv" + "bGxpbmdEZXRhaWwucHJvdG8aHXR5cGUuRmFjaWxpdHlSb29tRGV0YWlsLnBy" + "b3RvIvYBChdGYWNpbGl0eUV2ZW50Um9sbGluZ0FjaxIvCg9GYWNpbGl0eU1h" + "cEluZm8YASABKAsyFi50eXBlcy5GYWNpbGl0eU1hcEluZm8SLQoKUm9vbURl" + "dGFpbBgCIAEoCzIZLnR5cGVzLkZhY2lsaXR5Um9vbURldGFpbBItCgdSb2xs" + "aW5nGAMgASgLMhwudHlwZXMuRmFjaWxpdHlSb2xsaW5nRGV0YWlsEiUKCUF3" + "YXJkTGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFu" + "Z2UYBSABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityRollingDetailReflection.Descriptor,
			TypeFacilityRoomDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityEventRollingAck), (MessageParser)(object)FacilityEventRollingAck.Parser, new string[5] { "FacilityMapInfo", "RoomDetail", "Rolling", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
