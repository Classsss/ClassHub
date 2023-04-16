namespace ClassHub.Client.Models
{
    public class Lecture
    {
        public int Id { get; set; } // 강의 번호
        public int Week { get; set; } // 강의 주차
        public int Lesson { get; set; } // 강의 차시
        public string Title { get; set;} // 강의 제목
        public string Description { get; set;} // 강의 설명
        public DateTime StartDate { get; set; } // 시작 시간
        public DateTime EndDate { get; set; } // 종료 시간
        public string VideoUrl { get; set;} // 강의 링크
        public int TotalLearningTime { get; set; } // 총 강의 시간
        public int CurrentLearningTime { get; set; } // 현재까지 수강 시간

        public bool IsEnrolled { get; set;} // 수강 여부
    }
}
