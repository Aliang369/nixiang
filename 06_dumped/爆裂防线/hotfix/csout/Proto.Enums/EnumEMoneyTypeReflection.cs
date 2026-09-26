using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEMoneyTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEMoneyTypeReflection()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVNb25leVR5cGUucHJvdG8SCkVNb25leVR5cGUqzQMKCkVNb25l" + "eVR5cGUSDgoKRW1wdHlWYWx1ZRAAEggKBEdvbGQQARILCgdEaWFtb25kEAIS" + "CwoHVm91Y2hlchADEggKBFRpbGkQBBILCgdSb2xlRXhwEAUSCgoGVmlwRXhw" + "EAYSDQoJQmFyTm9ybWFsEAcSDgoKQmFyU3BlY2lhbBAIEgwKCEhlcm9Tb3Vs" + "EAsSDQoJRXF1aXBTb3VsEAwSDAoIRnJpZW5kbHkQDRIJCgVIb25vchAOEgkK" + "BVRvb3RoEA8SDQoJTGVnZW5kYXJ5EBASDQoJQXJlbmFDb2luEBESDAoIR2Vu" + "ZUNvaW4QExIPCgtCYXJUcmVhc3VyZRAUEhMKD0JhclRyZWFzdXJlUGx1cxAV" + "EhEKDUxpbWl0VGltZUhlcm8QHxIUChBMaW1pdFRpbWVDYXRjaGVyECESFQoR" + "TGltaXRUaW1lUm91bGV0dGUQIhIPCgtDcnlzdGFsQmFsbBAjEhQKEEZhY2ls" + "aXR5Q2hlc3RLZXkQJBIPCgtCb3NzU3ViamVjdBAlEg8KC1RyYWluVGlja2V0" + "ECYSDQoJVGFjdGljQmFyECcSDgoKVGFjdGljQ29pbhAoEg4KClNlYXNvbkNv" + "aW4QKUIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90" + "b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EMoneyType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
