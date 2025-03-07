using UnityEditor;
using UnityEngine;

/// <summary>
/// ProjectionCamera applies an off-axis projection based on a specified ProjectionPlane.
/// It calculates the camera-space frustum from the plane's corners and updates the camera's matrices.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ProjectionCamera : MonoBehaviour {
    // Reference to the projection plane.
    public ProjectionPlane m_ProjectionPlane;

    // Toggle to draw gizmos for debugging.
    public bool m_DrawGizmo = true;

    // Option to set the near clip plane equal to the distance from the camera to the projection plane.
    public bool m_SetProjectionPlaneNearClip;

    private Camera m_Camera;

    // Previous frame's camera position for checking movement.
    private Vector3 m_OldPosition;

    // Camera-space vectors for the plane's corners.
    private Vector3 v_tl_cam;
    private Vector3 v_tr_cam;
    private Vector3 v_bl_cam;
    private Vector3 v_br_cam;

    private void Awake() {
        m_Camera = GetComponent<Camera>();
    }

    private void OnDrawGizmos() {
        if (m_ProjectionPlane == null)
            return;

        if (m_DrawGizmo) {
            Vector3 pos = transform.position;

            // Draw lines from the camera position to each corner.
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, pos + v_tl_cam);
            Gizmos.DrawLine(pos, pos + v_tr_cam);
            Gizmos.DrawLine(pos, pos + v_bl_cam);
            Gizmos.DrawLine(pos, pos + v_br_cam);

            // Calculate and draw the average view direction.
            Vector3 viewDir = (v_tl_cam + v_tr_cam + v_bl_cam + v_br_cam) / 4.0f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, pos + viewDir);
#if UNITY_EDITOR
            Handles.Label(pos + viewDir / 2, "View Dir");
#endif
        }
    }

    private void LateUpdate() {
        if (m_ProjectionPlane == null)
            return;

        // Retrieve the projection plane's corner positions and directional vectors.
        Vector3 tl = m_ProjectionPlane.m_TopLeft;
        Vector3 tr = m_ProjectionPlane.m_TopRight;
        Vector3 bl = m_ProjectionPlane.m_BottomLeft;
        Vector3 br = m_ProjectionPlane.m_BottomRight;

        Vector3 vu = m_ProjectionPlane.m_UpDir;
        Vector3 vr = m_ProjectionPlane.m_RightDir;
        Vector3 vn = m_ProjectionPlane.m_NormalDir;

        Matrix4x4 projectionPlaneModelMatrix = m_ProjectionPlane.m_M;

        Vector3 cameraPosition = transform.position;

        // Compute vectors from the camera to each plane corner.
        v_tl_cam = tl - cameraPosition;
        v_tr_cam = tr - cameraPosition;
        v_bl_cam = bl - cameraPosition;
        v_br_cam = br - cameraPosition;

        // Calculate the distance from the camera to the projection plane using the plane center.
        Vector3 planeCenter = m_ProjectionPlane.transform.position;
        Vector3 camToCenter = planeCenter - cameraPosition;
        float distance = -Vector3.Dot(camToCenter, vn);

        // If the camera is behind the projection plane, revert to the previous position.
        if (distance < 0) {
            transform.position = m_OldPosition;
            return;
        }

        // Optionally set the near clip plane to the calculated distance.
        if (m_SetProjectionPlaneNearClip) {
            m_Camera.nearClipPlane = distance;
        }

        // Update the old position.
        if (m_OldPosition != cameraPosition) {
            m_OldPosition = cameraPosition;
        }

        float nearClip = m_Camera.nearClipPlane;
        float farClip = m_Camera.farClipPlane;

        // Calculate frustum extents using the camera-space vectors.
        float left = Vector3.Dot(vr, v_bl_cam) * nearClip / distance;
        float right = Vector3.Dot(vr, v_br_cam) * nearClip / distance;
        float bottom = Vector3.Dot(vu, v_bl_cam) * nearClip / distance;
        float top = Vector3.Dot(vu, v_tl_cam) * nearClip / distance;
        Matrix4x4 projectionMatrix = Matrix4x4.Frustum(left, right, bottom, top, nearClip, farClip);

        // Build the view matrix.
        Matrix4x4 translationMatrix = Matrix4x4.Translate(-cameraPosition);
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation) * m_ProjectionPlane.transform.rotation);

        // Set the camera's world-to-camera and projection matrices.
        m_Camera.worldToCameraMatrix = projectionPlaneModelMatrix * rotationMatrix * translationMatrix;
        m_Camera.projectionMatrix = projectionMatrix;
    }
}
