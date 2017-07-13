using Gridsum.DataflowEx;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowEx_Playground
{
    [TestFixture]
    public class BroadcastTest
    {
        [Test]
        public async Task MainAsync()
        {
            // Check the testcase ouput for logging
            var broadcaster = new DataBroadcaster<string>();

            var printer1 = new ActionBlock<string>(s => Console.WriteLine("Printer1: {0}", s)).ToDataflow();
            var printer2 = new ActionBlock<string>(s => Console.WriteLine("Printer2: {0}", s)).ToDataflow();
            var printer3 = new ActionBlock<string>(s => Console.WriteLine("Printer3: {0}", s)).ToDataflow();

            broadcaster.LinkTo(printer1);
            broadcaster.LinkTo(printer2);
            broadcaster.LinkTo(printer3);

            broadcaster.Post("first message");
            broadcaster.Post("second message");
            broadcaster.Post("third message");

            await broadcaster.SignalAndWaitForCompletionAsync();
            await printer1.CompletionTask;
            await printer2.CompletionTask;
            await printer3.CompletionTask;
        }
    }
}
