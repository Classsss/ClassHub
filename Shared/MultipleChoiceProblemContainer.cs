namespace ClassHub.Shared {
    // 객관식 문제와 그 보기들의 리스트를 함께 담는 클래스
    public class MultipleChoiceProblemContainer {
        // 객관식 문제
        public MultipleChoiceProblem? MultipleChoiceProblem { get; set; } = null;

        // 객관식 문제의 보기들
        public List<MultipleChoice> MultipleChoices { get; set; } = new List<MultipleChoice>();
    }
}
