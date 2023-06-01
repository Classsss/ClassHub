namespace ClassHub.Client.Models {
    /// <summary>
    /// 무언가를 제출해야 하는 과제, 실습, 시험 등의 모델을 정의하는 클래스 입니다.
    /// </summary>
    public class Submission {
        public int Id { get; set; }
        /// <summary>작성자</summary>
        public string Author { get; set; }
        /// <summary>제목</summary>
        public string Title { get; set; }
        /// <summary>설명</summary>
        public string Description { get; set; }
        /// <summary>시작 날짜</summary>
        public DateTime StartDate { get; set; }
        /// <summary>종료 날짜</summary>
        public DateTime EndDate { get; set; }
        /// <summary>제출 또는 참여 여부</summary>
        public bool IsSubmitted { get; set; } = false;
        /// <summary>제출일</summary>
        public DateTime? SubmissionDate { get; set; }
        /// <summary>제출일</summary>
        public int? TotalSubmitters { get; set; }
    }

    /// <summary>
    /// 과제와 실습은 동일한 모델 클래스를 사용합니다
    /// </summary>
    public class Assignment : Submission {
        /// <summary>과제 유형</summary>
        public string Type { get; set; }
        /// <summary> 제출한 파일의 경로</summary>
        public string SubmissionFilePath { get; set; }
    }

    public class Exam : Submission {
        /// <summary>문제 순서 랜덤 여부</summary>
        public bool IsRandomProblem { get; set; } = false;
        /// <summary>제한 시간 표시 여부</summary>
        public bool IsShowTimeLimit { get; set; } = false;
        /// <summary>이전 문제로 돌아갈 수 있는지 여부</summary>
        public bool IsBackToPreviousProblem { get; set; } = false;
    }
}
