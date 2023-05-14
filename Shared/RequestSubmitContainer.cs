namespace ClassHub.Shared {
    // 채점을 요청하기 위해 JudgeRequest 객체와 CodeSubmit 객체를 함께 저장하는 클래스
    public class RequestSubmitContainer {
        // JudgeRequest 객체
        public JudgeRequest? JudgeRequest { get; private set; }
        // CodeSubmit 객체
        public CodeSubmit? CodeSubmit {  get; private set; }

        public RequestSubmitContainer(JudgeRequest judgeRequest, CodeSubmit codeSubmit) {
            JudgeRequest = judgeRequest;
            CodeSubmit = codeSubmit;
        }
    }
}
