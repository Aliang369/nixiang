using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MainQuestInfo : IMessage<MainQuestInfo>, IMessage, IEquatable<MainQuestInfo>, IDeepCloneable<MainQuestInfo>, IBufferMessage
{
	private static readonly MessageParser<MainQuestInfo> _parser = new MessageParser<MainQuestInfo>((Func<MainQuestInfo>)(() => new MainQuestInfo()));

	private UnknownFieldSet _unknownFields;

	public const int MainLevelFieldNumber = 1;

	private int mainLevel_;

	public const int NextLevelFieldNumber = 2;

	private int nextLevel_;

	public const int QuestListFieldNumber = 3;

	private static readonly FieldCodec<MainQuestDetail> _repeated_questList_codec = FieldCodec.ForMessage<MainQuestDetail>(26u, MainQuestDetail.Parser);

	private readonly RepeatedField<MainQuestDetail> questList_ = new RepeatedField<MainQuestDetail>();

	public const int DoubleAwardFieldNumber = 4;

	private int doubleAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MainQuestInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMainQuestInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MainLevel
	{
		get
		{
			return mainLevel_;
		}
		set
		{
			mainLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NextLevel
	{
		get
		{
			return nextLevel_;
		}
		set
		{
			nextLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MainQuestDetail> QuestList => questList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DoubleAward
	{
		get
		{
			return doubleAward_;
		}
		set
		{
			doubleAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MainQuestInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MainQuestInfo(MainQuestInfo other)
		: this()
	{
		mainLevel_ = other.mainLevel_;
		nextLevel_ = other.nextLevel_;
		questList_ = other.questList_.Clone();
		doubleAward_ = other.doubleAward_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MainQuestInfo Clone()
	{
		return new MainQuestInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MainQuestInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MainQuestInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MainLevel != other.MainLevel)
		{
			return false;
		}
		if (NextLevel != other.NextLevel)
		{
			return false;
		}
		if (!questList_.Equals(other.questList_))
		{
			return false;
		}
		if (DoubleAward != other.DoubleAward)
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
		if (MainLevel != 0)
		{
			num ^= MainLevel.GetHashCode();
		}
		if (NextLevel != 0)
		{
			num ^= NextLevel.GetHashCode();
		}
		num ^= ((object)questList_).GetHashCode();
		if (DoubleAward != 0)
		{
			num ^= DoubleAward.GetHashCode();
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
		if (MainLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MainLevel);
		}
		if (NextLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(NextLevel);
		}
		questList_.WriteTo(ref output, _repeated_questList_codec);
		if (DoubleAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DoubleAward);
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
		if (MainLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MainLevel);
		}
		if (NextLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NextLevel);
		}
		num += questList_.CalculateSize(_repeated_questList_codec);
		if (DoubleAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DoubleAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MainQuestInfo other)
	{
		if (other != null)
		{
			if (other.MainLevel != 0)
			{
				MainLevel = other.MainLevel;
			}
			if (other.NextLevel != 0)
			{
				NextLevel = other.NextLevel;
			}
			questList_.Add((IEnumerable<MainQuestDetail>)other.questList_);
			if (other.DoubleAward != 0)
			{
				DoubleAward = other.DoubleAward;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
			case 8u:
				MainLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				NextLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				questList_.AddEntriesFrom(ref input, _repeated_questList_codec);
				break;
			case 32u:
				DoubleAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
