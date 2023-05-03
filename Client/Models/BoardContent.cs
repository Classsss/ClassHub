namespace ClassHub.Client.Models {
    public class BoardContent
    {
        public int room_id { get; set; } // 강의실 번호
        public int content_id { get; set; } // 게시글 번호
        public string title { get; set; } // 게시글 제목
        public string author { get; set; } // 작성자
        public string contents { get; set; } // 게시글 내용
        public DateTime publish_date { get; set; } // 게시일
        public DateTime up_date { get; set; } // 업데이트일
        public int view_count { get; set; } // 조회수
    }

    public class LectureMaterialContent : BoardContent
    {
        public int week { get; set; } // 강의자료 활용 주차
    }

    public class NoticeContent : BoardContent
    {
    }
}
