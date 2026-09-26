using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBarrageCmdLangDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBarrageCmdLangDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkJhcnJhZ2VDbWRMYW5nRGV0YWlsLnByb3RvEgV0eXBlcxoYZW51" + "bS5FTGFuZ3VhZ2VUeXBlLnByb3RvIlMKFEJhcnJhZ2VDbWRMYW5nRGV0YWls" + "EioKBExhbmcYASABKA4yHC5FTGFuZ3VhZ2VUeXBlLkVMYW5ndWFnZVR5cGUS" + "DwoHQ29udGVudBgCIAEoCUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELanguageTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BarrageCmdLangDetail), (MessageParser)(object)BarrageCmdLangDetail.Parser, new string[2] { "Lang", "Content" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
