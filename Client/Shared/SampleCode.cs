namespace ClassHub.Client.Shared
{
    public class SampleCode
    {
        public const string csharp = "using System;\r\n\r\nclass Test {\r\n    static void Main() {\r\n        string[] inputs = Console.ReadLine().Split();\r\n        int result = int.Parse(inputs[0]) + int.Parse(inputs[1]);\r\n        Console.Write(result);\r\n    }\r\n}";
        public const string c = "#include <stdio.h>\r\n\r\nint main() {\r\n    int a, b;\r\n    scanf(\"%d %d\", &a, &b);\r\n    printf(\"%d\", a + b);\r\n}";
        public const string cpp = "#include <iostream>\r\nusing namespace std;\r\n\r\nint main() {\r\n    int a, b;\r\n    cin >> a >> b;\r\n    cout << a + b;\r\n}";
        public const string java = "import java.util.Scanner;\r\n\r\npublic class Main {\r\n    public static void main(String[] args) {\r\n        Scanner scanner = new Scanner(System.in);\r\n        int num1 = scanner.nextInt();\r\n        int num2 = scanner.nextInt();\r\n        int result = num1 + num2;\r\n        System.out.println(result);\r\n        scanner.close();\r\n    }\r\n}";
        public const string python = "num1, num2 = map(int, input().split())\nprint(num1 + num2)";
        public const string etc = "Unknown Sample Code";
    }
}