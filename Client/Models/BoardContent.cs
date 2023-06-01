using System.Text.Json.Serialization;

namespace ClassHub.Client.Models {
    public class BoardContent
    {
        public int room_id { get; set; } // 강의실 번호
        public virtual int content_id { get; set; } // 게시글 번호
        public string title { get; set; } // 게시글 제목
        public string author { get; set; } // 작성자
        public string contents { get; set; } // 게시글 내용
        public DateTime publish_date { get; set; } // 게시일
        public DateTime up_date { get; set; } // 업데이트일
        public int view_count { get; set; } // 조회수
    }

    public class LectureMaterialContent : BoardContent
    {
		[JsonIgnore]
		public override int content_id { get; set; }

		public int material_id {
			get { return content_id; }
			set { content_id = value; }
		}

		public int week { get; set; } // 강의자료 활용 주차
    }

    public class NoticeContent : BoardContent
    {
		[JsonIgnore]
		public override int content_id { get; set; }

		public int notice_id {
			get { return content_id; }
			set { content_id = value; }
		}
	}
}
