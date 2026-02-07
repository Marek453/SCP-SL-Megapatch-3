using System;
using System.Linq;
using System.Text;
using GameConsole;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DiscordController : MonoBehaviour
{
	public DiscordRpc.RichPresence presence;

	public string applicationId;

	public string optionalSteamId;

	public int callbackCalls;

	public DiscordRpc.JoinRequest joinRequest;

	public UnityEvent onConnect;

	public UnityEvent onDisconnect;

	public UnityEvent hasResponded;

	public DiscordJoinEvent onJoin;

	public DiscordJoinEvent onSpectate;

	public DiscordJoinRequestEvent onJoinRequest;

	private GameConsole.Console console;

	public TextMeshProUGUI joinText;

	public Animator joinAnimator;

	private DiscordRpc.EventHandlers handlers;

	public void RequestRespondYes()
	{
		joinAnimator.SetBool("Requested", false);
		console.AddLog("Discord: Accepted join request.", new Color32(114, 137, 218, byte.MaxValue));
		DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
		hasResponded.Invoke();
	}

	public void RequestRespondNo()
	{
		joinAnimator.SetBool("Requested", false);
		console.AddLog("Discord: Join request rejected.", new Color32(114, 137, 218, byte.MaxValue));
		DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
		hasResponded.Invoke();
	}

	public void ReadyCallback()
	{
		callbackCalls++;
		console.AddLog("Discord: ready!", new Color32(114, 137, 218, byte.MaxValue));
		onConnect.Invoke();
	}

	public void DisconnectedCallback(int errorCode, string message)
	{
		callbackCalls++;
		Debug.Log(string.Format("Discord: disconnected - {0} ({1})", errorCode, message));
		onDisconnect.Invoke();
	}

	public void ErrorCallback(int errorCode, string message)
	{
		callbackCalls++;
		console.AddLog(string.Format("Discord: error - {0} ({1})", errorCode, message), new Color32(114, 137, 218, byte.MaxValue));
	}

	public void JoinCallback(string secret)
	{
		callbackCalls++;
		string @string = Encoding.UTF8.GetString(Convert.FromBase64String(secret));
		try
		{
			CustomNetworkManager component = GetComponent<CustomNetworkManager>();
			string[] ipAndPort = @string.Split(':');
			int result = 0;
			if (!int.TryParse(ipAndPort[1], out result))
			{
				throw new Exception();
			}
			component.networkAddress = ipAndPort[0];
			CustomNetworkManager.ConnectionIp = ipAndPort[0];
			component.networkPort = result;
			if (component.CompatibleVersions.Any((string item) => item == ipAndPort[2]))
			{
				component.ShowLog(13);
				component.StartClient();
				return;
			}
			console.AddLog("Discord: Could not join the server - version mismatch.", new Color32(114, 137, 218, byte.MaxValue));
		}
		catch
		{
			console.AddLog("Discord: Could not join the server - incorrect IP address - " + @string, new Color32(114, 137, 218, byte.MaxValue));
		}
		onJoin.Invoke(secret);
	}

	public void SpectateCallback(string secret)
	{
		callbackCalls++;
		console.AddLog("Discord: SpectateCallback fired.", new Color32(114, 137, 218, byte.MaxValue));
		onSpectate.Invoke(secret);
	}

	public void RequestCallback(ref DiscordRpc.JoinRequest request)
	{
		callbackCalls++;
		joinAnimator.SetBool("Requested", true);
		joinText.text = string.Format("<b><color=#7289DA>{0}<color=#99AAB5>#</color>{1}</color></b> would like to join your match!", request.username, request.discriminator);
		console.AddLog(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId), new Color32(114, 137, 218, byte.MaxValue));
		joinRequest = request;
		onJoinRequest.Invoke(request);
	}

	private void Start()
	{
		DiscordRpc.UpdatePresence(ref presence);
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long startTimestamp = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
		presence.startTimestamp = startTimestamp;
		console = GameConsole.Console.singleton;
	}

	private void Update()
	{
		DiscordRpc.RunCallbacks();
	}

	private void OnEnable()
	{
		callbackCalls = 0;
		handlers = new DiscordRpc.EventHandlers
		{
			readyCallback = ReadyCallback
		};
		ref DiscordRpc.EventHandlers reference = ref handlers;
		reference.disconnectedCallback = (DiscordRpc.DisconnectedCallback)Delegate.Combine(reference.disconnectedCallback, new DiscordRpc.DisconnectedCallback(DisconnectedCallback));
		ref DiscordRpc.EventHandlers reference2 = ref handlers;
		reference2.errorCallback = (DiscordRpc.ErrorCallback)Delegate.Combine(reference2.errorCallback, new DiscordRpc.ErrorCallback(ErrorCallback));
		ref DiscordRpc.EventHandlers reference3 = ref handlers;
		reference3.joinCallback = (DiscordRpc.JoinCallback)Delegate.Combine(reference3.joinCallback, new DiscordRpc.JoinCallback(JoinCallback));
		ref DiscordRpc.EventHandlers reference4 = ref handlers;
		reference4.spectateCallback = (DiscordRpc.SpectateCallback)Delegate.Combine(reference4.spectateCallback, new DiscordRpc.SpectateCallback(SpectateCallback));
		ref DiscordRpc.EventHandlers reference5 = ref handlers;
		reference5.requestCallback = (DiscordRpc.RequestCallback)Delegate.Combine(reference5.requestCallback, new DiscordRpc.RequestCallback(RequestCallback));
		DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
	}

	private void OnApplicationQuit()
	{
		DiscordRpc.Shutdown();
	}

	private void OnDestroy()
	{
	}
}
