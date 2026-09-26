using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegendaryDataAck : IMessage<LegendaryDataAck>, IMessage, IEquatable<LegendaryDataAck>, IDeepCloneable<LegendaryDataAck>, IBufferMessage
{
	private static readonly MessageParser<LegendaryDataAck> _parser = new MessageParser<LegendaryDataAck>((Func<LegendaryDataAck>)(() => new LegendaryDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int LegendaryInfoFieldNumber = 1;

	private LegendaryInfo legendaryInfo_;

	public const int HelpInfoFieldNumber = 2;

	private HelpInfo helpInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegendaryDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegendaryDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegendaryInfo LegendaryInfo
	{
		get
		{
			return legendaryInfo_;
		}
		set
		{
			legendaryInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpInfo HelpInfo
	{
		get
		{
			return helpInfo_;
		}
		set
		{
			helpInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegendaryDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegendaryDataAck(LegendaryDataAck other)
		: this()
	{
		legendaryInfo_ = ((other.legendaryInfo_ != null) ? other.legendaryInfo_.Clone() : null);
		helpInfo_ = ((other.helpInfo_ != null) ? other.helpInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegendaryDataAck Clone()
	{
		return new LegendaryDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegendaryDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegendaryDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(LegendaryInfo, other.LegendaryInfo))
		{
			return false;
		}
		if (!object.Equals(HelpInfo, other.HelpInfo))
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
		if (legendaryInfo_ != null)
		{
			num ^= LegendaryInfo.GetHashCode();
		}
		if (helpInfo_ != null)
		{
			num ^= HelpInfo.GetHashCode();
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
		if (legendaryInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegendaryInfo);
		}
		if (helpInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HelpInfo);
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
		if (legendaryInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegendaryInfo);
		}
		if (helpInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HelpInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegendaryDataAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.legendaryInfo_ != null)
		{
			if (legendaryInfo_ == null)
			{
				LegendaryInfo = new LegendaryInfo();
			}
			LegendaryInfo.MergeFrom(other.LegendaryInfo);
		}
		if (other.helpInfo_ != null)
		{
			if (helpInfo_ == null)
			{
				HelpInfo = new HelpInfo();
			}
			HelpInfo.MergeFrom(other.HelpInfo);
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
				if (legendaryInfo_ == null)
				{
					LegendaryInfo = new LegendaryInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegendaryInfo);
				break;
			case 18u:
				if (helpInfo_ == null)
				{
					HelpInfo = new HelpInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HelpInfo);
				break;
			}
		}
	}
}
