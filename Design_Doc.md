===========================

1. COMPLETE WORKING GAME DESIGN DOCUMENT (v1.0)

===========================

Title: TBD (Working Title: “Legacy of the Shattered Crown”)
Genre: Tactical RPG / Squad Builder / Realm Liberation
Engine: Monogame (C#)
Target: PC initially

⸻

1.0 HIGH-LEVEL OVERVIEW

You command a squad of three heroes attempting to liberate a land fractured into multiple occupied realms.
Each realm was once ruled by an Ancient House.
By freeing all provinces within a realm, you restore that House, gaining:
	•	A new hero for your permanent roster
	•	A new troop type (hero loadout modifier)
	•	A realm buff providing passive power for the rest of the run

Combat takes place on an 8×8 grid, with decisive push/pull positioning mechanics, tight tactical turns, enemy intent telegraphs, and terrain interactions.

Each run involves liberating the full map with different hero combinations.
Across runs, you permanently expand your roster and troop options.

⸻

2.0 CORE GAMEPLAY LOOP

2.1 Out-of-run (Meta Progression)
	•	Unlock heroes permanently
	•	Unlock troop types permanently
	•	Build a team of 3 heroes before starting a run
	•	Choose troop attachments for each hero
	•	Begin run in your starting realm

2.2 In-run (Campaign Layer)
	•	Move between provinces
	•	Engage enemy forces
	•	Battle in tactical combat
	•	Liberate provinces
	•	Restore Ancient Houses
	•	Gain realm buffs

2.3 Tactical Combat Layer

Small-grid tactical engagements emphasizing:
	•	Turn-based combat
	•	Perfect-information enemy telegraphs
	•	Positioning and displacement
	•	Terrain influence
	•	Hero abilities + troop abilities
	•	Fast, deterministic encounters

⸻

3.0 HEROES & TROOP SYSTEM

3.1 Heroes (Permanent Unlocks)

Each hero is a unique tactical unit with:
	•	Stats (HP, movement, attack range, ability cooldowns)
	•	1–2 hero abilities
	•	A passive trait
	•	Personality and House affiliation

Heroes are the primary pieces on the battlefield.

⸻

3.2 Troop Attachments (Per-run Loadouts)

Each hero may equip one troop type.
That troop confers:
	•	Stat modifier
	•	One passive
	•	One special ability

Examples:
	•	Infantry – +HP, “Hold Ground”, Shield Bash
	•	Archers – ranged modifier, “Volley Ready”, Line Shot
	•	Spearmen – push resistance, “Phalanx”, Brace
	•	Cavalry – high mobility, “Momentum”, Charge Attack
	•	Mages – elemental scaling, “Arcane Conduit”, Fireburst
	•	Priests – healing utility
	•	Berserkers – damage scaling when wounded

This system provides deep buildcrafting.

⸻

4.0 BATTLEFIELD SYSTEM

Battlefield supports dynamic sizes and non-rectangular shapes.

4.1 Tile Types (Implemented)
	•	Plains - Standard terrain, no modifiers
	•	Forest - Movement cost 2, +1 Defense when defending
	•	Hill - Movement cost 1, +1 Attack when attacking from high ground
	•	Hazard - Deals 2 damage to units at end of turn
	•	Water - Impassable terrain, blocks movement
	•	Mud - Movement cost 2, difficult terrain
	•	Ice - Slippery terrain, extends push effects
	•	Void - Off-map tiles, used to create non-rectangular shapes

4.2 Battlefield Shapes (Implemented)
	•	Standard Battlefield - 10x8 open field with scattered terrain
	•	Diamond Arena - 9x9 diamond shape with central hill
	•	River Crossing - 10x8 with water river and bridge chokepoints
	•	Corner Ruins - 10x8 L-shaped battlefield
	•	Island Fortress - 11x9 with water moat and bridges
	•	Mountain Pass - 12x7 narrow canyon corridor
	•	Hexagon Arena - 10x8 hexagonal approximation

Battlefields are selected based on province difficulty for variety.

4.3 Movement
	•	Hero has movement budget equal to movement stat
	•	Uses flood-fill pathfinding accounting for terrain costs
	•	Forest and Mud cost 2 movement points to enter
	•	Water and Void tiles are impassable
	•	Cannot move into occupied tiles
	•	Cannot move off-map or onto Void tiles

4.4 Combat Flow

Each turn:
	1.	Select unit
	2.	Move or ability
	3.	Resolve attacks
	4.	Displacement occurs
	5.	Check knockback collisions
	6.	End turn
	7.	Enemies act

4.5 Push/Pull Mechanics
	•	Many abilities deal 1-tile displacement
	•	Collision with units → damage
	•	Collision with terrain → hazard effects
	•	Heavy units resist push
	•	Flanking bonuses apply based on displacement direction

This is the tactical “core identity.”

4.6 Enemy Intent Telegraphs

Before enemy turn:
	•	Their planned move or attack is shown
	•	Allows player foresight and puzzle-like play

⸻

5.0 REALM SYSTEM

5.1 Ancient Houses

Each realm contains provinces belonging to a Great House.
Freeing all provinces restores that House.

Restoration yields:
	•	A new hero
	•	A troop type
	•	A realm buff (stackable passive)

Example buffs:
	•	Movement +1
	•	Ability cooldown reduction
	•	Push strength +1
	•	Hazard immunity
	•	+HP to all heroes
	•	Terrain bonuses

⸻

6.0 ART DIRECTION (Light Spec)

Tactical layer:
	•	Clean, readable tiles
	•	Low-noise sprites
	•	Clear telegraph icons
	•	Contrasting team colors

Overworld:
	•	Node-based province map
	•	Ancient House emblems

⸻

7.0 TECH SPECS (MONOGAME)

7.1 Core Classes
	•	Game1.cs – entrypoint
	•	BattleState.cs – tactical state machine
	•	Grid.cs – tile coordinate logic and terrain-aware pathfinding
	•	GridRenderer.cs – drawing for board with terrain visualization
	•	Unit.cs – hero/enemy representation
	•	UnitRenderer.cs
	•	InputManager.cs – mouse handling
	•	TurnManager.cs
	•	AbilitySystem.cs
	•	TerrainType.cs – terrain type enum with effect properties
	•	Battlefield.cs – battlefield data with shape and terrain layout

7.2 Rendering
	•	Immediate mode rendering with SpriteBatch
	•	Single-pixel texture for shapes
	•	Animations added later

7.3 Future Systems
	•	Save/load for meta unlocks
	•	Province graph
	•	AI for enemies
	•	UI overlays
