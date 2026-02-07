using System;
using System.Collections.Generic;
using System.Linq;
using RemoteAdmin;

public class PermissionsHandler
{
	private readonly string _overridePassword;

	private readonly string _overrideRole;

	private readonly Dictionary<string, UserGroup> _groups;

	private readonly Dictionary<string, string> _members;

	private readonly Dictionary<string, int> _permissions;

	private readonly HashSet<int> _raPermissions;

	private readonly YamlConfig _config;

	private int _lastPerm;

	private bool _isVerified;

	private readonly bool _staffAccess;

	private readonly bool _managerAccess;

	private readonly bool _banTeamAccess;

	private int _fullPerm;

	public UserGroup OverrideGroup
	{
		get
		{
			if (!OverrideEnabled)
			{
				return null;
			}
			return _groups.ContainsKey(_overrideRole) ? _groups[_overrideRole] : null;
		}
	}

	public bool OverrideEnabled
	{
		get
		{
			if (string.IsNullOrEmpty(_overridePassword) || _overridePassword == "none")
			{
				return false;
			}
			if (!_isVerified)
			{
				return true;
			}
			if (_overridePassword.Length < 8)
			{
				ServerConsole.AddLog("Override password refused, because it's too short (requirement for verified servers only).");
				return false;
			}
			if (_overridePassword.ToLower() == _overridePassword || _overridePassword.ToUpper() == _overridePassword)
			{
				ServerConsole.AddLog("Override password refused, because it must contain mixed case chars (requirement for verified servers only).");
				return false;
			}
			if (_overridePassword.Any((char c) => !char.IsLetter(c)))
			{
				return true;
			}
			ServerConsole.AddLog("Override password refused, because it must contain digit or special symbol (requirement for verified servers only).");
			return false;
		}
	}

	public bool IsVerified
	{
		get
		{
			return _isVerified;
		}
	}

	public int FullPerm
	{
		get
		{
			return _fullPerm;
		}
	}

	public bool StaffAccess
	{
		get
		{
			return _staffAccess;
		}
	}

	public bool ManagersAccess
	{
		get
		{
			return _managerAccess || _banTeamAccess || _staffAccess || _isVerified;
		}
	}

	public bool BanningTeamAccess
	{
		get
		{
			return _banTeamAccess || _staffAccess || _isVerified;
		}
	}

	public PermissionsHandler(ref YamlConfig configuration)
	{
		_config = configuration;
		_overridePassword = configuration.GetString("override_password", "none");
		_overrideRole = configuration.GetString("override_password_role", "owner");
		_staffAccess = configuration.GetBool("enable_staff_access");
		_managerAccess = configuration.GetBool("enable_manager_access", true);
		_banTeamAccess = configuration.GetBool("enable_banteam_access", true);
		_groups = new Dictionary<string, UserGroup>();
		_raPermissions = new HashSet<int>();
		List<string> stringList = configuration.GetStringList("Roles");
		foreach (string item in stringList)
		{
			string @string = configuration.GetString(item + "_badge", string.Empty);
			string string2 = configuration.GetString(item + "_color", string.Empty);
			if (!(@string == string.Empty) && !(string2 == string.Empty))
			{
				_groups.Add(item, new UserGroup
				{
					BadgeColor = string2,
					BadgeText = @string,
					Permissions = 0
				});
			}
		}
		_members = configuration.GetStringDictionary("Members");
		_lastPerm = 1;
		foreach (KeyValuePair<string, string> member in _members)
		{
			if (!_groups.ContainsKey(member.Value))
			{
				_members.Remove(member.Key);
			}
		}
		_permissions = new Dictionary<string, int>();
		string[] names = Enum.GetNames(typeof(PlayerPermissions));
		foreach (string text in names)
		{
			int num = (int)Enum.Parse(typeof(PlayerPermissions), text);
			_fullPerm += num;
			_permissions.Add(text, num);
			if (num != 4096)
			{
				_raPermissions.Add(num);
			}
			if (num > _lastPerm)
			{
				_lastPerm = num;
			}
		}
		RefreshPermissions();
	}

	public int RegisterPermission(string name, bool remoteAdmin, bool refresh = true)
	{
		_lastPerm = (int)Math.Pow(2.0, Math.Log(_lastPerm, 2.0) + 1.0);
		_fullPerm += _lastPerm;
		_permissions.Add(name, _lastPerm);
		if (remoteAdmin)
		{
			_raPermissions.Add(_lastPerm);
		}
		if (refresh)
		{
			RefreshPermissions();
		}
		return _lastPerm;
	}

	public void RefreshPermissions()
	{
		foreach (KeyValuePair<string, UserGroup> group in _groups)
		{
			group.Value.Permissions = 0;
		}
		Dictionary<string, string> stringDictionary = _config.GetStringDictionary("Permissions");
		foreach (string key2 in _permissions.Keys)
		{
			int num = _permissions[key2];
			if (!stringDictionary.ContainsKey(key2))
			{
				continue;
			}
			string[] array = YamlConfig.ParseCommaSeparatedString(stringDictionary[key2]);
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (_groups.ContainsKey(key))
				{
					_groups[key].Permissions += num;
				}
			}
		}
	}

	public bool IsRaPermitted(int permissions)
	{
		foreach (int raPermission in _raPermissions)
		{
			if (IsPermitted(permissions, raPermission))
			{
				return true;
			}
		}
		return false;
	}

	public UserGroup GetGroup(string name)
	{
		return _groups.ContainsKey(name) ? _groups[name].Clone() : null;
	}

	public List<string> GetAllGroupsNames()
	{
		return _groups.Keys.ToList();
	}

	public Dictionary<string, UserGroup> GetAllGroups()
	{
		Dictionary<string, UserGroup> dictionary = new Dictionary<string, UserGroup>();
		foreach (string key in _groups.Keys)
		{
			dictionary.Add(key, _groups[key]);
		}
		return dictionary;
	}

	public string GetPermissionName(int value)
	{
		return _permissions.FirstOrDefault((KeyValuePair<string, int> x) => x.Value == value).Key;
	}

	public int GetPermissionValue(string name)
	{
		return _permissions.FirstOrDefault((KeyValuePair<string, int> x) => x.Key == name).Value;
	}

	public List<string> GetAllPermissions()
	{
		return _permissions.Keys.ToList();
	}

	public void SetServerAsVerified()
	{
		_isVerified = true;
	}

	public bool IsPermitted(int permissions, PlayerPermissions check)
	{
		return IsPermitted(permissions, Convert.ToInt32(check));
	}

	public bool IsPermitted(int permissions, string check)
	{
		return _permissions.ContainsKey(check) && IsPermitted(permissions, _permissions[check]);
	}

	public bool IsPermitted(int permissions, int check)
	{
		check = (int)Math.Log(check, 2.0);
		return (permissions >> check) % 2 == 1;
	}

	public byte[] DerivePassword(byte[] serverSalt, byte[] clientSalt)
	{
		return QueryProcessor.DerivePassword(_overridePassword, serverSalt, clientSalt);
	}

	public UserGroup GetUserGroup(string steamId)
	{
		return _members.ContainsKey(steamId) ? _groups[_members[steamId]] : null;
	}
}
