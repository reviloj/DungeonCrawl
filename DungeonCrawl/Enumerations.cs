
public enum Direction
{
    Left = -1,
    Up,
    Right = 1,
    Down,
    UpLeft,
    UpRight,
    DownRight,
    DownLeft
}

public enum PlayerActions
{
    Idle,
    MoveLeft,
    MoveRight,
    Jump,
    Attack,
    Skill1,
    Skill2,
    Skill3
}

public enum CollisionTypes
{
    Enemy,
    Item,
    Obstacle,
    ActiveItem
}

public enum TypesOfStates
{
    Movement,
    Attack,
    MovementLock,
    Status
}

public enum ObjectStates
{
    Idle,
    Airborne,
    Running,
    AttackPrep,
    Attacking,
    VelocityLocked,
    ControlLocked,
    Knockback,
    Stun
}

public enum StatTypes
{
    Health,
    Attack,
    Movement,
    Jump,
    CoolDownMultiplyer,
    Invincible,
    AttackSpeed,
    Block,
    InstantKill,
    Vampirism,
    Kills,
    PotionEffectiveness,
    CritChance,
    CritDamage,
    Knockback,
    Stun,
    Armour
}

public enum ItemSlots
{
    Armour,
    Boots,
    Weapon,
    Accessory,
    Skill1,
    Skill2,
    Skill3,
    Bag1,
    Bag2,
    Bag3
}

public enum BuffTypes
{
    Additive,
    Multiplicative,
    TotalAdditive
}

public enum EffectTargets
{
    Self,
    Target,
    Value
}

public enum LevelThemes
{
    Cosmos,
    Candy,
    Fall,
    Fertile,
    Grassland,
    Winter
}

public enum PlatformSubTheme
{
    Soil,
    Stone,
    Rock
}

public enum PathType
{
    Straight,
    Jump,
    Free
}

public enum AIState
{
    Correctional,
    Idling,
    Tired,
    Chasing
}