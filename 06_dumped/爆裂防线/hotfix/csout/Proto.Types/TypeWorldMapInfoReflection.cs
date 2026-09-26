using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldMapInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldMapInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLldvcmxkTWFwSW5mby5wcm90bxIFdHlwZXMaHXR5cGUuV29ybGRN" + "YXBHcmlkRGV0YWlsLnByb3RvIlsKDFdvcmxkTWFwSW5mbxIOCgZNYXhDb2wY" + "ASABKAUSDgoGTWF4Um93GAIgASgFEisKCEdyaWRMaXN0GAMgAygLMhkudHlw" + "ZXMuV29ybGRNYXBHcmlkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldMapGridDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldMapInfo), (MessageParser)(object)WorldMapInfo.Parser, new string[3] { "MaxCol", "MaxRow", "GridList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
