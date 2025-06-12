using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunControlleScript : MonoBehaviour
{
    private List<GameObject> childGunList;

    // Start is called before the first frame update
    void Start()
    {
        childGunList = Utils.GetChildren(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject gameObject in childGunList)
        {
            Debug.Log(gameObject.name);
        }
    }
}
public static class Utils
{
    //note to self, figure out how to only extract the first layer of children
    public static List<GameObject> GetChildren(GameObject go)
    {
        List<GameObject> list = new List<GameObject>();
        return GetChildrenHelper(go, list);
    }

    private static List<GameObject> GetChildrenHelper(GameObject go, List<GameObject> list)
    {
        if (go == null || go.transform.childCount == 0)
        {
            return list;
        }
        foreach (Transform t in go.transform)
        {
            list.Add(t.gameObject);
            GetChildrenHelper(t.gameObject, list);
        }
        return list;
    }
}