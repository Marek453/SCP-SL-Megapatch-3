using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MEC;

public class Sco008PlayerScript : NetworkBehaviour
{
    public bool iAm008;
    [SyncVar(hook = "SetInfect")]
    public bool Infect;
    public int distance;
    public AudioClip[] Sounds;
    public AudioSource Source;
    public AudioSource AmbientSource;
    public AudioSource Heart;
    public GameObject plyCam;
    public Animator animator;
    public bool sameClass;
    public Color color = new Color(255, 255, 255, 0);

    bool Attacked;

    void Update()
    {
        if (!isLocalPlayer) return;
        Atack();
    }

    void Atack()
    {
        if (!Attacked && iAm008 && Input.GetKeyUp(NewInput.GetKey("Shoot")))
        {

            CmdShootAnim();
            animator.SetTrigger("Shoot");
            Attacked = true;
            animator.speed = 1;
            Timing.CallDelayed(0.8f, () => Attack());
        }
    }

    [Command]
    private void CmdShootAnim()
    {
        RpcShootAnim();
    }

    public void ResetAll()
    {
        StopAllCoroutines();
        color.a = 0;
        FindObjectOfType<ScpInterfaces>().Scp008_Hud.color = color;
        Heart.Stop();
        AmbientSource.Stop();
    }

    [ClientRpc]
    private void RpcShootAnim()
    {
        GetComponent<AnimationController>().DoAnimation("Shoot");
    }

    public void SetInfect(bool _Infect)
    {
        Infect = _Infect;
    }


    public void StartInfectVoid()
    {
        StartCoroutine(StartInfect());
    }

    IEnumerator StartInfect()
    {
        if (Infect) yield break;
        yield return new WaitForSeconds(30);
        StartCoroutine(Efect(0.1f));
        yield return new WaitForSeconds(60);
        StartCoroutine(SoundInfect());
        StartCoroutine(Efect(0.2f));
        yield return new WaitForSeconds(60);
        Heart.Play();
        StartCoroutine(Efect(0.3f));
        yield return new WaitForSeconds(30);
        AmbientSource.Play();
        CmdSlowSpeed();
        yield return new WaitForSeconds(30);
        GetComponent<PlayerStats>().CmdSTartDie();
    }

    [Command]
    void CmdSlowSpeed()
    {
        if (isLocalPlayer)
        {
            GetComponent<FirstPersonController>().m_WalkSpeed = GetComponent<FirstPersonController>().m_WalkSpeed / 2;
            GetComponent<FirstPersonController>().m_RunSpeed = GetComponent<FirstPersonController>().m_RunSpeed / 2;
        }
    }

    IEnumerator SoundInfect()
    {
        Source.PlayOneShot(Sounds[Random.Range(0, Sounds.Length)]);
        yield return new WaitForSeconds(Random.Range(10, 30));
        StartCoroutine(SoundInfect());
    }
    IEnumerator Efect(float i)
    {
        while (color.a < i)
        {
            color.a += 0.01f * Time.deltaTime;
            FindObjectOfType<ScpInterfaces>().Scp008_Hud.color = color;
            yield return null;
        }
        yield break;
    }

    private void Attack()
    {
        RaycastHit hitInfo;
        if (iAm008 && Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, distance))
        {
            Sco008PlayerScript component = hitInfo.transform.GetComponent<Sco008PlayerScript>();
            if (component != null && !component.sameClass)
            {
                if (component.Infect)
                {

                    CmdDeduct(component.gameObject);
                }
                else
                {
                    CmdSetInfectVoid(component.gameObject);
                    CmdDeduct(component.gameObject);
                }
            }
        }
        Attacked = false;
    }

    [Command]
    void CmdDeduct(GameObject ply)
    {
        RpcDeduct(ply);
    }
    [ClientRpc]
    void RpcDeduct(GameObject ply)
    {
        ply.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(30, "", "SCP:0492", 0), ply);
    }

    [Command]
    void CmdSetInfectVoid(GameObject ply)
    {
        ply.GetComponent<Sco008PlayerScript>().SetInfect(true);
        RpcSetInfectVoid(ply);
    }

    [ClientRpc]
    void RpcSetInfectVoid(GameObject ply)
    {
        ply.GetComponent<Sco008PlayerScript>().StartInfectVoid();

    }

    public void Init(int classID, Class c)
    {
        sameClass = c.team == Team.SCP || c.team == Team.SH;
        if (classID == 20)
        {
            iAm008 = true;
        }
        else
        {
            iAm008 = false;
        }
        animator.gameObject.SetActive(base.isLocalPlayer && iAm008);
    }
}
