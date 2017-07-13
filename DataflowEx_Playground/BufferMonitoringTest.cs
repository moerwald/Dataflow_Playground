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

    public class SlowFlow : Dataflow<string>
    {
        private Dataflow<string, char> _splitter;
        private Dataflow<char> _printer;

        public SlowFlow(DataflowOptions dataflowOptions)
            : base(dataflowOptions)
        {
            _splitter = new TransformManyBlock<string, char>(new Func<string, IEnumerable<char>>(this.SlowSplit),
                dataflowOptions.ToExecutionBlockOption())
                .ToDataflow(dataflowOptions, "SlowSplitter");

            _printer = new ActionBlock<char>(c => Console.WriteLine(c),
                dataflowOptions.ToExecutionBlockOption())
                .ToDataflow(dataflowOptions, "Printer");

            RegisterChild(_splitter);
            RegisterChild(_printer);

            _splitter.LinkTo(_printer);
        }

        private IEnumerable<char> SlowSplit(string s)
        {
            foreach (var c in s)
            {
                System.Threading.Thread.Sleep(1000); //slow down
                yield return c;
            }
        }

        public override ITargetBlock<string> InputBlock { get { return _splitter.InputBlock; } }

    }

    
    [TestFixture]
    public class BufferMonitoringTest
    {
        [Test]
        [Description("Check the test output (test explorer) to see monitoring results")]
        public async Task TestMonitoring()
        {
            //consumer
            var slowFlow = new SlowFlow(new DataflowOptions
            {
                FlowMonitorEnabled = true,
                MonitorInterval = TimeSpan.FromSeconds(2),
                PerformanceMonitorMode = DataflowOptions.PerformanceLogMode.Verbose
            });

            await slowFlow.ProcessAsync(new[]
                                        {
                                    "abcd",
                                    "abc",
                                    "ab",
                                    "a"
                                });
        }
    }
}
