using Gridsum.DataflowEx;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowEx_Playground
{
    public class Person : IEventProvider
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public DataflowEvent GetEvent()
        {
            if (Age > 70)
            {
                return new DataflowEvent("OldPerson");
            }
            else
            {
                //returning empty so it will not be recorded as an event
                return DataflowEvent.Empty;
            }
        }
    }

    public class PeopleFlow : Dataflow<string, Person>
    {
        private TransformBlock<string, Person> m_converter;
        private TransformBlock<Person, Person> m_recorder;
        private StatisticsRecorder m_peopleRecorder;

        public PeopleFlow(DataflowOptions dataflowOptions)
            : base(dataflowOptions)
        {
            m_peopleRecorder = new StatisticsRecorder(this) { Name = "PeopleRecorder" };

            m_converter = new TransformBlock<string, Person>(s => JsonConvert.DeserializeObject<Person>(s));
            m_recorder = new TransformBlock<Person, Person>(
                p =>
                {
                    //record every person
                    m_peopleRecorder.Record(p);
                    return p;
                });

            m_converter.LinkTo(m_recorder, new DataflowLinkOptions() { PropagateCompletion = true });

            RegisterChild(m_converter);
            RegisterChild(m_recorder);
        }

        public override ITargetBlock<string> InputBlock { get { return m_converter; } }
        public override ISourceBlock<Person> OutputBlock { get { return m_recorder; } }
        public StatisticsRecorder PeopleRecorder { get { return m_peopleRecorder; } }
    }


    [TestFixture]
    public class StatisticsrecorderTests
    {
        [Test]
        public async Task TestMain()
        {
            var f = new PeopleFlow(DataflowOptions.Default);
            var sayHello = new ActionBlock<Person>(p => Console.WriteLine("Hello, I am {0}, {1}", p.Name, p.Age)).ToDataflow(name: "sayHello");
            f.LinkTo(sayHello, p => p.Age > 0);
            f.LinkLeftToNull(); //object flowing here will be recorded by GarbageRecorder

            f.Post("{Name: 'aaron', Age: 20}");
            f.Post("{Name: 'bob', Age: 30}");
            f.Post("{Name: 'carmen', Age: 80}");
            f.Post("{Name: 'neo', Age: -1}");
            await f.SignalAndWaitForCompletionAsync();
            await sayHello.CompletionTask;

            Console.WriteLine("Total people count: " + f.PeopleRecorder[typeof(Person)]);
            Console.WriteLine(f.PeopleRecorder.DumpStatistics());
            Console.WriteLine(f.GarbageRecorder.DumpStatistics());
        }
    }
}
