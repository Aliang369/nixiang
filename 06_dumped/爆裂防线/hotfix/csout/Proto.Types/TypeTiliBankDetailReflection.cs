using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTiliBankDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTiliBankDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlRpbGlCYW5rRGV0YWlsLnByb3RvEgV0eXBlcxoXZW51bS5FQ29t" + "bW9uU3RhdGUucHJvdG8iXgoOVGlsaUJhbmtEZXRhaWwSDgoGVGlsaVRtGAEg" + "ASgDEhEKCVRpbGlDb3VudBgCIAEoBRIpCgVTdGF0ZRgDIAEoDjIaLkVDb21t" + "b25TdGF0ZS5FQ29tbW9uU3RhdGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECommonStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TiliBankDetail), (MessageParser)(object)TiliBankDetail.Parser, new string[3] { "TiliTm", "TiliCount", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
