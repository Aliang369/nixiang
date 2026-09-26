using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeNoticeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeNoticeDetailReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLk5vdGljZURldGFpbC5wcm90bxIFdHlwZXMaF2VudW0uRU5vdGlj" + "ZVN0YXRlLnByb3RvIqgBCgxOb3RpY2VEZXRhaWwSCgoCSWQYASABKAkSDgoG" + "U29ydElkGAIgASgFEg0KBVRpdGxlGAMgASgJEhAKCFN1YlRpdGxlGAQgASgJ" + "Eg8KB0NvbnRlbnQYBSABKAkSEAoIRXhwaXJlVG0YBiABKAMSDAoETGluaxgH" + "IAEoCRIqCgZTdGF0dXMYCCABKA4yGi5FTm90aWNlU3RhdGUuRU5vdGljZVN0" + "YXRlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumENoticeStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(NoticeDetail), (MessageParser)(object)NoticeDetail.Parser, new string[8] { "Id", "SortId", "Title", "SubTitle", "Content", "ExpireTm", "Link", "Status" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
