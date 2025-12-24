# VANDULL : A First-Person Shooter and Unity 3D Project

A technically-focused first-person shooter developed in Unity over 16 weeks, emphasizing robust systems implementation and custom shader work.

## Overview

This project demonstrates comprehensive game systems development in Unity, featuring custom shader programming, advanced gunplay mechanics, enemy AI, and a complete user settings interface. The focus was on building production-ready systems with placeholder art assets.

## Key Features

### Movement
- Walking / Sprinting
- Crouching / Crouch Walking
- Leaning left and right
- Jumping

### Custom Shader Programming
- **Hybrid Lighting Shader**: Custom shader combining Unity URP/Lit with cel-shaded aesthetics for a unique visual style
- **Context-Aware X-Ray Shader**: Dynamic shader that renders objects as X-ray when occluded (for magazine bullet inspection) but renders normally when directly visible

### Gunplay Systems
- Scene independent items, items can simply be dragged onto a player or enemies
- Bullet trail rendering with physics-based trajectories
- Recoil mechanics with weapon feedback
- Two distinct reload types implemented
- X-ray ammo checking mechanics through physically viewing the bullets in a magazine
- Responsive and polished shooting feel

### Enemy AI
- Detection and tracking mechanics
- Combat behavior and player engagement
- Enemies can use weapons
- Health and damage systems

### Complete UI/UX
- Fully functional settings menu
- Audio volume controls
- Rebindable keyboard and controller inputs
- Sensitivity adjustment
- Level selection system
- Detection and damage indicator

### Technical Implementation
- PC-targeted build
- Controller support with full rebinding
- Modular systems architecture for scalability

## Development Notes

- **Timeline**: 16 weeks
- **Engine**: Unity (URP)
- **Current Status**: Functional systems complete, one playable level, using Mixamo character models as placeholders
- **Focus**: Systems programming and technical implementation over final art assets

## Technical Highlights

The custom shader work represents the core technical achievement of this project. The X-ray shader's ability to conditionally render based on occlusion required understanding of depth testing and stencil buffers, while the hybrid lighting shader demonstrates the ability to modify and extend Unity's rendering pipeline for specific artistic goals.

All core gameplay systems are feature-complete.

## Assets

- Character Models: Mixamo (placeholder)
- All systems, shaders, and mechanics: Custom implementation

---

*This project showcases technical proficiency in Unity 3D development with emphasis on shader programming, systems architecture, and complete feature implementation.*
