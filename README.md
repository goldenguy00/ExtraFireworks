# Risk of Rain 2 Extra Fireworks
#### Created by Physics System and Smxrez

Adds 7 new fully-configurable firework-related items (*only for the most hardcore firework enthusiasts*)

The creators of this mod assume no responsibility for any injuries sustained while playing with fireworks

## Items
| Icon | Item | Description | Tier |
|:-|-|------|-|
|<img src="https://i.postimg.cc/WzxBQXgc/Firework-Dagger.png" width="96"/> | **Firework Dagger** | Gain a 10% (+10% per stack) chance on hit to fire a firework for 300% base damage.| White |
|<img src="https://i.postimg.cc/CxG3QCK9/Fungus.png" width="96"/> | **Fungus** | After standing still for 1 second, shoot fireworks at 9% (+8 per stack) speed (hyperbolic up to 100%) that deal 300% base damage. | White |
| | | | |
|<img src="https://i.postimg.cc/TYDMrvvd/Firework-Daisy.png" width="96"/> | **Firework Daisy** | Release a barrage of fireworks during the Teleporter event, dealing 40x300% base damage. Occurs 2 (+1 per stack) times. | Green |
|<img src="https://i.postimg.cc/8zg84XTh/Firework-Stuffed-Head.png" width="96"/> | **Firework-Stuffed Head** | Using a non-primary skill fires 1 (+1 per stack) fireworks for 300% base damage. | Green |
|<img src="https://i.postimg.cc/yxJqQwJT/Bottled-Fireworks.png" width="96"/> | **Will-o'-the-Firework** | On killing an enemy, release a barrage of 2 (+1 per stack) fireworks for 300% base damage each. | Green |
| | | | |
|<img src="https://i.postimg.cc/1RT1n5T7/Spare-Fireworks.png" width="96"/> | **Spare Fireworks** | Non-player allies gain an automatic firework launcher that propels 4 (+2 per stack) fireworks every 4 seconds for 300% base damage each. | Red |
| | | | |
|<img src="https://i.postimg.cc/C1rT4FK1/Power-Works.png" width="96"/> | **Power Works** | Taking damage to below 25% consumes this item, releasing a barrage of fireworks dealing 20x300% (+20 per stack) base damage. (Refreshes next stage) | Void |

## Known Issues

* Item rarity outlines appear broken on some of the models

### Contact info
Discord: rainbowphysics (can be pinged in Risk of Rain 2 Modding server)

## Changelog
**1.4.1**
* Updated some item descriptions to be more in-line with vanilla
* Updated readme to include images and descriptions for all items

**1.4.0**
* New void item: Power 'Works
* Some very minor fixes

**1.3.0**
* Addition of proper 3D models (created by Smxrez!)
* Update for SotS DLC
* Various tweaks and fixes

**1.2.6**
* Fixed funny scaling issues with lunar/equipment items
* Possibly fixed NPE when items are disabled

**1.2.5**
* Fixed firework-stuffed head triggering on non-primary no-cooldown abilities, such as Railgunner secondary

**1.2.4**
* Fixed rare bug with item scaling being wrong in trishops
* Fixed scaling of firework items that become command essences
* Fixed command essence NPE from firework items due to model scaling

**1.2.3**
* Fixed firework placement for firework dagger, so that it now works correctly for magma worms and other large enemies

**1.2.2**
* Hotfix: fix acrid not inflicting poison/blight

**1.2.1**
* Fixed Spare Fireworks item so it doesn't spam console and now fully works as intended
* Firework spawners now properly attach to character movement
* Buff item descriptions
* Added item tags, so firework-related items now show up in damage chests... and not on enemies
* Remove the while(true) I got mercilessly bullied for

**1.2.0**
* Add near-full config
* Nerf Firework-stuffed Head: primary no longer shoots fireworks
* Complete code refactor because Bubbet called me out (please don't decompile 1.0.0)

**1.1.0**
* Nerf fungus massively: now uses hyperbolic scaling with a = 0.10 and a max cap of 100% default/interactable firework speed
* Nerfed on-hit fireworks to 1x on hit, with the hit chance now depending on the proc coefficient
* Fixed conflict between Firework Daisy and ZetItemTweaks Lepton Daisy
* Fixed on-hit fireworks pushing up on enemies (especially greater wisps and wandering vagrants)
* Fixed tri-shops/multi-shops breaking (RIP free executive card)
* Fixed fungus effect persisting when CharacterBody dies

**1.0.0**
* :) 