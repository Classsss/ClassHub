namespace ClassHub.Shared {
    public class Exam {
        public int exam_id { get; set; }
        public int room_id { get; set; }
        public int week { get; set; }
        public string title { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public bool is_random_problem { get; set; }
        public bool is_random_choice { get; set; }
        public bool is_show_time_limit { get; set; }
        public bool is_back_to_previous_problem { get; set; }
    }
}
