# Subnautica VR Enhancements Mod
This mod fixes the majority of issues with Subnautica in VR as shown here [https://www.youtube.com/watch?v=aTnEtO-YqMg](https://www.youtube.com/watch?v=aTnEtO-YqMg). You can download the mod [here](https://www.nexusmods.com/subnautica/mods/173?tab=description). The mod started as direct edits to the Assembly-CSharp.dll for the game to fix problems that were annoying me in VR and I never intended for it to be a mod so the code will not be as clean and structured as it could be. A lot of the code is from Googling and following patterns of other mods so I'm sure better programmers will have better ways to accomplish some of the things that the mod does.

## Changes in Version 1.8.2
- The Sunbeam timer is now visible in VR. I made the timer smaller and the background transparent to make it less obtrusive in VR.
(The sad thing about this is that I only played Subnautica in VR and never even knew about this timer problem so I missed that whole event in the story.)
## Changes in Version 1.8.1
- Updated to work with QModManager 4
## Changes in Version 1.8.0
- You can now move your head independently of the PDA position
- PDA now opens in the correct position regardless of the Seamoth's pitch
- Fixed the issue where the player body would show up in front of the head position.(If you find any cases where this still happens, please let me know)
- The Seaglide position is now more consistent.
- The mouse cursor should not be hidden behind interface elements anymore.

## Previous Changes
- Adds an option to enable VR animations under the General Tab in Options which allows the opening cinematic to play and re-enables the fire in the pod. Animations for climbing ladders, entering and exiting habitats and vehicles also work as well.
- Fixes the badly positioned player model. The Seaglide will no longer block your entire field of view, all tools are fully visible. For those that had a problem with the PDA being too close, it has now been moved further back, tilted and scaled up to improve visibility.
- Subtitles were not visible before and have been shifted up so they are now visible in VR.
- A slider has been added to adjust the walk speed in VR since the default VR walk speed was 60% of the non-VR walking speed.
- The mouse cursor was originally invisible in VR and even when using the gaze based cursor, the alignment of the cursor was wrong. I made the cursor visible when using mouse and keyboard and not using the gaze based cursor option. Also fixed the cursor alignment issue on menus in the world like the scanner room and cyclops menus. Renaming beacons seem to work properly now as well as long as you keep the cursor on the text field, when using the mouse and keyboard. You can also use the mouse to drag and drop items on the toolbar and to rearrange the toolbar items.
- Added an option in the General tab to disable the HUD unless you're looking down to help with the issue of the HUD obstructing the PDA.
- Fixed the issue where the position of sounds in the game would be in the wrong direction if you turned your head in real life instead of turning with the controller or mouse. Without the mod, if you heard a sound on your right side and turned your head in real life to look in the direction of the sound, you would continue hearing the sound on your right side when it should be heard from the front.
- Makes the loading screen more comfortable to look at by removing the background image and displaying the Alterra logo and loading text in the middle of a black screen.
- Fixed the Cyclops camera bug where the view angle at the steering wheel would be off center after using the camera. Also scaled down the HUD for the cyclops camera to make it more visible.
- Scaled down the HUD for the Drone Camera so that the energy and health values are now visible
- Fixed the problem with the builder menu causing problems with the player body position during animations.
- Slightly raised the Main Menu
- Added an option in the in game menu to recenter the VR position so you don't have to blindly search for the F2 key on the keyboard while the headset is on.
- Fixed the body offset problem when re-centering your position in VR
- Removed the automatic recentering of the VR position when cinematics/animations are skipped.
- Made changes to the body positions to keep the the player's head matched to the body under all conditions.
If I have the time I will be adding more improvements in later versions.
