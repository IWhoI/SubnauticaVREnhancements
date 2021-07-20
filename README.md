# Subnautica VR Enhancements Mod
This mod fixes the majority of issues with Subnautica in VR as shown here [https://www.youtube.com/watch?v=aTnEtO-YqMg](https://www.youtube.com/watch?v=aTnEtO-YqMg). You can download the mod [here](https://github.com/IWhoI/SubnauticaVREnhancements/releases) or [here](https://www.nexusmods.com/subnautica/mods/173?tab=description). The mod started as direct edits to the Assembly-CSharp.dll for the game to fix problems that were annoying me in VR and I never intended for it to be a mod so the code will not be as clean and structured as it could be. A lot of the code is from Googling and following patterns of other mods so I'm sure better programmers will have better ways to accomplish some of the things that the mod does. The code also contains commented out experimental stuff.

## Mod Features
- Adds an option to enable VR animations under the General Tab in Options which allows the opening cinematic to play and re-enables the fire in the pod. Animations for climbing ladders, entering and exiting habitats and vehicles also work as well.
- Fixes the badly positioned player model. The Seaglide will no longer block your entire field of view, all tools are fully visible. For those that had a problem with the PDA being too close, it has now been moved further back, tilted and scaled up to improve visibility. The initial open distance is also configurable.
- Subtitles were not visible before and have been shifted up so they are now visible in VR and the height and scale is adjustable.
- A slider has been added to adjust the walk speed in VR since the default VR walk speed was 60% of the non-VR walking speed.
- The mouse cursor was originally invisible in VR and even when using the gaze based cursor, the alignment of the cursor was wrong. I made the cursor visible when using mouse and keyboard and not using the gaze based cursor option. Also fixed the cursor alignment issue on menus in the world like the scanner room and cyclops menus. Renaming beacons seem to work properly now as well as long as you keep the cursor on the text field, when using the mouse and keyboard. You can also use the mouse to drag and drop items on the toolbar and to rearrange the toolbar items.
- Fixed an issue with incorrect sound direction when turning with your head instead of the game controller or mouse.
- Makes the loading screen more comfortable to look at by removing the background image and displaying the Alterra logo and loading text in the middle of a black screen.
- Scaled down the HUD for the cyclops and drone cameras to make the edges more visible.
- Added an option in the in game menu to re-center the VR position so you don't have to blindly search for the F2 key on the keyboard while the headset is on.
- The Sunbeam timer is now visible in VR.
- You can now move your head independently of the PDA position.
- When piloting vehicles, the HUD is now attached to the vehicle instead of your head.
- Added options for customizing the HUD opacity, distance, scale and element separation.
- Auto Re-centering VR is done in the main menu and after loading a game.

If I have the time I will be adding more improvements in later versions.
