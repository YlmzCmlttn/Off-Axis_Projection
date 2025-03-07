# Off-Axis Projection

This Unity project demonstrates an off-axis projection technique using a custom projection plane and camera. It shows how to calculate an off-axis frustum based on a projection plane defined by its corner points and then apply a custom view and projection matrix to a camera.

## Features

- **ProjectionPlane.cs:**  
  Calculates the world-space positions of the four corners of the projection plane, its up/right/normal vectors, and builds a model matrix representing its orientation.

- **ProjectionCamera.cs:**  
  Uses the computed data from the ProjectionPlane to create an off-axis projection matrix. The camera’s view is adjusted so that its frustum exactly aligns with the projection plane.

- **Gizmo Visualization:**  
  In the Editor, the project draws gizmos (with labels and color-coded lines) to help visualize the plane’s corners, directional vectors, and the computed view direction.

## Requirements

- Unity 2019.4 or later (with support for C# 7.2 or later).
- This project is set up to execute in edit mode so you can preview the off-axis projection in the Scene view.

## Setup

1. **Import the Scripts:**  
   Add `ProjectionPlane.cs` and `ProjectionCamera.cs` to your Unity project (e.g., in a `Scripts` folder).

2. **Create a Projection Plane:**  
   - Create a new GameObject and attach the `ProjectionPlane` script.
   - Adjust the `m_PlaneSize` and transform values as desired.

3. **Set Up the Projection Camera:**  
   - Create or select a Camera GameObject and attach the `ProjectionCamera` script.
   - Assign the ProjectionPlane GameObject to the `m_ProjectionPlane` field in the inspector.
   - Optionally, enable `m_SetProjectionPlaneNearClip` to adjust the near clip plane automatically.

4. **Preview the Setup:**  
   In the Scene view, ensure that gizmos are enabled to see the labels and lines representing the projection plane and computed view direction.

## How It Works

- **ProjectionPlane:**  
  The script calculates the corners (top-left, top-right, bottom-left, bottom-right) and the directional vectors (up, right, normal) of the plane in world space. It also constructs a model matrix based on these vectors.

- **ProjectionCamera:**  
  The camera script computes the off-axis frustum by determining the relative positions of the plane's corners from the camera’s position. Using the dot product of these vectors with the plane’s directional vectors, it calculates the left, right, top, and bottom extents of the frustum. Finally, it sets the camera's world-to-camera and projection matrices accordingly.
