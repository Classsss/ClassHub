
﻿namespace ClassHub.Shared {
    public class JudgeResult {
        // 채점 결과
        public enum JResult {
            Accepted,               // 모든 테스트 케이스 통과
            WrongAnswer,            // 테스트 케이스와 실행 결과 불일치
            CompileError,           // 컴파일 에러
            RuntimeError,           // 런타임 에러
            TimeLimitExceeded,      // 시간 초과
            MemoryLimitExceeded,    // 메모리 초과
            PresentationError,      // 출력 형식 에러
            OutputLimitExceeded,    // 출력 한도 초과
            JudgementFailed,        // 예외 사항 발생으로 채점 실패
            Pending                 // 채점 미완료
        }

        public JResult Result { get; set; } = JResult.Pending;  // 채점 결과
        public double ExecutionTime { get; set; } = 0;          // 실행 시간(ms)    // TODO : 기준 정립 필요 (ex. 평균)
        public long MemoryUsage { get; set; } = 0;              // 메모리 사용량(?) // TODO : 단위 지정 필요, 기준 정립 필요 (ex. 평균)
        public string? Message { get; set; }                    // 채점 결과와 동반되는 메시지
    }
}
