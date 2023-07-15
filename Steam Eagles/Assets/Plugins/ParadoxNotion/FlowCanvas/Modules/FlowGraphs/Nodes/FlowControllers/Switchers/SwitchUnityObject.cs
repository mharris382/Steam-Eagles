using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes
{
    [Category("Flow Controllers/Switchers")]
    [Description("Branch the Flow based on a Unity Object value. The Default output is called if there is no other matching output same as the input value")]
    [ContextDefinedInputs(typeof(UnityEngine.Object))]
    [HasRefreshButton, ExposeAsDefinition]
    public class SwitchUnityObject<T> : FlowControlNode where T : UnityEngine.Object
    {
        [Name("Cases")]
        public List<T> objectCases = new List<T>();

        protected override void RegisterPorts() {
            var selector = AddValueInput<T>("Value");
            var cases = new FlowOutput[objectCases.Count];
            for ( var i = 0; i < cases.Length; i++ ) {
                cases[i] = AddFlowOutput(objectCases[i] != null ? objectCases[i].name : "null", i.ToString());
            }
            var defaultCase = AddFlowOutput("Default");
            AddValueOutput<T>("Value", () => selector.value);

            AddFlowInput("In", (f) =>
            {
                var selectorValue = selector.value;
                var caseCalled = false;
                if ( selectorValue != null ) {
                    for ( var i = 0; i < objectCases.Count; i++ ) {
                        if ( UnityEngine.Object.Equals(selectorValue, objectCases[i]) ) {
                            caseCalled = true;
                            cases[i].Call(f);
                        }
                    }
                }
                if ( !caseCalled ) {
                    defaultCase.Call(f);
                }
            });
        }
    }

}