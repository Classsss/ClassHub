namespace ClassHub.Client.Models {
	public class Assignment // 과제 클래스
	{
		public int Id { get; set; } // 과제 번호
		public string Title { get; set; } // 과제 제목
		public string Description { get; set; } // 과제 내용
		public string Author { get; set; } // 작성자
		public DateTime Deadline { get; set; } // 마감일
		public bool IsSubmitted { get; set; } // 제출여부
		public DateTime? SubmissionDate { get; set; } // 제출일

		public string SubmissionFilePath { get; set; } // 제출한 파일의 경로

	}
}
