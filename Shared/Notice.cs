namespace ClassHub.Shared {
    public class Notice { // 공지사항 클래스
        public int room_id { get; set; } // 강의실 번호
        public int notice_id { get; set; } // 공지사항 번호
        public string title { get; set; } // 공지사항 제목
        public string author { get; set; } // 작성자
        public string contents { get; set; } // 공지사항 내용
        public DateTime publish_date { get; set; } // 게시일
        public DateTime up_date { get; set; } // 업데이트일
        public int ViewCount { get; set; } // 조회수
    }
}
