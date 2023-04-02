namespace ClassHub.Shared {
    public class JudgeRequest {
        public string? Id { get; set; }         // 문제를 식별할 수 있는 Id
        public string? Code { get; set; }       // 에디터에 작성하여 제출한 코드
        public string? Language { get; set; }   // 코드의 언어 // TODO : 채점 DB와 연결하면 제거, 현재는 디버깅을 위해 사용
    }
}
