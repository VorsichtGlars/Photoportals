# Photoportals Tutorial Window

## Overview
The Photoportals Tutorial Window provides quick access to all important resources and documentation for the VRSYS Photoportals package.

## How to Access
In Unity Editor, navigate to:
```
Window > VRSYS > Photoportals Tutorial
```

## Window Layout

The Tutorial Window features a clean, scrollable interface with the following sections:

### 1. Header
**"VRSYS Photoportals"** - Main package title

### 2. Welcome Section
Provides an overview of the Photoportals package:
> "Implementation of Photoportals - a shared reference system in space and time for Virtual Reality. Based on research by Kunert et al., this package enables collaborative VR experiences with spatial anchors."

### 3. Quick Start Guide
- **Description**: Learn how to set up and use the Photoportals package in your Unity project.
- **Button**: "Open Setup Guide"
- **Action**: Opens the GitHub Wiki Setup page in your browser

### 4. Documentation
- **Description**: Comprehensive documentation including API reference, tutorials, and usage examples.
- **Button**: "Open Documentation"
- **Action**: Opens the GitHub Wiki in your browser

### 5. Research Paper
- **Description**: Read the original research paper by Kunert et al. on Photoportals: Shared references in space and time.
- **Button**: "Open Research Paper"
- **Action**: Opens the ResearchGate publication in your browser

### 6. Example Scene
- **Description**: Import the example scene to see Photoportals in action with sample implementations.
- **Button**: "Import Example Scene"
- **Action**: Opens the Unity Package Manager focused on the Photoportals package, where you can import the example scene sample

### 7. GitHub Repository
- **Description**: Visit the source code repository to contribute, explore the codebase, or stay updated.
- **Button**: "Open Repository"
- **Action**: Opens the GitHub repository in your browser

### 8. Bug Reporting
- **Description**: Found an issue? Report bugs or request features on our GitHub Issues page.
- **Button**: "Report Bug"
- **Action**: Opens the GitHub Issues page in your browser

## Features

- **Scrollable Interface**: All content is accessible even in smaller window sizes
- **Minimum Window Size**: 400x500 pixels
- **Theme Adaptive**: Automatically adapts to Unity's light/dark editor theme
- **Consistent Styling**: Uses Unity's EditorStyles for a native look and feel
- **Quick Access**: All buttons directly open the relevant resource in your default browser or Unity interface

## Implementation Details

### Files Created
- `Editor/PhotoportalsTutorialWindow.cs` - Main window implementation (162 lines)
- `Editor/vrsys.photoportals.editor.asmdef` - Editor assembly definition
- `Editor/README.md` - This documentation file

### Menu Location
The window is accessible via the Unity Editor menu: 
```
Window > VRSYS > Photoportals Tutorial
```

### Technical Details
- **Class**: `VRSYS.Photoportals.Editor.PhotoportalsTutorialWindow`
- **Base Class**: `UnityEditor.EditorWindow`
- **Namespace**: `VRSYS.Photoportals.Editor`
- **Assembly**: `vrsys.photoportals.editor`

## Usage Tips

1. **First Time Users**: Open this window when you first import the package to get familiar with available resources
2. **Quick Reference**: Keep the window docked in your Unity layout for easy access to documentation
3. **Example Scene**: Use the "Import Example Scene" button to quickly add the sample scene to your project
4. **Support**: Use the "Report Bug" button to report any issues you encounter

## Customization
The window uses Unity's EditorStyles for consistency with the Unity Editor theme. The styling includes:
- Header text: 18pt bold
- Section titles: 14pt bold
- Descriptions: 11pt word-wrapped
- Buttons: 12pt with 30px height

All external links use `Application.OpenURL()` for cross-platform compatibility.

