using System.Collections.Generic;

public class CyclesProvider : ITestProvider {

    public interface ICycle {
        int A { get; set; }
        ICycle Cycle { get; set; }
        int B { get; set; }
    }
    public class CycleDerivedA : ICycle {
        public int A { get; set; }
        public ICycle Cycle { get; set; }
        public int B { get; set; }
    }
    public class CycleDerivedB : ICycle {
        public int A { get; set; }
        public ICycle Cycle { get; set; }
        public int B { get; set; }
    }

    public IEnumerable<TestItem> GetValues() {
        var simpleCycle = new CycleDerivedA() {
            A = 1,
            B = 2
        };
        simpleCycle.Cycle = simpleCycle;

        yield return new TestItem {
            Item = simpleCycle,
            Comparer = (a, b) => {
                var deserialized = (CycleDerivedA)b;
                return
                    deserialized.A == 1 &&
                    deserialized.B == 2 &&
                    ReferenceEquals(deserialized.Cycle, deserialized);
            }
        };

        //--
        //
        //
        //

        ICycle simpleInheritCycle = new CycleDerivedA {
            A = 1,
            B = 2
        };
        simpleInheritCycle.Cycle = simpleInheritCycle;
        yield return new TestItem {
            Item = new ValueHolder<ICycle>(simpleInheritCycle),
            Comparer = (a, b) => {
                var deserialized = (ValueHolder<ICycle>)b;
                return
                    deserialized.Value.GetType() == typeof(CycleDerivedA) &&
                    deserialized.Value.A == 1 &&
                    deserialized.Value.B == 2 &&
                    ReferenceEquals(deserialized.Value.Cycle, deserialized.Value);
            }
        };

        //--
        //
        //
        //

        ICycle complexInheritCycle = new CycleDerivedA {
            A = 1,
            B = 2
        };
        complexInheritCycle.Cycle = new CycleDerivedB {
            A = 3,
            B = 4
        };
        complexInheritCycle.Cycle.Cycle = complexInheritCycle;
        yield return new TestItem {
            Item = new ValueHolder<ICycle>(complexInheritCycle),
            Comparer = (a, b) => {
                var deserialized = (ValueHolder<ICycle>)b;

                return
                    deserialized.Value.GetType() == typeof(CycleDerivedA) &&
                    deserialized.Value.Cycle.GetType() == typeof(CycleDerivedB) &&

                    deserialized.Value.A == 1 &&
                    deserialized.Value.B == 2 &&
                    deserialized.Value.Cycle.A == 3 &&
                    deserialized.Value.Cycle.B == 4 &&
                    ReferenceEquals(deserialized.Value.Cycle.Cycle, deserialized.Value);
            }
        };
    }
}