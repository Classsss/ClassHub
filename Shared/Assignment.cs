namespace ClassHub.Shared
{
    public class Assignment
    {
        public int room_id { get; set; }
        public int assignment_id { get; set; }
        public string title { get; set; } 
        public string author { get; set; } 
        public string contents { get; set; } 
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }
}
