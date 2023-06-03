namespace ClassHub.Shared {
    // ExamSubmit 클래스와 시험 문제 제출 클래스들을 함께 저장하는 클래스
    public class ExamSubmitContainer {
        // 시험 제출
        public ExamSubmit? ExamSubmit { get; set; } = null;

        // 객관식 문제 제출 리스트
        public List<MultipleChoiceProblemSubmit> MultipleChoiceProblemSubmits { get; set; } = new List<MultipleChoiceProblemSubmit>();

        // 단답형 문제 제출 리스트
        public List<ShortAnswerProblemSubmit> ShortAnswerProblemSubmits { get; set; } = new List<ShortAnswerProblemSubmit>();

        // 코드형 문제 제출 리스트
        public List<CodingExamProblemSubmit> CodingExamProblemSubmits { get; set; } = new List<CodingExamProblemSubmit>();
    }
}
