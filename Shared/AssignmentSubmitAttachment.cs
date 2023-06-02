namespace ClassHub.Shared {
    public class AssignmentSubmitAttachment {
        public int attachment_id { get; set; }
        public int submit_id { get; set; }
        public string file_name { get; set; }
        public string file_url { get; set; }
        public DateTime update { get; set; }
        public double file_size { get; set; }
    }
}
