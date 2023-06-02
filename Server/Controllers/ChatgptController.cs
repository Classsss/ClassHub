using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace GptServer.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ChatgptController : ControllerBase {
        private string API_KEY = "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";
        // 교수가 해당 강의실의 실습번호에 해당하는 제출리스트들을 불러온다.
        [HttpPost]
        public async Task<string> AskGptAsync([FromForm] string Code) { 
            // api URI
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            // OpenAI API Key
            string apiKey = API_KEY;

            // 질문
            string question = Code;

            //보낼 질문 생성
            var messages = new[] {
            new { role = "system", content = "assistant는 코드의 오류나 아쉬운 부분 대한 설명을 해준다." },
            new { role = "user", content = question }};

            // HTTP 요청 객체 생성
            using (HttpClient client = new HttpClient()) {
                // HTTP 요청 헤더에 API Key 추가
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                // HTTP 요청 바디에 데이터 추가
                string requestBody = JsonConvert.SerializeObject(new {
                    model = "gpt-3.5-turbo",
                    messages
                });


                var httpContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

                // OpenAI API에 HTTP POST 요청 전송
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                // HTTP 응답 바디에서 데이터 추출
                string responseString = await response.Content.ReadAsStringAsync();

                // JSON 데이터 파싱
                System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(responseString);
                string output = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").ToString();
                string resultWithLineBreaks = output.Replace("'''", "\n");
                // 결과 출력
                return resultWithLineBreaks;
            }
        }
    }
}