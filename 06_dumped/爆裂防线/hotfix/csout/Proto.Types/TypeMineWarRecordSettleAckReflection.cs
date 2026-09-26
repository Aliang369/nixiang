using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarRecordSettleAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarRecordSettleAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLk1pbmVXYXJSZWNvcmRTZXR0bGVBY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8a" + "HnR5cGUuTWluZVdhclJlY29yZERldGFpbC5wcm90byKWAQoWTWluZVdhclJl" + "Y29yZFNldHRsZUFjaxIuCgpSZWNvcmRMaXN0GAEgAygLMhoudHlwZXMuTWlu" + "ZVdhclJlY29yZERldGFpbBIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5B" + "d2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNo" + "YW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMineWarRecordDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarRecordSettleAck), (MessageParser)(object)MineWarRecordSettleAck.Parser, new string[3] { "RecordList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
