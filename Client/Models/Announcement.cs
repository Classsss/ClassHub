namespace ClassHub.Client.Models {
    public class Announcement { // 공지사항 클래스
        public int Id { get; set; } // 공지사항 번호
        public string Title { get; set; } // 공지사항 제목
        public string Content { get; set; } // 공지사항 내용
        public string Author { get; set; } // 작성자
        public DateTime StartDate { get; set; } // 게시일
    }
}
