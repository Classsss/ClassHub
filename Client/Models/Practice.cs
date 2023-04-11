namespace ClassHub.Client.Models {
    public class Practice {
        public int Id { get; set; } // 실습 번호
        public string Title { get; set; } // 실습 제목
        public string Content { get; set; } // 실습 내용
        public string Author { get; set; } // 작성자
        public List<string> InputCases { get; set; } //입력케이스
        public List<string> OutputCases { get; set; } //입력케이스
        public string Language { get; set; } // 사용 언어
        public bool isApplyScore { get; set; } //점수 반영 여부
        public bool isGptAvailable { get; set; } //GPT 사용 가능 여부
        public DateTime StartDate { get; set; } // 시작일
        public DateTime EndDate { get; set; } // 종료일
    }
}