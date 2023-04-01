using System.Net;

namespace Cloops.Exceptions
{
    public class CLHttpException : Exception
    {
        int _hs = (int)HttpStatusCode.InternalServerError;

        public CLHttpException()
        {
            
        }

        public virtual string serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { code = (int)_hs, msg = Message });
        }

        public CLHttpException(string message) : base(message)
        {
            
        }
        public CLHttpException(string message, Exception innerException)
        : base(message, innerException)
        {
            
        }

        public int Hs { get => _hs; set => _hs = value; }

    }
}