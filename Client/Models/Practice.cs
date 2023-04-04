namespace ClassHub.Client.Models {
    public class Practice {
        public int Id { get; set; } // 실습 번호
        public string Title { get; set; } // 실습 제목
        public string Author { get; set; } // 작성자
        public DateTime StartDate { get; set; } // 시작일
        public DateTime EndDate { get; set; } // 종료일
    }
}