# Inititative Tracker
 An initiative tracker for DND 5E that is designed to go one step further than traditional initiative trackers. As a desktop app, it also allows further functionality like saving and loading your tracker's state!

## Core Tracking Functionality

### Character and NPC Tracking
The tracker breaks down combat order into tracker **entries** that represent the simple necessary information about a character/NPC required for determining, managing, and tracking combat order and actions. From here on, any mention to an **entry** is referring to this concept.

#### Entry Data:
- Name
- Initiative
- Dexterity Modifier
- Health Points
- Armor Class

Entries support many helpful functions like rolling initiative, saving, duplication, deletion, and moving up and down in the combat order.

The tracker also allows mass initiative rolls and automatic sorting based on initiative rolls.

#### Entries vs. Encounters
As previously stated, an entry contains the necessary data for a single individual. The way an **encounter** differs from an entry is by simply being a collection of entries into one package. This simplistic modular approach allows the combination of any amount of entries greater than one to create a whole new combat **encounter**.

## Built-In Standard Rules Creatures
One of the grievances with many other initiative trackers is the lack of any built-in creatures or NPCs. This programs remedies this by making use of Open Game Content derived under the Open Game License V1.0a provided by **Wizards of the Coast**. This includes **326** standard rules animals, monsters, and NPCs from the world of DND 5e! Furthermore, the tracker contains some built-in encounters that utilize this content to allow for even easier encounter building.

## Persistence and Modding
A key feature of this tracker is the ability to save and load your own custom entries and encounters! You can either create them using the helpful features inside of the tracker's UI or by creating your own custom XML files in the project's user data folder found in all godot projects. This feature is extremely helpful for keeping track of your character's as well as specific NPC encounters. Multiple different entries or encounters can be loaded into the tracker allowing for great modularity.

### Godot User Data Path
`C:\Users\USER\AppData\Roaming\Godot\app_userdata\Initiative Tracker\saves`

### Entry XML Layout
```XML 
<monster>
    <name>Example Entry</name>
    <initiative>0</initiative>
    <dexModifier>2</dexModifier>
    <armorClass>15</armorClass>
    <healthPoints>10</healthPoints>
</monster>
```

### Encounter XML Layout
```XML
<encounter>
    <monster>
        <name>Goblin Captain</name>
        <initiative>0</initiative>
        <dexModifier>2</dexModifier>
        <armorClass>15</armorClass>
        <healthPoints>12</healthPoints>
    </monster>
    <monster>
        <name>Goblin 1</name>
        <initiative>0</initiative>
        <dexModifier>2</dexModifier>
        <armorClass>15</armorClass>
        <healthPoints>9</healthPoints>
    </monster>
    <monster>
        <name>Goblin 2</name>
        <initiative>0</initiative>
        <dexModifier>2</dexModifier>
        <armorClass>15</armorClass>
        <healthPoints>9</healthPoints>
    </monster>
</encounter>
```