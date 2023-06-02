namespace ClassHub.Shared
{
    public class CodeSubmit
    {
        public int room_id { get; set; }
        public int week { get; set; } 
        public int assignment_id { get; set; } 
        public int submit_id { get; set; }
        public DateTime submit_date { get; set; }
        public string status { get; set; }
        public int student_id { get; set; }
        public string student_name { get; set; }
        public double exec_time { get; set; }      
        public long mem_usage { get; set; }             
        public string code { get; set; }
        public string message { get; set; }
        public string language { get; set; }
    }
}
