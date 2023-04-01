/*
 * Copyright (C) 2017-present Connection Loops Pvt. Ltd., Inc - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
*/
using System.Net;

namespace Cloops.Exceptions
{
    public class InvalidParameterException : CLHttpException
    {
        

        public InvalidParameterException()
        {
            Hs = (int)HttpStatusCode.BadRequest;
        }

        public InvalidParameterException(string message) : base(message)
        {
            Hs = (int)HttpStatusCode.BadRequest;
        }

        public InvalidParameterException(string message, int hs) : base(message)
        {
            Hs = hs;
        }

        public InvalidParameterException(string message, Exception innerException)
        : base(message, innerException)
        {
            Hs = (int)HttpStatusCode.BadRequest;
        }

        
    }
}
