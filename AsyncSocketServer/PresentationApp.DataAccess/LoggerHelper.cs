using System;
using System.Text;

namespace PresentationApp.DataAccess
{
    public class LoggerHelper
    {
        //static LoggerHelper()
        //{
        //    log4net.Config.XmlConfigurator.Configure();
        //}

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Info(string info)
        {
            log.Info(info);
        }

        public static void LogRequestRecieved(string methodName)
        {
            log.Debug(string.Format("{0}  Client request received for {1}.", DateTime.Now.ToString(), methodName));
        }

        public static void LogMessage(string methodName, string message)
        {
            log.Debug(string.Format("  {0} : {1} ", methodName, message));
        }

        public static void LogResponseToClient(string methodName)
        {
            log.Debug(string.Format("Response return to client for {0}", methodName));
        }

        public static void LogException(string methodName, Exception exp, string functionParameters = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}  {1} - Exception : {2} . {3}", DateTime.Now.ToString(), methodName, exp.Message, Environment.NewLine);

            if (!string.IsNullOrEmpty(functionParameters))
            {
                sb.AppendFormat("Input Parameters : {0}", functionParameters);
            }

            sb.AppendFormat("Exception Source: {0} {1} ", exp.Source, Environment.NewLine);
            sb.AppendFormat("Exception Details: {0} {1}", exp.InnerException != null ? exp.InnerException.ToString() : "", Environment.NewLine);
            sb.AppendFormat("Exception Stack Trace : {0} {1}", exp.StackTrace, Environment.NewLine);

            log.Error(sb.ToString());
        }
    }
}