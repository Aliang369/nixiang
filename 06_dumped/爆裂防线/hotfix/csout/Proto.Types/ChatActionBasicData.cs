using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChatActionBasicData : IMessage<ChatActionBasicData>, IMessage, IEquatable<ChatActionBasicData>, IDeepCloneable<ChatActionBasicData>, IBufferMessage
{
	private static readonly MessageParser<ChatActionBasicData> _parser = new MessageParser<ChatActionBasicData>((Func<ChatActionBasicData>)(() => new ChatActionBasicData()));

	private UnknownFieldSet _unknownFields;

	public const int TextFieldNumber = 1;

	private ContentDetail text_;

	public const int LegionInviteFieldNumber = 2;

	private LegionBaseInfo legionInvite_;

	public const int BarShareFieldNumber = 3;

	private CommonAwardInfo barShare_;

	public const int HeroShareFieldNumber = 4;

	private HeroDetail heroShare_;

	public const int EquipShareFieldNumber = 5;

	private EquipDetail equipShare_;

	public const int BattleResultFieldNumber = 6;

	private BattleResultInfo battleResult_;

	public const int WorldLairFieldNumber = 7;

	private WorldLairDetail worldLair_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChatActionBasicData> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChatActionBasicDataReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ContentDetail Text
	{
		get
		{
			return text_;
		}
		set
		{
			text_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBaseInfo LegionInvite
	{
		get
		{
			return legionInvite_;
		}
		set
		{
			legionInvite_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CommonAwardInfo BarShare
	{
		get
		{
			return barShare_;
		}
		set
		{
			barShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroDetail HeroShare
	{
		get
		{
			return heroShare_;
		}
		set
		{
			heroShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipDetail EquipShare
	{
		get
		{
			return equipShare_;
		}
		set
		{
			equipShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo BattleResult
	{
		get
		{
			return battleResult_;
		}
		set
		{
			battleResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairDetail WorldLair
	{
		get
		{
			return worldLair_;
		}
		set
		{
			worldLair_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionBasicData()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionBasicData(ChatActionBasicData other)
		: this()
	{
		text_ = ((other.text_ != null) ? other.text_.Clone() : null);
		legionInvite_ = ((other.legionInvite_ != null) ? other.legionInvite_.Clone() : null);
		barShare_ = ((other.barShare_ != null) ? other.barShare_.Clone() : null);
		heroShare_ = ((other.heroShare_ != null) ? other.heroShare_.Clone() : null);
		equipShare_ = ((other.equipShare_ != null) ? other.equipShare_.Clone() : null);
		battleResult_ = ((other.battleResult_ != null) ? other.battleResult_.Clone() : null);
		worldLair_ = ((other.worldLair_ != null) ? other.worldLair_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionBasicData Clone()
	{
		return new ChatActionBasicData(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChatActionBasicData);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChatActionBasicData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Text, other.Text))
		{
			return false;
		}
		if (!object.Equals(LegionInvite, other.LegionInvite))
		{
			return false;
		}
		if (!object.Equals(BarShare, other.BarShare))
		{
			return false;
		}
		if (!object.Equals(HeroShare, other.HeroShare))
		{
			return false;
		}
		if (!object.Equals(EquipShare, other.EquipShare))
		{
			return false;
		}
		if (!object.Equals(BattleResult, other.BattleResult))
		{
			return false;
		}
		if (!object.Equals(WorldLair, other.WorldLair))
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
		if (text_ != null)
		{
			num ^= Text.GetHashCode();
		}
		if (legionInvite_ != null)
		{
			num ^= LegionInvite.GetHashCode();
		}
		if (barShare_ != null)
		{
			num ^= BarShare.GetHashCode();
		}
		if (heroShare_ != null)
		{
			num ^= HeroShare.GetHashCode();
		}
		if (equipShare_ != null)
		{
			num ^= EquipShare.GetHashCode();
		}
		if (battleResult_ != null)
		{
			num ^= BattleResult.GetHashCode();
		}
		if (worldLair_ != null)
		{
			num ^= WorldLair.GetHashCode();
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
		if (text_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Text);
		}
		if (legionInvite_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionInvite);
		}
		if (barShare_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BarShare);
		}
		if (heroShare_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HeroShare);
		}
		if (equipShare_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EquipShare);
		}
		if (battleResult_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleResult);
		}
		if (worldLair_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldLair);
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
		if (text_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Text);
		}
		if (legionInvite_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionInvite);
		}
		if (barShare_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BarShare);
		}
		if (heroShare_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HeroShare);
		}
		if (equipShare_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EquipShare);
		}
		if (battleResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleResult);
		}
		if (worldLair_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldLair);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChatActionBasicData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.text_ != null)
		{
			if (text_ == null)
			{
				Text = new ContentDetail();
			}
			Text.MergeFrom(other.Text);
		}
		if (other.legionInvite_ != null)
		{
			if (legionInvite_ == null)
			{
				LegionInvite = new LegionBaseInfo();
			}
			LegionInvite.MergeFrom(other.LegionInvite);
		}
		if (other.barShare_ != null)
		{
			if (barShare_ == null)
			{
				BarShare = new CommonAwardInfo();
			}
			BarShare.MergeFrom(other.BarShare);
		}
		if (other.heroShare_ != null)
		{
			if (heroShare_ == null)
			{
				HeroShare = new HeroDetail();
			}
			HeroShare.MergeFrom(other.HeroShare);
		}
		if (other.equipShare_ != null)
		{
			if (equipShare_ == null)
			{
				EquipShare = new EquipDetail();
			}
			EquipShare.MergeFrom(other.EquipShare);
		}
		if (other.battleResult_ != null)
		{
			if (battleResult_ == null)
			{
				BattleResult = new BattleResultInfo();
			}
			BattleResult.MergeFrom(other.BattleResult);
		}
		if (other.worldLair_ != null)
		{
			if (worldLair_ == null)
			{
				WorldLair = new WorldLairDetail();
			}
			WorldLair.MergeFrom(other.WorldLair);
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
				if (text_ == null)
				{
					Text = new ContentDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Text);
				break;
			case 18u:
				if (legionInvite_ == null)
				{
					LegionInvite = new LegionBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionInvite);
				break;
			case 26u:
				if (barShare_ == null)
				{
					BarShare = new CommonAwardInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BarShare);
				break;
			case 34u:
				if (heroShare_ == null)
				{
					HeroShare = new HeroDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HeroShare);
				break;
			case 42u:
				if (equipShare_ == null)
				{
					EquipShare = new EquipDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EquipShare);
				break;
			case 50u:
				if (battleResult_ == null)
				{
					BattleResult = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleResult);
				break;
			case 58u:
				if (worldLair_ == null)
				{
					WorldLair = new WorldLairDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldLair);
				break;
			}
		}
	}
}
