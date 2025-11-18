## Unity Input System Configuration

### Input Actions Asset Setup

**Create Touch Action Map:**
```
Input Actions Asset: MobileInputActions
├── Touch (Action Map)
│   ├── TouchPress (Action)
│   │   ├── Type: Button
│   │   ├── Binding: <Touchscreen>/primaryTouch/press
│   │   └── Interactions: (none)
│   │
│   ├── TouchPosition (Action)
│   │   ├── Type: Value (Vector2)
│   │   ├── Binding: <Touchscreen>/primaryTouch/position
│   │   └── Control Type: Vector2
│   │
│   └── TouchDelta (Action) [optional]
│       ├── Type: Value (Vector2)
│       ├── Binding: <Touchscreen>/primaryTouch/delta
│       └── Control Type: Vector2
```

### PlayerInput Component Configuration

**Settings:**
```
PlayerInput Component:
├── Actions: MobileInputActions (assign asset)
├── Default Action Map: Touch
├── Behavior: Send Messages (or Invoke Unity Events)
├── Camera: Main Camera (optional)
└── UI Input Module: None (or assign for UI)
```

### Supporting Mouse for Editor Testing

**Add Mouse Bindings:**
```
TouchPress Action:
├── Binding 1: <Touchscreen>/primaryTouch/press
└── Binding 2: <Mouse>/leftButton

TouchPosition Action:
├── Binding 1: <Touchscreen>/primaryTouch/position
└── Binding 2: <Mouse>/position
```
