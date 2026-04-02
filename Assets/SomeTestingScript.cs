using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeTestingScript : MonoBehaviour
{
    [ContextMenu("Demo of Context menu")]

    public void TestMethod()
    {
        Debug.Log("Test");
    }
}
