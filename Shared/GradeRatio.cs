using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassHub.Shared {
    public class GradeRatio {
        [JsonIgnore]
        private readonly HttpClient? Http;

        public int room_id { get; set; }
        public double attendance_ratio { get; set; } = 1d;
        public double assignment_ratio { get; set; } = 1d;
        public double practice_ratio { get; set; } = 1d;
        public double exam_ratio { get; set; } = 1d;

        public GradeRatio() {

        }

        public GradeRatio(HttpClient httpClient) {
            Http = httpClient;
        }

        public async Task UpdateGradeRatio() {
            if(Http == null) throw new Exception("Http is null");

            string requestUri = "/api/classroom/modify/graderatio";
            string jsonString = JsonSerializer.Serialize(this);
            HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            await Http.PutAsync(requestUri, httpContent);
        }

        public async Task SetGradeRatio() {
            if(Http == null) throw new Exception("Http is null");

            string requestUri = "/api/classroom/set/graderatio";
            string jsonString = JsonSerializer.Serialize(this);
            HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            await Http.PostAsync(requestUri, httpContent);
        }
    }
}
