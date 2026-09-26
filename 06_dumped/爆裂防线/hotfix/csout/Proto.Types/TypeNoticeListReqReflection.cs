using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeNoticeListReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeNoticeListReqReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLk5vdGljZUxpc3RSZXEucHJvdG8SBXR5cGVzGhdlbnVtLkVDaGFu" + "bmVsVHlwZS5wcm90bxoYZW51bS5FTGFuZ3VhZ2VUeXBlLnByb3RvGhhlbnVt" + "LkVQbGF0Zm9ybVR5cGUucHJvdG8imwEKDU5vdGljZUxpc3RSZXESLgoIUGxh" + "dGZvcm0YASABKA4yHC5FUGxhdGZvcm1UeXBlLkVQbGF0Zm9ybVR5cGUSLgoK" + "QXBwQ2hhbm5lbBgCIAEoDjIaLkVDaGFubmVsVHlwZS5FQ2hhbm5lbFR5cGUS" + "KgoETGFuZxgDIAEoDjIcLkVMYW5ndWFnZVR5cGUuRUxhbmd1YWdlVHlwZUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumELanguageTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(NoticeListReq), (MessageParser)(object)NoticeListReq.Parser, new string[3] { "Platform", "AppChannel", "Lang" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
