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
        public async Task GeneratedSources_AsExpected(string directory, string sourcePath, params string[] expectedGeneratedSourcePath)
        {
            directory += "/";
            (string, string)[] generatedSources = new (string, string)[expectedGeneratedSourcePath.Length + 2];

            generatedSources[0] = GetGeneratedSourceFromFile("PacketFieldAttribute.g.cs");
            generatedSources[1] = GetGeneratedSourceFromFile("Packet.g.cs");

            for (int i = 0; i < expectedGeneratedSourcePath.Length; i++)
            {
                generatedSources[i + 2] = GetGeneratedSourceFromFile(directory + expectedGeneratedSourcePath[i]);
            }

            await Verify.VerifyGeneratedSources(
                GetSourceFromFile(directory + sourcePath),
                generatedSources
            );
        }
    }
}
