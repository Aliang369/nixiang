using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementBarDrawAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementBarDrawAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkFkdmVydGlzZW1lbnRCYXJEcmF3QWNrLnByb3RvEgV0eXBlcxoc" + "dHlwZS5BZHZlcnRpc2VtZW50SW5mby5wcm90bxoWdHlwZS5Bd2FyZERldGFp" + "bC5wcm90bxoSdHlwZS5CYXJJbmZvLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2Uu" + "cHJvdG8ivQEKF0FkdmVydGlzZW1lbnRCYXJEcmF3QWNrEjMKEUFkdmVydGlz" + "ZW1lbnRJbmZvGAEgASgLMhgudHlwZXMuQWR2ZXJ0aXNlbWVudEluZm8SHwoH" + "QmFySW5mbxgCIAEoCzIOLnR5cGVzLkJhckluZm8SJQoJQXdhcmRMaXN0GAMg" + "AygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIR" + "LnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeBarInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementBarDrawAck), (MessageParser)(object)AdvertisementBarDrawAck.Parser, new string[4] { "AdvertisementInfo", "BarInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
