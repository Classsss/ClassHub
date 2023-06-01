namespace ClassHub.Client.Models {
    public class ExamProblem {
        // 문제 ID
        public int ProblemId { get; set; } = -1;

        // 문제 내용
        public string Description { get; set; } = string.Empty;

        // 문제 점수
        public int? Score { get; set; } = -1;
    }

    // 객관식
    public class MultipleChoiceProblem : ExamProblem {
        // 문제에 대한 보기들
        public string[] Questions { get; set; }

        // 문제에 대한 답안
        public string Answer { get; set; } = string.Empty;

        // 학생이 제출한 답안
        public string StudentAnswer { get; set; } = string.Empty;
    }

    // 단답형
    public class ShortAnswerProblem : ExamProblem {
        // 문제에 대한 모범 답안
        public string Answer { get; set; } = "";

        // 학생이 제출한 답안
        public string StudentAnswer { get; set; } = "";
    }

    // 코드형
    public class CodingProblem : ExamProblem {
        // 문제와 함께 제공할 예시 코드
        public string Example { get; set; } = "";

        // 문제에 대한 모범 답안 코드
        public string Answer { get; set; } = "";

        // 학생이 제출한 답안 코드
        public string StudentAnswer { get; set; } = "";
    }
}
