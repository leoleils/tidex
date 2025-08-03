********************* TRIGGER WARNINGS:
*********************  - Quinn's story deals with themes of depression and anxiety. It's not very different from Shane's 6 hearts event, BUT it borrows a lot from my personal feelings and experiences, so it could very well have an impact.
********************* CHECK 'POTENTIAL TRIGGERS' AT THE END OF THE DOCUMENT FOR A DETAILED BREAKDOWN (SPOILERS!)

VERSION 1.1.7

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

Installation:
       - Download and install SMAPI;
       - Download Content Patcher and drop it into the 'StardewValley/Mods';
       - Download the latest mod file from the official mod page at nexus.com;
       - Drop the folder '[CP] Jakk's Quinn - A new romanceable NPC' into the 'StardewValley/Mods' folder;
       - You're good to go!

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

WHAT'S NEW IN v1.1.7
       - Fixed another bug with translations. Mon4 dialogue wasn't translatable and now it is.

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

TL;DR
  - Configs:
       -> compatibility fixes available for some popular mods(see details below);
  - Events:
       -> multiple routes that change with player choices(community center or joja warehouse)
  - Schedule:
       -> changes with relationship progression(see details below)

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

TRANSLATIONS
       !!! Check official mod page on NexusMods for updates on translations.

       i18n FILES: 
       - English - It's the default language so it's done.
       - Português - Finalmente chegouB! Feita por ProbablyTachi03 S2 - Funciona com qualquer versão acima da 1.1.0
       - Spanish - Works with any version above 1.1.0  - Very kindly made by devy003

       These require you to download a single file named after the language you want to play in and drop it into the i18n folder of the mod. You need to have both files (the official mod + the translation .json) to work. You can download as many i18n files as you'd like and SMAPI'll automatically choose the same language you've chosen for the main game.

       OTHER:
       - Russian - Very kindly made by moonova 
       - Mandarin - Very kindly made by passerby10086
       These require you to download the full translated version of the mod. You can only have ONE of these installed (either the official mod OR the translation)

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

SETTINGS:

See 'Articles' tab on the official mod page for detailed breakdown of the configs

       !! Generic Mod Config is a great tool here. Quinn has a couple of compatibility fixes that have to be manually activated.
       Set whatever compatibility fix you need to true.
       If you don't have Generic Mod Config, open the config.json and manually type the corresponding values within the "" of the config you want to change.
       ig.:
              "New mail paper": "True"
       OR
              "New mail paper": "False"

       Possible Values:
       [
              Seasonal cuter Quinn: 'True'  OR 'False'
              defaults to False
              Quinn changes Clothes through the seasons! Includes Festival Clothing. This is supposed to fit Quinn in alongside Poltergeister's Seasonal Outfits - Slightly Cuter Aesthetic
              >>>
              Has nose: 'True'  OR 'False'
              defaults to True
              Also made to go alongside Poltergeister's Seasonal Outfits - Slightly Cuter Aesthetic. If true, adds a nose on top of Quinn's sprites, if false, keeps the sprites nose-less - not the portraits, though! The nose will always be in the portraits!
              >>>
              New mail paper: 'True'  OR 'False'
              defaults to True
              Quinn has her own unique mail paper. This edits base-game files, so if you have any other mods that interact with mail paper in any shape, way or form you might want to disable this one. If true, this config won't mess with any of the vanilla mail, just Quinn's!
              >>>
              Include Quinn in CT: 'True'  OR 'False'
              defaults to True
              SPOILERY!! Adds Quinn to Emily's 8 Hearts event. This may conflict with mods that change Emily's events.
              >>>
              Meet_up.Day: '4', '5', '6', '7', '9', '10', '14', '18', '19', '26'
              defaults to 7
              SVE EXCLUSIVE. Quinn and other characters will meet every month on this day after you've seen all of their 6 hearts events.

              PORTRAIT CONFIGS!
                     REGULAR
                     >> 'Outfit' allows you to chose which of Quinn's outfits will be shown in her portrait. You don't need to have 'Seasonal cuter Quinn' enabled in order to choose any of them.
                     >> 'Background Color' changes the color of the portrait background. 

                     SPECIAL
                     - If 'Is Special Portrat' is set to true, you'll be able to pick one of four different less-Vanilla-like portraits of Quinn. (This was inspired by Hitme's 'Spouse Portraits Reworked', so kudos to them!)
                     >> 'What Special Portrait' allows you to chose which portrait you want.
       ]

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

Known Bugs:
       - If you've already bought Quinn's portrait, it'll disappear. I had to mess with the item id. You can now obtain it from the travelling cart or by running the command 'debug furniture JakkQuinnPortrait' on your SMAPI pannel.
       - Quinn's portrait might be missing one of its pieces when you first update the mod. This is mostly because of the new configs, so sleeping or forcing the mod to update by changing the configs using GenericModConfig will fix that. If it doesn't, let me know!
       - Quinn's Spouse Room windows stay lit during green rain and at night
       
       !!!> Please report any schedule conflicts if you happen to find any! I've tested Quinn's schedule but I made some changes and I haven't finished re-testing it yet.

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

TO-DO:

See 'Articles' tab on the official mod page for detailed TO-DO list

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

Events: 
       !! None of these work if it's a festival day

       - First event:
              CC COMPLETE: Enter town before 5 pm. Must have at least 1 friendship point with Quinn. Must not have watched 6 hearts event.
              IF NOT: After earning at least 2500g, enter Farm before 12 pm on a sunny day. Must have at least 1 heart with Quinn.

       - 2 Hearts:
              IF CC COMPLETE: Enter Pierre's Shop between 9 am and 4 pm. 
              IF JOJA COMPLETE: Enter Town after 5 pm on a sunny WEEKDAY. 
              IF NEITHER: Enter the Saloon after 8pm.

       - 3 Hearts(JOJA COMPLETE EXCLUSIVE):
              Enter the Saloon after 8pm.

       - 4 Hearts:
              Part one: Enter Town after 7 pm.
              Part two: Enter Beach after 7 pm.

       - 6 Hearts:
              Part one: Enter Haley's House before noon.
              Part two: Enter Haley's House on a sunny day. You must have chosen to stay in part 1. Only triggers if you've slept at least once since watching part 1.

       - 8 Hearts:
              Enter Haley's House before noon on a sunny day.

       - 10 Hearts:
              Enter Mountain after 8 pm on a sunny day.

       - 14 Hearts(Bus repaired):
              Part one: Enter Farm on a sunny day.
              Part two: Enter Bus Stop on a sunny day before 14h00.

       - Extra events(***SPOILERS***):
              a. Enter WizardHouse while Quinn is there;
              b. After being invited by Quinn(winter 27th dialogue), enter beach after 8 pm on the 28th of Winter. This event changes according to your relationship with Quinn. This event repeats ONCE one year after you've first watched it. It's not an exact replay, the second time is different from the first.
              c. SVE exclusive: enter Animal Shop between 9am and 7pm while dating Quinn, Shane and Claire. Must not have watched any of the 'dumped' events.

It's not possible to obtain all of Quinn's events naturally. Mods are required for that if you want to see it all in a single save.
Community Center and Joja runs have roughly the same amount of content.

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

THIS SECTION IS INCOMPLETE. SCHEDULE NEEDS TESTING.
Schedule:
       Quinn has a change in her schedule that reflects her character development, so this section is filled with spoilers!
       Schedule changes when ANY player has seen her 6 hearts event.

       == BEFORE 6 HEARTS:

       == AFTER 6 HEARTS:
              rain = 16h30: leaves house to go to Saloon/22h30 goes home to sleep
              green rain = stays in saloon
              spring = 9h40: leaves house to go to JojaMart/16h40 leaves JojaMart to go to Mountain/22H00 goes home to sleep
                     *sat =
                     *sun =
              summer =
              fall =
              winter =

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

Gift Tastes:

LOVED
       >> "A gift, just like that? No special occasion? This is awesome. Thanks a ton!"
       Pancakes[211], Cheese Cauliflower [197],  Cheese [424],  Goat Cheese [426],  Banana Pudding[904],  Salmon Dinner [212], Seafoam Pudding [265], Cranberry Sauce [238], Chicken Statue [113], Mango [834], Baked Fish [198], Midnight Carp [269], Pyrite [559], Jack-O-Lantern [746], Banana [91], Midnight Squid [798]
LIKED
       >> "Hey, this is cool! Thank you!"
       All legendary fish, Strawberry [400], Qi Seasoning [917] Catfish [143], Squid [151], Flounder [267], Pepper Poppers [215], Pepper [260], Spicy Eel[226], Joja Cola [167], Bait And Bobber [SkillBook_1], Jewels of The Sea [Book_Roe], Queen of Sauce Cookbook [Book_QueenOfSauce]
NEUTRAL
       >> "Thanks, @. I appreciate you going out of your way."
       Squid Ink [814], Roe (any) [812], Maple Bar [731], Cave Carrot [78], Quartz [80], Squid Ink Ravioli [921]
DISLIKED
       >> "I, um... I'm sure I have room for this... somewhere."
       Tortilla [229], Pumpkin Soup [236], Stuffing [239], Pale Broth [457], Coleslaw [648], Chowder [727], Escargot [729], Fried Calamari [202], Hashbrowns [210], Maple Syrup [724], Tropical Curry [907], Baryte [540], Esperite [544]
HATED
       >> "You're really giving me this...? Whatever have I done to you?"
       Any alcoholic beverage, Jasper [563], Hematite [573], Field Snack [403], Shrimp [720], Shrimp Cocktail [733], Eel [148], Sturgeon [698], Blobfish [800], Snail [721],  Oyster [723], Periwinkle [722], Mussel [719], Cockle [718], Parsnip Soup[199], Tom Kha Soup [218], Moss Soup [MossSoup], Book of Mysteries [Book_Mystery], Bean Hotpot [207], Bean Starter [473], Caviar [445], Green Bean [188], Hops [304]

SVE ONLY:
LOVED
       >> Grilled Cheese Sandwich, Fish Dumpling, Cheese Charcuterie, Chocolate Truffle Bar, Frog;
LIKED
       >> Super Joja Cola, Slime Berry, Joja Berry, Alligator, Arrowhead Shark, Barred Knifejaw, Daggerfish, Blue Tang, Gemfish, Diamond Carp;
NEUTRAL
       >> Bonefish;
DISLIKED
       >> Aged Blue Moon Wine, Undead Fish;
HATED
       >> Blue Moon Wine, Frog Legs, Gren Mushroom, Mushroom Colony, Void Mayo Sandwich, Nectarine Fruit Bread.

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

SPOILER WARNING. Obviously.
***POTENTIAL TRIGGERS***

       Quinn's story deals with themes of depression and anxiety. It's not very different from Shane's 6 hearts event, BUT it borrows a lot from my personal feelings and experiences, so it could very well have an impact.
       
       This is INCOMPLETE but I believe its inclusion is fundamentally important, so here's an incomplete and possibly messy version of the TRIGGER WARNING LIST:

       >> DEPRESSION
              Quinn talks openly about her depression. At the beginning of the story, she talks more about feelings and less about labels. She talks about her feelings in broader terms.
              Here's an example:
                     "I'm falling each day further behind... I feel so miserable."
              Later on she does label it as depression.
              **!!> I tried keeping any talk about mental health and mental health struggles similar to how those themes are expressed in Shane's 6 Hearts event.**
       
       >> LGBTQIA+PHOBIA
              Quinn's 14 Heart Event includes her mom, who will be homophobic towards a female farmer or transphobic towards a non-binary player.
              Here is the full extent of the homophobia:
                     "A *woman*? Humpf. I thought it was just a phase."
              And the transphobia:
                     "By the looks of you, you're one of them 'theys' slash 'thems'. Humpf. And here I thought it was just a phase..."
              Don't worry, though, she won't misgender you. She'll use the pronouns you've chosen using 'Gender Neutral Tokens'.
       
       >> OTHER SENSIBLE THEMES 
              ADHD > I didn't originally mean for Quinn to be neurodivergent, but re-reading everything I wrote now, she definitively has ADHD. This is because I was undiagnosed when I started working on this mod and I included a lot of what I thought were common everyday experiences. Because of that, Quinn may hit 'close to home', so to say.

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

STARDEW VALLEY EXPANDED COMPATIBILITY (SPOILERS!)
       - Quinns events have been fixed to work in the new maps;
       - Quinn shows up in festivals;
       - Quinn now interacts with some of the SVE characters and has new dialogue regarding them;
       - Claire and Morris can react to Quinn's events(like Shane already did);
       - Quinn, Shane, Sam and Claire will meet once a month to either hang-out of trash-talk joja(or both) after you've seen Quinn's 6 hearts, Claire's 6 hearts and Shane's 6 hearts. No need to see Sam's 6 hearts. There are new events related to that.
       - If you date Claire, Shane and Quinn at the same time and see all of their 10 hearts events, you'll get a 'dumped' event just like 'girls-dumped' or 'boys-dumped'.
       - Quinn now has reactions to being gifted exclusive SVE items.

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

EXTRA STUFF!!!
       - Quinn now has anime portraits! Done by the delightful and very talented CapMita. Please go show them some love and make sure to leave your endorsement!
              p.s. CapMita has also worked on some very cute alternatives for Quinn's summer outfit and Quinn's spring casual outfit that are only available through their mod.
       - Quinn now has Nyapu style portraits! Huge thanks to DoffeeEnjoyer11, who put in a lot of heart and effort into the artwork. Also go show them some love and don't forget to leave your endorsement! <3

=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
Special thanks!

       - To tiakall, for helping me out with resources and for a lot of encouragement;

       - To Hana (Hanatsuki on NexusMods) for creating the amazing Gender Neutral Tokens Mod and to tenthousandcats(AirynS on nexus mods) who let me dissect their 'Immersive Characters - Shane by tenthousandcats' mods files for refference on how to use the GNTokens;

       - To the lovely folks at the Stardew Valley official discord and at the Stardew Modmakers server, who helped with coding and very kind encouragement;

       - To the incredible folks that created the mods I've added compatibility for;

       - And to the incredibly kind modding community that recieved me and Quinn with such love and such care. The amount of love and support I got was unreal. Thank you so so much!

       - And to you, dear modded player, for giving Quinn a chance :)

       Thanks! s2