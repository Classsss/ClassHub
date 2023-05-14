namespace ClassHub.Shared
{
    public class CodeAssignment
    {
        public int room_id { get; set; }
        public int week { get; set; } 
        public int assignment_id { get; set; } 
        public int problem_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public bool is_apply_score { get; set; }

    }
}
