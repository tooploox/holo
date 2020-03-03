using UnityEngine;

/* Information about layer.
 * It should be present in asset bundle for each GameObject representing a layer.
 */
public class VolumetricModelLayer : MonoBehaviour
{
    // Data width
    public int Width;

    // Data height
    public int Height;

    // Data depth
    public int Depth;

    // Number of channles in data
    public int Channels;
}
