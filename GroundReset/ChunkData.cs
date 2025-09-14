namespace GroundReset;

[Serializable] 
public class ChunkData
{
    // ReSharper disable  InconsistentNaming
    public bool[] m_modifiedHeight;
    public float[] m_levelDelta;
    public float[] m_smoothDelta;
    public bool[] m_modifiedPaint;
    public Color[] m_paintMask;
    public float m_lastOpRadius;
    public Vector3 m_lastOpPoint;
    public int m_operations;
    // ReSharper restore InconsistentNaming

    public ChunkData()
    {
        var num = Reseter.HeightmapWidth + 1;
        m_modifiedHeight    = new bool[ num * num];
        m_levelDelta        = new float[num * num];
        m_smoothDelta       = new float[num * num];
        m_modifiedPaint     = new bool[ num * num];
        m_paintMask         = new Color[num * num];
    }
}