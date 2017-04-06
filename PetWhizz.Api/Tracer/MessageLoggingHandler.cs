using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity.Validation;
using PetWhizz.Data;

namespace PetWhizz.Api.Tracer
{
    public class MessageLoggingHandler : MessageHandler
    {
        private static Logger m_Logger = LogManager.GetCurrentClassLogger();
        protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() => UpdateLogMessage(correlationId, requestInfo, message));
        }

        private void UpdateLogMessage(string correlationId, string requestInfo, byte[] message)
        {
            try
            {
                using (var ctx = new PetWhizzEntities())
                {
                    ioLogger ioLogger = new ioLogger()
                    {
                        json = Encoding.UTF8.GetString(message),
                        triggeredTime = DateTime.Now,
                        type = "Request",
                        url = requestInfo,
                        userName =""
                    };
                    ctx.ioLoggers.Add(ioLogger);
                    ctx.SaveChanges();
                }
              
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                // throw;
            }
            catch (Exception e)
            { }
        }

        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
                m_Logger.Debug(string.Format("{0} - Response: {1}\r\n{2}", correlationId, requestInfo, Encoding.UTF8.GetString(message))));
        }
    }
}