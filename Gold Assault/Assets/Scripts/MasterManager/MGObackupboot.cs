using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class MGObackupboot : MonoBehaviour
{
    /*<summery>
	This script will create the master manager if there isnt one.
	*/

    void Awake()
    {
        if (MasterManger.current == null)
        {
            GameObject go = Instantiate(Resources.Load("MasterGameObject", typeof(GameObject)) as GameObject, Vector3.zero, Quaternion.identity);//, transform.Find("Slot" + indexNumber));
            go.name = "MasterGameObject BU";
            Debug.LogWarning("WARNING!\nMaster Manager was not detected, automatically instaciated the object");
        }
    }

    public static MasterManger CheckAndCreateMasterManager()
    {
        if (MasterManger.current == null)
        {
            Debug.LogWarning("WARNING!\nMaster Manager was not detected, automatically instaciated the object");
            Instantiate(Resources.Load("MasterGameObject", typeof(GameObject)) as GameObject, Vector3.zero, Quaternion.identity);//, transform.Find("Slot" + indexNumber));
            return MasterManger.current;
        }
        else
        {
            return MasterManger.current;
        }
    }
}
