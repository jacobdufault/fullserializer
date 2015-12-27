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
            ItemStorageType = simpleCycle.GetType(),
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
            ItemStorageType = typeof(ValueHolder<ICycle>),
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
            ItemStorageType = typeof(ValueHolder<ICycle>),
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

        //--
        //
        //
        //

        {
            Cyclic a0 = new Cyclic(), a1 = new Cyclic(), a2 = new Cyclic();
            yield return new TestItem {
                Item = new List<object> { a0, a1, a2, a1, a2, a0, a0, a0, a0 },
                ItemStorageType = typeof(List<object>),
                Comparer = (a, b) => {
                    var listB = (List<object>)b;

                    var b0 = listB[0];
                    var b1 = listB[1];
                    var b2 = listB[2];

                    return
                        listB[3] == b1 &&
                        listB[4] == b2 &&
                        listB[5] == b0 &&
                        listB[6] == b0 &&
                        listB[7] == b0 &&
                        listB[8] == b0;
                }
            };
        }
    }

    public class Cyclic {
    }
}