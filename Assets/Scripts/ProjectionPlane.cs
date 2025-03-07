using UnityEditor;
using UnityEngine;

/// <summary>
/// ProjectionPlane computes the corner positions, directional vectors,
/// and model matrix for an off-axis projection plane.
/// </summary>
[ExecuteInEditMode]
public class ProjectionPlane : MonoBehaviour {
    // Public read-only properties for the plane corners.
    public Vector3 m_TopLeft { get; private set; }
    public Vector3 m_TopRight { get; private set; }
    public Vector3 m_BottomLeft { get; private set; }
    public Vector3 m_BottomRight { get; private set; }

    // Public read-only directional vectors.
    public Vector3 m_UpDir { get; private set; }
    public Vector3 m_RightDir { get; private set; }
    public Vector3 m_NormalDir { get; private set; }

    // The model matrix representing the plane's orientation.
    public Matrix4x4 m_M { get; private set; }

    // The size of the plane (modifiable in the Inspector).
    public Vector2 m_PlaneSize;

    // Minimum allowed size for the plane.
    public float m_MinimumPlaneSize = 0.01f;

    // Toggle gizmo drawing in the editor.
    public bool m_DrawGizmo = true;

    // Internal field to detect changes in plane size.
    private Vector2 m_OldPlaneSize;

    /// <summary>
    /// Checks if the plane size has been updated.
    /// </summary>
    /// <returns>True if updated; otherwise, false.</returns>
    private bool IsSizeUpdated() {
        if (m_PlaneSize != m_OldPlaneSize) {
            m_OldPlaneSize = m_PlaneSize;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ensures the plane size does not drop below the minimum allowed.
    /// </summary>
    private void CheckScale() {
        if (m_MinimumPlaneSize < 0.01f) {
            m_MinimumPlaneSize = 0.01f;
        }
        if (m_PlaneSize.x < m_MinimumPlaneSize) {
            m_PlaneSize.x = m_MinimumPlaneSize;
        }
        if (m_PlaneSize.y < m_MinimumPlaneSize) {
            m_PlaneSize.y = m_MinimumPlaneSize;
        }
    }

    /// <summary>
    /// Calculates the world-space corner positions and directional vectors.
    /// </summary>
    private void CalculateVariables() {
        // Compute corner positions based on the plane size.
        m_TopLeft = transform.TransformPoint(new Vector3(-m_PlaneSize.x, m_PlaneSize.y) * 0.5f);
        m_TopRight = transform.TransformPoint(new Vector3(m_PlaneSize.x, m_PlaneSize.y) * 0.5f);
        m_BottomLeft = transform.TransformPoint(new Vector3(-m_PlaneSize.x, -m_PlaneSize.y) * 0.5f);
        m_BottomRight = transform.TransformPoint(new Vector3(m_PlaneSize.x, -m_PlaneSize.y) * 0.5f);

        // Calculate the directional vectors.
        m_UpDir = (m_TopLeft - m_BottomLeft).normalized;
        m_RightDir = (m_BottomRight - m_BottomLeft).normalized;
        // Use negative cross to ensure proper orientation.
        m_NormalDir = -Vector3.Cross(m_RightDir, m_UpDir).normalized;

        CalculateModelMatrix();
    }

    /// <summary>
    /// Constructs the model matrix from the directional vectors.
    /// </summary>
    private void CalculateModelMatrix() {
        Matrix4x4 modelMatrix = Matrix4x4.zero;
        modelMatrix[0, 0] = m_RightDir.x;
        modelMatrix[0, 1] = m_RightDir.y;
        modelMatrix[0, 2] = m_RightDir.z;

        modelMatrix[1, 0] = m_UpDir.x;
        modelMatrix[1, 1] = m_UpDir.y;
        modelMatrix[1, 2] = m_UpDir.z;

        modelMatrix[2, 0] = m_NormalDir.x;
        modelMatrix[2, 1] = m_NormalDir.y;
        modelMatrix[2, 2] = m_NormalDir.z;

        modelMatrix[3, 3] = 1.0f;
        m_M = modelMatrix;
    }

    private void Start() {
        // Initial check and calculation.
        IsSizeUpdated();
        CheckScale();
        CalculateVariables();
    }

    private void Update() {
        // If the size has changed, enforce minimum scale.
        if (IsSizeUpdated()) {
            CheckScale();
        }
        // If the transform has changed, recalculate variables.
        if (transform.hasChanged) {
            CalculateVariables();
        }
    }

    private void OnDrawGizmos() {
        if (!m_DrawGizmo)
            return;

#if UNITY_EDITOR
        // Draw labels for corners and directional vectors in the Scene view.
        Handles.Label(m_TopLeft, "Top Left");
        Handles.Label(m_TopRight, "Top Right");
        Handles.Label(m_BottomLeft, "Bottom Left");
        Handles.Label(m_BottomRight, "Bottom Right");

        Handles.Label(transform.position + m_NormalDir * (m_PlaneSize.magnitude / 4f), "Normal");
        Handles.Label(transform.position + m_UpDir * (m_PlaneSize.magnitude / 4f), "Up");
        Handles.Label(transform.position + m_RightDir * (m_PlaneSize.magnitude / 4f), "Right");
#endif
        // Draw edges of the projection plane.
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(m_TopLeft, m_BottomLeft);
        Gizmos.DrawLine(m_TopLeft, m_TopRight);
        Gizmos.DrawLine(m_TopRight, m_BottomRight);
        Gizmos.DrawLine(m_BottomLeft, m_BottomRight);

        // Draw directional vectors from the plane's center.
        Gizmos.DrawLine(transform.position, transform.position + m_NormalDir * (m_PlaneSize.magnitude / 4f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + m_UpDir * (m_PlaneSize.magnitude / 4f));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + m_RightDir * (m_PlaneSize.magnitude / 4f));
    }
}
