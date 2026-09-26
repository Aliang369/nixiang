using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarPointStateDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPointStateDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxlZ2lvbldhclBvaW50U3RhdGVEZXRhaWwucHJvdG8SBXR5cGVz" + "GhplbnVtLkVMZWdpb25XYXJGb3JjZS5wcm90bxokdHlwZS5MZWdpb25XYXJQ" + "b2ludEZvcmNlRGV0YWlsLnByb3RvIp8BChlMZWdpb25XYXJQb2ludFN0YXRl" + "RGV0YWlsEg8KB1BvaW50SWQYASABKAUSNwoNT2NjdXB5Rm9yY2VJZBgCIAEo" + "DjIgLkVMZWdpb25XYXJGb3JjZS5FTGVnaW9uV2FyRm9yY2USOAoOUG9pbnRG" + "b3JjZUxpc3QYAyADKAsyIC50eXBlcy5MZWdpb25XYXJQb2ludEZvcmNlRGV0" + "YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumELegionWarForceReflection.Descriptor,
			TypeLegionWarPointForceDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPointStateDetail), (MessageParser)(object)LegionWarPointStateDetail.Parser, new string[3] { "PointId", "OccupyForceId", "PointForceList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
