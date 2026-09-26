using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumETranslateTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumETranslateTypeReflection()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChllbnVtLkVUcmFuc2xhdGVUeXBlLnByb3RvEg5FVHJhbnNsYXRlVHlwZSqO" + "AgoORVRyYW5zbGF0ZVR5cGUSDgoKRW1wdHlWYWx1ZRAAEgYKAlpoEAESBgoC" + "RW4QAhIGCgJKcBADEgcKA0tvchAEEgcKA0ZyYRAFEgcKA1NwYRAGEgYKAlRo" + "EAcSBwoDQXJhEAgSBgoCUnUQCRIGCgJQdBAKEgYKAkRlEAsSBgoCSXQQDBIG" + "CgJFbBANEgYKAk5sEA4SBgoCUGwQDxIHCgNCdWwQEBIHCgNFc3QQERIHCgNE" + "YW4QEhIHCgNGaW4QExIGCgJDcxAUEgcKA1JvbRAVEgcKA1NsbxAWEgcKA1N3" + "ZRAXEgYKAkh1EBgSBwoDQ2h0EBkSBwoDVmllEBoSBwoDRmlsEBsSBgoCVHIQ" + "HEIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xF" + "bnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ETranslateType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
