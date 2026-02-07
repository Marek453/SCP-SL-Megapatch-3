using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuSoundtrackController : MonoBehaviour
{
    public enum MenuSoundtrackState
    {
        None = 0,
        PlayTopolis = 1,
        FadeOutTopolis = 2,
        PlayIntense = 3,
        PlayRetro = 4
    }
    private UnityEngine.Events.UnityAction<Scene, LoadSceneMode> unityAction;
    public static bool DontPlayIntensive;
    public AudioSource ToposThemeSource;
    public AudioClip IntroClip;
    public AudioClip ToposClip;
    public System.Single ToposMetric;
    public System.Single ToposLength = 5.0999999f;
    public AudioSource IntenseThemeSource;
    public System.Single IntenseDropTime;
    public int IntenseSequencesToDrop;
    public System.Single IntenseOldToposFadeoffTime;
    public AudioSource RetroThemeSource;
    public MenuSoundtrackState SoundtrackState;
    public UnityEngine.AnimationCurve FadeoffAnimationCurve;
    public System.Single fadeoffAnim;
    public CustomNetworkManager cnm;
    public bool DebugMode;
    public bool DebugTrigger;
    public MenuSoundtrackState DebugOverride;


    private void Awake()
    {
        unityAction = new UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);
        SceneManager.sceneLoaded += unityAction;

        cnm = GetComponentInParent<CustomNetworkManager>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
        {
            // if (!MenuAnimator.retro)
            // {
            DontPlayIntensive = false;
                 SoundtrackState = MenuSoundtrackState.None;
                ToposThemeSource.Stop();
                ToposThemeSource.volume = 1;
                IntenseThemeSource.volume = 0;
                ToposThemeSource.loop = false;
                ToposThemeSource.clip = IntroClip;
                ToposThemeSource.PlayOneShot(IntroClip);
           // }
            //else
           // {
           //     SoundtrackState = MenuSoundtrackState.PlayRetro;
           //     ToposThemeSource.Stop();
           //     RetroThemeSource.Play();
           // }
        }
        else
        {
            DontPlayIntensive = true;
            SoundtrackState = MenuSoundtrackState.PlayIntense;
        }
        // switch (SoundtrackState)
        // {
        //     case MenuSoundtrackState.PlayTopolis:
        //         ToposThemeSource.Stop();
        //         IntenseThemeSource.volume = 0;
        //         ToposThemeSource.volume = 1;
        //         ToposThemeSource.loop = false;
        //         ToposThemeSource.clip = IntroClip;
        //         ToposThemeSource.PlayOneShot(IntroClip);
        //         SoundtrackState = MenuSoundtrackState.None;
        //         break;
        //     case MenuSoundtrackState.PlayRetro:
        //         ToposThemeSource.Stop();
        //         IntenseThemeSource.Stop();
        //         RetroThemeSource.Play();
        //         break;
        //     default:
        //         DontPlayIntensive = true;
        //         SoundtrackState = MenuSoundtrackState.PlayIntense;
        //         break;
        // }
    }

    private void LateUpdate()
    {
        float time = 0;
        if (DebugMode && DebugTrigger)
        {
            SoundtrackState = DebugOverride;
            DebugTrigger = false;
        }
        if (ToposThemeSource.clip == ToposClip)
        {
            if (ToposLength > ToposThemeSource.time)
            {
                ToposMetric = ToposThemeSource.time;
            }
            else
            {
                ToposMetric += ToposThemeSource.pitch * Time.deltaTime;
            }
        }
        if (ToposMetric > ToposLength)
        {
            ToposMetric -= ToposLength;
        }
        if (SoundtrackState == MenuSoundtrackState.PlayTopolis && !DontPlayIntensive && !DebugMode && cnm.ShouldPlayIntensive())
        {
            SoundtrackState = MenuSoundtrackState.FadeOutTopolis;
        }
        if (SoundtrackState == MenuSoundtrackState.None)
        {
            if (!ToposThemeSource.isPlaying)
            {
                ToposThemeSource.Stop();
                ToposThemeSource.clip = ToposClip;
                ToposThemeSource.loop = true;
                ToposThemeSource.Play();
                ToposThemeSource.time = 0;
                ToposMetric = 0;
                SoundtrackState = MenuSoundtrackState.PlayTopolis;
                goto IL_80;
            }
        }
        if (SoundtrackState != MenuSoundtrackState.FadeOutTopolis)
        {
            if (SoundtrackState == MenuSoundtrackState.PlayRetro && !RetroThemeSource.isPlaying)
            {
                ToposThemeSource.Stop();
                IntenseThemeSource.Stop();
                RetroThemeSource.Play();
            }

            IntenseThemeSource.volume -= Time.deltaTime / 6;
            
            float volume = 0;
            if (SoundtrackState == MenuSoundtrackState.PlayIntense)
            {
                fadeoffAnim += Time.deltaTime;
                if (IntenseThemeSource.volume <= 0 && fadeoffAnim <= 1)
                {
                    goto IL_53;
                }
                volume = FadeoffAnimationCurve.Evaluate(fadeoffAnim);
            }
            else
            {
                volume = Time.deltaTime * 0.5f + ToposThemeSource.volume;
            }
            ToposThemeSource.volume = volume;
        IL_53:
            if (IntenseThemeSource.volume > 0)
            {
                goto IL_80;
            }
            
            time = IntenseDropTime
                         - (((ToposMetric > (ToposLength * 0.5f) ? 1f : 0f) + 1) * ToposLength) 
                         + ToposMetric
                         - (IntenseSequencesToDrop * ToposLength);
            
            IntenseThemeSource.time = time;
            goto IL_80;
        }
        
        if(!DebugMode)
        {
            if(!cnm.ShouldPlayIntensive())
            {
                SoundtrackState = MenuSoundtrackState.PlayTopolis;
            }
        }
        
        IntenseThemeSource.volume += Time.deltaTime * 0.5f;
        
        if(IntenseThemeSource.time > IntenseOldToposFadeoffTime)
        {
            ToposThemeSource.volume -= Time.deltaTime * 0.5f;
        }
        
        /*if (ToposThemeSource.volume <= 0)
        {
            time = IntenseThemeSource.time - IntenseDropTime;
            while (time > ToposClip.length)
            {
                time -= ToposClip.length;
            }
            for (; time < 0.0; time += ToposClip.length)
            {
            }
            IntenseThemeSource.time = time;
        }*/
        IL_80:
        if (SoundtrackState != MenuSoundtrackState.PlayIntense)
        {
            fadeoffAnim = 0;
            if (SoundtrackState != MenuSoundtrackState.PlayRetro)
            {
                RetroThemeSource.Stop();
            }
        }
    }
}