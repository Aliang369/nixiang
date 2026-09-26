using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantUpgradeAttrReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantUpgradeAttrReqReflection()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlNlcnZhbnRVcGdyYWRlQXR0clJlcS5wcm90bxIFdHlwZXMiKgoV" + "U2VydmFudFVwZ3JhZGVBdHRyUmVxEhEKCVNlcnZhbnRJZBgBIAEoCUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantUpgradeAttrReq), (MessageParser)(object)ServantUpgradeAttrReq.Parser, new string[1] { "ServantId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
