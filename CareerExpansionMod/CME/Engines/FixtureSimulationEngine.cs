using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using v2k4FIFAModdingCL.Career.CME;
using System.Threading.Tasks;

namespace v2k4FIFAModding.Career.CME.Engines
{
    public class FixtureSimulationEngine
    {
        public IEnumerable<Fixture> Fixtures { get; set; }
        public FixtureSimulationEngine(IEnumerable<Fixture> fixtures) => Fixtures = fixtures;

        public async Task<bool> Run()
        {
            return await new TaskFactory().StartNew(() => {

                var lstTasks = new List<Task<Fixture>>();
                for (var i = 0; i < Fixtures.Count(); i++)
                    lstTasks.Add(SimulateFixture(Fixtures.ElementAt(i)));


                return Task.WhenAll(lstTasks) != null;
            });
        }

        public async Task<Fixture> SimulateFixture(Fixture fixture)
        {
            return await new TaskFactory().StartNew(() => {

                

                return fixture;
            });
        }
    }
}
