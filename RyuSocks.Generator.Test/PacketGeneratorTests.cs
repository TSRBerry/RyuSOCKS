using System.IO;
using System.Threading.Tasks;
using Xunit;
using Verify = RyuSocks.Generator.Test.SourceGeneratorVerifier<RyuSocks.Generator.PacketGenerator>;

namespace RyuSocks.Generator.Test
{
    public class PacketGeneratorTests
    {
        private const string DataDirectory = "data/PacketGenerator/";

        private static string GetSourceFromFile(string path) => File.ReadAllText(DataDirectory + path);
        private static (string filename, string content) GetGeneratedSourceFromFile(string path) =>
            (Path.GetFileName(path), File.ReadAllText(DataDirectory + path));

        [Theory]
        [InlineData("SimpleProps", "TestPacket.cs",
            "TestPacket.Byte1.g.cs", "TestPacket.SByte1.g.cs",
            "TestPacket.UShort1.g.cs", "TestPacket.Short1.g.cs",
            "TestPacket.UInt1.g.cs", "TestPacket.Int1.g.cs",
            "TestPacket.ULong1.g.cs", "TestPacket.Long1.g.cs",
            "TestPacket.String1.g.cs", "TestPacket.String2.g.cs", "TestPacket.String3.g.cs")]
        [InlineData("SimplePropsOffsetMember", "TestPacket.cs",
            "TestPacket.Byte1.g.cs", "TestPacket.SByte1.g.cs",
            "TestPacket.UShort1.g.cs", "TestPacket.Short1.g.cs",
            "TestPacket.UInt1.g.cs", "TestPacket.Int1.g.cs",
            "TestPacket.ULong1.g.cs", "TestPacket.Long1.g.cs",
            "TestPacket.String1.g.cs", "TestPacket.String2.g.cs", "TestPacket.String3.g.cs")]
        [InlineData("Namespaces", "TestPacket.cs",
            "TestSpace.TestPacket.TestField.g.cs", "TestSpace.TestPacket1.FirstField.g.cs",
            "AnotherSpace.TestPacket.TestField.g.cs", "AnotherSpace.TestPacket2.SecondField.g.cs")]
        [InlineData("StringEncodings", "TestPacket.cs",
            "LittleEndian.ASCIIPacket.String1.g.cs", "LittleEndian.ASCIIPacket.String2.g.cs", "LittleEndian.ASCIIPacket.String3.g.cs", "LittleEndian.ASCIIPacket.String4.g.cs",
            "LittleEndian.UnicodePacket.String1.g.cs", "LittleEndian.UnicodePacket.String2.g.cs", "LittleEndian.UnicodePacket.String3.g.cs", "LittleEndian.UnicodePacket.String4.g.cs",
            "LittleEndian.UTF7Packet.String1.g.cs", "LittleEndian.UTF7Packet.String2.g.cs", "LittleEndian.UTF7Packet.String3.g.cs", "LittleEndian.UTF7Packet.String4.g.cs",
            "LittleEndian.UTF8Packet.String1.g.cs", "LittleEndian.UTF8Packet.String2.g.cs", "LittleEndian.UTF8Packet.String3.g.cs", "LittleEndian.UTF8Packet.String4.g.cs",
            "LittleEndian.UTF32Packet.String1.g.cs", "LittleEndian.UTF32Packet.String2.g.cs", "LittleEndian.UTF32Packet.String3.g.cs", "LittleEndian.UTF32Packet.String4.g.cs",
            "BigEndian.ASCIIPacket.String1.g.cs", "BigEndian.ASCIIPacket.String2.g.cs", "BigEndian.ASCIIPacket.String3.g.cs", "BigEndian.ASCIIPacket.String4.g.cs",
            "BigEndian.UnicodePacket.String1.g.cs", "BigEndian.UnicodePacket.String2.g.cs", "BigEndian.UnicodePacket.String3.g.cs", "BigEndian.UnicodePacket.String4.g.cs",
            "BigEndian.UTF7Packet.String1.g.cs", "BigEndian.UTF7Packet.String2.g.cs", "BigEndian.UTF7Packet.String3.g.cs", "BigEndian.UTF7Packet.String4.g.cs",
            "BigEndian.UTF8Packet.String1.g.cs", "BigEndian.UTF8Packet.String2.g.cs", "BigEndian.UTF8Packet.String3.g.cs", "BigEndian.UTF8Packet.String4.g.cs",
            "BigEndian.UTF32Packet.String1.g.cs", "BigEndian.UTF32Packet.String2.g.cs", "BigEndian.UTF32Packet.String3.g.cs", "BigEndian.UTF32Packet.String4.g.cs")]
        public async Task GeneratedSources_AsExpected(string directory, string sourcePath, params string[] expectedGeneratedSourcePath)
        {
            const int DefaultSourcesAmount = 3;
            (string, string)[] generatedSources = new (string, string)[expectedGeneratedSourcePath.Length + DefaultSourcesAmount];

            generatedSources[0] = GetGeneratedSourceFromFile("StringEncoding.g.cs");
            generatedSources[1] = GetGeneratedSourceFromFile("PacketFieldAttribute.g.cs");
            generatedSources[2] = GetGeneratedSourceFromFile("Packet.g.cs");

            directory += "/";

            for (int i = 0; i < expectedGeneratedSourcePath.Length; i++)
            {
                generatedSources[i + DefaultSourcesAmount] = GetGeneratedSourceFromFile(directory + expectedGeneratedSourcePath[i]);
            }

            await Verify.VerifyGeneratedSources(
                GetSourceFromFile(directory + sourcePath),
                generatedSources
            );
        }
    }
}
