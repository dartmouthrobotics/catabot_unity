using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTag : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TagGameObjects();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TagGameObjects()
    {
        GameObject[] objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in objs)
        {
            TagChildren(obj);

        }
    }

    void TagChildren(GameObject obj)
    {
        if (obj.transform.childCount != 0)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if (obj.tag != "Untagged" && child.tag != "MainCamera")
                {
                    child.tag = obj.tag;
                }
                TagChildren(child);
            }
        }
    }
}
