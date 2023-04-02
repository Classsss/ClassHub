namespace ClassHub.Client.Models {
    public class Announcement {
        public int Id { get; set; } // 공지사항 번호
        public string Title { get; set; } // 공지사항 제목
        public string Content { get; set; } // 공지사항 내용
        public string Author { get; set; } // 작성자
        public DateTime StartDate { get; set; } // 시작일
        // public DateTime EndDate { get; set; } // 종료일
        // public int Priority { get; set; } // 우선순위
        // public List<string> Attachments { get; set; } // 첨부 파일 정보
        // public string Category { get; set; } // 분류
        // public bool IsImportant { get; set; } // 중요 여부
    }
}
