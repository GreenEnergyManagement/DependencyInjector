using System;
using System.ServiceModel;

namespace Gem.DependencyInjector.Wcf.Test
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        void SetCurrentDate(DateTime date);

        [OperationContract]
        DateTime GetCurrentDate();
    }

    public class TestService : ITestService
    {
        private DateTime? date;
        private static Object synch = new object();
        private static Object synch2 = new object();
        public TestService()
        {
            Console.WriteLine("Hey, I'm created....");
        }

        public void SetCurrentDate(DateTime date)
        {
            lock (synch)
            {
                if (this.date == null)
                {
                    lock (synch2)
                    {
                        this.date = date;    
                    }
                }
                else
                {
                    if (this.date > date) throw new ArgumentException("This is just to see if we can get a faulted state on the channel...");
                }
            }
        }

        public DateTime GetCurrentDate()
        {
            return date.Value;
        }
    }
}