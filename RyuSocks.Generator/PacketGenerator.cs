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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RyuSocks.Generator.Builder;
using RyuSocks.Generator.Packet;
using System;
using System.Linq;
using System.Threading;

namespace RyuSocks.Generator
{
    // TODO: Write an analyzer for PacketFieldAttribute
    //         - make sure (Offset and OffsetMember) and (Length and LengthMember) aren't used at the same time
    //         - check whether the attribute is used on a property within a class/subclass that extends Packet

    [Generator]
    public class PacketGenerator : IIncrementalGenerator
    {
        private const string Namespace = "RyuSocks.Packets";
        private const string AbstractClassName = "Packet";
        private const string PacketFieldAttributeName = "PacketFieldAttribute";

        #region SourceText
        private const string PacketFieldAttributeText = @"
using System;

namespace %NAMESPACE%
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    [System.Diagnostics.Conditional(""RyuSocks_PacketGenerator_DEBUG"")]
    public sealed class %ATTRIBUTE_NAME% : Attribute
    {
        private int offset = %DEFAULT_OFFSET%;
        private string offsetMember = %DEFAULT_OFFSET_MEMBER%;

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref=""%ABSTRACT_CLASS_NAME%""/>.
        /// </summary>
        /// <param name=""offset"">The offset of this field in the packet.</param>
        public %ATTRIBUTE_NAME%([System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)] int offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref=""%ABSTRACT_CLASS_NAME%""/>.
        /// </summary>
        /// <param name=""offsetMember"">The name of the member which specifies the offset of this field in the packet.</param>
        public %ATTRIBUTE_NAME%(string offsetMember)
        {
            this.offsetMember = offsetMember;
        }

        /// <summary>
        /// The length of this field.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public int Length { get; set; } = %DEFAULT_LENGTH%;

        /// <summary>
        /// The name of the member which specifies the length of this field.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public string LengthMember { get; set; } = %DEFAULT_LENGTH_MEMBER%;

        /// <summary>
        /// Whether this field is big endian.
        /// </summary>
        public bool IsBigEndian { get; set; } = %DEFAULT_IS_BIG_ENDIAN%;

        /// <summary>
        /// The minimum length of array or string data allowed in this field.
        /// </summary>
        public int MinLength { get; set; } = %DEFAULT_MIN_LENGTH%;

        /// <summary>
        /// The maximum length of array or string data allowed in this field.
        /// </summary>
        public int MaxLength { get; set; } = %DEFAULT_MAX_LENGTH%;

        /// <summary>
        /// The name of the underlying type of the property type.
        /// Only required if the type of the property is source generated.
        /// </summary>
        public string AssumeGeneratedEnumType { get; set; } = %DEFAULT_ASSUME_GENERATED_ENUM_TYPE%;

        /// <summary>
        /// The name of the method which should be invoked before the getter/setter is executed.
        /// </summary> 
        /// <remarks>
        /// The method type must be void and have one optional parameter with the same type as the property.
        /// If verification fails an exception should be thrown.
        /// </remarks>
        public string ValidationMethod { get; set; } = %DEFAULT_VALIDATION_METHOD%;

        public int Offset => offset;
        public string OffsetMember => offsetMember;
    }
}
";

        private const string AbstractPacketClassText = @"
using System;

namespace %NAMESPACE%
{
    public abstract partial class %CLASS_NAME%
    {
        /// <summary>
        /// The contents of the packet.
        /// </summary>
        public byte[] Bytes { get; protected set; }

        /// <inheritdoc cref=""Bytes""/>
        public byte this[int i]
        {
            get => Bytes[i];
            set => Bytes[i] = value;
        }

        /// <summary>
        /// Creates a new span over the packet.
        /// </summary>
        /// <seealso cref=""Bytes""/>
        /// <seealso cref=""M:System.MemoryExtensions.AsSpan``1(``0[])""/>
        public Span<byte> AsSpan() => Bytes;

        /// <summary>
        /// Creates a new Span over the portion of the packet beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name=""start"">The index at which to begin the Span.</param>
        /// <param name=""length"">The number of items in the Span.</param>
        /// <seealso cref=""M:System.MemoryExtensions.AsSpan``1(``0[],System.Int32,System.Int32)""/>
        public Span<byte> AsSpan(int start, int length) => Bytes.AsSpan(start, length);

        protected %CLASS_NAME%() { }

        protected %CLASS_NAME%(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}
";
        #endregion

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{PacketFieldAttributeName}.g.cs",
                    PacketFieldAttributeText
                        .Replace("%NAMESPACE%", Namespace)
                        .Replace("%ATTRIBUTE_NAME%", PacketFieldAttributeName)
                        .Replace("%ABSTRACT_CLASS_NAME%", AbstractClassName)
                        .Replace("%DEFAULT_OFFSET%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.Offset)))
                        .Replace("%DEFAULT_OFFSET_MEMBER%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.OffsetMember)))
                        .Replace("%DEFAULT_LENGTH%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.Length)))
                        .Replace("%DEFAULT_LENGTH_MEMBER%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.LengthMember)))
                        .Replace("%DEFAULT_IS_BIG_ENDIAN%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.IsBigEndian)))
                        .Replace("%DEFAULT_MIN_LENGTH%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.MinLength)))
                        .Replace("%DEFAULT_MAX_LENGTH%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.MaxLength)))
                        .Replace("%DEFAULT_ASSUME_GENERATED_ENUM_TYPE%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.AssumeGeneratedEnumType)))
                        .Replace("%DEFAULT_VALIDATION_METHOD%", PacketFieldAttributeData.Default.AsSourceString(nameof(PacketFieldAttributeData.Default.ValidationMethod)))
                        .TrimStart()
                )
            );
            // Register the abstract packet class source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{AbstractClassName}.g.cs",
                    AbstractPacketClassText
                        .Replace("%NAMESPACE%", Namespace)
                        .Replace("%CLASS_NAME%", AbstractClassName)
                        .TrimStart()
                )
            );

            // Create an incremental value provider using the generated attribute
            var packetFieldProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{Namespace}.{PacketFieldAttributeName}",
                IsValidPacketFieldProperty,
                TransformPacketFieldProperty
            );

            context.RegisterImplementationSourceOutput(packetFieldProvider, ProduceSourceCode);
        }

        /// <summary>
        /// Check whether <paramref name="syntaxNode"/> is a valid packet field property.
        /// </summary>
        /// <param name="syntaxNode">The syntax node to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>True</c> if <paramref name="syntaxNode"/> is a valid packet field property.</returns>
        private static bool IsValidPacketFieldProperty(SyntaxNode syntaxNode, CancellationToken cancellationToken) =>
            syntaxNode is PropertyDeclarationSyntax { Parent: ClassDeclarationSyntax { BaseList.Types.Count: > 0 } classDeclaration } propertyDeclaration
            && propertyDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
            && classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));

        private static PacketFieldModel TransformPacketFieldProperty(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var propertySymbol = (context.TargetSymbol as IPropertySymbol)!;
            var attributeData = new PacketFieldAttributeData(context.Attributes[0]);
            var classSymbol = (context.TargetSymbol.ContainingSymbol as INamedTypeSymbol)!;

            // Get information about the type of the property

            ITypeSymbol actualTypeSymbol = propertySymbol.Type;
            bool isArray = propertySymbol.Type.TypeKind == TypeKind.Array;
            bool isEnum = propertySymbol.Type.TypeKind == TypeKind.Enum;
            bool isStruct = IsActualStruct(propertySymbol.Type);
            if (!Enum.TryParse(actualTypeSymbol.Name, out ActualType actualType))
            {
                actualType = ActualType.NamedType;
            }

            if (isArray)
            {
                // Get the type of the array
                actualTypeSymbol = (propertySymbol.Type as IArrayTypeSymbol)!.ElementType;
                isEnum = actualTypeSymbol.TypeKind == TypeKind.Enum;
                isStruct = IsActualStruct(actualTypeSymbol);

                // Parse the ActualType value if the array type is neither an enum, a struct, a class or a generated type
                if (!isEnum && !isStruct && actualTypeSymbol.TypeKind != TypeKind.Class && actualTypeSymbol.TypeKind != TypeKind.Error)
                {
                    actualType = (ActualType)Enum.Parse(typeof(ActualType), actualTypeSymbol.Name, true);
                }
            }

            if (isEnum)
            {
                // Parse the underlying type for enums
                actualType = (ActualType)Enum.Parse(typeof(ActualType), (actualTypeSymbol as INamedTypeSymbol)!.EnumUnderlyingType!.Name, true);
            }

            // Get information about length member if applicable
            ActualType lengthMemberType = ActualType.NamedType;
            Permissions lengthMemberPermissions = Permissions.Unknown;
            if (attributeData.LengthMember.Length > 0)
            {
                ISymbol lengthMemberSymbol = classSymbol.GetMembers(attributeData.LengthMember).Single();

                switch (lengthMemberSymbol)
                {
                    case IPropertySymbol lengthProperty:
                        lengthMemberType = (ActualType)Enum.Parse(typeof(ActualType), lengthProperty.Type.Name, true);
                        if (lengthProperty.IsReadOnly)
                        {
                            lengthMemberPermissions = Permissions.ReadOnly;
                        }
                        else if (lengthProperty.IsWriteOnly)
                        {
                            lengthMemberPermissions = Permissions.WriteOnly;
                        }
                        else
                        {
                            lengthMemberPermissions = Permissions.ReadWrite;
                        }
                        break;
                    case IFieldSymbol lengthField:
                        lengthMemberType = (ActualType)Enum.Parse(typeof(ActualType), lengthField.Type.Name, true);
                        if (lengthField.IsReadOnly || lengthField.IsConst)
                        {
                            lengthMemberPermissions = Permissions.ReadOnly;
                        }
                        else
                        {
                            lengthMemberPermissions = Permissions.ReadWrite;
                        }
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Can't determine member type of LengthMember: {lengthMemberSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
                }

                if (lengthMemberPermissions == Permissions.WriteOnly)
                {
                    throw new InvalidOperationException("Can't generate property accessors, LengthMember is write-only.");
                }
            }

            // NOTE: This is only necessary because property types could be generated by other source generators,
            //       which aren't available at this point.

            // Process auto generated property types

            // Assume generated enum type
            if (actualTypeSymbol.TypeKind == TypeKind.Error && attributeData.AssumeGeneratedEnumType.Length > 0)
            {
                isEnum = true;
                actualType = (ActualType)Enum.Parse(typeof(ActualType), attributeData.AssumeGeneratedEnumType, true);
            }

            // Get imports

            CompilationUnitSyntax rootNode = context.TargetNode.SyntaxTree.GetCompilationUnitRoot();
            string[] imports = rootNode.Usings.ToString().Split('\n');
            for (int i = 0; i < imports.Length; i++)
            {
                imports[i] = imports[i].Trim();
            }

            // Create model from data
            return new PacketFieldModel(
                attributeData.Offset,
                attributeData.OffsetMember,
                attributeData.Length,
                attributeData.LengthMember,
                lengthMemberType,
                lengthMemberPermissions,
                attributeData.MinLength,
                attributeData.MaxLength,
                attributeData.IsBigEndian,
                new FieldTypeModel(
                    RemoveGlobalAlias(propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                    isArray,
                    isEnum,
                    isStruct,
                    actualType
                ),
                propertySymbol.Name,
                GetAccessModifierString(propertySymbol),
                GetAccessorModifierString(propertySymbol, true),
            GetAccessorModifierString(propertySymbol, false),
                RemoveGlobalAlias(classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                imports,
                attributeData.ValidationMethod
            );
        }

        private static void ProduceSourceCode(SourceProductionContext context, PacketFieldModel packetField)
        {
            string namespaceName = string.Empty;
            string className = packetField.ClassName;

            if (className.Contains("."))
            {
                namespaceName = className.Substring(0, className.LastIndexOf(".", StringComparison.Ordinal));
                className = className.Substring(className.LastIndexOf(".", StringComparison.Ordinal) + 1);
            }

            // Begin building the generated source
            CodeBuilder source = new();
            string[] requiredImports = ["using System;", "using System.Buffers.Binary;", "using System.Text;"];

            // Add imports

            foreach (var importString in packetField.Imports)
            {
                source.AppendLine(importString);
            }

            foreach (var requiredImportString in requiredImports)
            {
                if (!packetField.Imports.Contains(requiredImportString))
                {
                    source.AppendLine(requiredImportString);
                }
            }

            source.AppendLine();

            // Add namespace if necessary

            if (namespaceName.Length > 0)
            {
                source.EnterScope($"namespace {namespaceName}");
            }

            // Add class and property

            source.EnterScope($"partial class {className}");
            source.EnterScope($"{packetField.PropertyAccessModifiers}partial {packetField.FieldType.Name} {packetField.PropertyName}");

            // Add code for getter
            source.AppendBlock(AccessorGenerator.GenerateGetter(packetField));
            // Add code for setter
            source.AppendBlock(AccessorGenerator.GenerateSetter(packetField));

            // Leave property and class scope
            source.LeaveScope();
            source.LeaveScope();

            // Leave namespace scope if necessary
            if (namespaceName.Length > 0)
            {
                source.LeaveScope();
            }

            // Add the generated source
            string fullyQualifiedName = namespaceName.Length > 0 ? $"{namespaceName}.{className}" : className;
            context.AddSource($"{fullyQualifiedName}.{packetField.PropertyName}.g.cs", source.ToString());
        }

        private static string RemoveGlobalAlias(string classOrNamespaceName) =>
            classOrNamespaceName.StartsWith("global::") ? classOrNamespaceName.Substring(8) : classOrNamespaceName;

        private static bool IsActualStruct(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            return typeSymbol.SpecialType is < SpecialType.System_Boolean or > SpecialType.System_String;
        }

        private static string GetAccessModifierString(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.DeclaringSyntaxReferences.Length != 1)
            {
                throw new InvalidOperationException($"PropertySymbol contains wrong amount of declaring syntax references: {propertySymbol.DeclaringSyntaxReferences.Length}");
            }

            if (propertySymbol.DeclaringSyntaxReferences[0].GetSyntax() is not PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                throw new InvalidOperationException($"DeclaringSyntaxReference is not {nameof(PropertyDeclarationSyntax)}: {propertySymbol.DeclaringSyntaxReferences[0].GetSyntax().Kind()}");
            }

            string result = "";

            foreach (var modifier in propertyDeclarationSyntax.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PartialKeyword))
                {
                    continue;
                }

                result += $"{modifier} ";
            }

            // NOTE: Keep the last space to make working with it easier.
            return result;
        }

        private static string GetAccessorModifierString(IPropertySymbol propertySymbol, bool isGetter)
        {
            if (propertySymbol.DeclaringSyntaxReferences.Length != 1)
            {
                throw new InvalidOperationException($"PropertySymbol contains wrong amount of declaring syntax references: {propertySymbol.DeclaringSyntaxReferences.Length}");
            }

            if (propertySymbol.DeclaringSyntaxReferences[0].GetSyntax() is not PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                throw new InvalidOperationException($"DeclaringSyntaxReference is not {nameof(PropertyDeclarationSyntax)}: {propertySymbol.DeclaringSyntaxReferences[0].GetSyntax().Kind()}");
            }

            SyntaxKind accessorKind = isGetter ? SyntaxKind.GetAccessorDeclaration : SyntaxKind.SetAccessorDeclaration;

            foreach (var accessor in propertyDeclarationSyntax.AccessorList!.Accessors)
            {
                if (!accessor.IsKind(accessorKind))
                {
                    continue;
                }

                if (accessor.Modifiers.Count > 0)
                {
                    // NOTE: Add a space to the end to make working with it easier.
                    return $"{accessor.Modifiers} ";
                }

                return string.Empty;
            }

            throw new InvalidOperationException($"Couldn't find property accessor for: {propertySymbol.Name}");
        }
    }
}
