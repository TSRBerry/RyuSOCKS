/*
 * Copyright (C) RyuSOCKS
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2,
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

namespace RyuSocks.Generator.Packet
{
    internal record struct FieldTypeModel(
        string Name,
        bool IsArray,
        bool IsEnum,
        bool IsStruct,
        ActualType ActualType
    );

    internal record struct PacketFieldModel(
        int Offset,
        string OffsetMember,
        int Length,
        string LengthMember,
        ActualType LengthMemberType,
        Permissions LengthMemberPermissions,
        int MinLength,
        int MaxLength,
        bool IsBigEndian,
        FieldTypeModel FieldType,
        StringEncoding FieldStringEncoding,
        string PropertyName,
        string PropertyAccessModifiers,
        string PropertyGetterModifiers,
        string PropertySetterModifiers,
        string ClassName,
        string[] Imports,
        string ValidationMethodName
    );
}
