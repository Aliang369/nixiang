using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTrainDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainDetailReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLlRyYWluRGV0YWlsLnByb3RvEgV0eXBlcxoTZW51bS5FUXVhbGl0" + "eS5wcm90bxoedHlwZS5UcmFpbkNhcnJpYWdlRGV0YWlsLnByb3RvIrMBCgtU" + "cmFpbkRldGFpbBITCgtUcmFuc3BvcnRJZBgBIAEoBRIjCgdRdWFsaXR5GAIg" + "ASgOMhIuRVF1YWxpdHkuRVF1YWxpdHkSDwoHU3RhcnRUbRgDIAEoAxIQCghB" + "cnJpdmVUbRgEIAEoAxIVCg1DcmVhdG9yUm9sZUlkGAUgASgDEjAKDENhcnJp" + "YWdlTGlzdBgGIAMoCzIaLnR5cGVzLlRyYWluQ2FycmlhZ2VEZXRhaWxCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEQualityReflection.Descriptor,
			TypeTrainCarriageDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainDetail), (MessageParser)(object)TrainDetail.Parser, new string[6] { "TransportId", "Quality", "StartTm", "ArriveTm", "CreatorRoleId", "CarriageList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
