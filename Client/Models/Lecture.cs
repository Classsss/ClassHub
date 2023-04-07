namespace ClassHub.Client.Models
{
    public class Lecture
    {
        public int Id { get; set; } // 강의 번호
        public string Title { get; set;} // 강의 제목
        public string Description { get; set;} // 강의 설명
        public string VideoUrl { get; set;} // 강의 링크

        public DateTime RunningTime { get; set; } // 강의 시간


        public bool IsEnrolled { get; set;} // 수강 여부
    }
}
