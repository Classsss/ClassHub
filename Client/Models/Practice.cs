namespace ClassHub.Client.Models {
    public class Practice {
        public int Id { get; set; } // 실습 번호
        public string Title { get; set; } // 실습 제목
        public string Content { get; set; } // 실습 내용
        public string Author { get; set; } // 작성자
        public List<string> ExamInputCases { get; set; } = new List<string>();//입력 케이스
        public List<string> ExamOutputCases { get; set; } = new List<string>();//출력 케이스
        public string Language { get; set; } // 사용 언어
        public bool isApplyScore { get; set; } //점수 반영 여부
        public DateTime StartDate { get; set; } // 시작일
        public DateTime EndDate { get; set; } // 종료일
        public List<string> IntputCases { get; set; } = new List<string>();// 실제 입력 케이스
        public string CorrectCode { get; set; } // 교수의 입력 코드
    }
}