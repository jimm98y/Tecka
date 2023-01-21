using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tecka
{
    public interface IHttpClientWrapper : IDisposable
    {
        Task<string> HttpPostAsync(string uri, string body, string contentType = "application/json");

        Task<string> HttpGetAsync(string uri);

        void SetDefaultHeader(string header, string value);
    }

    public class NetHttpClientWrapper : IHttpClientWrapper
    {
        private HttpClient _client = new HttpClient();
        
        private bool disposedValue;

        public virtual async Task<string> HttpPostAsync(string uri, string body, string contentType = "application/json")
        {
            var content = new StringContent(body, System.Text.Encoding.UTF8, contentType);
            using (var response = await _client.PostAsync(uri, content))
            {
                return await GetHttpResultAsync(response);
            }
        }

        public virtual async Task<string> HttpGetAsync(string uri)
        {
            using (var response = await _client.GetAsync(uri))
            {
                return await GetHttpResultAsync(response);
            }
        }

        protected virtual Task<string> GetHttpResultAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync();
            else
                throw new Exception(response.ReasonPhrase);
        }

        public void SetDefaultHeader(string header, string value)
        {
            _client.DefaultRequestHeaders.Add(header, value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client?.Dispose();
                    _client = null;
                }

                disposedValue = true;
            }
        }

        ~NetHttpClientWrapper()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
