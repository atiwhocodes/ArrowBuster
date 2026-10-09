using UnityEngine;

namespace ArrowBuster
{
    // Readonly payload structs for GameEvents (01 §10.2). No allocations when raised.

    public readonly struct LevelSessionInfo
    {
        public readonly string LevelId;
        public readonly int WorldId;
        public readonly int GlobalLevel;
        public readonly int Attempt;
        public readonly int ArrowsStart;

        public LevelSessionInfo(string levelId, int worldId, int globalLevel, int attempt, int arrowsStart)
        {
            LevelId = levelId;
            WorldId = worldId;
            GlobalLevel = globalLevel;
            Attempt = attempt;
            ArrowsStart = arrowsStart;
        }
    }

    public readonly struct ArrowFiredInfo
    {
        public readonly ArrowType ArrowType;
        public readonly float AngleDeg;
        public readonly float Power01;
        public readonly int ArrowIndex;
        public readonly int ArrowsRemaining;
        public readonly Vector3 Origin;

        public ArrowFiredInfo(ArrowType arrowType, float angleDeg, float power01, int arrowIndex, int arrowsRemaining, Vector3 origin)
        {
            ArrowType = arrowType;
            AngleDeg = angleDeg;
            Power01 = power01;
            ArrowIndex = arrowIndex;
            ArrowsRemaining = arrowsRemaining;
            Origin = origin;
        }
    }

    public readonly struct ArrowImpactInfo
    {
        public readonly ArrowType ArrowType;
        public readonly MaterialKind Material;
        public readonly BodyKind Target;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly float Speed;
        public readonly ImpactKind Outcome;
        public readonly bool IsMeaningful;

        public ArrowImpactInfo(ArrowType arrowType, MaterialKind material, BodyKind target, Vector3 point, Vector3 normal,
            float speed, ImpactKind outcome, bool isMeaningful)
        {
            ArrowType = arrowType;
            Material = material;
            Target = target;
            Point = point;
            Normal = normal;
            Speed = speed;
            Outcome = outcome;
            IsMeaningful = isMeaningful;
        }
    }

    public readonly struct ArrowResolvedInfo
    {
        public readonly int ArrowId;
        public readonly ArrowResolution Resolution;

        public ArrowResolvedInfo(int arrowId, ArrowResolution resolution)
        {
            ArrowId = arrowId;
            Resolution = resolution;
        }
    }

    public readonly struct BreakInfo
    {
        public readonly MaterialKind Material;
        public readonly Vector3 Position;
        public readonly float Size;
        public readonly DamageSource Cause;

        public BreakInfo(MaterialKind material, Vector3 position, float size, DamageSource cause)
        {
            Material = material;
            Position = position;
            Size = size;
            Cause = cause;
        }
    }

    public readonly struct PropTriggerInfo
    {
        public readonly PropTriggerKind Kind;
        public readonly Vector3 Position;

        public PropTriggerInfo(PropTriggerKind kind, Vector3 position)
        {
            Kind = kind;
            Position = position;
        }
    }

    public readonly struct ObjectiveInfo
    {
        public readonly ObjectiveKind Kind;
        public readonly int Index;
        public readonly int Remaining;
        public readonly Vector3 Position;

        public ObjectiveInfo(ObjectiveKind kind, int index, int remaining, Vector3 position)
        {
            Kind = kind;
            Index = index;
            Remaining = remaining;
            Position = position;
        }
    }

    public readonly struct ProtectedInfo
    {
        public readonly ProtectedKind Kind;
        public readonly ProtectedLossReason Reason;
        public readonly Vector3 Position;

        public ProtectedInfo(ProtectedKind kind, ProtectedLossReason reason, Vector3 position)
        {
            Kind = kind;
            Reason = reason;
            Position = position;
        }
    }

    public readonly struct LevelResultInfo
    {
        public readonly LevelSessionInfo Session;
        public readonly int ArrowsUsed;
        public readonly int Stars;
        public readonly bool BonusArrowUsed;
        public readonly bool Bullseye;
        public readonly float DurationSeconds;
        public readonly FailReason FailReason;
        public readonly int ObjectivesRemaining;
        public readonly int GoldPar;

        public LevelResultInfo(LevelSessionInfo session, int arrowsUsed, int stars, bool bonusArrowUsed, bool bullseye,
            float durationSeconds, FailReason failReason, int objectivesRemaining, int goldPar)
        {
            Session = session;
            ArrowsUsed = arrowsUsed;
            Stars = stars;
            BonusArrowUsed = bonusArrowUsed;
            Bullseye = bullseye;
            DurationSeconds = durationSeconds;
            FailReason = failReason;
            ObjectivesRemaining = objectivesRemaining;
            GoldPar = goldPar;
        }
    }
}
