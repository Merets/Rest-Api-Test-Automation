using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using static RestApiTestAutomation.TestBase;

namespace RestApiTestAutomation
{
    [TestClass]
    public abstract class TestBase
    {
        //[Obsolete("Override the Initialize method instead of using [TestIniatialize]", true)]
        //public class TestInitializeAttribute : Attribute
        //{
        //}

        //[Obsolete("Use AddCleanupAction method instead of using [TestCleanup]", true)]
        //public class TestCleanupAttribute : Attribute
        //{
        //}

        private readonly List<Action> _cleanupActions = new List<Action>();

        [TestInitialize]
        public void TestInitialize()
        {
            Console.WriteLine("=============================== Test initialize ===============================");
            _cleanupActions.Clear();

            try
            {
                Initialize();
            }
            catch (Exception)
            {
                CallCleanupActions();
                throw;
            }

            Console.WriteLine("=============================== Test method ===============================");
            Console.WriteLine();
        }

        
        protected void Initialize() { }
        //protected virtual Initialize() { }

        [TestCleanup]
        public void TestCleanup()
        {
            Console.WriteLine("=============================== Test Cleanup ===============================");
            Console.WriteLine();

            CallCleanupActions();
        }

        private void CallCleanupActions()
        {
            _cleanupActions.Reverse();
            var exceptions = new List<Exception>();

            foreach (var action in _cleanupActions)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Console.WriteLine("Cleanup action failed: " + ex);
                }
            }

            if (exceptions.Count == 0)
                return;

            if (exceptions.Count == 1)
                throw exceptions.Single();

            throw new AggregateException("Multiple exceptions occured in Cleanup. See test log for more details", exceptions);
        }

        public void AddCleanupAction(Action cleanupAction)
        {
            _cleanupActions.Add(cleanupAction);
        }

        //private List<Action> _cleanupActions = new List<Action>();

        //public void AddCleanupAction(Action cleanupAction)
        //{
        //    _cleanupActions.Add(cleanupAction);
        //}

        //[TestCleanup]
        //public void Cleanup()
        //{
        //    _cleanupActions.Reverse();
        //    foreach (var action in _cleanupActions)
        //    {
        //        action();
        //    }
        //}
    }
}