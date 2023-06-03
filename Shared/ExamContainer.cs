namespace ClassHub.Shared {
    // Exam 클래스와 각종 시험문제 클래스를 함께 저장하는 클래스
    public class ExamContainer {
        // 시험
        public Exam? Exam { get; set; } = null;

        // 객관식 문제와 그 보기들의 리스트를 함께 담는 MultipleChoiceProblemContainer 클래스의 리스트
        public List<MultipleChoiceProblemContainer> MultipleChoiceProblemContainers { get; set; } = new List<MultipleChoiceProblemContainer>();

        // 단답형 문제들의 리스트
        public List<ShortAnswerProblem> ShortAnswerProblems { get; set; } = new List<ShortAnswerProblem>();

        // 코드형 문제들의 리스트
        public List<CodingExamProblem> CodingExamProblems { get; set; } = new List<CodingExamProblem>();
    }
}
