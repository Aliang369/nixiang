using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESuitGradeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESuitGradeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVTdWl0R3JhZGUucHJvdG8SCkVTdWl0R3JhZGUqMwoKRVN1aXRH" + "cmFkZRIOCgpFbXB0eVZhbHVlEAASCgoGTm9ybWFsEAESCQoFU3VwZXIQAkIu" + "WgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVt" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESuitGrade) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
