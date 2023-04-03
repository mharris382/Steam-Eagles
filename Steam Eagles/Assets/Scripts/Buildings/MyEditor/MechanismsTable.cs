using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Mechanisms;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Buildings.MyEditor
{
    // public class MechanismsTable
    // {
    //     private readonly Building _building;
    //     private readonly BuildingMechanisms _mechanisms;
    //
    //
    //     [ValidateInput(nameof(ValidateMechanisms))]
    //     [ShowInInspector, TableList(IsReadOnly = true, AlwaysExpanded = true)]
    //     private List<MechanismWrapper> _mechanismWrappers;
    //
    //     private List<Type> _mechanismTypes = new List<Type>();
    //     bool ValidateMechanisms(List<MechanismWrapper> mechanisms, ref string errMsg)
    //     {
    //         HashSet<string> names = new HashSet<string>();
    //         foreach (var mechanism in mechanisms)
    //         {
    //             if (names.Contains(mechanism._mechanism.name))
    //             {
    //                 errMsg = $"Duplicate mechanism name: {mechanism._mechanism.name}";
    //                 return false;
    //             }
    //
    //             names.Add(mechanism._mechanism.name);
    //         }
    //
    //         return true;
    //     }
    //     bool HasDuplicateNamingIssue()
    //     {
    //         HashSet<string> names = new HashSet<string>();
    //         foreach (var mechanism in _mechanismWrappers)
    //         {
    //             if (names.Contains(mechanism._mechanism.name))
    //             {
    //                 return true;
    //             }
    //
    //             names.Add(mechanism._mechanism.name);
    //         }
    //
    //         return false;
    //     }
    //
    //     [GUIColor(nameof(GetErrorColor))]
    //     [Button(ButtonSizes.Large), ShowIf(nameof(HasDuplicateNamingIssue))]
    //     void FixDuplicateNamingIssue()
    //     {
    //         Dictionary<Type, int> typeCounts = new Dictionary<Type, int>();
    //         foreach (var mechanism in _mechanismWrappers)
    //         {
    //             var mechanismType = mechanism._mechanism.GetType();
    //             if (!typeCounts.ContainsKey(mechanismType))
    //             {
    //                 typeCounts.Add(mechanismType, 0);
    //             }
    //             else
    //             {
    //                 typeCounts[mechanismType]++;
    //             }
    //
    //             mechanism._mechanism.name = $"{mechanismType.Name} {typeCounts[mechanismType]}";
    //         }
    //     }
    //     Color GetErrorColor() => Color.Lerp(Color.red, Color.white, 0.4f);
    //
    //     public MechanismsTable(Building building)
    //     {
    //         _building = building;
    //         if (!_building.gameObject.TryGetComponent(out _mechanisms))
    //         {
    //             _mechanisms = _building.gameObject.AddComponent<BuildingMechanisms>();
    //         }
    //         _mechanismWrappers = new List<MechanismWrapper>(_mechanisms.GetComponentsInChildren<BuildingMechanism>().Select(t => new MechanismWrapper(this, t)));
    //         _mechanismTypes = typeof(BuildingMechanism).GetNestedTypes().Where(t => t.IsSubclassOf(typeof(BuildingMechanism)) && !t.IsAbstract).ToList();
    //     }
    //
    //     public class MechanismWrapper : IComparable<MechanismWrapper>
    //     {
    //         internal readonly BuildingMechanism _mechanism;
    //         private readonly MechanismsTable _table;
    //
    //         public string MechanismName
    //         {
    //             get => _mechanism.name;
    //             set => _mechanism.name = value;
    //         }
    //
    //         public MechanismWrapper(MechanismsTable table, BuildingMechanism mechanism)
    //         {
    //             this._table = table;
    //             this._mechanism = mechanism;
    //         }
    //
    //         public int CompareTo(MechanismWrapper other)
    //         {
    //             return String.Compare(_mechanism.name, other._mechanism.name, StringComparison.Ordinal);
    //         }
    //     }
    // }
}