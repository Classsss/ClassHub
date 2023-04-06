namespace ClassHub.Client.Models {
    public class Classroom {
        public int Id { get; set; }
        public string CourseId { get; set; } // ABC12345
        public int SectionId { get; set; } // 0810
        public string Semester { get; set; } // SPRING, SUMMER, FALL, WINTER         
        public int Year { get; set; } // 2023
        public string Title { get; set; } // 캡스톤디자인
    }
}
