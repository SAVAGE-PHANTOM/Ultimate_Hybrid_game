# Ultimate Hybrid Game

## Overview

The Ultimate Hybrid Game is an ambitious project to create the ultimate tactical shooter by combining the best elements from PUBG, Free Fire, and Call of Duty (COD). This mobile-first game aims to deliver a high-performance ecosystem with realistic ballistics, fluid movement, unique character abilities, and accessible gameplay. Starting on mobile platforms (Android/iOS), with plans to expand to PC and Console later.

## Vision

This game fills the gap for the "Elite Mobile Player." It's deeper than Free Fire, faster than PUBG, and more tactical than COD Mobile. By leveraging modern development tools like Google Antigravity (agent-first IDE), we focus on the Director's Vision—creating a high-speed tactical sport that looks and feels premium.

## Core Features

### Gameplay & "The Feel"
- **Movement**: COD-style sliding and tactical sprinting with a Stamina Bar to prevent spamming.
- **Gunplay**: PUBG-style projectile ballistics with bullet drop and travel time, combined with COD's One-Tap ADS for snappy mobile response.
- **Skills**: Each player selects a "Specialist" with one Active Skill (e.g., Sonar Pulse) and one Passive Skill (e.g., Mule for extra ammo capacity).

### Map: "Neo-Bermuda" (4x4km)
- Medium-sized map optimized for 15-minute matches.
- Visual Style: "Clean-Tech" with vibrant abandoned luxury resorts and high-tech military bunkers.
- Dynamic POIs: Sector Shifts for varied gameplay, Data Terminals for Scorestreaks.

### Victory Condition: "The Extraction"
- Cinematic sequence with bullet time, VTOL arrival, and holographic After-Action Report.
- Triumphant, professional vibe.

### Technical Architecture
- **Engine**: Unreal Engine 5 (Mobile-First Build).
- **Optimization**: LOD Bias Scaling for different device capabilities.
- **Input**: Input-Agnostic for touch, gamepad, mouse/keyboard.
- **Security**: AI-driven anomaly detection for cheating.
- **Backend**: Google Agones for global server orchestration.

### Economy & Social
- Monetization: Identity-focused (skins, animations, outfits).
- Fair Play: Skills unlocked via gameplay or Battle Pass.
- Social Hub: Ready Room for testing and showcasing.

## Development Roadmap

### Phase 1: Choose Your Engine
- Unreal Engine 5 for high-end graphics and networking.
- Or Unity for broader device compatibility.

### Phase 2: Core Mechanics
- Decide on Movement Meta: Fast-paced with tactical constraints.
- Implement Weighted Fluidity.

### Phase 3: Character System
- Gacha-style with balanced abilities.

### Challenges
- Server Costs: Use Google Agones.
- Optimization: Balance PUBG realism with Free Fire accessibility.
- create maps and assets.

### How to Start
- Download Unreal Engine 5 or Unity.
- Follow Third-Person Shooter tutorials.
- Build a Greybox map.
- Focus on shooting feel first.

## Agent-Based Development (Google Antigravity)

Using Google Antigravity's agent-first workflow:

### Agent A: Physics & Ballistics Specialist
- Handles weapon systems, recoil, ADS, damage curves.
- Artifact: Firing Range environment.

### Agent B: Movement & Fluidity Specialist
- Player controller, sprint, slide, parkour, stamina.
- Artifact: Dynamic HUD.

### Agent C: Hero & Meta Specialist
- Skill system, victory cinematics, optimization.
- Integrates LOD Bias and Agones.

### Agent D: Lead Architect & Integration Specialist
- Merges systems, resolves conflicts, builds match loop.
- Artifact: Master Build preview.

### Sample Prompts
- **Project Skeleton**: Create folder and plan artifact.
- **Logic Brain**: Basic weapon script.

## Getting Started

1. Set up Google Antigravity IDE.
2. Launch agents with provided prompts.
3. Monitor artifacts and integration logs.
4. Run stress tests and generate previews.

## Platforms
- Primary: Android/iOS
- Secondary: PC/Console (via Cloud Vault)

## Status
- Planning Phase: Complete
- Development: Ready to launch agents

## Current Prototype

This repository now includes a desktop playable prototype built with C# and OpenTK. It is not a full production mobile shooter yet, but it does provide a vertical slice of the README vision:

- Top-down arena on a "Neo-Bermuda" inspired battlefield
- COD-style sprint and slide with stamina management
- PUBG-style range-sensitive shooting and bullet accuracy falloff
- Specialist scan skill that highlights nearby enemies
- Shrinking zone pressure
- Extraction unlock after enough eliminations
- Visual HUD bars for health, stamina, and zone state

### Controls

**Essential Movement & Interaction**
- `WASD`: Move
- `Space`: Jump (prototype hop)
- `V`: Vault (not implemented yet)
- `C`: Crouch (toggle)
- `Z`: Prone (toggle)
- `Left Shift`: Sprint
- `Left Ctrl`: Walk
- `F`: Interact (hold to extract once unlocked)
- `R`: Reload

**Combat Controls**
- `Left Click`: Fire
- `Right Click (Hold)`: Aim Down Sights (ADS)
- `Caps Lock`: Aim (Hipfire tighten toggle)
- `Q` / `E`: Lean Left / Right (currently accuracy penalty, animation not implemented)
- `Left Shift (while ADS)`: Hold Breath
- `B`: Toggle firing mode (Auto/Semi)
- `G`: Scorestreak (UAV when ready)
- `Mouse Wheel`: Scope/ADS zoom (not implemented yet)

**Inventory & Gear**
- `Tab` or `I`: Inventory (toggles a pause-like state)
- `1`-`5`, `G`, `~`, `8`-`0`: Not implemented yet

**Vehicle & Map**
- `M`, `F1`, `Insert`, `Middle Mouse Button`: Not implemented yet

Notes:
- `Middle Mouse Button` currently triggers the scan pulse as a stand-in for quick marker.
- `Esc` quits.

### Run

```bash
dotnet run
```

If `google-services.json` is present, the prototype will also attempt Firebase initialization. If not, it runs in offline mode.

## Godot Build (Recommended For Catalina)

On macOS 10.15 Catalina (MacBook Pro late 2012), Unreal Engine is not a practical target. This repo includes a Godot 3.x starter project in `godot/` that matches the control scheme and starts building the hybrid pillars (movement, loot pickup, vehicle enter/drive, abilities placeholder, UAV scorestreak).

### Install Godot 3.x

- Download Godot 3.x for macOS from the official Godot site on your Mac.
- Use the standard `.dmg` app install.

### Run The Godot Project

1. Open Godot.
2. Import the project by selecting the `godot/project.godot` file.
3. Press Play.

### Multiplayer (Prototype)

This Godot build includes a very small ENet-based multiplayer prototype (position sync only).

- Host (server): run the Godot project with `--server`
- Join (client): run with `--connect=IP`
- Optional: `--port=24570`

Example (two Macs on the same network):

- On host: run with `--server`
- On client: run with `--connect=HOST_LAN_IP`

## Contributing
- Use Antigravity agents for code generation.
- Focus on Director-level instructions.

## License
[Add license if applicable]

---

*This project is inspired by the ultimate tactical shooters and aims to innovate in mobile gaming.*
