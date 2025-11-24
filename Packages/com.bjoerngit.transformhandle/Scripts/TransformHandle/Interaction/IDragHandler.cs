using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Interface for different drag handling strategies.
    /// Some implementations use the handleSpace parameter to support Local/Global axes.
    /// </summary>
    public interface IDragHandler
    {
        /// <summary>
        /// Called when a drag operation starts on the specified axis.
        /// </summary>
        /// <param name="target">Transform being manipulated.</param>
        /// <param name="axis">Index of the axis being dragged (0=X,1=Y,2=Z).</param>
        /// <param name="mousePos">Current mouse position in screen space.</param>
        /// <param name="handleSpace">Local or Global axis space.</param>
        void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace handleSpace);

        /// <summary>
        /// Called each frame during dragging.
        /// </summary>
        /// <param name="mousePos">Current mouse position in screen space.</param>
        void UpdateDrag(Vector2 mousePos);

        /// <summary>
        /// Called when the drag operation ends.
        /// </summary>
        void EndDrag();
    }
}