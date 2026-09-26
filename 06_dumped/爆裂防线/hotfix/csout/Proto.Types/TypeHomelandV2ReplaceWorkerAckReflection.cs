using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2ReplaceWorkerAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2ReplaceWorkerAckReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkhvbWVsYW5kVjJSZXBsYWNlV29ya2VyQWNrLnByb3RvEgV0eXBl" + "cxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhd0eXBlLldvcmtlckRldGFpbC5w" + "cm90byJ/ChpIb21lbGFuZFYyUmVwbGFjZVdvcmtlckFjaxIQCghXb3JrZXJJ" + "ZBgBIAEoBRIoCgtSZXBsYWNlTGlzdBgCIAMoCzITLnR5cGVzLldvcmtlckRl" + "dGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeWorkerDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2ReplaceWorkerAck), (MessageParser)(object)HomelandV2ReplaceWorkerAck.Parser, new string[3] { "WorkerId", "ReplaceList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
