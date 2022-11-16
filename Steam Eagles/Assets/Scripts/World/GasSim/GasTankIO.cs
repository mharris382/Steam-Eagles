using System.Collections.Generic;
using System.Linq;
using GasSim;
using UnityEngine;

[RequireComponent(typeof(GasTank))]
public class GasTankIO : MonoBehaviour
{
    public List<GameObject> suppliersObjects;
    public List<GameObject> consumerObjects;
    
    private GasTank _tank;
    
    IGasSupplier[] _suppliers;
    IGasConsumer[] _consumers;
    
    void Start()
    {
        _tank = GetComponentInParent<GasTank>();
        Debug.Assert(_tank != null);
        suppliersObjects.RemoveAll(t => t==null);
        _suppliers = suppliersObjects.Select(t => t.GetComponent<IGasSupplier>()).Where(t => t != null).ToArray();
        _consumers = consumerObjects.Select(t => t.GetComponent<IGasConsumer>()).Where(t => t != null).ToArray();
    }
    
}