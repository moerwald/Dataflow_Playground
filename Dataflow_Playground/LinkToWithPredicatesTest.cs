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

        private List<ActionBlock<int>> productionLines = new List<ActionBlock<int>>();

        [Test]
        public void LinkTo_WithPredicate ()
        {

            ITargetBlock<int> fabricInput = BuildPipeline(NumProductionLines: 5);

            foreach (var i in Enumerable.Range(0,10))
            {
                fabricInput.Post(i);
            }

            fabricInput.Complete();
            if (Task.WaitAll(this.productionLines.Select(actionBlock => actionBlock.Completion).ToArray(),TimeSpan.FromMilliseconds(1000)) == false)
            {
                Assert.IsFalse(true, "Not all production lines finished within requested time period ...");
            }
            
        }

        private ITargetBlock<int> BuildPipeline(int NumProductionLines)
        {
            var productionQueue = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = -1, });
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            for (int i = 0; i < NumProductionLines; i++)
            {
                int j = i; // Avoid closure uups
                ActionBlock<int> productionLine = new ActionBlock<int>(num => TraceHelper.TraceWithTreadId($"Processed by line [{j}]: input-number={num}"));
                this.productionLines.Add(productionLine);

                productionQueue.LinkTo(productionLine, linkOptions , x => x % NumProductionLines == j); // Route different messages to different target blocks
            }

            ActionBlock<int> discardedLine = new ActionBlock<int>(num => TraceHelper.TraceWithTreadId("Discarded: {num}"));
            productionQueue.LinkTo(discardedLine);

            return productionQueue;
        }
    }
}
