using ClassHub.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using static System.Net.WebRequestMethods;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClassHub.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JudgeController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JudgeRequest request)
        {

            //test출력
            Console.WriteLine("#문제 번호 : " + request.TaskId);
            Console.WriteLine("#언어 : " + request.Language);
            Console.WriteLine("#코드 : " + request.Code);
               
            
            using HttpClient httpClient = new HttpClient();
            string apiUrl = "https://localhost:7024/Judge";

            // db에서 받아서 request에 채점 데이터를 추가함


  
            using StringContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);


            // 통신 여부에 따라 구분
            if (response.IsSuccessStatusCode) 
            {
                JudgeResult? judgeData = await response.Content.ReadFromJsonAsync<JudgeResult>();

                Console.WriteLine("메모리 사용량 : " + judgeData.MemoryUsage);
                Console.WriteLine("시간 : " + judgeData.ExecutionTime);
                return Ok(judgeData);
             }
             else
             {
                 return StatusCode((int)response.StatusCode, "Error in Judge server");
             }
             
            return Ok();
        }
    }
}
