using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassHub.Shared {
    public class Notice {
		[JsonIgnore]
		private readonly HttpClient? Http;

		public int room_id { get; set; } // 강의실 번호
        public int notice_id { get; set; } // 공지 번호
        public string title { get; set; } = string.Empty; // 게시글 제목
        public string author { get; set; } = string.Empty; // 작성자
        public string contents { get; set; } = string.Empty; // 게시글 내용
        public DateTime publish_date { get; set; } = DateTime.Now; // 게시일
        public DateTime up_date { get; set; } = DateTime.Now; // 업데이트일
        public int view_count { get; set; } = 0; // 조회수

        public Notice() {

        }

        public Notice(HttpClient httpClient) {
            Http = httpClient;
		}

        public async Task PostNoticeAsync() {
            if(Http == null) throw new Exception("Http is null");

            string requestUri = "/api/classroom/register/notice";
            string jsonString = JsonSerializer.Serialize(this);
            HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await Http.PostAsync(requestUri, httpContent);
            if(!response.IsSuccessStatusCode) throw new Exception($"Error register notice: {response.StatusCode}");
        }

        public async Task PutNoticeAsync() {
            if(Http == null) throw new Exception("Http is null");

            string requestUri = "/api/classroom/modify/notice";
            string jsonString = JsonSerializer.Serialize(this);
            HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await Http.PutAsync(requestUri, httpContent);
            if(!response.IsSuccessStatusCode) throw new Exception($"Error modify notice: {response.StatusCode}");
        }

        public async Task DeleteNoticeAsync() {
            if(Http == null) throw new Exception("Http is null");

            // 게시글 수정 요청
            string requestUri = $"/api/classroom/{room_id}/delete/notice/{notice_id}";
            await Http.DeleteAsync(requestUri);
        }
    }
}
