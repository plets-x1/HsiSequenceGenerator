using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Plets.Core.ControlAndConversionStructures;
using Plets.Modeling.FiniteStateMachine;
using Plets.Modeling.TestSuitStructure;
using Plets.Modeling.Uml;

namespace Plets.Modeling.FiniteStateMachine.Hsi {
    public class GenerateTestSuit {
        TestSuit suit = new TestSuit (DateTime.Now.ToString ());
        Scenario scenario;
        float actualTestCaseProb = 0;
        int equalsTcCount = 0;

        #region Public Methods
        public TestSuit PopulateTestSuit (String[][] matriz, Plets.Modeling.FiniteStateMachine.FiniteStateMachine machine, GeneralUseStructure modelGeneralUseStructure) {
            UmlModel model = (UmlModel) modelGeneralUseStructure;
            foreach (UmlUseCaseDiagram ucDiagram in model.Diagrams.OfType<UmlUseCaseDiagram> ()) {
                UmlUseCase equivalentUC = ucDiagram.UmlObjects.OfType<UmlUseCase> ().Where (x => x.Name.Equals (machine.Name)).FirstOrDefault ();

                foreach (UmlActor actor in ucDiagram.UmlObjects.OfType<UmlActor> ()) {
                    foreach (UmlAssociation association in ucDiagram.UmlObjects.OfType<UmlAssociation> ()) {
                        if ((association.End1.Equals (actor) && association.End2.Equals (equivalentUC)) || (association.End1.Equals (equivalentUC) && association.End2.Equals (actor))) {
                            try {
                                actualTestCaseProb = float.Parse (association.GetTaggedValue ("TDprob"), CultureInfo.InvariantCulture);
                            } catch {
                                actualTestCaseProb = 0;
                            }
                            if (suit.Scenarios.Count < 1) {
                                scenario = new Scenario ();
                                scenario.Name = actor.Name;
                                AddScenarioInformation (actor);
                                suit.Scenarios.Add (scenario);
                            }

                            foreach (Scenario scenarioAlreadyAdded in suit.Scenarios) {
                                if (actor.Name.Equals (scenarioAlreadyAdded.Name)) {
                                    scenario = scenarioAlreadyAdded;
                                } else {
                                    scenario = new Scenario ();
                                    scenario.Name = actor.Name;
                                    AddScenarioInformation (actor);
                                    suit.Scenarios.Add (scenario);
                                }
                            }
                        }
                    }
                }
            }

            for (int k = 0; k < matriz.Length; k++) {
                List<Transition> listTransition = new List<Transition> ();
                String[] arraySequence = matriz[k];

                foreach (String input in arraySequence) {
                    Transition tran = new Transition ();
                    tran = GetTransitionFSM (input, machine);

                    if (tran != null) {
                        listTransition.Add (tran);
                    }
                }
                scenario.TestCases.Add (FillTestCase (machine, listTransition));
            }
            return suit;
        }

        #endregion

        #region Private Methods
        private Transition GetTransitionFSM (String input, Plets.Modeling.FiniteStateMachine.FiniteStateMachine fsm) {
            List<Transition> transition = fsm.Transitions.Where (x => x.Input.Equals (input)).ToList ();

            foreach (Transition t in transition) {
                return t;
            }

            return null;
        }

        private TestCase FillTestCase (Plets.Modeling.FiniteStateMachine.FiniteStateMachine machine, List<Transition> listTransition) {
            TestCase testCase = new TestCase ();
            testCase.Name = machine.Name;
            testCase.Probability = actualTestCaseProb;
            Transaction transaction = null;
            bool existsLane = false;

            foreach (Transition t in listTransition) {
                State s = machine.States.Where (x => x.Name.Equals (t.SourceState.Name)).FirstOrDefault ();
                try {
                    existsLane = !String.IsNullOrEmpty (s.TaggedValues["Lane"]);
                } catch {
                    existsLane = false;
                }
                if (existsLane) {
                    List<Transaction> transactions = testCase.Transactions.Where (x => x.Name.Equals (s.TaggedValues["Lane"])).ToList ();
                    if (transactions.Count == 0) {
                        transaction = new Transaction ();
                        transaction.Name = s.TaggedValues["Lane"];
                        Subtransaction subtransaction = new Subtransaction ();
                        subtransaction.Name = t.SourceState.Name;
                        Request request = new Request ();
                        request.Name = t.SourceState.Name;
                        GetRequestTags (t, request);
                        subtransaction.Begin = request;
                        subtransaction.End = request;
                        transaction.Subtransactions.Add (subtransaction);
                        testCase.Transactions.Add (transaction);
                    } else {
                        transaction = transactions.FirstOrDefault ();
                        Subtransaction subtransaction = new Subtransaction ();
                        subtransaction.Name = t.SourceState.Name;
                        Request request = new Request ();
                        request.Name = t.SourceState.Name;
                        GetRequestTags (t, request);
                        subtransaction.Begin = request;
                        subtransaction.End = request;
                        transaction.Subtransactions.Add (subtransaction);
                    }
                } else {
                    transaction = new Transaction ();
                    transaction.Name = t.SourceState.Name;
                    Request request = new Request ();
                    GetRequestTags (t, request);
                    transaction.Begin = request;
                    transaction.End = request;
                    testCase.Transactions.Add (transaction);
                }
                AddRequestsToTestCase (testCase, t);
            }

            return testCase;
        }

        private void AddRequestsToTestCase (TestCase testCase, Transition t) {
            Request request = new Request ();
            GetRequestTags (t, request);
            testCase.Requests.Add (request);
        }

        private void GetRequestTags (Transition t, Request request) {
            try {
                request.Action = t.TaggedValues["TDACTION"];
            } catch {

            }
            try {
                request.Body = t.TaggedValues["TDBODY"];
            } catch {

            }
            try {
                //request.Cookies = t.TaggedValues["TDCOOKIES"];
            } catch {

            }
            try {
                request.ExpectedTime = Convert.ToDouble (t.TaggedValues["TDEXPECTEDTIME"]);
            } catch {

            }
            try {
                request.Method = t.TaggedValues["TDMETHOD"];
            } catch {

            }
            try {
                request.Name = t.SourceState.Name;
            } catch {

            }
            try {
                //request.OptimisticTime = Convert.ToDouble(t.TaggedValues["TDOPTIMISTICTIME"]);
            } catch {

            }
            try {
                //request.Parameters = t.TaggedValues["TDPARAMETERS"];
            } catch {

            }
            try {
                //request.PessimisticTime = Convert.ToDouble(t.TaggedValues["TDPESSIMISTICTIME"]);
            } catch {

            }
            try {
                request.Referer = t.TaggedValues["TDREFERER"];
            } catch {

            }
            try {
                //request.SaveParameters = t.TaggedValues["TDSAVEPARAMETERS"];
            } catch {

            }
            try {
                request.ThinkTime = Convert.ToDouble (t.TaggedValues["TDTHINKTIME"]);
            } catch {

            }
        }

        private void AddScenarioInformation (UmlActor actor) {
            try {
                //scenario.HostSUT.Name = actor.GetTaggedValue("TDHOST");
                scenario.ExecutionTime = Convert.ToInt32 (actor.GetTaggedValue ("TDTIME"));
                scenario.RampUpTime = Convert.ToInt32 (actor.GetTaggedValue ("TDRAMPUPTIME"));
                scenario.RampUpUser = Convert.ToInt32 (actor.GetTaggedValue ("TDRAMPUPUSER"));
                scenario.RampDownTime = Convert.ToInt32 (actor.GetTaggedValue ("TDRAMPDOWNTIME"));
                scenario.RampDownUser = Convert.ToInt32 (actor.GetTaggedValue ("TDRAMPDOWNUSER"));
                scenario.Population = Convert.ToInt32 (actor.GetTaggedValue ("TDPOPULATION"));
            } catch {
                //do something
            }
        }
        #endregion
    }
}