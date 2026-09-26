using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeResourceChangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeResourceChangeDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlJlc291cmNlQ2hhbmdlRGV0YWlsLnByb3RvEgV0eXBlcxoZdHlw" + "ZS5SZXNvdXJjZURldGFpbC5wcm90byJKChRSZXNvdXJjZUNoYW5nZURldGFp" + "bBIlCgZVcGRhdGUYASADKAsyFS50eXBlcy5SZXNvdXJjZURldGFpbBILCgNE" + "ZWwYAiADKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeResourceDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ResourceChangeDetail), (MessageParser)(object)ResourceChangeDetail.Parser, new string[2] { "Update", "Del" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
