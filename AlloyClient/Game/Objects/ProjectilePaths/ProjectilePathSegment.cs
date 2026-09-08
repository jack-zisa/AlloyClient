using System;
using System.Xml.Linq;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Networking;
using Alloy.Common;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Objects.ProjectilePaths;

/// <summary>
///     Class for a segment as part of a chained sequence of segments that form a <see cref="ProjectilePath" />.
/// </summary>
public class ProjectilePathSegment
{
    protected int _mods;

    protected float? _angle;

    protected int? _lifetimeMs;

    protected AccelerationDesc _acceleration;
    private int _lastAccelerationStep = -1;
    
    public ProjectilePathSegment(PathType pathType) {
        Type = pathType;
    }
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectilePathSegment" /> class.
    /// </summary>
    /// <param name="pathType">Path type.</param>
    public ProjectilePathSegment(PathType pathType, float speed, float? angle = null, int? lifetimeMs = null, AccelerationDesc acceleration = null, int? timeOffset = null, params PathSegmentModifier[] mods)
    {
        Type = pathType;
        Speed = speed;
        TimeOffset = timeOffset ?? 0;
        _angle = angle * MathHelper.DegToRad;
        _lifetimeMs = lifetimeMs;
        _acceleration = acceleration;
        _mods = GetModsFlag(mods);
    }

    /// <summary>
    ///     Gets or sets the type of Path, this is used so we can send to the client for correct parsing.
    /// </summary>
    public PathType Type { get; private set; }

    /// <summary>
    ///     Gets or sets information about the projectile that controls this path segment.
    /// </summary>
    public ProjectileInfo Info { get; set; }

    /// <summary>
    ///     Gets or sets the speed of this path segment.
    /// </summary>
    public float Speed { get; protected set;  }

    /// <summary>
    ///     Gets the time offset at which the segment starts modifying the projectile's position.
    /// </summary>
    public int TimeOffset { get; protected set; }

    /// <summary>
    ///     How long the path segment will run for in Ms.
    /// </summary>
    public int LifetimeMs
    {
        get => _lifetimeMs ?? Info.LifetimeMs;
        set => _lifetimeMs = value;
    }

    /// <summary>
    ///     Projectile acceleration per-path.
    /// </summary>
    public AccelerationDesc Acceleration { get => _acceleration; protected set => _acceleration = value; }

    /// <summary>
    ///     Gets or sets the angle of this path segment.
    /// </summary>
    public float Angle
    {
        get => _angle ?? Info.ShootAngle;
        set => _angle = value;
    }

    public bool HasMod(PathSegmentModifier mod)
    {
        return (_mods & (1 << (int)mod)) != 0;
    }

    protected void ApplyModifiers(ref float elapsedLifetimeMs)
    {
        if (HasMod(PathSegmentModifier.Boomerang))
            if (elapsedLifetimeMs > LifetimeMs / 2f)
                elapsedLifetimeMs = LifetimeMs - elapsedLifetimeMs;
    }

    public void UpdateAcceleration(float elapsedLifetimeMs)
    {
        if (Acceleration == null)
            return;

        var elapsed = elapsedLifetimeMs - Acceleration.DelayMS;

        if (elapsed < 0)
            return;

        if (Acceleration.CooldownMS <= 0)
        {
            if (_lastAccelerationStep >= 0)
                return;

            Speed += Acceleration.Acceleration;
            _lastAccelerationStep = 0;

            ClampAccelerationSpeed();
            return;
        }

        var currentStep = (int)(elapsed / Acceleration.CooldownMS);

        if (Acceleration.CooldownRepeat >= 0)
        {
            currentStep = Math.Min(
                currentStep,
                Acceleration.CooldownRepeat - 1
            );
        }

        var applications = currentStep - _lastAccelerationStep;

        if (applications <= 0)
            return;

        Speed += Acceleration.Acceleration * applications;

        _lastAccelerationStep = currentStep;

        ClampAccelerationSpeed();
    }

    private void ClampAccelerationSpeed()
    {
        if (Acceleration == null)
            return;

        if (!float.IsNaN(Acceleration.MinSpeed))
            Speed = MathF.Max(Speed, Acceleration.MinSpeed);

        if (!float.IsNaN(Acceleration.MaxSpeed))
            Speed = MathF.Min(Speed, Acceleration.MaxSpeed);
    }

    /// <summary>
    ///     Gets the position offset of the projectile (relative to the start position of the segment), at a specified time
    ///     relative to the start of the segment.
    /// </summary>
    /// <param name="startPos">Start position for this path.</param>
    /// <param name="elapsedLifetimeMs">Time since the start of the segment.</param>
    /// <returns>The position offset relative to the segment's start position.</returns>
    /// <exception cref="NotImplementedException">Throws exception if not implemented in a derived class.</exception>
    public virtual Vector2 PositionAt(float elapsedLifetimeMs)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Gets the position offset at the end of this segment.
    /// </summary>
    /// <returns>Position offset at the end of the segment.</returns>
    public Vector2 PositionAtEnd()
    {
        return PositionAt(LifetimeMs);
    }

    public virtual void Read(ref SpanReader rdr) {
        Speed = rdr.ReadSingle();
        LifetimeMs = rdr.ReadInt32();
        Acceleration = new AccelerationDesc(ref rdr);
        _angle = rdr.ReadSingle();
        if (float.IsNaN(_angle.Value))
            _angle = null;
        TimeOffset = rdr.ReadInt32();
        _mods = rdr.ReadInt32();
    }
    
    public static ProjectilePathSegment ReadNew(ref SpanReader rdr) {
        var type = (PathType)rdr.ReadByte();
        ProjectilePathSegment ret = type switch {
            PathType.AmplitudePath => new AmplitudePath(),
            PathType.BoomerangPath => new BoomerangPath(),
            PathType.CirclePath => new CirclePath(),
            PathType.CombinedPath => new CombinedPath(),
            PathType.LinePath => new LinePath(),
            PathType.WavyPath => new WavyPath(),
            _ => null
        };
        ret?.Read(ref rdr);
        return ret;
    }
    
    /// <summary>
    ///     Clones itself into a new instance. Required since a behavior references a segment, and then that segment will be
    ///     reused,
    ///     so we need a new instance.
    /// </summary>
    /// <returns>A new instance of the segment with the same values.</returns>
    public virtual ProjectilePathSegment Clone() {
        ProjectilePathSegment ret = new ProjectilePathSegment(0, 0);
        ret.Acceleration = Acceleration.Clone();
        ret._lastAccelerationStep = -1;
        return ret;
    }
    
    public virtual void SetInfo(ProjectileInfo info)
    {
        Info = info;
    }

    public static ProjectilePathSegment ParsePath(XElement pathElement)
    {
        if (pathElement == null)
            return new LinePath(10, null, 100);

        var type = pathElement.Value.Trim();
        var lifeTimeMs = pathElement.GetAttribute<int>("lifetimeMs");
        var acceleration = pathElement.HasElement("Acceleration") ? new AccelerationDesc(pathElement.Element("Acceleration")) : null;
        
        switch (type)
        {
            case "Line":
                var speed = pathElement.GetAttribute<float>("speed");
                return new LinePath(speed, null, lifeTimeMs, acceleration);
            case "Wavy":
                speed = pathElement.GetAttribute<float>("speed");
                return new WavyPath(speed, null, lifeTimeMs, acceleration);
            case "Boomerang":
                speed = pathElement.GetAttribute<float>("speed");
                return new BoomerangPath(speed, null, lifeTimeMs, acceleration);
            case "Circle":
                var rps = pathElement.GetAttribute<float>("rotationsPerSecond");
                var radius = pathElement.GetAttribute<float>("radius");
                return new CirclePath(rps, radius, null, lifeTimeMs, acceleration);
            case "Amplitude":
                speed = pathElement.GetAttribute<float>("speed");
                var amplitude = pathElement.GetAttribute<float>("amplitude");
                var frequency = pathElement.GetAttribute<float>("frequency");
                return new AmplitudePath(speed, amplitude, frequency, null, lifeTimeMs, acceleration);
            /*
            case "Accelerate":
                speed = pathElement.GetAttribute<float>("speed");
                return new AcceleratePath(speed, null, lifeTimeMs);
            */
            /*case "Decelerate":
                speed = pathElement.GetAttribute<float>("speed");
                return new DeceleratePath(speed, null, lifeTimeMs);*/
            /*case "ChangeSpeed":
                speed = pathElement.GetAttribute<float>("speed");
                var inc = pathElement.GetAttribute<float>("inc");
                var cooldown = pathElement.GetAttribute<int>("cooldown");
                var cooldownOffset = pathElement.GetAttribute<int>("cooldownOffset");
                var repeat = pathElement.GetAttribute<int>("repeat");
                return new ChangeSpeedPath(speed, inc, cooldown, null, lifeTimeMs, cooldownOffset, repeat);*/
        }
        
        return null;
    }
    
    /// <summary>
    ///     Parse a projectile desc into a projectile path.
    /// </summary>
    /// <param name="projDesc">Projectile Desc.</param>
    /// <returns>Projectile path segment.</returns>
    public static ProjectilePathSegment ParsePath(ProjectileProperties projDesc)
    {
        // No path defined, import path from old system
        ProjectilePathSegment path;
        if (projDesc.Root.HasElement("Amplitude") || projDesc.Root.HasElement("Frequency"))
        {
            path = new AmplitudePath(projDesc.Speed, projDesc.Amplitude, projDesc.Frequency, null, (int)projDesc.LifetimeMs);
        }
        // else if (projDesc.Parametric) {
        //     path = new ParametricPath(projDesc.Speed);
        // }
        else if (projDesc.Wavy)
        {
            path = new WavyPath(projDesc.Speed, null, (int)projDesc.LifetimeMs);
        }
        else if (projDesc.Boomerang)
        {
            path = new BoomerangPath(projDesc.Speed, null, (int)projDesc.LifetimeMs);
        }
        else
        {
            path = new LinePath(projDesc.Speed, null, (int)projDesc.LifetimeMs);
        }

        return path;
    }

    /// <summary>
    ///     Convert a solo segment to a <see cref="ProjectilePath" />.
    /// </summary>
    /// <returns>Projectile Path.</returns>
    public ProjectilePath ToPath()
    {
        return new ProjectilePath(LifetimeMs, this);
    }

    private static int GetModsFlag(PathSegmentModifier[] mods)
    {
        var ret = 0;
        foreach (var mod in mods)
            ret |= 1 << (int)mod;
        return ret;
    }
}

public struct ProjectileInfo
{
    public Vector2 StartPos;
    public float ShootAngle;
    public int LifetimeMs;
    public AccelerationDesc Acceleration;
    public int ProjId;
    public long StartTime;
}

public enum PathSegmentModifier : byte
{
    None,
    Boomerang
}