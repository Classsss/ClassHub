namespace ClassHub.Shared {
    public class AssignmentMaterialAttachment {
        public int attachment_id { get; set; }
        public int assignment_id { get; set; }
        public string file_name { get; set; }
        public string file_url { get; set; }
        public DateTime update { get; set; }
        public double file_size { get; set; }
    }
}
