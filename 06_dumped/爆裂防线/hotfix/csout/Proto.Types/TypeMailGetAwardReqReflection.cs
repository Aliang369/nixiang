using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeMailGetAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMailGetAwardReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLk1haWxHZXRBd2FyZFJlcS5wcm90bxIFdHlwZXMaGGVudW0uRUxh" + "bmd1YWdlVHlwZS5wcm90byJNCg9NYWlsR2V0QXdhcmRSZXESDgoGTWFpbElk" + "GAEgASgDEioKBExhbmcYAiABKA4yHC5FTGFuZ3VhZ2VUeXBlLkVMYW5ndWFn" + "ZVR5cGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELanguageTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MailGetAwardReq), (MessageParser)(object)MailGetAwardReq.Parser, new string[2] { "MailId", "Lang" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
