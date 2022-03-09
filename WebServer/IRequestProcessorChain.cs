namespace ShiroiTool.WebServer
{
    public interface IRequestProcessorChain : IRequestProcessor
    {
        IRequestProcessorChain Add(IRequestProcessor requestProcessor);
        void Clear();
    }
}
