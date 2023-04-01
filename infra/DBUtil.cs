using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Cloops.Infra
{
    public static class DBUtils
    {
        static char[] qparEscapes = { ' ', ',', ')' };

        public static List<string> GetQueryParams(string cmd)
        {
            List<string> qpars = new List<string>();
            for (int i = 0, start = -1; i < cmd.Length; i++)
            {
                if (cmd[i] == '@')
                {
                    i++; //now pointing to start of substring. 
                    start = i - 1;

                    //fast forward to a space 
                    while (Array.IndexOf(qparEscapes, cmd[i]) == -1)
                    {
                        i++;
                    }

                    //at this point cmd[i] = ' '; @cmd 
                    qpars.Add(cmd.Substring(start, i - start));
                }
            }
            return qpars;
        }
    }
}
