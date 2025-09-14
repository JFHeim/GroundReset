using System.Collections.Concurrent;
using System.Text;

// ReSharper disable PossibleLossOfFraction

namespace GroundReset;

public static class Terrains
{
    public static async Task<int> ResetTerrains(bool checkWards, int? maxDegreeOfParallelism = null)
    {
        Reseter.watch.Restart();
        var zdos = await ZoneSystem.instance.GetWorldObjectsAsync(Consts.TerrCompPrefabName);
        
        if (zdos.Count == 0)
        {
            LogInfo("0 chunks have been reset. Took 0.0 seconds");
            Reseter.watch.Stop();
            return 0;
        }
        
        LogInfo($"Found {zdos.Count} chunks to reset", insertTimestamp:true);
        
        int dop = maxDegreeOfParallelism ?? Math.Max(1, Environment.ProcessorCount - 1);
        var semaphore = new SemaphoreSlim(dop, dop);
        var tasks = new List<Task>(zdos.Count);
        
        int completed = 0;
        void ReportProgress()
        {
            var c = Interlocked.Increment(ref completed);
            if ((c & 63) == 0) LogInfo($"Progress: {c}/{zdos.Count}");
        }
        
        var results = new ConcurrentBag<(ZDO zdo, ChunkData data)>();

        foreach (var zdo in zdos)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            var task = Task.Run(async () =>
            {
                try
                {
                    ChunkData? newChunkData = await ResetTerrainComp(zdo, checkWards).ConfigureAwait(false);
                    if (newChunkData is not null)
                    {
                        results.Add((zdo, newChunkData));
                        ReportProgress();
                    }
                }
                catch (Exception ex)
                {
                    // Лог ошибочного чанка, чтобы не ронять весь процесс
                    LogWarning($"ResetTerrainComp failed for zdo {zdo.m_uid}: {ex}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        LogInfo($"New data been generated for {completed} chunks. Applying each to game world...", insertTimestamp:true);
        
        int saved = 0;
        foreach (var (zdo, data) in results)
        {
            try
            {
                SaveData(zdo, data);
                saved++;
            }
            catch (Exception ex)
            {
                LogWarning($"SaveData failed for zdo {zdo.m_uid}: {ex}");
            }
        }

        foreach (var comp in TerrainComp.s_instances)
            comp.m_hmap?.Poke(false);

        var totalSeconds = TimeSpan.FromMilliseconds(Reseter.watch.ElapsedMilliseconds).TotalSeconds;
        LogInfo($"{completed} chunks have been reset, {saved} saved. Took {totalSeconds} seconds", insertTimestamp:true);
        Reseter.watch.Stop();

        return completed;
    }

    private static async Task<ChunkData?> ResetTerrainComp(ZDO zdo, bool checkWards)
    {
        var divider                 = ConfigsContainer.Divider;
        var resetSmooth             = ConfigsContainer.ResetSmoothing;
        var resetSmoothingLast      = ConfigsContainer.ResetSmoothing;
        var minHeightToSteppedReset = ConfigsContainer.MinHeightToSteppedReset;
        var zoneCenter            = ZoneSystem.GetZonePos(ZoneSystem.GetZone(zdo.GetPosition()));

        ChunkData? data;
        try
        {
            data = LoadOldData(zdo);
        }
        catch (Exception e)
        {
            LogError(e);
            return null;
        }

        if (data == null) return null;

        var num = Reseter.HeightmapWidth + 1;
        for (var h = 0; h < num; h++)
        for (var w = 0; w < num; w++)
        {
            var idx = h * num + w;

            if (!data.m_modifiedHeight[idx]) continue;
            if (checkWards && Reseter.IsInWard(zoneCenter, w, h)) continue;

            data.m_levelDelta[idx] /= divider;
            if (Abs(data.m_levelDelta[idx]) < minHeightToSteppedReset) data.m_levelDelta[idx] = 0;
            if (resetSmooth && (resetSmoothingLast == false || data.m_levelDelta[idx] == 0))
            {
                data.m_smoothDelta[idx] /= divider;
                if (Abs(data.m_smoothDelta[idx]) < minHeightToSteppedReset) data.m_smoothDelta[idx] = 0;
            }

            var flag_b = resetSmooth && data.m_smoothDelta[idx] != 0;
            data.m_modifiedHeight[idx] = data.m_levelDelta[idx] != 0 || flag_b;
        }
        
        var paintLenMun1 = data.m_modifiedPaint.Length - 1;
        for (var h = 0; h < num; h++)
        for (var w = 0; w < num; w++)
        {
            var idx = h * num + w;
            if (idx > paintLenMun1) continue;
            if (data.m_modifiedPaint[idx] == false) continue;
            
            var currentPaint = data.m_paintMask[idx];
            if (IsPaintIgnored(currentPaint) != false) continue;
            
            if (checkWards || ConfigsContainer.ResetPaintResetLastly)
            {
                var worldPos = Reseter.HmapToWorld(zoneCenter, w, h);
                if (checkWards && Reseter.IsInWard(worldPos)) continue;
                if (ConfigsContainer.ResetPaintResetLastly)
                {
                    Reseter.WorldToVertex(worldPos, zoneCenter, out var x, out var y);
                    // var heightIdx = y * (Reseter.HeightmapWidth + 1) + x;
                    var heightIdx = idx;
                    if (data.m_modifiedHeight.Length > heightIdx && data.m_modifiedHeight[heightIdx]) continue;
                }
            }

            
            data.m_modifiedPaint[idx] = false;
            data.m_paintMask[idx] = Heightmap.m_paintMaskNothing;
        }

        return data;
    }

    private static bool IsPaintIgnored(Color color) =>
        ConfigsContainer.PaintsToIgnore
            .Exists(x =>
                Abs(x.r - color.r) < ConfigsContainer.PaintsCompareTolerance &&
                Abs(x.b - color.b) < ConfigsContainer.PaintsCompareTolerance &&
                Abs(x.g - color.g) < ConfigsContainer.PaintsCompareTolerance &&
                Abs(x.a - color.a) < ConfigsContainer.PaintsCompareTolerance
            );

    private static void SaveData(ZDO zdo, ChunkData data)
    {
        var package = new ZPackage();
        package.Write(1);
        package.Write(data.m_operations);
        package.Write(data.m_lastOpPoint);
        package.Write(data.m_lastOpRadius);
        package.Write(data.m_modifiedHeight.Length);
        for (var index = 0; index < data.m_modifiedHeight.Length; ++index)
        {
            package.Write(data.m_modifiedHeight[index]);
            if (data.m_modifiedHeight[index])
            {
                package.Write(data.m_levelDelta[index]);
                package.Write(data.m_smoothDelta[index]);
            }
        }

        package.Write(data.m_modifiedPaint.Length);
        for (var index = 0; index < data.m_modifiedPaint.Length; ++index)
        {
            package.Write(data.m_modifiedPaint[index]);
            if (data.m_modifiedPaint[index])
            {
                package.Write(data.m_paintMask[index].r);
                package.Write(data.m_paintMask[index].g);
                package.Write(data.m_paintMask[index].b);
                package.Write(data.m_paintMask[index].a);
            }
        }

        var bytes = Utils.Compress(package.GetArray());
        zdo.Set(ZDOVars.s_TCData, bytes);
    }

    private static ChunkData? LoadOldData(ZDO zdo)
    {
        var chunkData = new ChunkData();
        var byteArray = zdo.GetByteArray(ZDOVars.s_TCData);
        if (byteArray == null)
        {
            LogWarning("ByteArray is null, aborting chunk load");
            return null;
        }

        var zPackage = new ZPackage(Utils.Decompress(byteArray));
        zPackage.ReadInt();
        chunkData.m_operations = zPackage.ReadInt();
        chunkData.m_lastOpPoint = zPackage.ReadVector3();
        chunkData.m_lastOpRadius = zPackage.ReadSingle();
        var num1 = zPackage.ReadInt();
        if (num1 != chunkData.m_modifiedHeight.Length)
        {
            LogWarning("Terrain data load error, height array missmatch");
            return null;
        }

        //ok
        for (var index = 0; index < num1; ++index)
        {
            chunkData.m_modifiedHeight[index] = zPackage.ReadBool();
            if (chunkData.m_modifiedHeight[index])
            {
                chunkData.m_levelDelta[index] = zPackage.ReadSingle();
                chunkData.m_smoothDelta[index] = zPackage.ReadSingle();
            } else
            {
                chunkData.m_levelDelta[index] = 0.0f;
                chunkData.m_smoothDelta[index] = 0.0f;
            }
        }

        var num2 = zPackage.ReadInt();

        if (num2 != chunkData.m_modifiedPaint.Length)
        {
            LogWarning($"Terrain data load error, paint array missmatch, num2={num2}, modifiedPaint.Length={chunkData.m_modifiedPaint.Length}, paintMask.Length={chunkData.m_paintMask.Length}");
            num2 = Max(num2, chunkData.m_modifiedPaint.Length, chunkData.m_paintMask.Length);
            if(chunkData.m_modifiedPaint.Length != num2) Array.Resize(ref chunkData.m_modifiedPaint, num2);
            if(chunkData.m_paintMask.Length != num2)     Array.Resize(ref chunkData.m_paintMask, num2);
        }

        for (var index = 0; index < num2; ++index)
        {
            chunkData.m_modifiedPaint[index] = zPackage.ReadBool();
            if (chunkData.m_modifiedPaint[index])
                chunkData.m_paintMask[index] = new Color
                {
                    r = zPackage.ReadSingle(),
                    g = zPackage.ReadSingle(),
                    b = zPackage.ReadSingle(),
                    a = zPackage.ReadSingle()
                };
            else chunkData.m_paintMask[index] = Color.black;
        }

        var flag_copyColor = num2 == Reseter.HeightmapWidth * Reseter.HeightmapWidth;

        if (flag_copyColor)
        {
            var colorArray = new Color[chunkData.m_paintMask.Length];
            chunkData.m_paintMask.CopyTo(colorArray, 0);
            var flagArray = new bool[chunkData.m_modifiedPaint.Length];
            chunkData.m_modifiedPaint.CopyTo(flagArray, 0);
            var num3 = Reseter.HeightmapWidth + 1;
            for (var index1 = 0; index1 < chunkData.m_paintMask.Length; ++index1)
            {
                var num4 = index1 / num3;
                var num5 = (index1 + 1) / num3;
                var index2 = index1 - num4;
                if (num4 == Reseter.HeightmapWidth)
                    index2 -= Reseter.HeightmapWidth;
                if (index1 > 0 && (index1 - num4) % Reseter.HeightmapWidth == 0 && (index1 + 1 - num5) % Reseter.HeightmapWidth == 0)
                    --index2;
                chunkData.m_paintMask[index1] = colorArray[index2];
                chunkData.m_modifiedPaint[index1] = flagArray[index2];
            }
        }

        // LogInfo(debugSb.ToString());
        return chunkData;
    }
}