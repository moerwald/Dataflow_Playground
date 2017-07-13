using Gridsum.DataflowEx;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowEx_Playground
{

    [TestFixture]
    public class LinktToLeftTest
    {
        [Test]
        public async Task TestLinkToLeft()
        {
            var d1 = new BufferBlock<int>().ToDataflow(name: "IntGenerator");
            var d2 = new ActionBlock<int>(s => Console.WriteLine(s)).ToDataflow();
            var d3 = new ActionBlock<int>(s => Console.WriteLine(s)).ToDataflow();
            var d4 = new ActionBlock<int>(s => Console.WriteLine(s + "[Left]")).ToDataflow();

            d1.LinkTo(d2, _ => _ % 3 == 0);
            d1.LinkTo(d3, _ => _ % 3 == 1);
            d1.LinkLeftTo(d4); // all objects not fitting to above links are forwarded on this link

            for (int i = 0; i < 10; i++)
            {
                d1.Post(i);
            }

            await d1.SignalAndWaitForCompletionAsync();
            await d2.CompletionTask;
            await d3.CompletionTask;
            await d4.CompletionTask;
        }


        [Test]
        public void TestLinkToError()
        {
            Assert.ThrowsAsync<AggregateException>(async () =>
            {
                var d1 = new BufferBlock<int>().ToDataflow(name: "IntGenerator");
                var d2 = new ActionBlock<int>(s => Console.WriteLine(s)).ToDataflow();
                var d3 = new ActionBlock<int>(s => Console.WriteLine(s)).ToDataflow();

                d1.LinkTo(d2, _ => _ % 3 == 0);
                d1.LinkTo(d3, _ => _ % 3 == 1);
                d1.LinkLeftToError();

                for (int i = 0; i < 10; i++)
                {
                    d1.Post(i);
                }

                await d1.SignalAndWaitForCompletionAsync();
                await d2.CompletionTask;
                await d3.CompletionTask;
            });
        }
    }
}
