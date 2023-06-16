using System.Collections.Generic;
using CoreLib;
using Items;
using Tools.ToolControllers;
using UI.Wheel;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.UI
{
    public class ToolWheelConverter : IWheelable<ToolControllerBase>, IWheelConverter<ToolControllerBase>
    {
        private readonly Switchboard<ToolControllerBase> _switchboard;

        public ToolWheelConverter(Switchboard<ToolControllerBase> switchboard)
        {
            _switchboard = switchboard;
        }

        public IWheelConverter<ToolControllerBase> Converter => this;
        public IEnumerable<ToolControllerBase> GetItems() => _switchboard;

        public UIWheelSelectable GetSelectableFor(ToolControllerBase item)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ToolRecipeConverter : IWheelable<Recipe>, IWheelConverter<Recipe>
    {
        public IWheelConverter<Recipe> Converter { get; }
        public IEnumerable<Recipe> GetItems()
        {
            throw new System.NotImplementedException();
        }

        public UIWheelSelectable GetSelectableFor(Recipe item)
        {
            throw new System.NotImplementedException();
        }
    }
    public class ToolModeWheelConverter : IWheelable<string>
    {
        private List<string> _modes;
        private IWheelConverter<string> _converter;
        public IWheelConverter<string> Converter => _converter ??= new StringConverter();
        private bool _isValid;
        private readonly ToolControllerBase _controllerBase;

        public ToolModeWheelConverter(ToolControllerBase controllerBase)
        {
            _controllerBase = controllerBase;
            var res = _controllerBase.ToolUsesModes(out _modes);
            Debug.Assert(res, $"Tool Controller {_controllerBase} does not use tool modes", _controllerBase);
        }
        public IEnumerable<string> GetItems()
        {
            if (_modes ==null || _modes .Count < 1)
            {
                Debug.LogError("Tried to create tool mode wheel for tool that doesn't use tool modes",_controllerBase);
                yield break;
            }   
        }
    }
}