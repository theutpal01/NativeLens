# NativeLens — XR Field Intelligence App for Native Plants

An AI-powered Augmented Reality (AR) mobile application for identifying and learning about native plant species in Vellore, Tamil Nadu.

## Project Overview

NativeLens transforms plant encounters into biodiversity discoveries. Users point their smartphone camera at a plant, AI identifies it, and AR displays contextual ecological and conservation information. The app includes a Discovery Gallery for tracking found species and an AI Botanical Guide for answering questions.

## Target Platform

- **Primary**: Android (ARCore compatible devices)
- **Unity Version**: 2022.3 LTS or 6000.0 LTS
- **AR Framework**: AR Foundation 5.x + ARCore XR Plugin

## MVP Features

1. **AR Scanner** - Camera view with plant identification
2. **AR Information Cards** - Contextual overlay with plant details
3. **Discovery Gallery** - 7 native species with locked/unlocked states
4. **AI Botanical Guide** - Contextual Q&A about identified plants
5. **Field Observations** - Save location, date, photo
6. **Progress Tracking** - Discovery counter and progress bar

## MVP Plant Species (7)

1. **Neem** — *Azadirachta indica*
2. **Jamun/Naval** — *Syzygium cumini*
3. **Indian Banyan** — *Ficus benghalensis*
4. **Golden Shower/Konrai** — *Cassia fistula*
5. **Indian Beech/Pungam** — *Pongamia pinnata*
6. **White-leaved Terminalia** — *Terminalia pallida* (Eastern Ghats endemic, Vulnerable)
7. **Flowering Murdah** — *Terminalia paniculata*

## Unity Project Setup

### 1. Open in Unity Hub

1. Open Unity Hub
2. Click "Add" → Select `/home/user/Projects/NativeLens` folder
3. Open with Unity 2022.3 LTS or 6000.0 LTS

### 2. Install Required Packages

Open Package Manager (Window → Package Manager) and install:

- **AR Foundation** (5.1.3+)
- **ARCore XR Plugin** (5.1.3+)
- **XR Plugin Management** (4.4.1+)
- **TextMeshPro** (3.0.6+)
- **Addressables** (1.21.19+)
- **Newtonsoft JSON** (3.2.1+)

### 3. Configure Project Settings

**Player Settings (Edit → Project Settings → Player):**
- Product Name: `NativeLens`
- Company Name: `NativeLens Team`
- Default Orientation: `Portrait`
- Minimum API Level: `Android 7.0 (API Level 24)`
- Target API Level: `Android 14 (API Level 34)`
- Scripting Backend: `IL2CPP`
- Target Architectures: `ARM64`
- Strip Engine Code: `Enabled`

**XR Plugin Management:**
- Enable "Initialize XR on Startup"
- Add ARCore to "Plug-in Providers"

**Graphics:**
- Graphics APIs: `OpenGLES3`, `Vulkan` (remove Vulkan if issues)
- Color Space: `Linear` or `Gamma`

### 4. AR Session Setup

1. Create new scene: `Assets/Scenes/ARScene.unity`
2. Right-click Hierarchy → XR → AR Session
3. Right-click Hierarchy → XR → XR Origin (AR Foundation)
4. Configure XR Origin:
   - Camera: Main Camera (child of Camera Offset)
   - Camera Offset: Position (0, 1.5, 0) for average eye height
5. Add components to XR Origin:
   - AR Raycast Manager
   - AR Plane Manager
   - AR Anchor Manager
   - AR Camera Background (on Main Camera)

### 5. UI Setup

Create Canvas with:
- **Render Mode**: Screen Space - Camera
- **Render Camera**: Main Camera (from XR Origin)
- **Plane Distance**: 1
- **UI Scale Mode**: Scale With Screen Size
- **Reference Resolution**: 1080 x 1920
- **Match**: 0.5 (Height)

### 6. Connect Scripts

1. Create empty GameObject "GameManager" → Add `GameManager` script
2. Create empty GameObject "Managers" → Add all manager scripts:
   - `PlantDataManager` (assign PlantDatabase asset)
   - `ARManager` (assign AR components and UI prefabs)
   - `GalleryManager` (assign UI prefabs)
   - `PlantIdentificationManager` (assign camera preview UI)
   - `BotanicalGuideManager` (assign chat UI)
   - `ObservationManager` (assign observation UI)
   - `UIManager` (assign screen panels and nav buttons)
3. Create `PlantDatabase` asset: Right-click → Create → NativeLens → Plant Database

### 7. Build for Android

1. File → Build Settings
2. Add `Assets/Scenes/ARScene.unity` to Scenes In Build
3. Platform: Android
4. Click "Build" → Save as `NativeLens.apk`
5. Install on ARCore-compatible Android device

## Development Phases

Following the specification's phased approach:

| Phase | Focus | Status |
|-------|-------|--------|
| 1 | AR Foundation (Camera + Tracking) | 🟡 Ready to implement |
| 2 | AR UI (Info Cards + Placement) | ⏳ Pending |
| 3 | Gallery (Cards + Progress) | ⏳ Pending |
| 4 | Plant Identification (AI Integration) | ⏳ Pending |
| 5 | Plant Database (Structured Data) | ✅ Complete |
| 6 | Discovery Integration | ⏳ Pending |
| 7 | Botanical Guide (AI Q&A) | ⏳ Pending |
| 8 | Field Observations | ⏳ Pending |
| 9 | Polish & Demo Prep | ⏳ Pending |

## Architecture

```
GameManager (State Machine)
├── PlantDataManager (Data + Discovery State)
├── ARManager (AR Foundation + Info Cards)
├── GalleryManager (Gallery UI + Discovery Animation)
├── PlantIdentificationManager (Camera + AI)
├── BotanicalGuideManager (Contextual AI Chat)
├── ObservationManager (GPS + Photo + Save)
└── UIManager (Navigation: Home/Scan/Gallery)
```

## Key Design Principles

1. **Modular AR** - AR functionality isolated in ARManager
2. **Data Separation** - Plant data in ScriptableObject, not hardcoded in UI
3. **Replaceable AI** - Identification manager abstracts AI provider
4. **Offline-First Gallery** - Works without cloud sync
5. **State Machine** - Clear app states (Home/Scan/Analysing/Identified/AR/Gallery)

## AI Integration

Currently uses mock identification (`useMockIdentification = true`). To integrate real AI:

1. Set `useMockIdentification = false` in PlantIdentificationManager
2. Implement `RealIdentificationRoutine()` to call your AI service
3. Options:
   - Custom TensorFlow Lite model (on-device)
   - Cloud Vision API (Google, AWS, Azure)
   - Custom model via Unity Barracuda/ONNX Runtime

## Testing on Device

**Requirements:**
- Android 7.0+ (API 24+)
- ARCore supported device
- Camera permission
- Location permission (for observations)

**Debug Tips:**
- Use `adb logcat -s Unity` for logs
- Enable "Development Build" for profiler connection
- Test AR tracking in various lighting conditions

## File Structure

```
Assets/
├── Scripts/
│   ├── Managers/          # All manager classes
│   ├── Models/            # Data models (Plant, DiscoveryState, etc.)
│   ├── Data/              # PlantDatabase ScriptableObject
│   ├── UI/                # UI components (PlantCardUI, etc.)
│   └── Utils/             # SceneBootstrap, helpers
├── Scenes/
│   └── ARScene.unity
├── Prefabs/
│   ├── ARInfoCard.prefab
│   ├── PlantCard.prefab
│   └── LockedCard.prefab
├── Resources/
│   ├── PlantData/         # Plant images, JSON
│   └── UI/                # UI assets
└── Materials/             # AR card materials, shaders
```

## Hackathon Demo Checklist

- [ ] AR session initializes on device
- [ ] Plane detection works
- [ ] Camera captures image
- [ ] Mock identification returns plant
- [ ] AR info card appears at tap position
- [ ] Card shows: name, scientific name, family, native status, ecology, conservation, confidence
- [ ] Buttons work: Learn More, Ecology, Conservation, Ask Guide, Add to Gallery, Save Observation
- [ ] Gallery shows 7 cards (discovered + locked)
- [ ] Discovery animation plays on new species
- [ ] Progress bar updates (X/7)
- [ ] Botanical Guide opens with plant context
- [ ] Quick questions work
- [ ] Field observation saves with GPS
- [ ] All 7 plant species have accurate data

## License

MIT License - Built for XR Hackathon "Field Intelligence App for Native Plants"

## Team

NativeLens Team - VIT Vellore Campus Biodiversity Project