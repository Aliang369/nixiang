using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EEquipType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Neuron")]
	Neuron,
	[OriginalName("Mnsr")]
	Mnsr,
	[OriginalName("ExoSkeleton")]
	ExoSkeleton
}
