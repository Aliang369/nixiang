using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTruckTransportDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckTransportDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlRydWNrVHJhbnNwb3J0RGV0YWlsLnByb3RvEgV0eXBlcxoTZW51" + "bS5FUXVhbGl0eS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90byKaAQoU" + "VHJ1Y2tUcmFuc3BvcnREZXRhaWwSEwoLVHJhbnNwb3J0SWQYASABKAUSIwoH" + "UXVhbGl0eRgCIAEoDjISLkVRdWFsaXR5LkVRdWFsaXR5EhAKCEFycml2ZVRt" + "GAMgASgDEiUKCUF3YXJkTGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWls" + "Eg8KB1JvYkxpc3QYBSADKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEQualityReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckTransportDetail), (MessageParser)(object)TruckTransportDetail.Parser, new string[5] { "TransportId", "Quality", "ArriveTm", "AwardList", "RobList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
