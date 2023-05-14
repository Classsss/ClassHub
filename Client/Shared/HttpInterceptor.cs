using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;

namespace ClassHub.Client.Shared {
    //주고받는 모든 API의 요청과 응답을 가로채는 HttpInterceptor
    public class HttpInterceptor : DelegatingHandler {
        private readonly NavigationManager navigationManager;
        public HttpInterceptor(NavigationManager navigationManager) {
            this.navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            await PreProcess(request); // 전처리 작업 수행
            var response = await base.SendAsync(request, cancellationToken);
            await PostProcess(response); // 후처리 작업 수행

            return response;
        }

        public virtual async Task PreProcess(HttpRequestMessage request) {
            // 요청 전처리 작업 수행 (헤더 추가, 인증 토큰 설정 등)
            // 필요한 경우 request.Headers.Add(...) 등을 사용하여 헤더 추가 가능
            // 필요한 경우 request.Headers.Authorization 등을 사용하여 인증 토큰 설정 가능
        }

        public virtual async Task PostProcess(HttpResponseMessage response) {
            // 응답 후처리 작업 수행 (응답 코드 확인, 로그아웃 처리 등)
            // 필요한 경우 response.StatusCode 등을 사용하여 응답 상태 코드 확인 가능
            // 필요한 경우 navigationManager.NavigateTo("/logout") 등을 사용하여 로그아웃 처리 가능
            Console.WriteLine("후처리 시작");
            Console.WriteLine(response);
            Console.WriteLine(response.StatusCode);
            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                // 로그아웃 처리
                Console.WriteLine("토큰이 만료되었는데요?");
                navigationManager.NavigateTo("/logout");
            }
        }
    }

public class HttpInterceptorHandler : DelegatingHandler {
        private readonly HttpInterceptor httpInterceptor;

        public HttpInterceptorHandler(HttpInterceptor httpInterceptor, HttpMessageHandler innerHandler)
            : base(innerHandler) {
            this.httpInterceptor = httpInterceptor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            await httpInterceptor.PreProcess(request); // 필요한 전처리 작업 수행

            var response = await base.SendAsync(request, cancellationToken);

            await httpInterceptor.PostProcess(response); // 필요한 후처리 작업 수행

            return response;
        }
    }
}
