using System.Collections.Generic;
using EC.Core;
using NUnit.Framework;

namespace EC.Tests.EditMode
{
    public class StateMachineTests
    {
        class Recorder : IState
        {
            readonly string name;
            readonly List<string> log;

            public Recorder(string name, List<string> log)
            {
                this.name = name;
                this.log = log;
            }

            public void Enter() { log.Add(name + ".Enter"); }
            public void Tick(float deltaTime) { log.Add(name + ".Tick"); }
            public void Exit() { log.Add(name + ".Exit"); }
        }

        static StateMachine Build(List<string> log)
        {
            var machine = new StateMachine();
            foreach (EntityState state in System.Enum.GetValues(typeof(EntityState)))
                machine.Register(state, new Recorder(state.ToString(), log));
            return machine;
        }

        [Test]
        public void Transition_ToRegisteredState_ChangesCurrent()
        {
            var machine = Build(new List<string>());
            Assert.IsTrue(machine.Transition(EntityState.Move));
            Assert.AreEqual(EntityState.Move, machine.Current);
        }

        [Test]
        public void Transition_CallsExitBeforeEnter()
        {
            var log = new List<string>();
            var machine = Build(log);
            machine.Transition(EntityState.Idle);
            log.Clear();
            machine.Transition(EntityState.Attack);
            CollectionAssert.AreEqual(new[] { "Idle.Exit", "Attack.Enter" }, log);
        }

        [Test]
        public void Transition_ToUnregisteredState_IsRejected()
        {
            var machine = new StateMachine();
            Assert.IsFalse(machine.Transition(EntityState.Idle));
        }

        [Test]
        public void Dead_IsTerminal()
        {
            var log = new List<string>();
            var machine = Build(log);
            machine.Transition(EntityState.Dead);
            log.Clear();
            Assert.IsFalse(machine.Transition(EntityState.Idle));
            Assert.AreEqual(EntityState.Dead, machine.Current);
            Assert.IsEmpty(log);
        }

        [Test]
        public void Tick_RunsCurrentState()
        {
            var log = new List<string>();
            var machine = Build(log);
            machine.Transition(EntityState.Move);
            log.Clear();
            machine.Tick(0.1f);
            CollectionAssert.AreEqual(new[] { "Move.Tick" }, log);
        }
    }
}
