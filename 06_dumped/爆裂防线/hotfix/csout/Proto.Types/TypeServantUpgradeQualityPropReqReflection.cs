using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantUpgradeQualityPropReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantUpgradeQualityPropReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLlNlcnZhbnRVcGdyYWRlUXVhbGl0eVByb3BSZXEucHJvdG8SBXR5" + "cGVzIkQKHFNlcnZhbnRVcGdyYWRlUXVhbGl0eVByb3BSZXESEQoJU2VydmFu" + "dElkGAEgASgJEhEKCVByb3BDb3VudBgCIAEoBUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantUpgradeQualityPropReq), (MessageParser)(object)ServantUpgradeQualityPropReq.Parser, new string[2] { "ServantId", "PropCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
