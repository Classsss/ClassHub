namespace ClassHub.Shared
{
    public class AssignmentSubmit
    {
        public int submit_id { get; set; }
        public int assignment_id { get; set; }
        public int room_id { get; set; } 
        public int student_id { get; set; }
        public string student_name { get; set; }
        public int score { get; set; }
        public DateTime submit_date { get; set; }
        public string message { get; set; }

    }
}
