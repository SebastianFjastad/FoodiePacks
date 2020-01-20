using FoodiePacks;
using Microsoft.Azure.WebJobs;

namespace FoodiePacksWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        public static void Main()
        {
            var host = new JobHost();
            host.Call(typeof(Program).GetMethod("RunTasks"));
        }

        [NoAutomaticTrigger]
        public static void RunTasks()
        {
            var manager = new OrderManager();
            //manager.SendEmail();
        }
    }
}
