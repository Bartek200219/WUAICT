# Windows Unattend AutoInstall Creation Tool
This tool should help to reinstall windows on many computers per day. It automatically installs program defined while creating autoinstallation file.

## How to use
0. Run program and edit generated config file.
1. Drag and drop Answer file (template.xml) or exampleInstall.toml on WUAICT.exe and follow program instructions.
template.xml must have:
	* FirstLogonCommands->SynchronousCommand "msiexec.exe /i https://just-install.github.io/stable/just-install.msi"
	* FirstLogonCommands->SynchronousCommand "just-install someprograms"
	* In specialize ProductKey type any value
2. Type key or grab it from current computer
3. Select programs to install after installation
4. Install windows

## Requirements
* Answer File. You can generate this file using [Windows AFG](https://www.windowsafg.com/win10x86_x64_uefi.html) or create file all by yourself [Tutorial](https://www.windowscentral.com/how-create-unattended-media-do-automated-installation-windows-10)
* Ventoy with auto install template configured
* End computer connected to the internet *to install programs*

## Example config
`config.toml`
```
[WUAICT]
defaultPrograms = "firefox google-chrome notepad++ vlc onlyoffice-desktopeditors anydesk 7zip"
otherProgramToDisplay = "microsoft-teams teamspeak discord obs-studio steam origin opera epic-games-launcher"

[ventoy]
XMLsavePath = '../'
ConfigPath = '../ventoy/ventoy.json'
```
### One installation config
`exampleInstall.toml`
```
filePath = 'path/to/file.xml'

[options]

keyGetMethod = "ask"
# keyGetMethod = "KeyFind"
# keyGetMethod = "XXXXX-XXXXX-XXXXX-XXXXX-XXXXX"
programsToInstall = "firefox google-chrome notepad++ vlc onlyoffice-desktopeditors anydesk 7zip"

```

## TO DO
* Remove need to change xml manually

# Contribute
1. Fork this repo
2. Clone your repo
3. Make changes
4. Submit a pull request

# Credits
* [mrpeardotnet/WinProdKeyFinder](https://github.com/mrpeardotnet/WinProdKeyFinder) for KeyDecoder.cs