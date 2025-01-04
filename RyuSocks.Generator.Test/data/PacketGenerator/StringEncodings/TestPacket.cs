using RyuSocks.Packets;

namespace LittleEndian
{
    public partial class ASCIIPacket : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength))]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength))]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6)]
        public partial string String4 { get; set; }
    }

    public partial class UnicodePacket : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.Unicode)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.Unicode)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.Unicode)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.Unicode)]
        public partial string String4 { get; set; }
    }

    public partial class UTF7Packet : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.UTF7)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.UTF7)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.UTF7)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.UTF7)]
        public partial string String4 { get; set; }
    }

    public partial class UTF8Packet : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.UTF8)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.UTF8)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.UTF8)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.UTF8)]
        public partial string String4 { get; set; }
    }

    public partial class UTF32Packet : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.UTF32)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.UTF32)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.UTF32)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.UTF32)]
        public partial string String4 { get; set; }
    }
}

namespace BigEndian
{
    public partial class ASCIIPacket : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, IsBigEndian = true)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), IsBigEndian = true)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), IsBigEndian = true)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, IsBigEndian = true)]
        public partial string String4 { get; set; }
    }

    public partial class UnicodePacket : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.Unicode, IsBigEndian = true)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.Unicode, IsBigEndian = true)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.Unicode, IsBigEndian = true)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.Unicode, IsBigEndian = true)]
        public partial string String4 { get; set; }
    }

    public partial class UTF7Packet : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.UTF7, IsBigEndian = true)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.UTF7, IsBigEndian = true)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.UTF7, IsBigEndian = true)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.UTF7, IsBigEndian = true)]
        public partial string String4 { get; set; }
    }

    public partial class UTF8Packet : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.UTF8, IsBigEndian = true)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.UTF8, IsBigEndian = true)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.UTF8, IsBigEndian = true)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.UTF8, IsBigEndian = true)]
        public partial string String4 { get; set; }
    }

    public partial class UTF32Packet : Packet
    {
        protected int AStringByteLength => 2;

        public int ReadWriteByteLength { get; set; } = 34;

        [PacketField(2, Length = 10, StringEncoding = StringEncoding.UTF32, IsBigEndian = true)]
        private partial string String1 { get; set; }

        [PacketField(12, LengthMember = nameof(AStringByteLength), StringEncoding = StringEncoding.UTF32, IsBigEndian = true)]
        partial string String2 { get; set; }

        [PacketField(14, LengthMember = nameof(ReadWriteByteLength), StringEncoding = StringEncoding.UTF32, IsBigEndian = true)]
        protected partial string String3 { get; set; }

        [PacketField(48, Length = 6, MinLength = 2, MaxLength = 6, StringEncoding = StringEncoding.UTF32, IsBigEndian = true)]
        public partial string String4 { get; set; }
    }
}

