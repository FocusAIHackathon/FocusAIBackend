/*
 * Copyright (C) 2017-present Connection Loops Pvt. Ltd., Inc - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Author: Gaurav Kalele, Siddhant Patil
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace Cloops.Infra
{
    /*
     * Connection Loops DB middleware. 
     * Author : Gaurav Kalele 
     * Discription: 
     * 
     * It connects to db to perform given queries 
     * able to return json natively 
     * can return data table 
     * perform update and insert operations as well 
     * manages connections efficiently 
     * 
     * */
    public class db
    {
        // private static Logger _logger = LogManager.GetLogger("Log.db");
        private static string discarded = "";
        public db()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public static async Task<JObject> readFromReadbind(SqlCommand sqlcmd)
        {
            using (SqlDataReader readbind = await sqlcmd.ExecuteReaderAsync())
            {
                JObject rtObject;
                string emptyJSON = "[]";
                System.Text.StringBuilder stagging = new System.Text.StringBuilder();
                int i = 0;
                while (await readbind.ReadAsync())
                {
                    i++;
                    stagging.Append(readbind[0].ToString());

                }
                if (i > 0)
                {
                    string dstagging = stagging.ToString();
                    dstagging = "{\"result\":" + dstagging + "}";
                    rtObject = JObject.Parse(dstagging);
                }
                else
                {
                    string dstagging = "{\"result\":" + emptyJSON + "}";
                    rtObject = JObject.Parse(dstagging);
                }

                return rtObject;
            }

        }

        public static void bulkDataExecuteUpdate(string cmd, JArray data)
        {
            // // _logger.Trace("Entering bulk update..");
            // string cnstr1 = services.configReader.config.GetSection("ConnectionStrings")["cnstr1"];
            string cnstr1 = currentInstance.cnstr1;

            if (data.Count == 0)
            {
                //// _logger.Debug("No Data Array given to bulkDataExecuteUpdate");
                return;
            }
            //// _logger.Debug("using cnstr as " + cnstr1);
            using (SqlConnection hookup = new SqlConnection(cnstr1))
            {
                hookup.Open();
                //// _logger.Debug("Connection opened. Command is: " + cmd);
                using (SqlCommand sqlcmd = new SqlCommand(cmd, hookup))
                {

                    //run the command n times 
                    for (int i = 0; i < data.Count; i++)
                    {
                        //// _logger.Info($"Running query for values: {data[i]}");
                        //set each of the par                         
                        foreach (JProperty jp in data[i])
                        {
                            if (i == 0)
                            {
                                //define  params 
                                sqlcmd.Parameters.AddWithValue("@" + jp.Name, jp.Value.ToString());

                            }
                            else
                            {
                                //set the params 
                                sqlcmd.Parameters["@" + jp.Name].Value = jp.Value.ToString();

                            }

                        }
                        sqlcmd.ExecuteNonQuery();
                        // _logger.Trace("Command executed..");
                    }
                }
                hookup.Close();
                // _logger.Trace("Connection closed.");
            }
        }
        public static int executeUpdate(params object[] pars)
        {
            char[] qparEscapes = { ' ', ',', ')' };
            // _logger.Trace("Entering executeUpdate...");
            bool useDifferentConnection = false;
            SqlConnection wildCardConnection = null;
            int rowsUpdated = 0;

            string cnstr1 = "";
            try
            {
                cnstr1 = currentInstance.cnstr1;
            }
            catch (Exception) { }
            string cmd = pars[0].ToString();
            // _logger.Info("For Query: " + cmd);
            //check for wild card
            if (cmd[0] == '@')
            {
                cmd = cmd.Substring(1); // reject the wild card
                // _logger.Debug("Will be using wildcard connection.. Skipping first wild card.");
                useDifferentConnection = true;
                try
                {
                    wildCardConnection = (SqlConnection)(pars[1]);
                    // _logger.Debug("Got a opened connection: " + pars[1]);
                }
                catch (Exception notAString)
                {
                    discarded = notAString.Message;
                    cnstr1 = pars[1].ToString();
                    // _logger.Debug("Will be opening connection for db with cnstr: " + pars[1]);
                }

            }
            List<string> qpars = new List<string>();
            for (int i = 0, start = -1; i < cmd.Length; i++)
            {
                if (cmd[i] == '@')
                {
                    i++; //now pointing to start of substring. 
                    start = i - 1;

                    while (Array.IndexOf(qparEscapes, cmd[i]) == -1)
                    {
                        i++;
                        if (i == cmd.Length)
                        {
                            break;
                        }
                    }

                    //at this point cmd[i] = ' '; @cmd 
                    qpars.Add(cmd.Substring(start, i - start));
                }
            }
            // _logger.Debug("Parameters parsed. Count: " + qpars.Count);

            //db aspects
            int offset = 1;
            if (useDifferentConnection)
            {
                offset = 2;
            }
            if (wildCardConnection == null)
            {
                // _logger.Debug("Opening new connection.");
                using (SqlConnection hookup = new SqlConnection(cnstr1))
                {
                    hookup.Open();
                    // _logger.Trace("Connection Opened.");
                    using (SqlCommand sqlcmd = new SqlCommand(cmd, hookup))
                    {
                        // _logger.Trace("Prepared statement: Adding parameters.");
                        for (int i = 0; i < qpars.Count; i++)
                        {
                            try
                            {
                                string s = sqlcmd.Parameters[qpars[i]].Value.ToString();
                                // _logger.Debug("Parameter already present. Par: " + qpars[i] + ", Value: " + s);
                            }
                            catch (Exception ty)
                            {
                                discarded = ty.Message;
                                if (pars[i + offset] == null)
                                {
                                    sqlcmd.Parameters.AddWithValue(qpars[i], DBNull.Value);
                                }
                                else
                                {
                                    sqlcmd.Parameters.AddWithValue(qpars[i], pars[i + offset]);
                                }
                                // _logger.Debug("Added parameter: Par: " + qpars[i] + ", Value: " + pars[i + offset]);
                            }
                        }

                        rowsUpdated = sqlcmd.ExecuteNonQuery();
                        // _logger.Trace("Command executed");
                    }
                    hookup.Close();
                    // _logger.Trace("Connection closed..");
                }
            }
            else
            {
                // _logger.Debug("Using wildcard connection..");
                using (SqlCommand sqlcmd = new SqlCommand(cmd, wildCardConnection))
                {
                    for (int i = 0; i < qpars.Count; i++)
                    {
                        // _logger.Trace("Prepared statement: Adding parameters.");
                        try
                        {
                            string s = sqlcmd.Parameters[qpars[i]].Value.ToString();
                            // _logger.Debug("Parameter already present. Par: " + qpars[i] + ", Value: " + s);

                        }
                        catch (Exception ty)
                        {
                            discarded = ty.Message;
                            if (pars[i + offset] == null)
                            {
                                sqlcmd.Parameters.AddWithValue(qpars[i], DBNull.Value);
                            }
                            else
                            {
                                sqlcmd.Parameters.AddWithValue(qpars[i], pars[i + offset]);
                            }
                            // _logger.Debug("Added parameter: Par: " + qpars[i] + ", Value: " + pars[i + offset]);
                        }
                    }

                    rowsUpdated = sqlcmd.ExecuteNonQuery();
                    // _logger.Trace("Command executed.. No need to close connection..");
                }
            }

            return rowsUpdated;

        }

        public static DataTable execute(params object[] pars)
        {
            //// _logger.Trace("In db.execute()");
            DataTable dt = executeRaw(pars) as DataTable;
            return dt;
        }

        public static JObject executeJSONWithCNRef(ref SqlConnection hookup, params object[] pars)
        {
            // TO DO : Implement the proper referencing of the Connection
            // _logger.Trace("In db.executeJSON()");
            JObject dt = executeRaw(pars) as JObject;
            return dt;
        }

        public static JObject executeJSON(params object[] pars)
        {
            // _logger.Trace("In db.executeJSON()");
            JObject dt = executeRaw(pars) as JObject;
            return dt;
        }

        private static Object executeRaw(params object[] pars)
        {
            char[] qparEscapes = { ' ', ',', ')' };
            // _logger.Trace("In db.executeRaw()");
            bool useDifferentConnection = false;
            SqlConnection wildCardConnection = null;
            string cnstr1 = "";
            string cmd = pars[0].ToString();
            // _logger.Debug("Command is: " + cmd);
            bool jSONMode = false;
            if (cmd.ToLower().Contains("for json "))
            {
                jSONMode = true;
                // _logger.Trace("jsonMode is true");
            }

            //check for wild card
            if (cmd[0] == '@')
            {
                cmd = cmd.Substring(1); // reject the wild card
                // _logger.Trace("Wild card conection. Skipping first wild card");
                useDifferentConnection = true;
                try
                {
                    wildCardConnection = (SqlConnection)(pars[1]);
                    // _logger.Debug("Got a opened connection: " + pars[1]);
                }
                catch (Exception notAString)
                {
                    discarded = notAString.Message;
                    cnstr1 = pars[1].ToString();
                    // _logger.Debug("Will be opening connection for db with cnstr: " + pars[1]);
                }

            }
            else
            {
                cnstr1 = currentInstance.cnstr1; // normal connection

            }
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
                        if (i == cmd.Length)
                        {
                            break;
                        }
                    }

                    //at this point cmd[i] = ' '; @cmd 
                    qpars.Add(cmd.Substring(start, i - start));
                }
            }
            // _logger.Debug("Parameters parsed. Parameters Count: " + qpars.Count);

            //db aspects
            int offset = 1;
            if (useDifferentConnection)
            {
                offset = 2;
            }
            Object rtObject = null;
            string emptyJSON = "[]";
            DataTable data = new DataTable(); // no data table in .net core
            if (wildCardConnection == null)
            {
                // _logger.Debug("Opening new connection. using cnstr as " + cnstr1);
                using (SqlConnection hookup = new SqlConnection(cnstr1))
                {
                    hookup.Open();
                    // _logger.Trace("Connection Opened.");
                    using (SqlCommand sqlcmd = new SqlCommand(cmd, hookup))
                    {
                        // _logger.Trace("Prepared statement: Adding parameters.");
                        for (int i = 0; i < qpars.Count; i++)
                        {
                            try
                            {
                                string s = sqlcmd.Parameters[qpars[i]].Value.ToString();
                                // _logger.Debug("Parameter already present. Par: " + qpars[i] + ", Value: " + s);
                            }
                            catch (Exception ty)
                            {
                                discarded = ty.Message;
                                sqlcmd.Parameters.AddWithValue(qpars[i], pars[i + offset]);
                                // _logger.Debug("Added parameter: Par: " + qpars[i] + ", Value: " + pars[i + offset]);
                            }
                        }
                        if (!jSONMode)
                        {
                            //throw (new Exception("Non JSON mode is not yet ported to .net core ! "));

                            data.Load(sqlcmd.ExecuteReader());
                            rtObject = data;
                        }
                        else
                        {
                            SqlDataReader readbind = sqlcmd.ExecuteReader();
                            System.Text.StringBuilder stagging = new System.Text.StringBuilder();
                            int i = 0;
                            while (readbind.Read())
                            {
                                i++;
                                stagging.Append(readbind[0].ToString());

                            }
                            if (i > 0)
                            {
                                string dstagging = stagging.ToString();
                                dstagging = "{\"result\":" + dstagging + "}";
                                // _logger.Info(dstagging);
                                rtObject = JObject.Parse(dstagging);
                            }
                            else
                            {
                                string dstagging = "{\"result\":" + emptyJSON + "}";
                                // _logger.Info(dstagging);
                                rtObject = JObject.Parse(dstagging);
                            }
                            readbind.Dispose();
                        }
                    }
                    hookup.Close();
                }
            }
            else
            {
                // _logger.Debug("Using wildcard/already opened connection..");
                using (SqlCommand sqlcmd = new SqlCommand(cmd, wildCardConnection))
                {
                    // _logger.Trace("Prepared statement: Adding parameters.");
                    for (int i = 0; i < qpars.Count; i++)
                    {
                        try
                        {
                            string s = sqlcmd.Parameters[qpars[i]].Value.ToString();
                            // _logger.Debug("Parameter already present. Par: " + qpars[i] + ", Value: " + s);
                        }
                        catch (Exception ty)
                        {
                            discarded = ty.Message;
                            sqlcmd.Parameters.AddWithValue(qpars[i], pars[i + offset]);
                            // _logger.Debug("Added parameter: Par: " + qpars[i] + ", Value: " + pars[i + offset]);
                        }
                    }
                    if (!jSONMode)
                    {
                        throw (new Exception("Non JSON mode is not yet ported to .net core ! "));
                        /*
                        data.Load(sqlcmd.ExecuteReader());
                        rtObject = data;*/
                    }
                    else
                    {
                        SqlDataReader readbind = sqlcmd.ExecuteReader();
                        System.Text.StringBuilder stagging = new System.Text.StringBuilder();
                        int i = 0;
                        while (readbind.Read())
                        {
                            i++;
                            stagging.Append(readbind[0].ToString());

                        }
                        if (i > 0)
                        {
                            string dstagging = stagging.ToString();
                            dstagging = "{\"result\":" + dstagging + "}";
                            // _logger.Info(dstagging);
                            rtObject = JObject.Parse(dstagging);
                        }
                        else
                        {
                            string dstagging = "{\"result\":" + emptyJSON + "}";
                            // _logger.Info(dstagging);
                            rtObject = JObject.Parse(dstagging);
                        }
                        readbind.Dispose();
                    }
                }

            }

            return rtObject;
        }

        public static List<T> getItemList<T>(params object[] pars) where T : new()
        {
            List<T> list = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            bool useDifferentConnection = false;
            SqlConnection wildCardConnection = null;
            string cnstr1 = "";
            string cmd = pars[0].ToString();

            //check for wild card
            if (cmd[0] == '@')
            {
                cmd = cmd.Substring(1); // reject the wild card
                // _logger.Trace("Wild card conection. Skipping first wild card");
                useDifferentConnection = true;
                try
                {
                    wildCardConnection = (SqlConnection)(pars[1]);
                    // _logger.Debug("Got a opened connection: " + pars[1]);
                }
                catch (Exception notAString)
                {
                    discarded = notAString.Message;
                    cnstr1 = pars[1].ToString();
                    // _logger.Debug("Will be opening connection for db with cnstr: " + pars[1]);
                }

            }
            else
            {
                cnstr1 = currentInstance.cnstr1; // normal connection
            }
            
            List<string> qpars = DBUtils.GetQueryParams(cmd);

            int offset = 1;
            if (useDifferentConnection)
            {
                offset = 2;
            }

            if (wildCardConnection == null)
            {
                using (SqlConnection con = new SqlConnection(cnstr1))
                {
                    con.Open();

                    using (SqlCommand sqlcmd = new SqlCommand(cmd, con))
                    {
                        for (int i = 0; i < qpars.Count; i++)
                        {
                            try
                            {
                                string s = sqlcmd.Parameters[qpars[i]].Value.ToString();
                            }
                            catch (Exception ty)
                            {
                                discarded = ty.Message;
                                sqlcmd.Parameters.AddWithValue(qpars[i], pars[i + offset]);
                            }
                        }

                        SqlDataReader reader = sqlcmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (reader.HasRows)
                            {
                                // create new object for the row
                                T _row_obj = new T();

                                // Iterate over columns included in select clause.
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string fieldName = reader.GetName(i);
                                    var prop = props.FirstOrDefault(x => x.Name.ToLower() == fieldName.ToLower());
                                    if (prop != null)
                                    {
                                        if (reader[i] != DBNull.Value)
                                        {
                                            prop.SetValue(_row_obj, reader[i], null);
                                        }
                                    }
                                }
                                list.Add(_row_obj);
                            }
                        }
                    }
                    con.Close();
                }
            }
            else
            {
                using (SqlCommand sqlcmd = new SqlCommand(cmd, wildCardConnection))
                {
                    for (int i = 0; i < qpars.Count; i++)
                    {
                        try
                        {
                            string s = sqlcmd.Parameters[qpars[i]].Value.ToString();
                        }
                        catch (Exception ty)
                        {
                            discarded = ty.Message;
                            sqlcmd.Parameters.AddWithValue(qpars[i], pars[i + offset]);
                        }
                    }

                    SqlDataReader reader = sqlcmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            // create new object for the row
                            T _row_obj = new T();

                            // Iterate over columns included in select clause.
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string fieldName = reader.GetName(i);
                                var prop = props.FirstOrDefault(x => x.Name.ToLower() == fieldName.ToLower());
                                if (prop != null)
                                {
                                    if (reader[i] != DBNull.Value)
                                    {
                                        prop.SetValue(_row_obj, reader[i], null);
                                    }
                                }
                            }
                            list.Add(_row_obj);
                        }
                    }
                }
            }

            return list;
        }
    }
}
