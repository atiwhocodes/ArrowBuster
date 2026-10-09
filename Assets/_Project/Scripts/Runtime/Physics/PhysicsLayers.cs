namespace ArrowBuster
{
    /// <summary>
    /// Gameplay physics layers, query masks and the layer collision matrix (D-048, docs/planning/03 §2).
    /// Single source of truth: <c>LayerSetup</c> applies it to the project and <c>PhysicsSettingsTests</c> verify it.
    /// </summary>
    public static class PhysicsLayers
    {
        public const int Environment = 6;
        public const int Structure = 7;
        public const int Objective = 8;
        public const int Protected = 9;
        public const int Prop = 10;
        public const int Rope = 11;
        public const int Portal = 12;
        public const int Arrow = 13;
        public const int Debris = 14;
        public const int KillZone = 15;
        public const int Field = 16;

        public const int FirstGameplayLayer = Environment;
        public const int LastGameplayLayer = Field;

        /// <summary>Layer names, indexed by <c>layer - FirstGameplayLayer</c>.</summary>
        private static readonly string[] GameplayLayerNames =
        {
            "Environment", "Structure", "Objective", "Protected", "Prop", "Rope",
            "Portal", "Arrow", "Debris", "KillZone", "Field"
        };

        /// <summary>Arrow flight and trajectory-preview sweeps. KillZone is deliberately excluded (03 §2.2).</summary>
        public const int ArrowCastMask =
            (1 << Environment) | (1 << Structure) | (1 << Objective) | (1 << Protected) |
            (1 << Prop) | (1 << Rope) | (1 << Portal);

        /// <summary>Powder-barrel blasts. Excludes Protected (D-020, D-092).</summary>
        public const int ExplosionMask = (1 << Structure) | (1 << Objective) | (1 << Prop) | (1 << Rope);

        /// <summary>Fire spread overlap queries.</summary>
        public const int FireOverlapMask = (1 << Rope) | (1 << Structure) | (1 << Objective) | (1 << Prop);

        /// <summary>Bodies tracked by the body registry and settle detection.</summary>
        public const int TrackedBodyLayers = (1 << Structure) | (1 << Objective) | (1 << Protected) | (1 << Prop);

        /// <summary>Ground probes (oil spill placement, dummy ground check).</summary>
        public const int GroundProbeMask = 1 << Environment;

        /// <summary>Point checks for flying arrows and the preview against kill zones.</summary>
        public const int KillZoneMask = 1 << KillZone;

        /// <summary>Gameplay layers each gameplay layer collides with (03 §2.1). Symmetric by construction.</summary>
        private static readonly int[] CollidesWith =
        {
            /* Environment */ (1 << Structure) | (1 << Objective) | (1 << Protected) | (1 << Prop) | (1 << Arrow) | (1 << Debris),
            /* Structure   */ (1 << Environment) | (1 << Structure) | (1 << Objective) | (1 << Protected) | (1 << Prop) | (1 << KillZone),
            /* Objective   */ (1 << Environment) | (1 << Structure) | (1 << Objective) | (1 << Protected) | (1 << Prop) | (1 << KillZone),
            /* Protected   */ (1 << Environment) | (1 << Structure) | (1 << Objective) | (1 << Protected) | (1 << Prop) | (1 << KillZone),
            /* Prop        */ (1 << Environment) | (1 << Structure) | (1 << Objective) | (1 << Protected) | (1 << Prop) | (1 << KillZone) | (1 << Field),
            /* Rope        */ 0,
            /* Portal      */ 0,
            /* Arrow       */ (1 << Environment) | (1 << KillZone),
            /* Debris      */ (1 << Environment) | (1 << KillZone),
            /* KillZone    */ (1 << Structure) | (1 << Objective) | (1 << Protected) | (1 << Prop) | (1 << Arrow) | (1 << Debris),
            /* Field       */ 1 << Prop,
        };

        /// <summary>True for layers 6–16.</summary>
        public static bool IsGameplayLayer(int layer) => layer >= FirstGameplayLayer && layer <= LastGameplayLayer;

        /// <summary>Canonical name for a gameplay layer, or null for any other layer.</summary>
        public static string NameOf(int layer) => IsGameplayLayer(layer) ? GameplayLayerNames[layer - FirstGameplayLayer] : null;

        /// <summary>
        /// Whether two layers should collide. Gameplay layers follow the D-048 matrix and never collide with
        /// non-gameplay layers. Pairs of non-gameplay layers keep Unity's default (collide).
        /// </summary>
        public static bool ShouldCollide(int a, int b)
        {
            bool aGameplay = IsGameplayLayer(a);
            bool bGameplay = IsGameplayLayer(b);
            if (!aGameplay && !bGameplay) return true;
            if (!aGameplay || !bGameplay) return false;
            return (CollidesWith[a - FirstGameplayLayer] & (1 << b)) != 0;
        }
    }
}
