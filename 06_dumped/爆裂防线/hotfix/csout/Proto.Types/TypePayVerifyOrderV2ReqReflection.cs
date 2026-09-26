using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePayVerifyOrderV2ReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePayVerifyOrderV2ReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlBheVZlcmlmeU9yZGVyVjJSZXEucHJvdG8SBXR5cGVzGhdlbnVt" + "LkVDaGFubmVsVHlwZS5wcm90byJTChNQYXlWZXJpZnlPcmRlclYyUmVxEi4K" + "CkFwcENoYW5uZWwYASABKA4yGi5FQ2hhbm5lbFR5cGUuRUNoYW5uZWxUeXBl" + "EgwKBERhdGEYAiABKAlCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChannelTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PayVerifyOrderV2Req), (MessageParser)(object)PayVerifyOrderV2Req.Parser, new string[2] { "AppChannel", "Data" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
