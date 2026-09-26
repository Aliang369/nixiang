using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HeroQualityUpgradeAck : IMessage<HeroQualityUpgradeAck>, IMessage, IEquatable<HeroQualityUpgradeAck>, IDeepCloneable<HeroQualityUpgradeAck>, IBufferMessage
{
	private static readonly MessageParser<HeroQualityUpgradeAck> _parser = new MessageParser<HeroQualityUpgradeAck>((Func<HeroQualityUpgradeAck>)(() => new HeroQualityUpgradeAck()));

	private UnknownFieldSet _unknownFields;

	public const int DataChangeFieldNumber = 1;

	private DataChange dataChange_;

	public const int OldQualityFieldNumber = 2;

	private int oldQuality_;

	public const int OldQualityStarFieldNumber = 3;

	private int oldQualityStar_;

	public const int NewQualityFieldNumber = 4;

	private int newQuality_;

	public const int NewQualityStarFieldNumber = 5;

	private int newQualityStar_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HeroQualityUpgradeAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHeroQualityUpgradeAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OldQuality
	{
		get
		{
			return oldQuality_;
		}
		set
		{
			oldQuality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OldQualityStar
	{
		get
		{
			return oldQualityStar_;
		}
		set
		{
			oldQualityStar_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NewQuality
	{
		get
		{
			return newQuality_;
		}
		set
		{
			newQuality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NewQualityStar
	{
		get
		{
			return newQualityStar_;
		}
		set
		{
			newQualityStar_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroQualityUpgradeAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroQualityUpgradeAck(HeroQualityUpgradeAck other)
		: this()
	{
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		oldQuality_ = other.oldQuality_;
		oldQualityStar_ = other.oldQualityStar_;
		newQuality_ = other.newQuality_;
		newQualityStar_ = other.newQualityStar_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroQualityUpgradeAck Clone()
	{
		return new HeroQualityUpgradeAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HeroQualityUpgradeAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HeroQualityUpgradeAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(DataChange, other.DataChange))
		{
			return false;
		}
		if (OldQuality != other.OldQuality)
		{
			return false;
		}
		if (OldQualityStar != other.OldQualityStar)
		{
			return false;
		}
		if (NewQuality != other.NewQuality)
		{
			return false;
		}
		if (NewQualityStar != other.NewQualityStar)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
		}
		if (OldQuality != 0)
		{
			num ^= OldQuality.GetHashCode();
		}
		if (OldQualityStar != 0)
		{
			num ^= OldQualityStar.GetHashCode();
		}
		if (NewQuality != 0)
		{
			num ^= NewQuality.GetHashCode();
		}
		if (NewQualityStar != 0)
		{
			num ^= NewQualityStar.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
		}
		if (OldQuality != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(OldQuality);
		}
		if (OldQualityStar != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(OldQualityStar);
		}
		if (NewQuality != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(NewQuality);
		}
		if (NewQualityStar != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(NewQualityStar);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (OldQuality != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OldQuality);
		}
		if (OldQualityStar != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OldQualityStar);
		}
		if (NewQuality != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NewQuality);
		}
		if (NewQualityStar != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NewQualityStar);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HeroQualityUpgradeAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
		}
		if (other.OldQuality != 0)
		{
			OldQuality = other.OldQuality;
		}
		if (other.OldQualityStar != 0)
		{
			OldQualityStar = other.OldQualityStar;
		}
		if (other.NewQuality != 0)
		{
			NewQuality = other.NewQuality;
		}
		if (other.NewQualityStar != 0)
		{
			NewQualityStar = other.NewQualityStar;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			case 16u:
				OldQuality = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				OldQualityStar = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				NewQuality = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				NewQualityStar = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
