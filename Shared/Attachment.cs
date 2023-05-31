namespace ClassHub.Shared {
    public class Attachment {
        public int attachment_id { get; set; }
        public string file_name { get; set; }
        public int file_size { get; set; } // Byte 단위
        public DateTime up_date { get; set; }
        public string download_url { get; set; }

        public string GetFileSizeString() {
            if(file_size >= 1024 * 1024) {
                // 1 MB 이상일 경우
                return (file_size / (1024.0 * 1024.0)).ToString("0.00") + " MB";
            } else if(file_size >= 1024) {
                // 1 KB 이상일 경우
                return (file_size / 1024.0).ToString("0.00") + " KB";
            } else {
                // Byte 단위일 경우
                return file_size + " Byte";
            }
        }
    }
}
