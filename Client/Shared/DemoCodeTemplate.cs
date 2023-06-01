namespace ClassHub.Client.Shared {
    // 데모 코드 제출을 위한 코드 템플릿
    // 기본적으로 덧셈 문제에 대한 답을 정답 코드로 함
    public static class DemoCodeTemplate {
        public enum Type {
            CORRECT,
            WRONG,
            COMPILE_ERROR,
            RUNTIME_ERROR
        }

        public static readonly Dictionary<string, Dictionary<Type, string>> codeTemplates = new Dictionary<string, Dictionary<Type, string>> {
            {
                "c", new Dictionary<Type, string>() {
                    { Type.CORRECT, C_CORRECT_CODE },
                    { Type.WRONG, C_WRONG_CODE },
                    { Type.COMPILE_ERROR, C_COMPILE_ERROR_CODE },
                    { Type.RUNTIME_ERROR, C_RUNTIME_ERROR_CODE }
                }
            },
            {
                "cpp", new Dictionary<Type, string>() {
                    { Type.CORRECT, CPP_CORRECT_CODE },
                    { Type.WRONG, CPP_WRONG_CODE },
                    { Type.COMPILE_ERROR, CPP_COMPILE_ERROR_CODE },
                    { Type.RUNTIME_ERROR, CPP_RUNTIME_ERROR_CODE }
                }
            },
            {
                "csharp", new Dictionary<Type, string>() {
                    { Type.CORRECT, CSHARP_CORRECT_CODE },
                    { Type.WRONG, CSHARP_WRONG_CODE },
                    { Type.COMPILE_ERROR, CSHARP_COMPILE_ERROR_CODE },
                    { Type.RUNTIME_ERROR, CSHARP_RUNTIME_ERROR_CODE }
                }
            },
            {
                "java", new Dictionary<Type, string>() {
                    { Type.CORRECT, JAVA_CORRECT_CODE },
                    { Type.WRONG, JAVA_WRONG_CODE },
                    { Type.COMPILE_ERROR, JAVA_COMPILE_ERROR_CODE },
                    { Type.RUNTIME_ERROR, JAVA_RUNTIME_ERROR_CODE }
                }
            },
            {
                "python", new Dictionary<Type, string>() {
                    { Type.CORRECT, PYTHON_CORRECT_CODE },
                    { Type.WRONG, PYTHON_WRONG_CODE },
                    { Type.COMPILE_ERROR, PYTHON_COMPILE_ERROR_CODE },
                    { Type.RUNTIME_ERROR, PYTHON_RUNTIME_ERROR_CODE }
                }
            },
        };

        private const string C_CORRECT_CODE = @"#include <stdio.h>

int main() {
    int a, b;
    scanf(""%d %d"", &a, &b);
    printf(""%d"", a + b);
}";
        private const string C_WRONG_CODE = @"#include <stdio.h>

int main() {
    int a, b;
    scanf(""%d %d"", &a, &b);
    printf(""%d"", a * b);
}";
        private const string C_COMPILE_ERROR_CODE = @"#include <stdio.h>

int main() {
    int a, b;
    scanf(""%d %d"", &a, &b)
    printf(""%d"", a + b);
}";
        private const string C_RUNTIME_ERROR_CODE = @"#include <stdio.h>

int main() {
    int a, b;
    scanf(""%d %d"", &a, &b);
    a /= 0;
    printf(""%d"", a + b);
}";

        private const string CPP_CORRECT_CODE = @"#include <iostream>
using namespace std;

int main() {
    int a, b;
    cin >> a >> b;
    cout << a + b;
}";
        private const string CPP_WRONG_CODE = @"#include <iostream>
using namespace std;

int main() {
    int a, b;
    cin >> a >> b;
    cout << a * b;
}";
        private const string CPP_COMPILE_ERROR_CODE = @"#include <iostream>
using namespace std;

int main() {
    int a, b;
    cin >> a >> b
    cout << a + b;
}";
        private const string CPP_RUNTIME_ERROR_CODE = @"#include <iostream>
using namespace std;

int main() {
    int a, b;
    cin >> a >> b;
    a /= 0;
    cout << a + b;
}";

        private const string CSHARP_CORRECT_CODE = @"using System;

class Test {
    static void Main() {
        string[] inputs = Console.ReadLine().Split();
        int result = int.Parse(inputs[0]) + int.Parse(inputs[1]);
        Console.Write(result);
    }
}";
        private const string CSHARP_WRONG_CODE = @"using System;

class Test {
    static void Main() {
        string[] inputs = Console.ReadLine().Split();
        int result = int.Parse(inputs[0]) * int.Parse(inputs[1]);
        Console.Write(result);
    }
}";
        private const string CSHARP_COMPILE_ERROR_CODE = @"using System;

class Test {
    static void Main() {
        string[] inputs = Console.ReadLine().Split()
        int result = int.Parse(inputs[0]) + int.Parse(inputs[1]);
        Console.Write(result);
    }
}";
        private const string CSHARP_RUNTIME_ERROR_CODE = @"using System;

class Test {
    static void Main() {
        string[] inputs = Console.ReadLine().Split();
        int result = int.Parse(inputs[0]) + int.Parse(inputs[1]);
        result /= 0;
        Console.Write(result);
    }
}";

        private const string JAVA_CORRECT_CODE = @"import java.util.Scanner;

public class Main {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);
        int num1 = scanner.nextInt();
        int num2 = scanner.nextInt();
        int result = num1 + num2;
        System.out.println(result);
        scanner.close();
    }
}";
        private const string JAVA_WRONG_CODE = @"import java.util.Scanner;

public class Main {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);
        int num1 = scanner.nextInt();
        int num2 = scanner.nextInt();
        int result = num1 * num2;
        System.out.println(result);
        scanner.close();
    }
}";
        private const string JAVA_COMPILE_ERROR_CODE = @"import java.util.Scanner;

public class Main {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in)
        int num1 = scanner.nextInt();
        int num2 = scanner.nextInt();
        int result = num1 + num2;
        System.out.println(result);
        scanner.close();
    }
}";
        private const string JAVA_RUNTIME_ERROR_CODE = @"import java.util.Scanner;

public class Main {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);
        int num1 = scanner.nextInt();
        num1 /= 0;
        int num2 = scanner.nextInt();
        int result = num1 + num2;
        System.out.println(result);
        scanner.close();
    }
}";

        private const string PYTHON_CORRECT_CODE = @"num1, num2 = map(int, input().split())
print(num1 + num2)";
        private const string PYTHON_WRONG_CODE = @"num1, num2 = map(int, input().split())
print(num1 * num2)";
        private const string PYTHON_COMPILE_ERROR_CODE = @"num1, num2 = map(int, input().split()
print(num1 + num2)";
        private const string PYTHON_RUNTIME_ERROR_CODE = @"num1, num2 = map(int, input().split())
print(num1 + (num2 / 0))";
    }
}
