namespace ClassHub.Client.Shared {
    public class LectureMaterial
    {
        public int Id { get; set; } // 강의자료 번호
		public int Week { get; set; } // 강의자료 활용 주차
		public string Title { get; set; } // 강의자료 제목
        public string Content { get; set; } // 강의자료 내용
        public string Author { get; set; } // 작성자
        public DateTime PublishDate { get; set; } // 게시일
        public int ViewCount { get; set; } // 조회수
    }
}
