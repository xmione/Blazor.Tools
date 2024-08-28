
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace Blazor.Tools.ConsoleApp
{
    public class ILToSourceCodeConverter
    {
        public ILToSourceCodeConverter()
        {
            string ilCodeString = "02 03 6F 2A 00 00 0A 28 2B 00 00 0A 02 2A";
            byte[] ilCode = ConvertHexStringToByteArray(ilCodeString);

            // Generate IL code and decompile
            string sourceCode = DecompileILCode(ilCode);
            Console.WriteLine("Decompiled Source Code:");
            Console.WriteLine(sourceCode);
        }

        private static byte[] ConvertHexStringToByteArray(string hex)
        {
            string[] hexValues = hex.Split(' ');
            byte[] bytes = new byte[hexValues.Length];
            for (int i = 0; i < hexValues.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexValues[i], 16);
            }
            return bytes;
        }

        private static string DecompileILCode(byte[] ilCode)
        {
            string decompiledCode = string.Empty;
            using (var stream = new MemoryStream(ilCode)) 
            {
                var module = new PEFile("EmployeeVM", stream);
                var typeSystem = new DecompilerTypeSystem(module, null);

                var decompiler = new CSharpDecompiler(typeSystem, new DecompilerSettings());
                // Decompile the entire module
                decompiledCode = decompiler.DecompileWholeModuleAsString();
            }

            return decompiledCode;
        }
    }
}
