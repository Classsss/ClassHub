namespace ClassHub.Client.Shared
{
    public class SampleCode
    {
        public const string csharp = "using System;\r\nclass Test\r\n{\r\n    static void Main()\r\n    {\r\n        string[] inputs = Console.ReadLine().Split();\r\n        int result = int.Parse(inputs[0]) + int.Parse(inputs[1]);\r\n        Console.Write(result);\r\n    }\r\n}";
        public const string c = "#include <stdio.h>\r\n\r\nint main() {\r\n    int a, b;\r\n    scanf(\"%d %d\", &a, &b);\r\n    printf(\"%d\", a + b);\r\n}";
        public const string cpp = "#include <iostream>\nusing namespace std;\n\nint main() {\n int a, b;\n cin >> a >> b;\n cout << a + b;\n}";
        public const string java = "import java.util.Scanner;\n\npublic class Main {\n public static void main(String[] args) {\n Scanner scanner = new Scanner(System.in);\n int num1 = scanner.nextInt();\n int num2 = scanner.nextInt();\n int result = num1 + num2;\n System.out.println(result);\n scanner.close();\n }\n}";
        public const string python = "num1, num2 = map(int, input().split())\nprint(num1 + num2)";
        public const string etc = "Unknown Sample Code";
    }
}