/*
 * Copyright (C) 2017-present Connection Loops Pvt. Ltd., Inc - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
*/
using System.Net;

namespace Cloops.Exceptions
{
    public class NotFoundException : CLHttpException
    {
        public NotFoundException()
        {
            Hs = (int)HttpStatusCode.NotFound;
        }

        public NotFoundException(string message) : base(message)
        {
            Hs = (int)HttpStatusCode.NotFound;
        }
        public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
        {
            Hs = (int)HttpStatusCode.NotFound;
        }
    }

}
