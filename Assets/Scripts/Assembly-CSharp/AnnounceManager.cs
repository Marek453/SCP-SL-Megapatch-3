using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

public class AnnounceManager : MonoBehaviour
{
    public GameObject scp173Prefab, scp096Prefab, scp106Prefab, scp939Prefab, scp049Prefab;

    public static AnnounceManager instance;
    void Awake()
    {
        instance = this;
    }

    public GameObject StartAnnounce(string ID)
    {
        switch (ID)
        {
            case "SCP-173":
                GameObject obj = Instantiate(scp173Prefab);
                return obj;
            case "SCP-096":
                GameObject obj1 = Instantiate(scp096Prefab);
                return obj1;
            case "SCP-106":
                GameObject obj2 = Instantiate(scp106Prefab);
                return obj2;
            case "SCP-939-53":
                GameObject obj3 = Instantiate(scp939Prefab);
                return obj3;
            case "SCP-939-89":
                GameObject obj4 = Instantiate(scp939Prefab);
                return obj4;
            case "SCP-049":
                GameObject obj5 = Instantiate(scp049Prefab);
                return obj5;
            default:
                return null;
        }
    }
}
