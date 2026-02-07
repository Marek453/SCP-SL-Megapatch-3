using System;

[Serializable]
public class UserGroup
{
	public string BadgeColor;

	public string BadgeText;

	public int Permissions;

	public UserGroup Clone()
	{
		UserGroup userGroup = new UserGroup();
		userGroup.BadgeColor = BadgeColor;
		userGroup.BadgeText = BadgeText;
		userGroup.Permissions = Permissions;
		return userGroup;
	}
}
