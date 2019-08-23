
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;
using System;


public class Hero : Character
{

    private bool jumpButtonDown = false;
    private float jumpTimer = -1;
    private const int JUMP_BOOST_DURATION = 45;

    public Hero(Vector2 pos, Sprite sprite, Sprite baseSwordTM, List<Stat> stats, List<Effect> effects) 
        : base(pos, new Vector2(68, 120), sprite, "Hero", "A mediocre hero at best", stats, effects, 
            new List<Item> { new BasicSword(new Vector2(0, 0), sprite, baseSwordTM, new List<Stat>(), new List<Effect>()) })
    {
        collisionArea.Add(new Vector2(-size.X / 2, -46));
        collisionArea.Add(new Vector2(size.X / 2, -46));
        collisionArea.Add(new Vector2(size.X / 2, 74));
        collisionArea.Add(new Vector2(-size.X / 2, 74));

        stats.Add(new Stat(50, StatTypes.Health, 50, 0));
        stats.Add(new Stat(6, StatTypes.Movement, null, 3));
        stats.Add(new Stat(7, StatTypes.Jump));

        effects.Add(new Effect(new Buff(new List<StatBuff> { new StatBuff(1, StatTypes.Invincible) }, 60), new List<StatTypes> { StatTypes.Health }, EffectTargets.Self));

        effects.Add(new StunEffect(new Buff(new List<StatBuff> { new StatBuff(1, StatTypes.Stun) }, 60), new List<StatTypes> { StatTypes.Attack }, EffectTargets.Target));

        equiptItem(new DashAttackSkill(Position, null, null, new List<Stat>(), new List<Effect>()));
    }

    public Hero(Hero h) : base(h)
    {

    }

    override
    public Character getCopy()
    {
        return new Hero(this);
    }

    /*override
    public void takeDamage(float damage)
    {
        base.takeDamage(damage);
        states[(int)TypesOfStates.Movement] = ObjectStates.KnockBack;
        states[(int)TypesOfStates.MovementLock] = ObjectStates.ControlLocked;
    }*/

    override
    public void update(float timeDisplacement)
    {
        base.update(timeDisplacement);

        if (jumpButtonDown && jumpTimer != -1)
            jumpTimer += timeDisplacement;
        if (!jumpButtonDown || Velocity.Y >= 0 || jumpTimer > JUMP_BOOST_DURATION)
            jumpTimer = -1;

        if (animationStates[(int)TypesOfStates.Attack] == ObjectStates.AttackPrep && animationTimer > calculateStat(StatTypes.AttackSpeed) / 4)
            attack();
    }

    override
    public void draw(SpriteBatch sb, Vector2 origin)
    {
        bool changed = false;
        for (int i = 0; i < Enum.GetNames(typeof(TypesOfStates)).Length; ++i)
            if (animationStates[i] != states[i])
                changed = true;
        if (changed)
        {
            if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Airborne && states[(int)TypesOfStates.Movement] != ObjectStates.Airborne
                || animationStates[(int)TypesOfStates.Movement] != ObjectStates.Airborne && states[(int)TypesOfStates.Movement] == ObjectStates.Airborne)
            {
                states[(int)TypesOfStates.Attack] = ObjectStates.Idle;
                animationStates[(int)TypesOfStates.Attack] = ObjectStates.Idle;
                ((Weapon)Items[(int)ItemSlots.Weapon]).deactivate();
            }
            animationTimer = 0;
            states.CopyTo(animationStates, 0);
        }
        
        int startSpriteIndex = 0;
        int spriteIndexOffset = 0;

        if(skillAnimationOverride != null)
        {
            spriteIndexOffset = ((Skill)Items[(int)skillAnimationOverride]).getAnimationIndex(animationTimer);
        }
        else if (animationStates[(int)TypesOfStates.Status] == ObjectStates.Stun || animationStates[(int)TypesOfStates.Status] == ObjectStates.Knockback)
        {
            int curAnimation = (int)animationTimer / 30;
            startSpriteIndex = 56;
            spriteIndexOffset = (curAnimation > 2) ? 2 : curAnimation;
            if (curAnimation > 3)
            {
                states[(int)TypesOfStates.MovementLock] = ObjectStates.Idle;
                animationTimer = 0;
            }
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Idle && animationStates[(int)TypesOfStates.Attack] != ObjectStates.Idle)
        {
            int curAnimation = (int)animationTimer / ((int)Items[(int)ItemSlots.Weapon].calculateStat(StatTypes.AttackSpeed) / 5);
            startSpriteIndex = 43;
            spriteIndexOffset = (curAnimation > 4) ? 4 : curAnimation;
            if (curAnimation > 5)
            {
                states[(int)TypesOfStates.Attack] = ObjectStates.Idle;
                animationTimer = 0;
                ((Weapon)Items[(int)ItemSlots.Weapon]).deactivate();
            }
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Running && animationStates[(int)TypesOfStates.Attack] != ObjectStates.Idle)
        {
            int curAnimation = (int)animationTimer / ((int)Items[(int)ItemSlots.Weapon].calculateStat(StatTypes.AttackSpeed) / 4);
            startSpriteIndex = 30;
            if (curAnimation < 4)
                spriteIndexOffset = (curAnimation > 3) ? 3 : curAnimation;
            else
                spriteIndexOffset = -18;
            if (curAnimation > 4)
            {
                states[(int)TypesOfStates.Attack] = ObjectStates.Idle;
                animationTimer = 0;
                ((Weapon)Items[(int)ItemSlots.Weapon]).deactivate();
            }
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Airborne && animationStates[(int)TypesOfStates.Attack] != ObjectStates.Idle)
        {
            int curAnimation = (int)animationTimer / ((int)Items[(int)ItemSlots.Weapon].calculateStat(StatTypes.AttackSpeed) / 3);
            startSpriteIndex = 34;
            spriteIndexOffset = (curAnimation > 2) ? 2 : curAnimation;
            if (curAnimation > 3)
            {
                states[(int)TypesOfStates.Attack] = ObjectStates.Idle;
                animationTimer = 0;
                ((Weapon)Items[(int)ItemSlots.Weapon]).deactivate();
            }
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Running)
        {
            spriteIndexOffset = (int)animationTimer / 7 % 6;
            startSpriteIndex = 8;
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Idle)
        {
            spriteIndexOffset = (int)animationTimer / 12 % 4;
            startSpriteIndex = 0;
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Airborne)
        {
            if (Velocity.Y < -4)
                startSpriteIndex = 28;
            else if (Velocity.Y <= 0)
                startSpriteIndex = 29;
            else if (Velocity.Y > 0)
            {
                spriteIndexOffset = (int)animationTimer / 7 % 2;
                startSpriteIndex = 54;
            }
        }

        Items[(int)ItemSlots.Weapon].Sprite.drawSpriteSheet(sb, Position, origin, direction, startSpriteIndex, spriteIndexOffset);
    }

    override
    public void collide(CollidableObj obj, List<Vector2>[] axis)
    {

    }

    public void react(List<PlayerActions> actions)
    {
        jumpButtonDown = false;
        if (states[(int)TypesOfStates.MovementLock] != ObjectStates.ControlLocked)
        {
            setVelocity(new Vector2(0, Velocity.Y));
            foreach (PlayerActions action in actions)
                switch (action)
                {
                    case PlayerActions.MoveLeft:
                        moveLeft();
                        break;
                    case PlayerActions.MoveRight:
                        moveRight();
                        break;
                    case PlayerActions.Jump:
                        jumpButtonDown = true;
                        if (jump())
                            jumpTimer = 0;
                        if (jumpTimer > 0)
                            addVelocity(new Vector2(0, -0.2f));
                        break;
                    case PlayerActions.Attack:
                        if(states[(int)TypesOfStates.Attack] != ObjectStates.Attacking)
                        states[(int)TypesOfStates.Attack] = ObjectStates.AttackPrep;
                        break;
                    case PlayerActions.Skill1:
                    case PlayerActions.Skill2:
                    case PlayerActions.Skill3:
                        ItemSlots skill = (ItemSlots)(int)action + ((int)ItemSlots.Skill1 - (int)PlayerActions.Skill1);
                        if (Items[(int)skill] != null && Items[(int)skill].activate()) 
                            skillAnimationOverride = skill;
                        break;
                }
        }
    }

}