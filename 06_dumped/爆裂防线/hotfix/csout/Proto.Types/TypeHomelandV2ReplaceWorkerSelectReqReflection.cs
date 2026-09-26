using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2ReplaceWorkerSelectReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2ReplaceWorkerSelectReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLkhvbWVsYW5kVjJSZXBsYWNlV29ya2VyU2VsZWN0UmVxLnByb3Rv" + "EgV0eXBlcyJGCiBIb21lbGFuZFYyUmVwbGFjZVdvcmtlclNlbGVjdFJlcRIQ" + "CghXb3JrZXJJZBgBIAEoBRIQCghTZWxlY3RJZBgCIAEoBUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2ReplaceWorkerSelectReq), (MessageParser)(object)HomelandV2ReplaceWorkerSelectReq.Parser, new string[2] { "WorkerId", "SelectId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
