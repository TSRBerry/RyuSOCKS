using System;

namespace RyuSocks.Packets
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    [System.Diagnostics.Conditional("RyuSocks_PacketGenerator_DEBUG")]
    public sealed class PacketFieldAttribute : Attribute
    {
        private int offset = -1;
        private string offsetMember = string.Empty;

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref="Packet"/>.
        /// </summary>
        /// <param name="offset">The offset of this field in the packet.</param>
        public PacketFieldAttribute([System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)] int offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref="Packet"/>.
        /// </summary>
        /// <param name="offsetMember">The name of the member which specifies the offset of this field in the packet.</param>
        public PacketFieldAttribute(string offsetMember)
        {
            this.offsetMember = offsetMember;
        }

        /// <summary>
        /// The length of this field in bytes.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public int Length { get; set; } = -1;

        /// <summary>
        /// The name of the member which specifies the length of this field in bytes.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public string LengthMember { get; set; } = string.Empty;

        /// <summary>
        /// Whether this field is big endian.
        /// </summary>
        public bool IsBigEndian { get; set; } = false;

        /// <summary>
        /// The minimum length of the array or chars allowed in this field.
        /// </summary>
        public int MinLength { get; set; } = -1;

        /// <summary>
        /// The maximum length of the array or chars allowed in this field.
        /// </summary>
        public int MaxLength { get; set; } = -1;

        /// <summary>
        /// The size of the element type of the array.
        /// Only required if the element type is a class or a string.
        /// </summary>
        public int ElementSize { get; set; } = -1;

        /// <summary>
        /// The encoding of the underlying string.
        /// Only required for string properties.
        /// </summary>
        public StringEncoding StringEncoding = StringEncoding.ASCII;

        /// <summary>
        /// The name of the underlying type of the property type.
        /// Only required if the type of the property is source generated.
        /// </summary>
        public string AssumeGeneratedEnumType { get; set; } = string.Empty;

        /// <summary>
        /// The name of the method which should be invoked before the getter/setter is executed.
        /// </summary> 
        /// <remarks>
        /// The method type must be void and have one optional parameter with the same type as the property.
        /// If verification fails an exception should be thrown.
        /// </remarks>
        public string ValidationMethod { get; set; } = string.Empty;

        public int Offset => offset;
        public string OffsetMember => offsetMember;
    }
}
