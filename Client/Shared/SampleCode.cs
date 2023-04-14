namespace ClassHub.Client.Shared
{
    public class SampleCode
    {
        public const string csharp = "using System;\r\nclass Test\r\n{\r\n    static void Main()\r\n    {\r\n        string[] inputs = Console.ReadLine().Split();\r\n        int result = int.Parse(inputs[0]) + int.Parse(inputs[1]);\r\n        Console.Write(result);\r\n    }\r\n}";
        public const string c = "#include <stdio.h>\r\n\r\nint main() {\r\n    int a, b;\r\n    scanf(\"%d %d\", &a, &b);\r\n    printf(\"%d\", a + b);\r\n}";
        public const string etc = "Unknown Sample Code";
    }
}