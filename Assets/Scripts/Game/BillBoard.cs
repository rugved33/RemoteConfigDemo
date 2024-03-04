using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
  [SerializeField] private Transform _cam;


  private void Start()
  {
    _cam = FindObjectOfType<Camera>().transform;
  }

  private void LateUpdate()
  {
    if (_cam != null)
      transform.LookAt(transform.position + _cam.forward);
  }
}