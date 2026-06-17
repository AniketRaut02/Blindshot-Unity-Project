# Sci-Fi Action/Stealth Game Framework

A robust, data-driven, and highly decoupled framework for a 3D Sci-Fi Action/Stealth game in Unity. This project is built utilizing a strict event-driven architecture, ScriptableObjects for configuration, and the New Unity Input System.

## 🚀 Key Features

* **Modular Player Controller:** Camera-relative movement using a `CharacterController`. Movement, rotation, and animation logic are completely separated into distinct single-responsibility components.
* **Data-Driven QTE System:** A highly flexible Quick Time Event (QTE) manager supporting Single Press, Hold, Mash, and Sequential inputs. Configured entirely via `ScriptableObjects` without touching C# code.
* **Dynamic Laser Hazards:** Laser beams feature dynamic raycasting to prevent clipping through environment walls, with customizable movement profiles (Ping-Pong, Rotation, Flashing) driven by data assets.
* **Timeline Integration:** Custom Timeline tracks and clips (e.g., `PlayerInputDisableTrack`) to seamlessly blend gameplay with cinematic cutscenes and QTE resolutions.
* **Decoupled Input & UI:** The New Unity Input System is wrapped in an `InputReader` ScriptableObject, allowing seamless hot-swapping of action maps (Gameplay, QTE, UI). All UI widgets are entirely event-driven.

## 🏗️ Architecture Overview

The codebase is built on three core design philosophies to prevent spaghetti code and circular dependencies:

### 1. Global Event Bus (`GameEvents.cs`)
Systems never hold direct references to one another. Instead, they communicate via a centralized static event bus.
* **Zero-Allocation:** Events pass data using `readonly struct` payloads (e.g., `PlayerStateChangedPayload`, `QTEZonePayload`) to prevent garbage collection overhead.
* **Null-Safe:** Helper methods (e.g., `RaisePlayerStateChanged`) handle `.Invoke()` logic safely.

### 2. Data-Oriented Configuration (ScriptableObjects)
All tuning and feel are extracted from behavioral logic into ScriptableObjects. Designers can create and swap profiles at runtime.
* `MovementData`: Adjust walk/crouch speeds, acceleration, gravity, and collider bounds.
* `CameraSettingsData`: Configure Cinemachine offsets, FOV, and noise per-zone.
* `QTESequenceData`: Define an entire QTE sequence, time limits, icons, and outcome Timelines.
* `LaserMovementData`: Share complex movement algorithms across multiple laser instances.

### 3. Component Segregation
Large behaviors are broken down into small, digestible MonoBehaviours. For example, the Player consists of:
* `PlayerInputComponent`: Snapshots frame-coherent input.
* `PlayerMovementComponent`: Handles physics and translation.
* `PlayerRotationComponent`: Rotates the body toward velocity.
* `PlayerAnimationComponent`: Translates velocity and crouch state into Animator hashes.
* `PlayerStateMachine`: Evaluates state (Idle, Walk, Crouch, QTE, Dead) and toggles physics layers (e.g., Invincibility frames).

## 🛠️ Setup & Configuration

### Prerequisites
* **Unity Version:** 2021.3 LTS or newer (required for ScriptableObject Timeline bindings).
* **Packages Required:** * Input System
    * Cinemachine (3.x API utilized)
    * Timeline

### Initial Project Setup
1.  **Tags & Layers:** * Ensure your player GameObject is tagged as `Player`.
    * Create an `Invincible` Layer in your Project Settings (used by the `PlayerStateMachine` to protect the player during QTEs and cutscenes).
2.  **Input Actions:** * Create an `.inputactions` asset with exactly three Action Maps: `Gameplay`, `QTE`, and `UI`. 
    * Action names must exactly match the string constants defined in `InputConstants.cs` (e.g., "Move", "Look", "Crouch", "Confirm", "Mash").
3.  **Assigning References:**
    * Assign the `InputReader` ScriptableObject to the various managers and player components that require it.

## 📂 Namespace Structure

* `SciFiGame.Core` - Central event bus, game states, and shared payloads.
* `SciFiGame.Input` - Input wrappers and string constants.
* `SciFiGame.Player` - Movement, state machine, input processing, and animation logic.
* `SciFiGame.Camera` - Cinemachine runtime configuration and settings data.
* `SciFiGame.QTE` - Managers, triggers, and data structures for Quick Time Events.
* `SciFiGame.Laser` - Laser collision, movement, and visual syncing.
* `SciFiGame.Interaction` - Raycast interactables and interface definitions (`IInteractable`).
* `SciFiGame.Checkpoint` - Respawn logic and trigger volumes.
* `SciFiGame.Timeline` - Custom tracks, clips, and directors.
* `SciFiGame.UI` - Event-listening widgets (Countdown, Prompts, Menus).
* `SciFiGame.Audio` - Singleton audio managers and spatial sound components.
