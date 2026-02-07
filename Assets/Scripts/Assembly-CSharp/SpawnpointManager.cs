using UnityEngine;

public class SpawnpointManager : MonoBehaviour
{
	public GameObject GetRandomPosition(int classID)
	{
		GameObject result = null;
		Class @class = GameObject.Find("Host").GetComponent<CharacterClassManager>().klasy[classID];
		if (@class.team == Team.CDP || @class.team == Team.TUT)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("SP_CDP");
			int num = Random.Range(0, array.Length);
			result = array[num];
		}
		if (classID == 10)
		{
			return null;
		}
		if (@class.team == Team.SCP)
		{
			switch (classID)
			{
			case 3:
			{
				GameObject[] array5 = GameObject.FindGameObjectsWithTag("SP_106");
				int num5 = Random.Range(0, array5.Length);
				result = array5[num5];
				break;
			}
			case 5:
			{
				GameObject[] array4 = GameObject.FindGameObjectsWithTag("SP_049");
				int num4 = Random.Range(0, array4.Length);
				result = array4[num4];
				break;
			}
			case 7:
			{
				GameObject[] array7 = GameObject.FindGameObjectsWithTag("SP_079");
				int num7 = Random.Range(0, array7.Length);
				result = array7[num7];
				break;
			}
			case 9:
			{
				GameObject[] array6 = GameObject.FindGameObjectsWithTag("SCP_096");
				int num6 = Random.Range(0, array6.Length);
				result = array6[num6];
				break;
			}
			case 19:
			{
				GameObject[] array68 = GameObject.FindGameObjectsWithTag("SP_457");
				int num68 = Random.Range(0, array68.Length);
				result = array68[num68];
				break;
			}
			case 20:
			{
				GameObject[] array68D = GameObject.FindGameObjectsWithTag("SP_008");
				int num68D = Random.Range(0, array68D.Length);
				result = array68D[num68D];
				break;
			}
			default:
				if (@class.fullName.Contains("SCP-939"))
				{
					GameObject[] array2 = GameObject.FindGameObjectsWithTag("SCP_939");
					int num2 = Random.Range(0, array2.Length);
					result = array2[num2];
				}
				else
				{
					GameObject[] array3 = GameObject.FindGameObjectsWithTag("SP_173");
					int num3 = Random.Range(0, array3.Length);
					result = array3[num3];
				}
				break;
			}
		}
		if (@class.team == Team.MTF)
		{
			GameObject[] array8 = ((classID != 15) ? GameObject.FindGameObjectsWithTag("SP_MTF") : GameObject.FindGameObjectsWithTag("SP_GUARD"));
			int num8 = Random.Range(0, array8.Length);
			result = array8[num8];
		}
        if (@class.team == Team.SH)
        {
            GameObject[] array8 = GameObject.FindGameObjectsWithTag("SP_SH");
            int num8 = Random.Range(0, array8.Length);
            result = array8[num8];
        }
        if (@class.team == Team.RSC)
		{
			GameObject[] array9 = GameObject.FindGameObjectsWithTag("SP_RSC");
			int num9 = Random.Range(0, array9.Length);
			result = array9[num9];
		}
		if (@class.team == Team.CHI)
		{
			GameObject[] array10 = GameObject.FindGameObjectsWithTag("SP_CI");
			int num10 = Random.Range(0, array10.Length);
			result = array10[num10];
		}
		return result;
	}
}
