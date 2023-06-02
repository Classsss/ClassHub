namespace ClassHub.Shared
{
    public class LectureProgress
    {
        public int room_id { get; set; }
        public int lecture_id { get; set; }
        public int student_id { get; set; }
        public string student_name { get; set; }
        public int elapsed_time { get; set; }
        public bool is_enroll { get; set; }
    }
}
