using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Enrichable.Tests
{

    public class when_intercepting_GET_request : with_owin_context
    {

        public when_intercepting_GET_request()
        {
            var target = new ReverseProxyInterceptComponent(async (e) => { });
            var result = target.Invoke(Context.Environment);
            if (!result.IsCompleted)
            {
                result.RunSynchronously();
            }
        }

        protected override object RequestBody => null;

        [Fact]
        public async void then_request_method_is_stored()
        {
            Asserts.DictionaryMatch("rp.RequestMethod", "GET", Context.Environment);
        }

        [Fact]
        public async void then_accept_header_is_stored()
        {
            Asserts.DictionaryMatch("rp.RequestAcceptHeader", "application/json", Context.Environment);
        }

        [Fact]
        public async void then_request_path_is_stored()
        {
            Asserts.DictionaryMatch("rp.RequestPath", "/", Context.Environment);
        }
    }
}