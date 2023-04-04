namespace ClassHub.Shared
{
        public class CourseResponse
        {
            public int Number { get; set; } // 강좌 번호
            public int Year { get; set; } // 개설 년도
            public int Semester { get; set; } // 개설 학기
            public string Name { get; set; } // 교과목명
            public string Instructor { get; set; } // 담당교수
            public DayOfWeek Day { get; set; } // 강의요일
            public int StartTime { get; set; } // 강의 시작시간
            public int EndTime { get; set; } // 강의 종료시간
            public string ClassRoom { get; set; } // 강의실
        }
}