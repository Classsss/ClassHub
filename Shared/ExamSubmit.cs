namespace ClassHub.Shared {
    public class ExamSubmit {
        public int submit_id { get; set; }
        public int exam_id { get; set; }
	    public int room_id { get; set; }
        public int student_id { get; set; }
	    public string student_name { get; set; }
        public int score { get; set; }
	    public DateTime submit_date { get; set; }
    }
}
