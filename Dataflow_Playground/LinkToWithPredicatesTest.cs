using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading.Tasks.Dataflow;

namespace Dataflow_Playground
{
    [TestFixture]
    public class LinkToWithPredicatesTest
    {
        [Test]
        public async Task yyy ()
        {
            System.Diagnostics.Debugger.Break();
            Random rnd = new Random();

            ITargetBlock<int> temp = BuildPipeline(5);

            foreach (var i in Enumerable.Range(0,20))
            {
                temp.Post(i);
            }

            temp.Complete();
            await Task.Delay(250); // Give worker time the consume messages
            
        }

        private static ITargetBlock<int> BuildPipeline(int NumProductionLines)
        {
            var productionQueue = new BufferBlock<int>();

            var opt = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 };
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            for (int i = 0; i < NumProductionLines; i++)
            {
                int j = i;
                ActionBlock<int> productionLine = new ActionBlock<int>(num => TraceHelper.TraceWithTreadId($"Processed by line {j}: {num}"));

                productionQueue.LinkTo(productionLine, linkOptions , x => x % NumProductionLines == j);

            }

            ActionBlock<int> discardedLine = new ActionBlock<int>(num => TraceHelper.TraceWithTreadId("Discarded: {num}"));
            productionQueue.LinkTo(discardedLine);

            return productionQueue;
        }
    }
}
