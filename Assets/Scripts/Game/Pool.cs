using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    private Queue<Component> _elements;
    private Component _prefab;
    private bool _dynamicSize;
    private Transform _defaultParent;


    public Pool(bool dynamicSize, Component prefab, int initialSize, Transform defaultParent)
    {
        _elements = new Queue<Component>();
        _prefab = prefab;
        _dynamicSize = dynamicSize;
        _defaultParent = defaultParent;

        for (int j = 0; j < initialSize; j++)
        {
            Component obj = Object.Instantiate(_prefab) as Component;
            obj.transform.name = _prefab.transform.name;

            obj.transform.SetParent(_defaultParent, false);
            obj.gameObject.SetActive(false);
            _elements.Enqueue(obj);
        }
    }

    public void Clear()
    {
        while (_elements.Count > 0)
        {
            Component element = _elements.Dequeue();
            Object.Destroy(element);
        }
    }

    public T GetElement<T>() where T : Component
    {
        return GetElement<T>(_defaultParent);
    }

    public T GetElement<T>(Transform newParent, bool worldPositionStays = true) where T : Component
    {
        T element = null;

        if (newParent == null)
        {
            newParent = _defaultParent;
        }

        if (_elements.Count > 0)
        {
            element = _elements.Dequeue() as T;
        }
        else
        {
            if (_dynamicSize)
            {
                element = Object.Instantiate(_prefab) as T;
                element.name = _prefab.name;
                element.transform.SetParent(_defaultParent, worldPositionStays);
            }
        }
        if (element != null)
        {
            element.transform.SetParent(newParent, worldPositionStays);
            element.gameObject.SetActive(true);
        }
        return element;
    }

    public void ReturnElement(Component component)
    {
        component.transform.SetParent(_defaultParent, true);
        _elements.Enqueue(component);
        component.gameObject.SetActive(false);
    }
}