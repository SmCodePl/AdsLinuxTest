namespace PlcDataModel.Abstract
{
    public abstract class PlcRequestHelper
    {
        protected abstract void HandleRequest();
        protected abstract short DoMeasurment(string readHeader, string readBody, string writeHeader, string writeBody, out string strReadExedution, out string strWriteExecution);
    }
}