# AR QR-Based Machine Visualization Project - Implemented Steps

## 1. Project Setup

### Unity Version
- Created the project in Unity with AR support.

### Packages Installed
Installed the following Unity packages:
- AR Foundation
- ARCore XR Plugin
- TextMeshPro
- Newtonsoft Json

### Platform Settings
Changed platform to:
- Android

### XR Plugin Settings
Enabled:
- ARCore

Path:
Edit → Project Settings → XR Plug-in Management

---

# 2. AR Scene Setup

## Added Components
Added the following to the scene:
- AR Session
- AR Session Origin / XR Origin
- AR Camera
- AR Plane Manager
- AR Raycast Manager

## Plane Detection
Enabled:
- Horizontal Plane Detection

Used plane visualization for surface detection.

---

# 3. QR Code Based Workflow

## Implemented QR Scanner Logic
Workflow:
1. Scan QR code
2. Get Machine ID / Machine Name
3. Call API
4. Download GLB Model
5. Place model in AR scene
6. Fetch machine data after model load

---

# 4. API Integration

## Implemented APIManager Script

### APIs Used
- Health API
- Model API
- Machine Data API

### JSON Handling
Used:
- Newtonsoft Json
- JObject parsing

### Implemented Classes
```csharp
MachineData
HealthResponse
```

### Features Implemented
- GET API calls
- JSON deserialization
- Async coroutine requests
- Error handling
- API health checking

---

# 5. GLB Model Loading

## Implemented Runtime GLB Loading
Features:
- Download GLB from server/CDN
- Instantiate at runtime
- Parent to AR anchor/object
- Scale adjustment
- Rotation adjustment

## Model Placement Logic
Implemented:
- Tap on detected plane
- Spawn model on plane
- Repositioning logic

---

# 6. API Call Sequence Optimization

## Updated Logic
Changed flow from:
```text
Data API → Model API
```

To:
```text
Model API → Data API
```

Reason:
- Ensure UI updates only after model successfully loads.

---

# 7. UI System

## UIManager Script
Implemented:
- Status text updates
- Machine parameter display
- Dynamic UI refresh
- Error messages

## TextMeshPro Integration
Used TMP components for:
- Labels
- Status
- Parameters
- Debug messages

---

# 8. Backend Server

## FastAPI Server Created

### Features
- Health endpoint
- Model serving endpoint
- CORS enabled
- Static GLB file hosting

### Libraries Used
```python
FastAPI
uvicorn
CORSMiddleware
pathlib
```

### Implemented Endpoints
```python
/health
/model
```

---

# 9. CDN / Runtime Model Delivery

## Implemented
- Runtime model fetching
- Local server hosting
- Dynamic GLB loading from URL

### Purpose
Avoid embedding models directly inside Unity build.

---

# 10. Plane Visualization Customization

## Existing Plane Visualization
Default AR plane visualization was not visually appealing.

## Planned Improvement
Custom dotted plane visualization similar to:
- AR placement guides seen in commercial AR apps/videos.

## Decision Taken
Create custom plane material/shader instead of using default plane prefab.

---

# 11. Error Handling Improvements

## Added Try-Catch Handling
Implemented:
- Safe float parsing
- API exception handling
- Null checks
- Connection error handling

---

# 12. AR Interaction Logic

## Implemented Features
- Plane detection
- Touch placement
- Model spawning
- Object positioning
- Runtime updates

---

# 13. Overall Architecture

## Final Runtime Flow

```text
Start App
   ↓
Initialize AR
   ↓
Scan QR Code
   ↓
Get Machine ID
   ↓
Call Health API
   ↓
Call Model API
   ↓
Download GLB
   ↓
Place Model in AR
   ↓
Call Data API
   ↓
Update UI Parameters
```

---

# 14. Technologies Used

## Unity Side
- Unity
- C#
- AR Foundation
- ARCore
- TextMeshPro
- Newtonsoft Json

## Backend Side
- Python
- FastAPI
- Uvicorn

---

# 15. Important Scripts Created

## Unity Scripts
- APIManager.cs
- UIManager.cs
- QR Scanner Script
- Model Loader Script
- AR Placement Script

## Backend Scripts
- FastAPI model server
- Health check API

---

# 16. Future Improvements Planned

## Possible Next Steps
- Custom dotted AR plane shader
- Object scaling gestures
- Rotation gestures
- Anchor persistence
- Better UI animations
- Multiple machine support
- Offline caching
- Cloud model hosting
- Occlusion support
- Lighting estimation improvements
- Better tracking stabilization
```
