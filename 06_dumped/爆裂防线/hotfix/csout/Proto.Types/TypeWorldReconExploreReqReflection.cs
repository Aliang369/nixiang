using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldReconExploreReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldReconExploreReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLldvcmxkUmVjb25FeHBsb3JlUmVxLnByb3RvEgV0eXBlcyI3ChRX" + "b3JsZFJlY29uRXhwbG9yZVJlcRIPCgdSZWNvbklkGAEgASgFEg4KBkdyaWRJ" + "ZBgCIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldReconExploreReq), (MessageParser)(object)WorldReconExploreReq.Parser, new string[2] { "ReconId", "GridId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
