namespace ClassHub.Client.Models {
    public class Exam {
        public int ExamId { get; set; } // 시험 ID
        public DateTime StartDate { get; set; } // 시작 시간
        public DateTime EndDate { get; set; } // 종료 시간
        public bool IsRandomProblem { get; set; } // 문제 순서 랜덤 여부
        public bool IsShowTimeLimit { get; set; } // 제한 시간 표시 여부
        public bool IsBackToPreviousProblem { get; set; } // 이전 문제로 돌아갈 수 있는지 여부
    }
}
