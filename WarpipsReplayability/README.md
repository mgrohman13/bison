--- Warpips Replayability Mod ---  
This mod aims to increase replayability of Conquest Mode largely by randomizing world maps. This is an early release of the mod, and will be added to in the future. I fell in love with Warpips instantly, and found myself wishing it had the replayability of a game like FTL, so decided to take steps in that direction.

--- HOW TO RUN IT ---  
Warpips does not natively support modding, so this was written as a BepInEx plugin. For quick plug-and-play, download the "WarpipsReplayability_with_BepInEx" file from [Nexus Mods (nexusmods.com)](https://www.nexusmods.com/warpips/mods/1?tab=files). To enable, copy the 4 contents of the zip file into your Warpips install directory (at the same level as Warpips.exe - for Steam this is likely "C:/Program Files (x86)/Steam/steamapps/common/Warpips/"). To disable, move the "BepInEx" folder back out. Note that other game modes such as Endless Mode are not supported and it is advisable to remove the mod if playing them.

--- DEFAULT FEATURES ---  
1) Conquest world maps are randomized. The island layouts and graphics are the same but you will start and end in different locations and the operations in each territory will be randomized. Additionally, not all adjacent territories are connected, so you will be forced to conquer in different directions each time. The number of War Bucks received for each territory are also randomized. Currently, this is done by shuffling the location of all existing operations, but a future update to this mod will fully randomize the operations themselves (still in progress). 
2) Non-adjacent territories are shrouded by a fog of war. You cannot see the enemy lineup nor rewards for standard territories you can't attack. You can always see the lineup of the Enemy Objective and High Value Reward territories. Some rewards are always hidden, even in adjacent territories. 
3) Player lives are set to 3 for all difficulties. Playing on General difficulty is the best experience, but having only one life is tedious and frustrating. This can be changed - see CONFIGURATION section below.
4) If you gain an Extra Life reward while at max lives, you gain 2 additional Combat Coupons instead.
5) The Arms Dealer now occasionally sells 2x of certain items. Many items were configured to do this, but the game was coded to reduce the maximum possible amount by 1. This makes many items that were previously never worth buying occasionally useful now. This feature can be disabled - see CONFIGURATION section below.
6) The number of War Bucks received throughout the campaign has been rebalanced. More are given out for each individual operation, but once the difficulty bar hits maximum, the amount in remaining territories is reduced to 1/3rd of its previous value (except for the High Value Reward and Enemy Objective territories which are not decreased). This is to disincentivize completing every operation on each island in order to be swimming in tech points, and ensure that you can fully complete the upgrade tree without ever maxing out the difficulty bar even on General difficulty. Even with these changes, it's probably still advantageous to complete islands, but less so. This feature can be disabled - see CONFIGURATION section below.

--- CONFIGURATION ---  
There is a configuration file located at /BepInEx/plugins/WarpipsReplayability.txt that can be used to control some of the mod features. It currently has 4 options:
1) PlayerLives - can be set to any reasonable number, or left blank to use the built in difficulty-based numbers. Defaults to 3
2) FixArmsDealer - increases the amount of certain low-value items the Arms dealer sells (see #5 in the DEFAULT FEATURES section above). Defaults to true
3) RebalanceTech - rebalances the number of War Bucks, making it a more appealing option to play through without conquering all island territories (see #6 in the DEFAULT FEATURES section above). Defaults to true
4) DifficultMode - this is an optional mode that makes early and easy operations much more difficult. You will need to bring a decent loadout to beat even the easiest operations, but will receive more rewards to make up for it. This will be a fun mode to try out if you are looking for additional replay value. Defaults to false

--- ADDITIONAL NOTES ---  
Check the log file at /BepInEx/LogOutput.log and send me any errors. If you run into anything weird or unexpected, describe what you were doing and send me the log file. Future updates are planned, most especially the ability to generate fully randomized operations, instead of simply moving existing ones around. Stay tuned!  

--- SOURCE CODE ---  
Source code for this mod can be found here: [bison/WarpipsReplayability at master · mgrohman13/bison (github.com)](https://github.com/mgrohman13/bison/tree/master/WarpipsReplayability)  
Source, readme, and license for BepInEx: [BepInEx/BepInEx: Unity / XNA game patcher and plugin framework (github.com)](https://github.com/BepInEx/BepInEx)
