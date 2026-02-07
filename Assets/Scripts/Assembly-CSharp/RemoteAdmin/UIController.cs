using System.Collections.Generic;
using System.Security.Cryptography;
using GameConsole;
using MEC;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class UIController : MonoBehaviour
	{
		public GameObject root_login;

		public GameObject root_panel;

		public GameObject root_tbra;

		public GameObject root_root;

		public Texture wrongPasswordTexture;

		public Button confirmButton;

		public InputField passwordField;

		public bool loggedIn;

		public bool opened;

		public int awaitingLogin;

		public bool textBasedVersion;

		private void Update()
		{
			if (Input.GetKeyDown(NewInput.GetKey("Remote Admin")))
			{
				ChangeConsoleStage();
			}
		}

		public bool IsAnyInputFieldFocused()
		{
			InputField[] componentsInChildren = GetComponentsInChildren<InputField>();
			foreach (InputField inputField in componentsInChildren)
			{
				if (inputField.isFocused)
				{
					return true;
				}
			}
			return false;
		}

		public void ChangeConsoleStage()
		{
			opened = !opened;
			RefreshStatus();
		}

		public void CallSendPassword()
		{
			Timing.RunCoroutine(_SendPassword(), Segment.FixedUpdate);
		}

		public void ChangeTextMode(bool b)
		{
			textBasedVersion = b;
			RefreshStatus();
		}

		public void RefreshStatus()
		{
			if (IsAnyInputFieldFocused())
			{
				opened = true;
			}
			CursorManager.raOp = opened;
			root_panel.SetActive(opened && loggedIn && !textBasedVersion);
			root_tbra.SetActive(opened && loggedIn && textBasedVersion);
			root_login.SetActive(opened && !loggedIn);
			root_root.SetActive(opened);
			FirstPersonController.usingRemoteAdmin = opened;
		}

		public void ActivateRemoteAdmin()
		{
			loggedIn = true;
			RefreshStatus();
		}

		private IEnumerator<float> _SendPassword()
		{
			QueryProcessor queryProc = PlayerManager.localPlayer.GetComponent<QueryProcessor>();
			if (!queryProc.OverridePasswordEnabled)
			{
				Console.singleton.AddLog("Password authentication is disabled on this server!", Color.magenta);
			}
			else
			{
				if (awaitingLogin == 1)
				{
					yield break;
				}
				confirmButton.interactable = false;
				float t = 0f;
				bool gen = false;
				if (queryProc.ClientSalt == null)
				{
					RandomNumberGenerator randomNumberGenerator = new RNGCryptoServiceProvider();
					byte[] array = new byte[16];
					randomNumberGenerator.GetBytes(array);
					queryProc.ClientSalt = array;
					gen = true;
				}
				if (queryProc.Salt == null || gen)
				{
					queryProc.CallCmdRequestSalt(queryProc.ClientSalt);
				}
				while (t < 20f)
				{
					t += Time.fixedDeltaTime;
					yield return 0f;
					if (queryProc.Salt != null)
					{
						break;
					}
				}
				if (queryProc.Salt == null)
				{
					Console.singleton.AddLog("Can't obtain salt from server!", Color.magenta);
					yield break;
				}
				queryProc.Key = QueryProcessor.DerivePassword(passwordField.text, queryProc.Salt, queryProc.ClientSalt);
				queryProc.CallCmdSendPassword(queryProc.HmacSign("Login", -1));
				Console.singleton.AddLog("Sent auth request to the server!", Color.blue);
				awaitingLogin = 1;
				while (awaitingLogin == 1 && t < 5f)
				{
					t += Time.fixedDeltaTime;
					yield return 0f;
				}
				if (awaitingLogin == 2)
				{
					ActivateRemoteAdmin();
				}
				else
				{
					passwordField.GetComponent<RawImage>().texture = wrongPasswordTexture;
				}
				confirmButton.interactable = true;
				awaitingLogin = 0;
			}
		}
	}
}
