## Unity Version

* Unity 2021.3.45f2

## Scene

* Main Scene: `Game`


---

## Gameplay Rules

* Each wheel contains rewards and one bomb slice.
* The player spins the wheel.
* Hitting the bomb causes all unclaimed rewards to be lost.
* Every 5th zone is a Safe Zone.
  * No bomb.
  * Player may safely claim rewards.
* Every 30th zone is a Super Zone.
  * Special rewards.
  * No bomb.
* The player can leave and collect rewards only during Safe or Super Zones.
* If the player loses, a gold-based revive option is available.

---


## FULL GAMEPLAY

https://github.com/user-attachments/assets/0008aead-e025-4f5a-9061-1b98725ea551

## CLAIM

https://github.com/user-attachments/assets/de49c053-fdc1-400f-a856-f958bf753a9d

## FAIL COIN REVIVE

https://github.com/user-attachments/assets/b46cda70-b820-45e8-ab59-8cf23b0e6efd

## FAL GIVE UP

https://github.com/user-attachments/assets/f1e75073-ab24-4477-aabb-08f9ae2b78ef

## APK GAMEPLAY

https://github.com/user-attachments/assets/b2de79d5-be18-4dd8-a09c-9a7483438e1e


-----

## 20:9

<img width="1920" height="864" alt="209" src="https://github.com/user-attachments/assets/68fbc407-72e3-43c9-8d93-415fa2935555" />

## 16:9

<img width="1684" height="947" alt="169" src="https://github.com/user-attachments/assets/926c4bd1-b9cd-41dd-89c8-98dbc3adf56b" />

## 4:3

<img width="1263" height="947" alt="43" src="https://github.com/user-attachments/assets/54a0dbf7-d659-4e6e-a8eb-079f125b08f5" />


-----



---

## Third Party Packages

* DOTween
* TextMeshPro

---

## Project Structure

### Core

Contains the main gameplay systems.

| Class                | Responsibility                                 |
| -------------------- | ---------------------------------------------- |
| Wheel                | Wheel spinning logic                           |
| WheelSlice           | Individual wheel slice management              |
| RewardManager        | Reward presentation and reward list management |
| RewardItem           | Collected reward representation                |
| ClaimManager         | Reward claiming flow                           |
| ZoneUIManager        | Zone progression UI                            |
| LoseScreen           | Failure and revive flow                        |
| PlayerEconomyManager | Currency management                            |
| LevelManager         | Level progression                              |
| ObjectPool           | Object pooling system                          |

---

## Event System

The project uses a custom event bus implementation to reduce direct dependencies between systems.

The event-driven approach allows gameplay systems and UI systems to communicate without tight coupling.

---

## Reward System

Rewards are configured through `RewardData` ScriptableObjects.

Available reward types:

* Cash
* Gold
* Chest
* Gun
* Bomb

Each reward contains:

* Display sprite
* Display name
* Placement weight
* Selection weight

Two different weight values are used:

* Placement Weight: determines which rewards appear on wheel slices.
* Selection Weight: determines which reward is selected by the spin.

---

## Slice Configuration

Wheel configurations are defined using `SliceData` ScriptableObjects.

Each SliceData contains:

* Available rewards
* Money amount multiplier

Different slice configurations are used depending on the player's current level.

---

## Level Progression

| Zone            | Type                            |
| --------------- | ------------------------------- |
| Normal Zones    | Standard rewards with bomb risk |
| Every 5th Zone  | Safe Zone                       |
| Every 30th Zone | Super Zone                      |

The reward pool changes according to player progression.

---

## Player Economy

Player currencies are stored using PlayerPrefs.

Stored values:

* Cash
* Gold

Gold is also used for the revive system.

---

## Object Pooling

UI elements are pooled to reduce runtime allocations.

Pooled objects include:

* Reward items
* Zone texts
* Inventory information texts

---

## Architecture

The project follows a combination of:

* Event-driven architecture
* Manager-based architecture
* Data-driven design using ScriptableObjects

### Benefits

* Low coupling between systems.
* Easily expandable reward system.
* Configurable content through the Inspector.
* Better runtime performance through object pooling.
* Simplified communication via EventBus.

---

## Case Study Requirements

Implemented features:

* Wheel-based gameplay.
* Editor-configurable wheel content.
* Bomb mechanic.
* Safe zones.
* Super zones.
* Risk and reward progression.
* Reward claiming system.
* Gold revive system (bonus requirement).

---

## Controls

* Spin Button → Spin the wheel.
* Claim Button → Collect rewards at safe zones.
* Give Up → Restart the run.
* Revive → Spend gold to continue.



---

Critical Strike ID: 6B4D6DF487486DDF

Critical Strike Name: dahaal

Polygun Arena ID: 1FE20C5B4F1CEEDC

Polygun Arena Name: dahaal













