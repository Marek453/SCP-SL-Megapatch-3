public enum PlayerPermissions
{
	KickingAndShortTermBanning = 1,
	BanningUpToDay = 2,
	LongTermBanning = 4,
	ForceclassSelf = 8,
	ForceclassToSpectator = 0x10,
	ForceclassWithoutRestrictions = 0x20,
	GivingItems = 0x40,
	WarheadEvents = 0x80,
	RespawnEvents = 0x100,
	RoundEvents = 0x200,
	SetGroup = 0x400,
	GameplayData = 0x800,
	Overwatch = 0x1000,
	FacilityManagement = 0x2000,
	PlayersManagement = 0x4000,
	PermissionsManagement = 0x8000,
	ServerConsoleCommands = 0x10000,
	ViewHiddenBadges = 0x20000,
	ServerConfigs = 0x40000
}
