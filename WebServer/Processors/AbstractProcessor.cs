namespace ShiroiTool.WebServer.Processors
{
    public abstract class AbstractProcessor : IRequestProcessor
    {
        #region IRequestProcessor Members

        public virtual void Pre(Request request)
        {}

        public virtual bool Intercept(Request request)
        {
            return false;
        }

        public virtual void Post(Request request)
        {}

        #endregion
    }
}
