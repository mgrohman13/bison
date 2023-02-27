--- Warpips Replayability Mod ---

This mod aims to increase replayability of the Conquest Mode largely by randomizing world maps. The initial release is a minimum viable product, and may be substantially added to in the future. I fell in love with Warpips instantly, and found myself wishing it had the replayability of a game like FTL, so decided to take steps in that direction.


--- HOW DO YOU RUN IT ---

Warpips does not natively support modding, so this was written as a BepInEx plugin. To enable, simply copy the 4 contents of the zip file into your Warpips install directory (at the same level as Warpips.exe). To disable, move the "BepInEx" folder back out or rename it to anything else (e.g. "BepInEx - Copy"). Other game modes such as Endless Mode are not supported.


--- WHAT DOES IT DO ---

1) Conquest world maps are randomized. The island layouts and graphics are the same but you will start and end in different locations and the operations in each territory will be randomized. Additionally, not all adjacent territories are connected, so you be forced to conquer in different directions each time. The number of War Bucks received for each territory are also randomized. 
2) Non-adjacent territories are shrouded by a fog of war. You cannot see the enemy lineup nor rewards for standard territories you can't attack. You can always see the lineup of the enemy base and high value locations. Some rewards are always hidden, even in adjacent territories. 
3) Player lives are set to 3 for all difficulties. Playing on General difficulty is by far the best experience, but having only one life is tedious and frustrating. In the future I will add a config file to control this and other behavior.
4) If you gain an Extra Life reward while at max lives, you gain some additional War Bucks instead.


--- ANYTHING ELSE ---

This is an initial release that was put together in a few days. Please check the log file at /BepInEx/LogOutput.log and send me any errors. There may also be bugs. If you run into anything unexpected, describe what you were doing and send me the log file. 


--- WHERE IS THE SOURCE ---

Source code for this mod can be found here: bison/WarpipsReplayability at master · mgrohman13/bison (github.com)
Source, readme, and license for BepInEx: BepInEx/BepInEx: Unity / XNA game patcher and plugin framework (github.com)

