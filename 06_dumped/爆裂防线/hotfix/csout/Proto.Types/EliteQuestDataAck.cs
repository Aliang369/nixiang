using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EliteQuestDataAck : IMessage<EliteQuestDataAck>, IMessage, IEquatable<EliteQuestDataAck>, IDeepCloneable<EliteQuestDataAck>, IBufferMessage
{
	private static readonly MessageParser<EliteQuestDataAck> _parser = new MessageParser<EliteQuestDataAck>((Func<EliteQuestDataAck>)(() => new EliteQuestDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int EliteQuestInfoFieldNumber = 1;

	private EliteQuestInfo eliteQuestInfo_;

	public const int HelpInfoFieldNumber = 2;

	private HelpInfo helpInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EliteQuestDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEliteQuestDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestInfo EliteQuestInfo
	{
		get
		{
			return eliteQuestInfo_;
		}
		set
		{
			eliteQuestInfo_ = value;
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
	public EliteQuestDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestDataAck(EliteQuestDataAck other)
		: this()
	{
		eliteQuestInfo_ = ((other.eliteQuestInfo_ != null) ? other.eliteQuestInfo_.Clone() : null);
		helpInfo_ = ((other.helpInfo_ != null) ? other.helpInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestDataAck Clone()
	{
		return new EliteQuestDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EliteQuestDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EliteQuestDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(EliteQuestInfo, other.EliteQuestInfo))
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
		if (eliteQuestInfo_ != null)
		{
			num ^= EliteQuestInfo.GetHashCode();
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
		if (eliteQuestInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EliteQuestInfo);
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
		if (eliteQuestInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EliteQuestInfo);
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
	public void MergeFrom(EliteQuestDataAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.eliteQuestInfo_ != null)
		{
			if (eliteQuestInfo_ == null)
			{
				EliteQuestInfo = new EliteQuestInfo();
			}
			EliteQuestInfo.MergeFrom(other.EliteQuestInfo);
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
				if (eliteQuestInfo_ == null)
				{
					EliteQuestInfo = new EliteQuestInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EliteQuestInfo);
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
