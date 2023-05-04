namespace ClassHub.Shared {
    public class Notice {
        public int room_id { get; set; } // 강의실 번호
        public int notice_id { get; set; } // 공지 번호
        public string title { get; set; } // 게시글 제목
        public string author { get; set; } // 작성자
        public string contents { get; set; } // 게시글 내용
        public DateTime publish_date { get; set; } // 게시일
        public DateTime up_date { get; set; } // 업데이트일
        public int view_count { get; set; } // 조회수
    }
}
