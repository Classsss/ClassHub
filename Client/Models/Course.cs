namespace ClassHub.Client.Models {
    public class Course {
        public string Name { get; set; }
        public string Instructor { get; set; }
        public DayOfWeek Day { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public string ClassRoom { get; set; }
    }

}
