using System.Web;

namespace ShiroiTool.WebServer.Processors
{
    public class HttpRuntimeProcessor : AbstractProcessor
    {
        public override bool Intercept(Request request)
        {
            request.PrepareResponse();

            // Hand the processing over to HttpRuntime
            HttpRuntime.ProcessRequest(request);

            return true;
        }
    }
}
